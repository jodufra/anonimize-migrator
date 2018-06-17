using System.Collections.Generic;
using System.Configuration;
using Anonimize.Migrator.Models;
using Newtonsoft.Json;

namespace Anonimize.Migrator.JSON
{
    public class JConfig : JReader<JConfig.Config>
    {
        public IList<Table> Tables => Document?.Tables;

        public JConfig() : base(ConfigurationManager.AppSettings["Uri:Config"])
        {
        }

        public class Config
        {
            public IList<Table> Tables;

            public override string ToString()
            {
                return JsonConvert.SerializeObject(Tables, Formatting.Indented);
            }
        }
    }
}
