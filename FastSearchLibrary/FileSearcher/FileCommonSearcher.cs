#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.IO;

namespace FastSearchLibrary
{
	internal class FileCommonSearcher : FileCommonSearcherBase
	{
		internal FileCommonSearcher(string folder, string pattern, ExecuteHandlers handlerOption) : base(folder, handlerOption)
		{
			Pattern = pattern;
		}

		internal FileCommonSearcher(string folder, Func<FileInfo, bool> isValid, ExecuteHandlers handlerOption) : base(folder, handlerOption)
		{
			IsValid = isValid;
		}
	}
}
