namespace Anonimize.Migrator.Models
{
    public class TableColumn
    {
        public string Name { get; set; }
        public string Converter { get; set; }

        public bool HasConverter => !string.IsNullOrWhiteSpace(Converter);
    }
}
