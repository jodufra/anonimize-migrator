using System;
using System.Collections.Generic;
using System.Linq;
using Anonimize.Migrator.IO;
using Anonimize.Migrator.Models;
using NLog;

namespace Anonimize.Migrator.Services
{
    public class DatabaseUpdateService : AUpdateService
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected XConfig xAppConfig;
        DbContext dbContext;

        public DatabaseUpdateService(JConfig jConfig, XConfig xAppConfig) : base(jConfig)
        {
            this.xAppConfig = xAppConfig;
        }

        public override bool Update()
        {
            try
            {
                using (dbContext = new DbContext(xAppConfig.ConnectionString))
                {
                    return base.Update();
                }
            }
            catch (Exception ex)
            {
                dbContext?.Dispose();
                logger.Fatal(ex.Message);
                logger.Debug(ex);
                return false;
            }
        }

        protected override bool UpdateTable(Table table)
        {
            if (!table.Columns.Any())
            {
                logger.Info("No columns for table `{0}`", table.Name);
                return true;
            }

            var items = (IEnumerable<IDictionary<string, object>>)dbContext.ReadAll(table.NameSnakeCase);
            if (!items.Any())
            {
                logger.Info("No actions needed for table `{0}`", table.NameSnakeCase);
                return true;
            }

            var modelItem = items.First();
            if (!modelItem.ContainsKey(table.PrimaryKey))
            {
                logger.Warn("Missing primary key `{0}` of table `{1}`", table.PrimaryKey, table.NameSnakeCase);
                return false;
            }

            foreach (var column in table.Columns)
            {
                if (!modelItem.ContainsKey(column.Name))
                {
                    logger.Warn("Missing column `{0}` of table `{1}`", column.Name, table.NameSnakeCase);
                    return false;
                }
            }

            var schemas = dbContext.GetTableSchema(table.NameSnakeCase).Where(q => q.RequiresUpdate());

            var columnsToAlter = new List<string>();

            foreach (var column in table.Columns)
            {
                if (schemas.Where(q => q.ColumnName == column.Name).Any())
                {
                    columnsToAlter.Add(column.Name);
                }
            }

            if (columnsToAlter.Any())
            {
                logger.Info($"Altering table `{table.NameSnakeCase}`");
                columnsToAlter.ForEach(column => logger.Info($"Altering column `{table.NameSnakeCase}`.`{column}`"));
                dbContext.AlterTable(table.NameSnakeCase, columnsToAlter);
            }


            logger.Info("Updating table `{0}` with total count of {1}", table.NameSnakeCase, items.Count());

            var anonimize = AnonimizeProvider.GetInstance();
            var cryptoService = anonimize.GetCryptoService();

            foreach (var item in items)
            {
                logger.Debug("Anonimizing `{0}`.`{1}` = {2}", table.NameSnakeCase, table.PrimaryKey, item[table.PrimaryKey]);

                var primaryKey = new Tuple<string, int>(table.PrimaryKey, (int)item[table.PrimaryKey]);
                var columns = new Dictionary<string, string>();

                foreach (var column in table.Columns)
                {
                    var updateColumn = false;
                    var decryptedValue = item[column.Name];
                    var encryptedValue = (string)decryptedValue;

                    if (decryptedValue != null && !decryptedValue.IsEncrypted())
                    {
                        logger.Debug("Encrypting field {0}", column.Name);
                        encryptedValue = cryptoService.Encrypt(decryptedValue);
                        updateColumn = true;
                    }
                    else if (decryptedValue == null)
                    {
                        logger.Debug("Field {0} is null", column.Name);
                    }
                    else
                    {
                        logger.Debug("Field {0} was previously encrypted", column.Name);
                    }

                    if (updateColumn)
                    {
                        columns.Add(column.Name, encryptedValue);
                    }
                }

                if (columns.Any())
                {
                    dbContext.Update(table.NameSnakeCase, columns, primaryKey);
                }
                else
                {
                    logger.Debug("No updates needed for `{0}`.`{1}` = {2}", table.NameSnakeCase, table.PrimaryKey, item[table.PrimaryKey]);
                }
            }

            return true;
        }

    }
}
