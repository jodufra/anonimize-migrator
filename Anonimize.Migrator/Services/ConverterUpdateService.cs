using Anonimize.Migrator.IO;
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

            if (updated)
            {
                xEntities.SaveXmlDocument();
            }

            return updated;
        }

        protected override bool UpdateTableColumn(Table table, TableColumn column)
        {
            return xEntities.SetConverter(table.Name, column.Name, column.Converter);
        }
    }
}
