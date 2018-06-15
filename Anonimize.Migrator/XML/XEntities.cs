using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NLog;

namespace Anonimize.Migrator.XML
{
    public class XEntities : XReader
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        const string URI = @"..\Entities\Generated\EntitiesModel.rlinq";

        public XEntities() : base(URI)
        {
        }

        public bool SetConverter(string className, string propertyName, string converter)
        {
            XNamespace xmlns = "http://www.telerik.com/orm";

            var orm = Document.Root.Descendants(xmlns + "orm");
            var ormNamespace = orm.Descendants(xmlns + "namespace");
            var ormClass = ormNamespace.Descendants(xmlns + "class").Where(q => (string)q.Attribute("name") == className).FirstOrDefault();

            if (ormClass == null)
            {
                logger.Debug("<orm:class name=\"{0}\"> not found", className);
                return false;
            }

            var tableName = ormClass.Descendants(xmlns + "table").Select(q => (string)q.Attribute("name")).First();
            var ormField = ormClass.Descendants(xmlns + "field").Where(q => (string)q.Attribute("property") == propertyName).FirstOrDefault();

            if (ormField == null)
            {
                logger.Debug("<orm:field property=\"{1}\"> of class {0} not found", className, propertyName);
                return false;
            }
            
            ApplyConverter(ormField.Descendants().First(), converter);

            var ormSchema = orm.Descendants(xmlns + "schema");
            var ormTable = ormSchema.Descendants(xmlns + "table").Where(q => (string)q.Attribute("name") == tableName).FirstOrDefault();

            if (ormTable == null)
            {
                logger.Debug("<orm:table name=\"{0}\"> not found", tableName);
                return false;
            }

            var ormTableColumn = ormTable.Descendants(xmlns + "column").Where(q => (string)q.Attribute("name") == propertyName).First();

            ApplyConverter(ormTableColumn, converter);

            return true;
        }

        static void ApplyConverter(XElement element, string converter)
        {
            var applyConverter = !string.IsNullOrWhiteSpace(converter);
            var attribute = element.Attribute("converter");

            if (applyConverter)
            {
                if (attribute == null)
                    element.Add(new XAttribute("converter", converter));
                else
                    attribute.Value = converter;
            }
            else if (attribute != null)
            {
                attribute.Remove();
            }
        }

    }
}
