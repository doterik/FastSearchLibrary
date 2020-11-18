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
		private readonly string pattern;

		public FileCancellationPatternSearcher(string folder, string pattern, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, suppressOperationCanceledException, token)
		{
			this.pattern = pattern;
		}

		protected override void GetFiles(string folder)
		{
			Token.ThrowIfCancellationRequested();

			DirectoryInfo dirInfo;
			DirectoryInfo[] directories;
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length == 0)
				{
					OnFilesFound(dirInfo.GetFiles(pattern).ToList()); // 'pattern'
					return;
				}
			}
			catch (UnauthorizedAccessException) { return; }
			catch (PathTooLongException) { return; }
			catch (DirectoryNotFoundException) { return; }

			foreach (var d in directories)
			{
				Token.ThrowIfCancellationRequested();

				GetFiles(d.FullName);
			}

			Token.ThrowIfCancellationRequested();

			try
			{
				OnFilesFound(dirInfo.GetFiles(pattern).ToList()); // 'pattern'
			}
			catch (UnauthorizedAccessException) { }
			catch (PathTooLongException) { }
			catch (DirectoryNotFoundException) { }
		}

		protected override List<DirectoryInfo> GetStartDirectories(string folder)
		{
			Token.ThrowIfCancellationRequested();

			DirectoryInfo[] directories;
			try
			{
				var dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				OnFilesFound(dirInfo.GetFiles(pattern).ToList()); // 'pattern'

				if (directories.Length > 1) return new List<DirectoryInfo>(directories);
				if (directories.Length == 0) return new();
			}
			catch (UnauthorizedAccessException) { return new(); }
			catch (PathTooLongException) { return new(); }
			catch (DirectoryNotFoundException) { return new(); }

			return GetStartDirectories(directories[0].FullName); // directories.Length == 1
		}
	}
}
