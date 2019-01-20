using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Flatsch.Helper
{
    // see https://stackoverflow.com/questions/3784477/c-sharp-approach-for-saving-user-settings-in-a-wpf-application
    public class XmlSerializerHelper
    {
        public static string Serialize<T>(T obj)
        {
            using (var sw = new StringWriter())
            using (var xw = XmlWriter.Create(sw))
            {
                new XmlSerializer(typeof(T)).Serialize(xw, obj);
                return sw.GetStringBuilder().ToString();
            }
        }

        public static T Deserialize<T>(string xmlSring)
        {
            using (var xw = XmlReader.Create(new StringReader(xmlSring)))
                return (T)new XmlSerializer(typeof(T)).Deserialize(xw);
        }
    }
}
