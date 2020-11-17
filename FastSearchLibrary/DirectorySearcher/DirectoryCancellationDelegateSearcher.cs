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
		private readonly Func<DirectoryInfo, bool> isValid;

		public DirectoryCancellationDelegateSearcher(string folder, Func<DirectoryInfo, bool> isValid, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, suppressOperationCanceledException, token)
		{
			this.isValid = isValid;
		}

		protected override void GetDirectories(string folder)
		{
			Token.ThrowIfCancellationRequested();

			DirectoryInfo[] directories;
			try
			{
				var dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length == 0) return;
			}
			catch (UnauthorizedAccessException) { return; }
			catch (PathTooLongException) { return; }
			catch (DirectoryNotFoundException) { return; }


			foreach (var dir in directories)
			{
				Token.ThrowIfCancellationRequested();

				GetDirectories(dir.FullName);
			}

			Token.ThrowIfCancellationRequested();

			try
			{
				var resultDirs = (directories.Where(dir => isValid(dir))).ToList();

				if (resultDirs.Count > 0) OnDirectoriesFound(resultDirs);

			}
			catch (UnauthorizedAccessException) { }
			catch (PathTooLongException) { }
			catch (DirectoryNotFoundException) { }
		}

		protected override List<DirectoryInfo> GetStartDirectories(string folder)
		{
			Token.ThrowIfCancellationRequested();

			DirectoryInfo[] directories;
			var resultDirs = new List<DirectoryInfo>();
			try
			{
				var dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length > 1)
				{
					resultDirs.AddRange(directories.Where(dir => isValid(dir)));

					if (resultDirs.Count > 0) OnDirectoriesFound(resultDirs);

					return new List<DirectoryInfo>(directories);
				}

				if (directories.Length == 0) return new List<DirectoryInfo>();

			}
			catch (UnauthorizedAccessException) { return new List<DirectoryInfo>(); }
			catch (PathTooLongException) { return new List<DirectoryInfo>(); }
			catch (DirectoryNotFoundException) { return new List<DirectoryInfo>(); }

			// if directories.Length == 1
			foreach (var dir in directories.Where(dir => isValid(dir)))
			{
				OnDirectoriesFound(new List<DirectoryInfo> { dir });
			}

			return GetStartDirectories(directories[0].FullName);
		}
	}
}
