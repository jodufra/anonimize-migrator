using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using NLog;

namespace Anonimize.Migrator.XML
{
    public class XEntities : XContext
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        readonly XNamespace XMLNS = ConfigurationManager.AppSettings["Entities:Xmlns"];

        public XEntities() : base(ConfigurationManager.AppSettings["Uri:Entities"])
        {
        }

        public bool SetConverter(string className, string propertyName, string converter)
        {
            var orm = Document.Descendants(XMLNS + "orm");
            var ormNamespace = orm.Descendants(XMLNS + "namespace");
            var ormClass = ormNamespace.Descendants(XMLNS + "class").FirstOrDefault(q => (string)q.Attribute("name") == className);

            if (ormClass == null)
            {
                logger.Debug("<orm:class name=\"{0}\"> not found", className);
                return false;
            }

            var tableName = ormClass.Descendants(XMLNS + "table").Select(q => (string)q.Attribute("name")).First();
            var ormField = ormClass.Descendants(XMLNS + "field").FirstOrDefault(q => (string)q.Attribute("property") == propertyName);

            if (ormField == null)
            {
                logger.Debug("<orm:field property=\"{1}\"> of class {0} not found", className, propertyName);
                return false;
            }

            ApplyConverter(ormField.Descendants().First(), converter);

            var ormSchema = orm.Descendants(XMLNS + "schema");
            var ormTable = ormSchema.Descendants(XMLNS + "table").FirstOrDefault(q => (string)q.Attribute("name") == tableName);

            if (ormTable == null)
            {
                logger.Debug("<orm:table name=\"{0}\"> not found", tableName);
                return false;
            }

            var ormTableColumn = ormTable.Descendants(XMLNS + "column").First(q => (string)q.Attribute("name") == propertyName);

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
                {
                    logger.Debug("Added converter '{0}'", converter);
                    element.Add(new XAttribute("converter", converter));
                }
                else if (attribute.Value != converter)
                {
                    logger.Debug("Updated converter from '{0}' to '{1}'", attribute.Value, converter);
                    attribute.Value = converter;
                }
                else
                {
                    logger.Debug("Previously included converter");
                }
            }
            else if (attribute != null)
            {
                logger.Debug("Removed converter '{0}'", attribute.Value);
                attribute.Remove();
            }
            else
            {
                logger.Debug("Previously removed converter");
            }
        }

    }
}
