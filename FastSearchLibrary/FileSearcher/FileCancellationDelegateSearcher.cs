#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FastSearchLibrary
{
	internal class FileCancellationDelegateSearcher : FileCancellationSearcherBase
	{
		private readonly Func<FileInfo, bool> isValid;

		public FileCancellationDelegateSearcher(string folder, Func<FileInfo, bool> isValid, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption, suppressOperationCanceledException, token)
		{
			this.isValid = isValid;
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
					OnFilesFound(dirInfo.GetFiles().Where(file => isValid(file)).ToList()); // 'isValid'
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
				OnFilesFound(dirInfo.GetFiles().Where(file => isValid(file)).ToList()); // 'isValid'
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

				OnFilesFound(dirInfo.GetFiles().Where(file => isValid(file)).ToList()); // 'isValid'

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
