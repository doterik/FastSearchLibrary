#pragma warning disable IDE0021 // Use expression body for constructors

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FastSearchLibrary
{
	internal class FilePatternSearcher : FileSearcherBase
	{
		private readonly string pattern;

		public FilePatternSearcher(string folder, string pattern, ExecuteHandlers handlerOption) : base(folder, handlerOption)
		{
			this.pattern = pattern;
		}

		public FilePatternSearcher(string folder, string pattern) : this(folder, pattern, ExecuteHandlers.InCurrentTask) { }

		public FilePatternSearcher(string folder) : this(folder, "*", ExecuteHandlers.InCurrentTask) { }

		/// <summary>
		/// Starts a file search operation with realtime reporting using several threads in thread pool.
		/// </summary>
		public override void StartSearch() => GetFilesFast();

		protected override void GetFiles(string folder)
		{
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
				GetFiles(d.FullName);
			}

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

			return GetStartDirectories(directories[0].FullName);
		}
	}
}
