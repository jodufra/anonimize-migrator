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
            var query = document.Root.Descendants();
            query = query.Where(q => q.Name == "connectionStrings").SelectMany(q => q.Descendants());
            query = query.Where(q => (string)q.Attribute("name") == connectionName);

            var connectionStrings = query.Select(q => (string)q.Attribute("connectionString"));

            return connectionStrings.FirstOrDefault();
        }

    }
}
