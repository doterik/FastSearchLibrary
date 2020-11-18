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
					GetFilesFrom(dirInfo);

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
				GetFilesFrom(dirInfo);
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

				GetFilesFrom(dirInfo);

				if (directories.Length > 1) return new List<DirectoryInfo>(directories);
				if (directories.Length == 0) return new List<DirectoryInfo>();
			}
			catch (UnauthorizedAccessException) { return new List<DirectoryInfo>(); }
			catch (PathTooLongException) { return new List<DirectoryInfo>(); }
			catch (DirectoryNotFoundException) { return new List<DirectoryInfo>(); }

			return GetStartDirectories(directories[0].FullName); // directories.Length == 1
		}

		private void GetFilesFrom(DirectoryInfo dirInfo)
		{
			var resFiles = dirInfo.GetFiles(pattern);
			if (resFiles.Length > 0) OnFilesFound(resFiles.ToList());
		}
	}
}
