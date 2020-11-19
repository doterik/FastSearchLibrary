using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
	/// <summary>Represents a class for fast directory search.</summary>
	public class DirectorySearcher : FileBase
	{
		#region Instance members

		private readonly DirectoryCancellationSearcherBase searcher;
		private readonly CancellationTokenSource tokenSource;

		/// <summary>Event fires when next portion of directories is found. Event handlers are not thread safe.</summary>

		public event EventHandler<DirectoryEventArgs> DirectoriesFound
		{
			add { searcher.DirectoriesFound += value; }
			remove { searcher.DirectoriesFound -= value; }
		}

		/// <summary>Event fires when search process is completed or stopped.</summary>

		public event EventHandler<SearchCompletedEventArgs> SearchCompleted
		{
			add { searcher.SearchCompleted += value; }
			remove { searcher.SearchCompleted -= value; }
		}

		#region DirectoryCancellationPatternSearcher constructors

		/// <summary>Initializes a new instance of the <see cref="DirectorySearcher"/> class.</summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="tokenSource">Instance of <see cref="CancellationTokenSource"/> for search process cancellation possibility.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public DirectorySearcher(string folder, CancellationTokenSource tokenSource) : this(folder, "*", tokenSource) { }

		/// <summary>Initializes a new instance of the <see cref="DirectorySearcher"/> class.</summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <param name="tokenSource">Instance of <see cref="CancellationTokenSource"/> for search process cancellation possibility.</param>
		/// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
		/// <param name="allowOperationCanceledException">if set to <c>true</c> [allow operation canceled exception].</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public DirectorySearcher(
			string folder,
			string pattern,
			CancellationTokenSource tokenSource,
			ExecuteHandlers handlerOption = ExecuteHandlers.InCurrentTask,
			bool allowOperationCanceledException = false)
		{
			CheckFolder(folder);
			CheckPattern(pattern);
			CheckTokenSource(tokenSource);

			searcher = new DirectoryCancellationSearcher(folder, pattern, tokenSource.Token, handlerOption, allowOperationCanceledException);
			this.tokenSource = tokenSource;
		}

		#endregion

		#region DirectoryCancellationDelegateSearcher constructor

		/// <summary>Initializes a new instance of the <see cref="DirectorySearcher"/> class.</summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
		/// <param name="tokenSource">Instance of <see cref="CancellationTokenSource"/> for search process cancellation possibility.</param>
		/// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
		/// <param name="allowOperationCanceledException">if set to <c>true</c> [allow operation canceled exception].</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		public DirectorySearcher(
			string folder,
			Func<DirectoryInfo, bool> isValid,
			CancellationTokenSource tokenSource,
			ExecuteHandlers handlerOption = ExecuteHandlers.InCurrentTask,
			bool allowOperationCanceledException = false)
		{
			CheckFolder(folder);
			CheckDelegate(isValid);
			CheckTokenSource(tokenSource);

			searcher = new DirectoryCancellationSearcher(folder, isValid, tokenSource.Token, handlerOption, allowOperationCanceledException);
			this.tokenSource = tokenSource;
		}

		#endregion

		/// <summary>Starts a directory search operation with realtime reporting using several threads in thread pool.</summary>
		public void StartSearch() => searcher.StartSearch();

		/// <summary>Starts a directory search operation with realtime reporting using several threads in thread pool as an asynchronous operation.</summary>
		public Task StartSearchAsync() => Task.Run(() => StartSearch(), tokenSource.Token);

		/// <summary>Stops a directory search operation.</summary>
		public void StopSearch() => tokenSource.Cancel();

		#endregion

		#region Public members

		/// <summary>Returns a list of directories that are contained in directory and all subdirectories.</summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <returns>List of finding directories.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static List<DirectoryInfo> GetDirectories(string folder, string pattern = "*")
		{
			var directories = new List<DirectoryInfo>();
			GetDirectories(folder, directories, pattern);

			return directories;
		}

		/// <summary>Returns a list of directories that are contained in directory and all subdirectories.</summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
		/// <returns>List of finding directories.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static List<DirectoryInfo> GetDirectories(string folder, Func<DirectoryInfo, bool> isValid)
		{
			var directories = new List<DirectoryInfo>();
			GetDirectories(folder, directories, isValid);

			return directories;
		}

		/// <summary>Returns a list of directories that are contained in directory and all subdirectories as an asynchronous operation.</summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static Task<List<DirectoryInfo>> GetDirectoriesAsync(string folder, string pattern = "*") => Task.Run(() => GetDirectories(folder, pattern));

		/// <summary>Returns a list of directories that are contained in directory and all subdirectories as an asynchronous operation.</summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static Task<List<DirectoryInfo>> GetDirectoriesAsync(string folder, Func<DirectoryInfo, bool> isValid) => Task.Run(() => GetDirectories(folder, isValid));

		/// <summary>Returns a list of directories that are contained in directory and all subdirectories using several threads in thread pool.</summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <returns>List of finding directories.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static List<DirectoryInfo> GetDirectoriesFast(string folder, string pattern = "*")
		{
			var dirs = new ConcurrentBag<DirectoryInfo>();

			GetStartDirectories(folder, dirs, pattern).AsParallel().ForAll((d1) =>
			{
				GetStartDirectories(d1.FullName, dirs, pattern).AsParallel().ForAll((d2) =>
				{
					GetDirectories(d2.FullName, pattern).ForEach((d) => dirs.Add(d));
				});
			});

			return dirs.ToList();
		}

		/// <summary>Returns a list of directories that are contained in directory and all subdirectories using several threads in thread pool.</summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
		/// <returns>List of finding directories.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static List<DirectoryInfo> GetDirectoriesFast(string folder, Func<DirectoryInfo, bool> isValid)
		{
			var dirs = new ConcurrentBag<DirectoryInfo>();

			GetStartDirectories(folder, dirs, isValid).AsParallel().ForAll((d1) =>
			{
				GetStartDirectories(d1.FullName, dirs, isValid).AsParallel().ForAll((d2) =>
				{
					GetDirectories(d2.FullName, isValid).ForEach((d) => dirs.Add(d));
				});
			});

			return dirs.ToList();
		}

		/// <summary>Returns a list of directories that are contained in directory and all subdirectories using several threads in thread pool as an asynchronous operation.</summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static Task<List<DirectoryInfo>> GetDirectoriesFastAsync(string folder, string pattern = "*") => Task.Run(() => GetDirectoriesFast(folder, pattern));

		/// <summary>Returns a list of directories that are contained in directory and all subdirectories using several threads in thread pool as an asynchronous operation.</summary>
		/// <param name="folder">The start search directory.</param>
		/// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="NullReferenceException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public static Task<List<DirectoryInfo>> GetDirectoriesFastAsync(string folder, Func<DirectoryInfo, bool> isValid) => Task.Run(() => GetDirectoriesFast(folder, isValid));

		#endregion

		#region Private members
		private static void GetDirectories(string folder, List<DirectoryInfo> result, string pattern)
		{
			DirectoryInfo? dirInfo = null;
			DirectoryInfo[]? directories = null;
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length == 0) return;
			}
			catch (UnauthorizedAccessException) { return; }
			catch (PathTooLongException) { return; }
			catch (DirectoryNotFoundException) { return; }

			Array.ForEach(directories, (d) => GetDirectories(d.FullName, result, pattern));

			try
			{
				Array.ForEach(dirInfo.GetDirectories(pattern), (d) => result.Add(d));
			}
			catch (UnauthorizedAccessException) { }
			catch (DirectoryNotFoundException) { }
		}

		private static void GetDirectories(string folder, List<DirectoryInfo> result, Func<DirectoryInfo, bool> isValid)
		{
			DirectoryInfo? dirInfo = null;
			DirectoryInfo[]? directories = null;
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length == 0) return;
			}
			catch (UnauthorizedAccessException) { return; }
			catch (PathTooLongException) { return; }
			catch (DirectoryNotFoundException) { return; }

			Array.ForEach(directories, (d) => GetDirectories(d.FullName, result, isValid));

			try
			{
				Array.ForEach(dirInfo.GetDirectories(), (d) => { if (isValid(d)) result.Add(d); });
			}
			catch (UnauthorizedAccessException) { }
			catch (PathTooLongException) { }
			catch (DirectoryNotFoundException) { }
		}

		private static List<DirectoryInfo> GetStartDirectories(string folder, ConcurrentBag<DirectoryInfo> dirs, string pattern)
		{
			DirectoryInfo? dirInfo = null;
			DirectoryInfo[]? directories = null;
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length > 1)
				{
					Array.ForEach(dirInfo.GetDirectories(pattern), (d) => dirs.Add(d));
					return new List<DirectoryInfo>(directories);
				}

				if (directories.Length == 0) return new List<DirectoryInfo>();

			}
			catch (UnauthorizedAccessException) { return new List<DirectoryInfo>(); }
			catch (PathTooLongException) { return new List<DirectoryInfo>(); }
			catch (DirectoryNotFoundException) { return new List<DirectoryInfo>(); }

			// if directories.Length == 1
			Array.ForEach(dirInfo.GetDirectories(pattern), (d) => dirs.Add(d));

			return GetStartDirectories(directories[0].FullName, dirs, pattern);
		}

		private static List<DirectoryInfo> GetStartDirectories(string folder, ConcurrentBag<DirectoryInfo> dirs, Func<DirectoryInfo, bool> isValid)
		{
			DirectoryInfo? dirInfo = null;
			DirectoryInfo[]? directories = null;
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length > 1)
				{
					Array.ForEach(directories, (d) => { if (isValid(d)) dirs.Add(d); });

					return new List<DirectoryInfo>(directories);
				}

				if (directories.Length == 0) return new List<DirectoryInfo>();

			}
			catch (UnauthorizedAccessException) { return new List<DirectoryInfo>(); }
			catch (PathTooLongException) { return new List<DirectoryInfo>(); }
			catch (DirectoryNotFoundException) { return new List<DirectoryInfo>(); }

			// if directories.Length == 1
			Array.ForEach(directories, (d) => { if (isValid(d)) dirs.Add(d); });

			return GetStartDirectories(directories[0].FullName, dirs, isValid);
		}

		#endregion
	}
}
