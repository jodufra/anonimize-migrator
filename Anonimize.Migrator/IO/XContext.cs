using System;
using System.Xml.Linq;
using System.IO;

namespace Anonimize.Migrator.IO
{
    public class XContext : IDisposable
    {
        public XDocument Document { get; private set; }
        public string Uri { get; private set; }

        public XContext(string uri)
        {
            Uri = uri;
        }

        /// <summary>
        /// Reads the XML document.
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FileLoadException"></exception>
        public virtual void ReadXmlDocument()
        {
            if(!File.Exists(Uri))
                throw new FileNotFoundException($"File '{Uri}' not found.");

            Document = XDocument.Load(Uri);

            if (Document == null)
                throw new FileLoadException($"Document '{Uri}' couldn't be loaded.");
        }

        /// <summary>
        /// Saves the XML document.
        /// </summary>
        public virtual void SaveXmlDocument()
        {
            Document.Save(Uri);
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
