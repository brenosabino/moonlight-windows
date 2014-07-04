﻿using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Threading; 
namespace Limelight
{
    /// <summary>
    /// XmlQuery object contains an XML string and methods to parse it
    /// </summary>
    public class XmlQuery : IDisposable
    {
        private ManualResetEvent completeEvent;
        private Uri uri;
        private XDocument rawXml;
        private string rawXmlString;
        WebClient client = new WebClient();

        /// <summary>
        ///  Initializes a new instance of the <see cref="XmlQuery"/> class. 
        /// </summary>
        /// <param name="url">URL of XML page</param>
        public XmlQuery(string url)
        {
            uri = new Uri(url);
            completeEvent = new ManualResetEvent(false);
            GetXml(); 
        }
        /// <summary>
        /// Gets the Xml as a string from the URL provided
        /// </summary>
        /// <returns>The server info XML as a string</returns>
        private void GetXml()
        {
            // TODO Handle page not found
            lock (this)
            {
                if (rawXmlString == null)
                {
                    client.DownloadStringCompleted += new DownloadStringCompletedEventHandler(XmlCallback);
                    client.DownloadStringAsync(uri);


                    // Wait for the callback to complete
                    completeEvent.WaitOne();
                    completeEvent.Dispose(); 
                }
            }
            this.rawXml = XDocument.Parse(rawXmlString); 
        }
        /// <summary>
        /// Given a tag, return the first XML attribute contained in this tag as a string
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public string XmlAttribute(string tag)
        {
            // TODO handle not found
            var query = from c in rawXml.Descendants(tag) select c;
            string attribute = query.FirstOrDefault().Value;
            return attribute;
        }

        /// <summary>
        /// Given a tag, return the first XML attribute contained in this tag as an XElement
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public XElement XmlAttributeElement(string tag)
        {
            // TODO handle not found
            var query = from c in rawXml.Descendants(tag) select c;
            return query.FirstOrDefault(); 
        }
       
        /// <summary>
        /// Given an XElement and a tag, search within that element for the first attribute contained within the tag
        /// </summary>
        /// <param name="tag">XML tag</param>
        /// <param name="element">XElement to search within</param>
        /// <returns></returns>
        public string XmlAttributeFromElement(string tag, XElement element){
            // TODO handle not found
            var query = from c in element.Descendants(tag) select c;
            string attribute = query.FirstOrDefault().Value;
            return attribute;
        }

        /// <summary>
        /// Sets the xmlString field to the XML string we just downloaded
        /// </summary>
        private void XmlCallback(object sender, DownloadStringCompletedEventArgs e)
        {
            // TODO error check
            if (e.Error != null) { 
                ExitFailure(e.Error.Message);
        }

            this.rawXmlString = e.Result;
  
            completeEvent.Set();
        }

        private void ExitFailure(string failureMessage)
        {
            Debug.WriteLine("Failed: " + failureMessage);
            MessageBox.Show(failureMessage);
            // TODO reset everything
        }

        #region IDisposable implementation

        protected virtual void Dispose(bool managed)
        {
            if (managed)
            {
                completeEvent.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable implementation
    }
}