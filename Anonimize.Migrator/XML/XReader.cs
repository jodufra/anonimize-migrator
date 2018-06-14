using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Anonimize.Migrator.XML
{
    public class XReader : IDisposable
    {
        XDocument document;
        string uri;
        bool isDisposed;

        public XDocument Document { get => document; }
        public string Uri { get => uri; }

        public XReader(string uri)
        {
            this.uri = uri;
        }

        protected virtual void Load()
        {
            document = XDocument.Load(uri);
        }

        public virtual void SaveChanges()
        {
            document.Save(uri);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            if (disposing)
                document = null;

            isDisposed = true;
        }
    }
}
