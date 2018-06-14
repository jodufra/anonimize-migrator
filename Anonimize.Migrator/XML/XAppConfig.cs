using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Anonimize.Migrator.XML
{
    public class XAppConfig : XReader
    {
        const string CONNECTION_NAME = "Connection";
        const string URI = @"..\App.config";

        string connectionString;

        public XAppConfig() : base(URI)
        {
        }

        public string ConnectionString
        {
            get
            {
                if (connectionString == null)
                    connectionString = GetConnectionString(Document, CONNECTION_NAME);
                return connectionString;
            }
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
