using System;
using System.Collections.Generic;

namespace Anonimize.Migrator.Models
{
    public class Table
    {
        public string Name { get; set; }
        public string PrimaryKey { get; set; }
        public IList<TableColumn> Columns { get; set; }

        public string NameSnakeCase => Name.ToSnakeCase();
    }
}
