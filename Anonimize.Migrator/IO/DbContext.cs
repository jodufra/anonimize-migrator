using System;
using System.Collections.Generic;
using Dapper;
using NLog;

namespace Anonimize.Migrator.IO
{
    public class DbContext : IDisposable
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected DbManager DbManager { get; private set; }

        public DbContext(string connectionString)
        {
            DbManager = new DbManager(connectionString);
        }

        public IEnumerable<dynamic> ReadAll(string table)
        {
            logger.Debug("Reading table '{0}'", table);

            var query = $"SELECT * FROM `{table}`";

            logger.Debug(query);

            using (var transaction = DbManager.BeginTransaction())
            {
                var db = transaction.Connection;
                return db.Query(query);
            }
        }

        public void Update(string table, Dictionary<string, string> columns, Tuple<string, int> primaryKey)
        {
            logger.Debug("Updating table '{0}'", table);

            var querySegments = new List<string>
            {
                $"UPDATE `{table}`"
            };

            var setSegments = new List<string>();
            foreach (var column in columns)
            {
                var value = column.Value != null ? $"'{column.Value}'" : "NULL";
                setSegments.Add($"`{column.Key}` = {value}");
            }

            querySegments.Add($"SET {string.Join(", ", setSegments)}");
            querySegments.Add($"WHERE `{primaryKey.Item1}` = {primaryKey.Item2};");

            var query = string.Join(" ", querySegments);

            logger.Debug(query);

            using (var transaction = DbManager.BeginTransaction())
            {
                var db = transaction.Connection;
                db.Execute(query);
            }
        }

        public void AlterTable(string table, IEnumerable<string> columns)
        {
            logger.Debug("Altering table '{0}'", table);

            var querySegments = new List<string>
            {
                $"ALTER TABLE `{table}`"
            };

            var alterSegments = new List<string>();
            foreach (var column in columns)
            {
                alterSegments.Add($"MODIFY COLUMN `{column}` varchar(255)");
            }

            querySegments.Add(string.Join(", ", alterSegments));

            var query = string.Join(" ", querySegments) + ";";

            logger.Debug(query);

            using (var transaction = DbManager.BeginTransaction())
            {
                var db = transaction.Connection;
                db.Execute(query);
            }
        }

        public IEnumerable<Schema> GetTableSchema(string table)
        {
            logger.Debug("Getting schema from table '{0}'", table);

            using (var transaction = DbManager.BeginTransaction())
            {
                var db = transaction.Connection;
                var query = string.Format(QUERY_TABLE_SCHEMA, db.Database, table);
                logger.Debug(query);
                return db.Query<Schema>(query);
            }
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                DbManager?.Dispose();
            }
        }

        public class Schema
        {
            public string ColumnName { get; set; }
            public string DataType { get; set; }
            public double? Length { get; set; }

            public bool RequiresUpdate()
            {
                if (DataType.Contains("text"))
                    return false;

                if(!DataType.Contains("char"))
                    return true;

                if (!Length.HasValue || Length.Value < 255)
                    return true;

                return false;
            }
        }

        #region Queries
        public const string QUERY_TABLE_SCHEMA = @"
SELECT COLUMN_NAME AS 'ColumnName', DATA_TYPE AS 'DataType', CHARACTER_MAXIMUM_LENGTH AS 'Length'
FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{0}' AND TABLE_NAME = '{1}';";
        #endregion
    }
}
