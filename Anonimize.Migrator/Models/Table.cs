using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anonimize.Migrator.Models
{
    public class Table
    {
        public string Name { get; set; }
        public IList<TableColumn> Columns { get; set; }
    }
}
