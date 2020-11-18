#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FastSearchLibrary
{
	internal class FileCancellationPatternSearcher : FileCancellationSearcherBase
	{
		//private readonly string pattern;

		public FileCancellationPatternSearcher(string folder, string pattern, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, suppressOperationCanceledException, token)
		{
			Pattern = pattern;
		}

		//protected override void GetFiles(string folder)
		//protected override List<DirectoryInfo> GetStartDirectories(string folder)
	}
}
