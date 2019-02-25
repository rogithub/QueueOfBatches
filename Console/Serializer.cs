using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Console
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
	}
}
