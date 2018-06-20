using System.Collections.Generic;
using Anonimize.Migrator.IO;
using Anonimize.Migrator.Models;
using NLog;

namespace Anonimize.Migrator.Services
{
    public abstract class AUpdateService
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected IList<Table> tables;

        protected AUpdateService(JConfig jConfig) : this(jConfig.Tables) { }

        protected AUpdateService(IList<Table> tables)
        {
            this.tables = tables;
        }

        public virtual bool Update()
        {
            var updated = true;
            foreach (var table in tables)
            {
                logger.Debug("Updating table '{0}'", table.Name);
                updated &= UpdateTable(table);
            }
            return updated;
        }

        protected virtual bool UpdateTable(Table table)
        {
            var updated = true;
            foreach (var column in table.Columns)
            {
                logger.Debug("Updating column '{0}'", column.Name);
                updated &= UpdateTableColumn(table, column);
            }
            return updated;
        }

        protected virtual bool UpdateTableColumn(Table table, TableColumn column)
        {
            return false;
        }
    }
}
