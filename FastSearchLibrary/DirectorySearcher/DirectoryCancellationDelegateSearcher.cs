#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FastSearchLibrary
{
	internal class DirectoryCancellationDelegateSearcher : DirectoryCancellationSearcherBase
	{
		//private readonly Func<DirectoryInfo, bool> isValid;

		public DirectoryCancellationDelegateSearcher(string folder, Func<DirectoryInfo, bool> isValid, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, suppressOperationCanceledException, token)
		{
			IsValid = isValid;
		}

		//protected override void GetDirectories(string folder)
		//protected override List<DirectoryInfo> GetStartDirectories(string folder)
	}
}
