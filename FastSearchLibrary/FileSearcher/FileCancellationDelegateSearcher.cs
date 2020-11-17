#pragma warning disable IDE0007 // Use implicit type
#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.Collections.Generic;
using System.IO;
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
			token.ThrowIfCancellationRequested();

			DirectoryInfo dirInfo;
			DirectoryInfo[] directories;
			var resultFiles = new List<FileInfo>();
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length == 0)
				{
					FileInfo[] files = dirInfo.GetFiles();

					foreach (var file in files)
						if (isValid(file))
							resultFiles.Add(file);

					if (resultFiles.Count > 0)
						OnFilesFound(resultFiles);

					return;
				}
			}
			catch (UnauthorizedAccessException)
			{
				return;
			}
			catch (PathTooLongException)
			{
				return;
			}
			catch (DirectoryNotFoundException)
			{
				return;
			}


			foreach (var d in directories)
			{
				token.ThrowIfCancellationRequested();

				GetFiles(d.FullName);
			}

			token.ThrowIfCancellationRequested();

			try
			{
				var files = dirInfo.GetFiles();

				foreach (var file in files)
					if (isValid(file))
						resultFiles.Add(file);

				if (resultFiles.Count > 0)
					OnFilesFound(resultFiles);
			}
			catch (UnauthorizedAccessException)
			{
			}
			catch (PathTooLongException)
			{
			}
			catch (DirectoryNotFoundException)
			{
			}

			return;
		}



		protected override List<DirectoryInfo> GetStartDirectories(string folder)
		{
			token.ThrowIfCancellationRequested();

			DirectoryInfo[] directories;
			var resultFiles = new List<FileInfo>();
			try
			{
				var dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				FileInfo[] files = dirInfo.GetFiles();

				foreach (var file in files)
					if (isValid(file))
						resultFiles.Add(file);

				if (resultFiles.Count > 0)
					OnFilesFound(resultFiles);

				if (directories.Length > 1)
					return new List<DirectoryInfo>(directories);

				if (directories.Length == 0)
					return new List<DirectoryInfo>();
			}
			catch (UnauthorizedAccessException)
			{
				return new List<DirectoryInfo>();
			}
			catch (PathTooLongException)
			{
				return new List<DirectoryInfo>();
			}
			catch (DirectoryNotFoundException)
			{
				return new List<DirectoryInfo>();
			}

			return GetStartDirectories(directories[0].FullName);
		}


	}
}
