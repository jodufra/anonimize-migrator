using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anonimize.Migrator.JSON;
using Anonimize.Migrator.Models;
using NLog;

namespace Anonimize.Migrator.Services
{
    public abstract class AUpdateService
    {
        protected IList<Table> tables;

        protected AUpdateService(JConfig jConfig) : this(jConfig.Document.Tables) { }

        protected AUpdateService(IList<Table> tables)
        {
            this.tables = tables;
        }

        public virtual bool Update()
        {
            var migrated = true;
            foreach (var table in tables)
            {
                migrated &= UpdateTable(table);
            }
            return migrated;
        }

        public virtual bool UpdateTable(Table table)
        {
            var migrated = true;
            foreach (var column in table.Columns)
            {
                migrated &= UpdateTableColumn(table, column);
            }
            return migrated;
        }

        public abstract bool UpdateTableColumn(Table table, TableColumn tableColumn);
    }
}
