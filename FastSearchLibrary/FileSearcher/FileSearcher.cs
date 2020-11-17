﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{

	/// <summary>
	/// Represents a class for fast file search.
	/// </summary>
	public class FileSearcher
	{
		#region Instance members

		private readonly FileSearcherBase searcher;

		private readonly CancellationTokenSource? tokenSource;

		/// <summary>
		/// Event fires when next portion of files is found. Event handlers are not thread safe. 
		/// </summary>
		public event EventHandler<FileEventArgs> FilesFound
		{
			add
			{ searcher.FilesFound += value; }
			remove
			{ searcher.FilesFound -= value; }
		}

		/// <summary>
		/// Event fires when search process is completed or stopped. 
		/// </summary>
		public event EventHandler<SearchCompletedEventArgs> SearchCompleted
		{
			add
			{ searcher.SearchCompleted += value; }
			remove
			{ searcher.SearchCompleted -= value; }
		}

		#region FilePatternSearcher constructors 

		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder, string pattern, ExecuteHandlers handlerOption)
		{
			CheckFolder(folder);

			CheckPattern(pattern);

			searcher = new FilePatternSearcher(folder, pattern, handlerOption);
		}

		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder, string pattern) : this(folder, pattern, ExecuteHandlers.InCurrentTask) { }


		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder) : this(folder, "*", ExecuteHandlers.InCurrentTask) { }

		#endregion

		#region FileDelegateSearcher constructors

		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of file selection.</param>
		/// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder, Func<FileInfo, bool> isValid, ExecuteHandlers handlerOption)
		{
			CheckFolder(folder);

			CheckDelegate(isValid);

			searcher = new FileDelegateSearcher(folder, isValid, handlerOption);
		}

		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of file selection.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder, Func<FileInfo, bool> isValid) : this(folder, isValid, ExecuteHandlers.InCurrentTask) { }

		#endregion

		#region FileCancellationPatternSearcher constructors

		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
		/// <param name="suppressOperationCanceledException">Determines whether necessary suppress OperationCanceledException if it possible.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder, string pattern, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption, bool suppressOperationCanceledException)
		{
			CheckFolder(folder);

			CheckPattern(pattern);

			CheckTokenSource(tokenSource);

			searcher = new FileCancellationPatternSearcher(folder, pattern, handlerOption, suppressOperationCanceledException, tokenSource.Token);
			this.tokenSource = tokenSource;
		}

		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder, string pattern, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption)
			: this(folder, pattern, tokenSource, handlerOption, true) { }

		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder, string pattern, CancellationTokenSource tokenSource)
			: this(folder, pattern, tokenSource, ExecuteHandlers.InCurrentTask, true) { }

		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder, CancellationTokenSource tokenSource)
			: this(folder, "*", tokenSource, ExecuteHandlers.InCurrentTask, true) { }

		#endregion

		#region FileCancellationDelegateSearcher constructors

		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of file selection.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
		/// <param name="suppressOperationCanceledException">Determines whether necessary suppress OperationCanceledException if it possible.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder, Func<FileInfo, bool> isValid, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption, bool suppressOperationCanceledException)
		{
			CheckFolder(folder);

			CheckDelegate(isValid);

			CheckTokenSource(tokenSource);

			searcher = new FileCancellationDelegateSearcher(folder, isValid, handlerOption, suppressOperationCanceledException, tokenSource.Token);
			this.tokenSource = tokenSource;
		}

		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of file selection.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder, Func<FileInfo, bool> isValid, CancellationTokenSource tokenSource, ExecuteHandlers handlerOption)
			: this(folder, isValid, tokenSource, handlerOption, true) { }

		/// <summary>
		/// Initializes a new instance of FileSearcher class.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of file selection.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcher(string folder, Func<FileInfo, bool> isValid, CancellationTokenSource tokenSource)
			: this(folder, isValid, tokenSource, ExecuteHandlers.InCurrentTask, true) { }

		#endregion

		#region Checking methods
		private static void CheckFolder(string folder)
		{
			if (folder == null) throw new ArgumentNullException(nameof(folder), "Argument is null.");

			if (folder == string.Empty) throw new ArgumentException("Argument is not valid.", nameof(folder));

			var dir = new DirectoryInfo(folder);

			if (!dir.Exists) throw new ArgumentException("Argument does not represent an existing directory.", nameof(folder));
		}

		private static void CheckPattern(string pattern)
		{
			if (pattern == null) throw new ArgumentNullException(nameof(pattern), "Argument is null.");

			if (pattern == string.Empty) throw new ArgumentException("Argument is not valid.", nameof(pattern));
		}

		private static void CheckDelegate(Func<FileInfo, bool> isValid)
		{
			if (isValid == null) throw new ArgumentNullException(nameof(isValid), "Argument is null.");
		}

		private static void CheckTokenSource(CancellationTokenSource tokenSource)
		{
			if (tokenSource == null) throw new ArgumentNullException(nameof(tokenSource), "Argument is null.");
		}

		#endregion

		/// <summary>
		/// Starts a file search operation with realtime reporting using several threads in thread pool.
		/// </summary>
		public void StartSearch() => searcher.StartSearch();

		/// <summary>
		/// Starts a file search operation with realtime reporting using several threads in thread pool as an asynchronous operation.
		/// </summary>
		public Task StartSearchAsync()
		{
			if (searcher is FileCancellationSearcherBase)
			{
				return Task.Run(() =>
				{
					StartSearch();

				}, tokenSource?.Token ?? default); // An empty cancellation token. (CancellationToken.None)
			}

			return Task.Run(() => StartSearch());
		}

		/// <summary>
		/// Stops a file search operation.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public void StopSearch()
		{
			if (tokenSource == null) throw new InvalidOperationException("Impossible to stop operation without instance of CancellationTokenSource.");

			tokenSource.Cancel();
		}

		#endregion

		#region Static members

		#region Public members

		/// <summary>
		/// Returns a list of files that are contained in directory and all subdirectories.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <returns>List of finding files</returns>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static List<FileInfo> GetFiles(string folder, string pattern = "*")
		{
			DirectoryInfo dirInfo;
			DirectoryInfo[] directories;
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length == 0) return new List<FileInfo>(dirInfo.GetFiles(pattern));
			}
			catch (UnauthorizedAccessException) { return new List<FileInfo>(); }
			catch (DirectoryNotFoundException) { return new List<FileInfo>(); }

			var result = new List<FileInfo>();

			foreach (var d in directories)
			{
				result.AddRange(GetFiles(d.FullName, pattern));
			}

			try
			{
				result.AddRange(dirInfo.GetFiles(pattern));
			}
			catch (UnauthorizedAccessException) { }
			catch (PathTooLongException) { }
			catch (DirectoryNotFoundException) { }

			return result;
		}

		/// <summary>
		/// Returns a list of files that are contained in directory and all subdirectories.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of file selection.</param>
		/// <returns>List of finding files.</returns>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static List<FileInfo> GetFiles(string folder, Func<FileInfo, bool> isValid)
		{
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
					return resultFiles;
				}
			}
			catch (UnauthorizedAccessException) { return new List<FileInfo>(); }
			catch (PathTooLongException) { return new List<FileInfo>(); }
			catch (DirectoryNotFoundException) { return new List<FileInfo>(); }

			foreach (var d in directories)
			{
				resultFiles.AddRange(GetFiles(d.FullName, isValid));
			}

			try
			{
				resultFiles.AddRange(dirInfo.GetFiles().Where(file => isValid(file)));
			}
			catch (UnauthorizedAccessException) { }
			catch (PathTooLongException) { }
			catch (DirectoryNotFoundException) { }

			return resultFiles;
		}

		/// <summary>
		/// Returns a list of files that are contained in directory and all subdirectories as an asynchronous operation.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static async Task<List<FileInfo>> GetFilesAsync(string folder, string pattern = "*") =>
			await Task.Run(() => GetFiles(folder, pattern));

		/// <summary>
		/// Returns a list of files that are contained in directory and all subdirectories as an asynchronous operation.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of file selection.</param>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static async Task<List<FileInfo>> GetFilesAsync(string folder, Func<FileInfo, bool> isValid) =>
			await Task.Run(() => GetFiles(folder, isValid));

		/// <summary>
		/// Returns a list of files that are contained in directory and all subdirectories using several threads of thread pool.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <returns>List of finding files.</returns>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static List<FileInfo> GetFilesFast(string folder, string pattern = "*")
		{
			var files = new ConcurrentBag<FileInfo>();

			GetStartDirectories(folder, files, pattern).AsParallel().ForAll((d1) =>
			{
				GetStartDirectories(d1.FullName, files, pattern).AsParallel().ForAll((d2) =>
				{
					GetFiles(d2.FullName, pattern).ForEach((f) => files.Add(f));
				});
			});

			return files.ToList();
		}

		/// <summary>
		/// Returns a list of files that are contained in directory and all subdirectories using several threads of thread pool.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of file selection.</param>
		/// <returns>List of finding files.</returns>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static List<FileInfo> GetFilesFast(string folder, Func<FileInfo, bool> isValid)
		{
			var files = new ConcurrentBag<FileInfo>();

			GetStartDirectories(folder, files, isValid).AsParallel().ForAll((d1) =>
			{
				GetStartDirectories(d1.FullName, files, isValid).AsParallel().ForAll((d2) =>
				{
					GetFiles(d2.FullName, isValid).ForEach((f) => files.Add(f));
				});
			});

			return files.ToList();
		}

		/// <summary>
		/// Returns a list of files that are contained in directory and all subdirectories using several threads of thread pool as an asynchronous operation.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static async Task<List<FileInfo>> GetFilesFastAsync(string folder, string pattern = "*") =>
			await Task.Run(() => GetFilesFast(folder, pattern));

		/// <summary>
		/// Returns a list of files that are contained in directory and all subdirectories using several threads of thread pool as an asynchronous operation.
		/// </summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of file selection.</param>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static async Task<List<FileInfo>> GetFilesFastAsync(string folder, Func<FileInfo, bool> isValid) =>
			await Task.Run(() => GetFilesFast(folder, isValid));

		#endregion

		#region Private members
		private static List<DirectoryInfo> GetStartDirectories(string folder, ConcurrentBag<FileInfo> files, string pattern)
		{
			DirectoryInfo[] directories;
			try
			{
				var dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				foreach (var f in dirInfo.GetFiles(pattern))
				{
					files.Add(f);
				}

				if (directories.Length > 1) return new List<DirectoryInfo>(directories);
				if (directories.Length == 0) return new List<DirectoryInfo>();

			}
			catch (UnauthorizedAccessException) { return new List<DirectoryInfo>(); }
			catch (PathTooLongException) { return new List<DirectoryInfo>(); }
			catch (DirectoryNotFoundException) { return new List<DirectoryInfo>(); }

			return GetStartDirectories(directories[0].FullName, files, pattern);
		}

		private static List<DirectoryInfo> GetStartDirectories(string folder, ConcurrentBag<FileInfo> resultFiles, Func<FileInfo, bool> isValid)
		{
			DirectoryInfo[] directories;
			try
			{
				var dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				foreach (var file in dirInfo.GetFiles().Where(file => isValid(file)))
				{
					resultFiles.Add(file);
				}

				if (directories.Length > 1) return new List<DirectoryInfo>(directories);
				if (directories.Length == 0) return new List<DirectoryInfo>();
			}
			catch (UnauthorizedAccessException) { return new List<DirectoryInfo>(); }
			catch (PathTooLongException) { return new List<DirectoryInfo>(); }
			catch (DirectoryNotFoundException) { return new List<DirectoryInfo>(); }

			return GetStartDirectories(directories[0].FullName, resultFiles, isValid);
		}

		#endregion

		#endregion
	}
}
