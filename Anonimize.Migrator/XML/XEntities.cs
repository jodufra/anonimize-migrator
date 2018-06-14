using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Anonimize.Migrator.XML
{
    public class XEntities : XReader
    {
        const string URI = @"..\Entities\Generated\EntitiesModel.rlinq";

        string connectionString;

        public XEntities() : base(URI)
        {
        }

        public static string GetConnectionString(XDocument document, string connectionName)
        {
            var query = from c in document.Root.Descendants("connectionStrings").Descendants()
                        where (string)c.Attribute("name") == connectionName
                        select c.Element("connectionString").Value;

            return query.FirstOrDefault();
        }

    }
}
