using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

namespace DataBase
{
	public static class Serializer
	{
		public static byte[] Serialize(object obj)
		{
			BinaryFormatter bf = new BinaryFormatter();
			using (var ms = new MemoryStream())
			{
				bf.Serialize(ms, obj);
				return ms.ToArray();
			}
		}

		public static T Deserialize<T>(byte[] param)
		{
			using (MemoryStream ms = new MemoryStream(param))
			{
				IFormatter br = new BinaryFormatter();
				return (T)br.Deserialize(ms);
			}
		}

		public static string XmlSerialize<T>(T obj)
		{
			XmlSerializer xsSubmit = new XmlSerializer(typeof(T));

			using (var sww = new StringWriter())
			{
				using (XmlWriter writer = XmlWriter.Create(sww))
				{
					xsSubmit.Serialize(writer, obj);
					return sww.ToString();
				}
			}
		}

		public static T XmlDeserialize<T>(string xml)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(T));
			using (TextReader reader = new StringReader(xml))
			{
				return (T)serializer.Deserialize(reader);
			}
		}
	}
}
