using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Start_a_Town_
{
    static public class XmlHelper
    {
        public static XDocument ToXDocument(this XmlDocument xmlDocument)
        {
            using var nodeReader = new XmlNodeReader(xmlDocument);
            nodeReader.MoveToContent();
            return XDocument.Load(nodeReader);
        }
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            using (var reader = xDocument.CreateReader())
            {
                XmlDocument xml = new();
                xml.Load(reader);
                return xml;
            }
        }
        public static string GetValueOrDefault(this XElement root, string name, string defaultVal)
        {
            var node = root.Descendants(name).FirstOrDefault();
            return node != null ? node.Value : defaultVal;
        }
        public static bool TryGetValue(this XElement root, string name, Action<string> valueFunc)
        {
            if (root.Descendants(name).FirstOrDefault() is XElement node)
            {
                valueFunc(node.Value);
                return true;
            }
            return false;
        }
    }
}
