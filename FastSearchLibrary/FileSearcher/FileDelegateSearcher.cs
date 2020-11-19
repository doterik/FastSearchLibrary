#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FastSearchLibrary
{
	internal class FileDelegateSearcher : FileSearcherBase
	{
		//private readonly Func<FileInfo, bool> isValid;

		//public FileDelegateSearcher(string folder) : this(folder, (arg) => true) { }
		//public FileDelegateSearcher(string folder, Func<FileInfo, bool> isValid) : this(folder, isValid, ExecuteHandlers.InCurrentTask) { }
		internal FileDelegateSearcher(string folder, Func<FileInfo, bool> isValid, ExecuteHandlers handlerOption) : base(folder, handlerOption)
		{
			IsValid = isValid;
		}

		//public override void StartSearch() => GetFilesFast();
		//protected override void GetFiles(string folder)
		//protected override List<DirectoryInfo> GetStartDirectories(string folder)
	}
}
