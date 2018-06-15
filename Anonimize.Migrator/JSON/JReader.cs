using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using Newtonsoft.Json;

namespace Anonimize.Migrator.JSON
{
    public class JReader<T> : IDisposable where T : class
    {
        T document;
        string uri;

        public T Document { get => document; }
        public string Uri { get => uri; }

        public JReader(string uri)
        {
            this.uri = uri;
        }

        /// <summary>
        /// Reads the JSON document.
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FileLoadException"></exception>
        public virtual void ReadJsonDocument()
        {
            if (!File.Exists(uri))
                throw new FileNotFoundException($"File '{uri}' not found.");

            using (StreamReader file = File.OpenText(uri))
            {
                var serializer = new JsonSerializer();
                document = (T)serializer.Deserialize(file, typeof(T));
            }

            if (document == null)
                throw new FileLoadException($"Document '{uri}' couldn't be loaded.");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                document = null;
            }
        }
    }
}
