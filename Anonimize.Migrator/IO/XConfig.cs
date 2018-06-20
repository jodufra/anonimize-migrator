using System;
using System.Configuration;
using System.Linq;
using System.Xml.Linq;
using Anonimize.Services;

namespace Anonimize.Migrator.IO
{
    public class XConfig : XContext
    {
        readonly string CONNECTION_NAME = ConfigurationManager.AppSettings["AppConfig:Connection"];
        readonly string CONNECTION_STRING_POSFIX = ConfigurationManager.AppSettings["AppConfig:ConnectionStringPosfix"];

        string connectionString;
        public string ConnectionString => connectionString;

        string iv;
        public string Iv => iv;

        string key;
        public string Key => key;

        public XConfig() : base(ConfigurationManager.AppSettings["Uri:AppConfig"])
        {

        }

        public override void ReadXmlDocument()
        {
            base.ReadXmlDocument();

            connectionString = GetConnectionString(Document, CONNECTION_NAME);
            if (!connectionString.EndsWith(";", StringComparison.Ordinal))
                connectionString += ";";
            connectionString += CONNECTION_STRING_POSFIX;

            var anonimize = AnonimizeProvider.GetInstance();
            var serviceName = GetAppSetting(Document, "Anonimize:CryptoService");

            if (!string.IsNullOrEmpty(serviceName))
            {
                serviceName = serviceName.ToUpperInvariant().Trim();

                if (serviceName.StartsWith("AES"))
                {
                    anonimize.SetCryptoService(new AesCryptoService());
                }
                else if(serviceName.StartsWith("TRIPLEDES"))
                {
                    anonimize.SetCryptoService(new TripleDESCryptoService());
                }
            }

            var iCryptoService = anonimize.GetCryptoService();

            if (iCryptoService is BaseSymmetricCryptoService cryptoService)
            {
                iv = GetAppSetting(Document, "Anonimize:Iv");
                if (!string.IsNullOrWhiteSpace(iv))
                    cryptoService.SetIV(iv);

                key = GetAppSetting(Document, "Anonimize:Key");
                if (!string.IsNullOrWhiteSpace(key))
                    cryptoService.SetIV(key);
            }
        }

        public static string GetConnectionString(XDocument document, string name)
        {
            var query = document.Root.Descendants();
            query = query.Where(q => q.Name == "connectionStrings").SelectMany(q => q.Descendants());
            query = query.Where(q => (string)q.Attribute("name") == name);

            var connectionStrings = query.Select(q => (string)q.Attribute("connectionString"));

            return connectionStrings.FirstOrDefault();
        }

        public static string GetAppSetting(XDocument document, string key)
        {
            var query = document.Root.Descendants();
            query = query.Where(q => q.Name == "appSettings").SelectMany(q => q.Descendants());
            query = query.Where(q => (string)q.Attribute("key") == key);

            var settings = query.Select(q => (string)q.Attribute("value"));

            return settings.FirstOrDefault();
        }

    }
}
