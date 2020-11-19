#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.IO;
using System.Threading;

namespace FastSearchLibrary
{
	internal class DirectoryCancellationSearcher : DirectoryCancellationSearcherBase
	{
		public DirectoryCancellationSearcher(string folder, string pattern, ExecuteHandlers handlerOption, bool allowOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, allowOperationCanceledException, token)
		{
			Pattern = pattern;
		}
		public DirectoryCancellationSearcher(string folder, Func<DirectoryInfo, bool> isValid, ExecuteHandlers handlerOption, bool allowOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, allowOperationCanceledException, token)
		{
			IsValid = isValid;
		}
	}
}
