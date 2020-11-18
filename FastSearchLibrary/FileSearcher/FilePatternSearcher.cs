#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FastSearchLibrary
{
	internal class FilePatternSearcher : FileSearcherBase
	{
		//private readonly string pattern;

		public FilePatternSearcher(string folder) : this(folder, "*") { }
		public FilePatternSearcher(string folder, string pattern) : this(folder, pattern, ExecuteHandlers.InCurrentTask) { }
		public FilePatternSearcher(string folder, string pattern, ExecuteHandlers handlerOption) : base(folder, handlerOption)
		{
			Pattern = pattern;
		}

		//public override void StartSearch() => GetFilesFast();
		//protected override void GetFiles(string folder)
		//protected override List<DirectoryInfo> GetStartDirectories(string folder)
	}
}
