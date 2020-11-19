#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FastSearchLibrary
{
	internal class FileCancellationDelegateSearcher : FileCancellationSearcherBase
	{
		//private readonly Func<FileInfo, bool> isValid;

		public FileCancellationDelegateSearcher(string folder, Func<FileInfo, bool> isValid, ExecuteHandlers handlerOption, bool allowOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, allowOperationCanceledException, token)
		{
			IsValid = isValid;
		}

		//protected override void GetFiles(string folder)
		//protected override List<DirectoryInfo> GetStartDirectories(string folder)
	}
}
