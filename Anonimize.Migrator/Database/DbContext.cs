using System;
using System.Collections.Generic;
using Dapper;
using NLog;

namespace Anonimize.Migrator.Database
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

            var querySegments = new List<string> {
                $"UPDATE `{table}`"
            };

            var setSegments = new List<string>();
            foreach (var column in columns)
            {
                var value = column.Value != null ? $"'{column.Value}'" : "NULL";
                setSegments.Add($"`{column.Key}` = {value}");
            }

            querySegments.Add($"SET {string.Join(", ", setSegments)}");
            querySegments.Add($"WHERE `{primaryKey.Item1}` = {primaryKey.Item2}");

            var query = string.Join(" ", querySegments);

            logger.Debug(query);

            using (var transaction = DbManager.BeginTransaction())
            {
                var db = transaction.Connection;
                db.Execute(query);
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
    }
}
