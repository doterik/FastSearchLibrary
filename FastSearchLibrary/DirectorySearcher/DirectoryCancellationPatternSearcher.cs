#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FastSearchLibrary
{
	internal class DirectoryCancellationPatternSearcher : DirectoryCancellationSearcherBase
	{
		//private readonly string pattern;

		public DirectoryCancellationPatternSearcher(string folder, string pattern, ExecuteHandlers handlerOption, bool allowOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, allowOperationCanceledException, token)
		{
			Pattern = pattern;
		}

		//protected override void GetDirectories(string folder)
		//protected override List<DirectoryInfo> GetStartDirectories(string folder)
	}
}
