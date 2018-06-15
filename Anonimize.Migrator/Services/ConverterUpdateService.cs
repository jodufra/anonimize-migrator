using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anonimize.Migrator.XML;
using Anonimize.Migrator.JSON;
using Anonimize.Migrator.Models;

namespace Anonimize.Migrator.Services
{
    public class ConverterUpdateService : AUpdateService
    {
        protected XEntities xEntities;

        public ConverterUpdateService(JConfig jConfig, XEntities xEntities) : base(jConfig)
        {
            this.xEntities = xEntities;
        }

        public override bool Update()
        {
            var updated = base.Update();

            xEntities.SaveXmlDocument();

            return updated;
        }

        public override bool UpdateTableColumn(Table table, TableColumn column)
        {
            return xEntities.SetConverter(table.Name, column.Name, column.Converter);
        }
    }
}
