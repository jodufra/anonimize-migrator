using System;
using System.IO;
using Newtonsoft.Json;

namespace Anonimize.Migrator.JSON
{
    public class JReader<T> : IDisposable where T : class
    {
        public T Document { get; private set; }
        public string Uri { get; private set; }

        public JReader(string uri)
        {
            Uri = uri;
        }

        /// <summary>
        /// Reads the JSON document.
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FileLoadException"></exception>
        public virtual void ReadJsonDocument()
        {
            if (!File.Exists(Uri))
                throw new FileNotFoundException($"File '{Uri}' not found.");

            using (StreamReader file = File.OpenText(Uri))
            {
                var serializer = new JsonSerializer();
                Document = (T)serializer.Deserialize(file, typeof(T));
            }

            if (Document == null)
                throw new FileLoadException($"Document '{Uri}' couldn't be loaded.");
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
                Document = null;
            }
        }
    }
}
