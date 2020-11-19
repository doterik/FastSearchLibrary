﻿#pragma warning disable CA1068 // CancellationToken parameters must come last
#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.IO;
using System.Threading;

namespace FastSearchLibrary
{
	internal class FileCancellationSearcher : FileCancellationSearcherBase
	{
		internal FileCancellationSearcher(string folder, string pattern, CancellationToken token, ExecuteHandlers handlerOption, bool allowOperationCanceledException)
			: base(folder, token, handlerOption, allowOperationCanceledException)
		{
			Pattern = pattern;
		}
		internal FileCancellationSearcher(string folder, Func<FileInfo, bool> isValid, CancellationToken token, ExecuteHandlers handlerOption, bool allowOperationCanceledException)
			: base(folder, token, handlerOption, allowOperationCanceledException)
		{
			IsValid = isValid;
		}
	}
}
