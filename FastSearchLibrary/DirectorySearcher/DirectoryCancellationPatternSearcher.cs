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
				Token.ThrowIfCancellationRequested();

				GetDirectories(d.FullName);
			}


			Token.ThrowIfCancellationRequested();

			try
			{
				var resultDirs = dirInfo.GetDirectories(pattern);
				if (resultDirs.Length > 0)
					OnDirectoriesFound(resultDirs.ToList());
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
		}

		protected override List<DirectoryInfo> GetStartDirectories(string folder)
		{
			Token.ThrowIfCancellationRequested();

			DirectoryInfo dirInfo;
			DirectoryInfo[] directories;
			DirectoryInfo[] resultDirs;
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();


				if (directories.Length > 1)
				{
					resultDirs = dirInfo.GetDirectories(pattern);
					if (resultDirs.Length > 0)
						OnDirectoriesFound(resultDirs.ToList());

					return new List<DirectoryInfo>(directories);
				}

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

			// if directories.Length == 1
			resultDirs = dirInfo.GetDirectories(pattern);
			if (resultDirs.Length > 0)
				OnDirectoriesFound(resultDirs.ToList());

			return GetStartDirectories(directories[0].FullName);
		}
	}
}
