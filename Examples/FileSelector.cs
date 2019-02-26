using System;
using System.IO;
using System.Threading;

namespace Examples
{
	public class FileCreator
	{
		private string FolderPath { get; set; }
		public FileCreator(string folderPath)
		{
			this.FolderPath = folderPath;
		}

		public void CreateFile(Guid name)
		{
			string fileName = string.Format("{0}.txt", name.ToString());
			string path = Path.Combine(this.FolderPath, fileName);

			if (!File.Exists(path))
			{
				// Create a file to write to.
				using (StreamWriter sw = File.CreateText(path))
				{
					sw.WriteLine("file: {0}", fileName);
					sw.WriteLine("Created from thread id: {0}", Thread.CurrentThread.ManagedThreadId);
				}
			}
			else
			{
				throw new Exception(string.Format("duplicated file found {0}", fileName));
			}
		}
	}
}
