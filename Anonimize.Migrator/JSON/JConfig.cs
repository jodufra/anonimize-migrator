using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anonimize.Migrator.Models;
using Newtonsoft.Json;

namespace Anonimize.Migrator.JSON
{
    public class JConfig : JReader<JConfig.Config>
    {
        const string URI = "config.json";

        public JConfig() : base(URI)
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
