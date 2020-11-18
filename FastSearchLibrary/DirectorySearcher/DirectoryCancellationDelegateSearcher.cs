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
				OnDirectoriesFound(directories.Where(dir => isValid(dir)).ToList()); // 'isValid'
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
					OnDirectoriesFound(directories.Where(dir => isValid(dir)).ToList()); // 'isValid'

					return new List<DirectoryInfo>(directories);
				}

				if (directories.Length == 0) return new();
			}
			catch (UnauthorizedAccessException) { return new(); }
			catch (PathTooLongException) { return new(); }
			catch (DirectoryNotFoundException) { return new(); }

			// if directories.Length == 1
			OnDirectoriesFound(directories.Where(dir => isValid(dir)).ToList()); // 'isValid'

			return GetStartDirectories(directories[0].FullName);
		}
	}
}
