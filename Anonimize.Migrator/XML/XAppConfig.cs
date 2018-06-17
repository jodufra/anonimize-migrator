using System;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;

namespace Anonimize.Migrator.XML
{
    public class XAppConfig : XContext
    {
        readonly string CONNECTION_NAME = ConfigurationManager.AppSettings["AppConfig:Connection"];
        readonly string CONNECTION_STRING_POSFIX = ConfigurationManager.AppSettings["AppConfig:ConnectionStringPosfix"];

        string connectionString;

        public XAppConfig() : base(ConfigurationManager.AppSettings["Uri:AppConfig"])
        {
        }

        public string ConnectionString
        {
            get
            {
                if (connectionString == null)
                {
                    connectionString = GetConnectionString(Document, CONNECTION_NAME);
                    if (!connectionString.EndsWith(";", StringComparison.Ordinal))
                        connectionString += ";";
                    connectionString += CONNECTION_STRING_POSFIX;
                }
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
