﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;

namespace Anonimize.Migrator.XML
{
    public class XReader : IDisposable
    {
        XDocument document;
        string uri;

        public XDocument Document { get => document; }
        public string Uri { get => uri; }

        public XReader(string uri)
        {
            this.uri = uri;
        }

        /// <summary>
        /// Reads the XML document.
        /// </summary>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="FileLoadException"></exception>
        public virtual void ReadXmlDocument()
        {
            if(!File.Exists(uri))
                throw new FileNotFoundException($"File '{uri}' not found.");

            document = XDocument.Load(uri);

            if (document == null)
                throw new FileLoadException($"Document '{uri}' couldn't be loaded.");
        }

        /// <summary>
        /// Saves the XML document.
        /// </summary>
        public virtual void SaveXmlDocument()
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
            if (disposing)
            {
                document = null;
            }
        }
    }
}
