#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;

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
			var resultFiles = new List<FileInfo>();
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length == 0)
				{
					resultFiles.AddRange(dirInfo.GetFiles().Where(file => isValid(file)));

					if (resultFiles.Count > 0) OnFilesFound(resultFiles);

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
				var files = dirInfo.GetFiles();
				resultFiles.AddRange(files.Where(file => isValid(file)));

				if (resultFiles.Count > 0) OnFilesFound(resultFiles);
				// if (resultFiles.Count > 0) OnFilesFound(files.Where(file => isValid(file)).ToList()); // TODO
			}
			catch (UnauthorizedAccessException) { }
			catch (PathTooLongException) { }
			catch (DirectoryNotFoundException) { }

			return;
		}

		protected override List<DirectoryInfo> GetStartDirectories(string folder)
		{
			Token.ThrowIfCancellationRequested();

			DirectoryInfo[] directories;
			var resultFiles = new List<FileInfo>();
			try
			{
				var dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				resultFiles.AddRange(dirInfo.GetFiles().Where(file => isValid(file)));

				if (resultFiles.Count > 0) OnFilesFound(resultFiles);

				if (directories.Length > 1) return new List<DirectoryInfo>(directories);
				if (directories.Length == 0) return new List<DirectoryInfo>();
			}
			catch (UnauthorizedAccessException) { return new List<DirectoryInfo>(); }
			catch (PathTooLongException) { return new List<DirectoryInfo>(); }
			catch (DirectoryNotFoundException) { return new List<DirectoryInfo>(); }

			return GetStartDirectories(directories[0].FullName);
		}
	}
}
