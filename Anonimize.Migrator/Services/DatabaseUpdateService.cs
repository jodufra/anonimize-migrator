using System;
using System.Collections.Generic;
using System.Linq;
using Anonimize.Migrator.Database;
using Anonimize.Migrator.JSON;
using Anonimize.Migrator.Models;
using Anonimize.Migrator.XML;
using NLog;

namespace Anonimize.Migrator.Services
{
    public class DatabaseUpdateService : AUpdateService
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected XAppConfig xAppConfig;
        DbContext dbContext;

        public DatabaseUpdateService(JConfig jConfig, XAppConfig xAppConfig) : base(jConfig)
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

            logger.Info("Updating table `{0}` with total count of {1}", table.NameSnakeCase, items.Count());

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
