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
		private readonly string pattern;

		public DirectoryCancellationPatternSearcher(string folder, string pattern, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, suppressOperationCanceledException, token)
		{
			this.pattern = pattern;
		}

		protected override void GetDirectories(string folder)
		{
			Token.ThrowIfCancellationRequested();

			DirectoryInfo dirInfo;
			DirectoryInfo[] directories;
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length == 0) return;
			}
			catch (UnauthorizedAccessException) { return; }
			catch (PathTooLongException) { return; }
			catch (DirectoryNotFoundException) { return; }

			foreach (var d in directories)
			{
				Token.ThrowIfCancellationRequested();

				GetDirectories(d.FullName);
			}

			Token.ThrowIfCancellationRequested();

			try
			{
				OnDirectoriesFound(dirInfo.GetDirectories(pattern).ToList()); // 'pattern'
			}
			catch (UnauthorizedAccessException) { }
			catch (PathTooLongException) { }
			catch (DirectoryNotFoundException) { }
		}

		protected override List<DirectoryInfo> GetStartDirectories(string folder)
		{
			Token.ThrowIfCancellationRequested();

			DirectoryInfo dirInfo;
			DirectoryInfo[] directories;
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length > 1)
				{
					OnDirectoriesFound(dirInfo.GetDirectories(pattern).ToList()); // 'pattern'

					return new List<DirectoryInfo>(directories);
				}

				if (directories.Length == 0) return new();
			}
			catch (UnauthorizedAccessException) { return new(); }
			catch (PathTooLongException) { return new(); }
			catch (DirectoryNotFoundException) { return new(); }

			// if directories.Length == 1
			OnDirectoriesFound(dirInfo.GetDirectories(pattern).ToList()); // 'pattern'

			return GetStartDirectories(directories[0].FullName);
		}
	}
}
