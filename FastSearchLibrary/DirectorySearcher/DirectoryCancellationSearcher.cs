#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.IO;
using System.Threading;

namespace FastSearchLibrary
{
	public class DirectoryCancellationSearcher : DirectoryCancellationSearcherBase
	{
		public DirectoryCancellationSearcher(string folder, string pattern, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, suppressOperationCanceledException, token)
		{
			Pattern = pattern;
		}
		public DirectoryCancellationSearcher(string folder, Func<DirectoryInfo, bool> isValid, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, suppressOperationCanceledException, token)
		{
			IsValid = isValid;
		}
	}
}
