using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DataBase
{

	internal static class Serializer
	{
		private static string StringFile()
		{
			return "using System;" +
			"using System.IO;" +
			"using System.Threading;" +
			"namespace Examples" +
			"{" +
			"	public class FileCreator" +
			"	{" +
			"		private string FolderPath { get; set; }" +
			"		public FileCreator(string folderPath)" +
			"		{" +
			"			this.FolderPath = folderPath;" +
			"		}" +
			"		public void CreateFile(Guid name)" +
			"		{" +
			"			string fileName = string.Format(\"{0}.txt\", name.ToString());" +
			"			string path = Path.Combine(this.FolderPath, fileName);" +
			"			if (!File.Exists(path))" +
			"			{" +
			"				using (StreamWriter sw = File.CreateText(path))" +
			"				{" +
			"					sw.WriteLine(\"file: {0}\", fileName);" +
			"					sw.WriteLine(\"Created from thread id: {0}\", Thread.CurrentThread.ManagedThreadId);" +
			"				}" +
			"			}" +
			"			else" +
			"			{" +
			"				throw new Exception(string.Format(\"duplicated file found {0}\", fileName));" +
			"			}" +
			"		}" +
			"	}" +
			"}";
		}
		/// <summary>
		/// https://stackoverflow.com/questions/7629799/how-to-serialize-deserialize-an-assembly-object-to-and-from-a-byte-array
		/// </summary>
		/// <param name="sourceFiles"></param>
		/// <returns></returns>
		public static byte[] SerializeAssembly()
		{
			var compilerOptions = new Dictionary<string, string> { { "CompilerVersion", "v4.0" } };
			CSharpCodeProvider provider = new CSharpCodeProvider(compilerOptions);

			CompilerParameters parameters = new CompilerParameters()
			{
				GenerateExecutable = false,
				GenerateInMemory = false,
				OutputAssembly = "Examples.dll",
				IncludeDebugInformation = false,
			};
			parameters.ReferencedAssemblies.Add("System.dll");

			ICodeCompiler compiler = provider.CreateCompiler();
			CompilerResults results = compiler.CompileAssemblyFromSource(parameters, StringFile());

			return File.ReadAllBytes(results.CompiledAssembly.Location);

		}

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
