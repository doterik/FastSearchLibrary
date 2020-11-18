#pragma warning disable IDE0021 // Use expression body for constructors
#pragma warning disable IDE0022 // Use expression body for methods
#pragma warning disable IDE0052 // Remove unread private members

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
	/// <summary>
	/// Represents a class for fast directory search in multiple directories.
	/// </summary>
	public class DirectorySearcherMultiple : FileBase
	{
		#region Instance members

		private readonly List<DirectoryCancellationSearcherBase> searchers;

		private readonly CancellationTokenSource tokenSource;

		private readonly bool suppressOperationCanceledException;
		private readonly ExecuteHandlers handlerOption;

		/// <summary>
		/// Event fires when next portion of directories is found. Event handlers are not thread safe. 
		/// </summary>
		public event EventHandler<DirectoryEventArgs> DirectoriesFound
		{
			add { searchers.ForEach((s) => s.DirectoriesFound += value); }
			remove { searchers.ForEach((s) => s.DirectoriesFound -= value); }
		}

		/// <summary>
		/// Event fires when search process is completed or stopped.
		/// </summary>
		public event EventHandler<SearchCompletedEventArgs>? SearchCompleted;

		/// <summary>
		/// Calls a SearchCompleted event.
		/// </summary>
		/// <param name="isCanceled">Determines whether search process canceled.</param>
		protected virtual void OnSearchCompleted(bool isCanceled)
		{
			SearchCompleted?.Invoke(this, new SearchCompletedEventArgs(isCanceled));
		}

		#region DirectoryCancellationPatternSearcher constructors

		/// <summary>
		/// Initialize a new instance of DirectorySearchMultiple class. 
		/// </summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public DirectorySearcherMultiple(List<string> folders, CancellationTokenSource tokenSource)
			: this(folders, "*", ExecuteHandlers.InCurrentTask, true, tokenSource) { }

		/// <summary>
		/// Initialize a new instance of DirectorySearchMultiple class. 
		/// </summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public DirectorySearcherMultiple(List<string> folders, string pattern, CancellationTokenSource tokenSource)
			: this(folders, pattern, ExecuteHandlers.InCurrentTask, true, tokenSource) { }

		/// <summary>
		/// Initialize a new instance of DirectorySearchMultiple class. 
		/// </summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public DirectorySearcherMultiple(List<string> folders, string pattern, ExecuteHandlers handlerOption, CancellationTokenSource tokenSource)
			: this(folders, pattern, handlerOption, true, tokenSource) { }

		/// <summary>
		/// Initialize a new instance of DirectorySearchMultiple class. 
		/// </summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
		/// <param name="suppressOperationCanceledException">Determines whether necessary suppress OperationCanceledException if it possible.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public DirectorySearcherMultiple(List<string> folders, string pattern, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationTokenSource tokenSource)
		{
			CheckFolders(folders);
			CheckPattern(pattern);
			CheckTokenSource(tokenSource);

			searchers = new List<DirectoryCancellationSearcherBase>();
			this.suppressOperationCanceledException = suppressOperationCanceledException;

			foreach (var folder in folders)
			{
				searchers.Add(new DirectoryCancellationPatternSearcher(folder, pattern, handlerOption, false, tokenSource.Token));
			}

			this.tokenSource = tokenSource;
		}

		#endregion

		#region DirectoryCancellationDelegateSearcher constructors

		/// <summary>
		/// Initialize a new instance of DirectorySearcherMultiple class.
		/// </summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public DirectorySearcherMultiple(List<string> folders, Func<DirectoryInfo, bool> isValid, CancellationTokenSource tokenSource)
			: this(folders, isValid, ExecuteHandlers.InCurrentTask, true, tokenSource) { }

		/// <summary>
		/// Initialize a new instance of DirectorySearcherMultiple class.
		/// </summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
		/// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public DirectorySearcherMultiple(List<string> folders, Func<DirectoryInfo, bool> isValid, ExecuteHandlers handlerOption, CancellationTokenSource tokenSource)
			: this(folders, isValid, ExecuteHandlers.InCurrentTask, true, tokenSource)
		{
			this.handlerOption = handlerOption;
		}

		/// <summary>
		/// Initialize a new instance of DirectorySearcherMultiple class.
		/// </summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
		/// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
		/// <param name="suppressOperationCanceledException">Determines whether necessary suppress OperationCanceledException if it possible.</param>
		/// <param name="tokenSource">Instance of CancellationTokenSource for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public DirectorySearcherMultiple(List<string> folders, Func<DirectoryInfo, bool> isValid, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationTokenSource tokenSource)
		{
			CheckFolders(folders);
			CheckDelegate(isValid);
			CheckTokenSource(tokenSource);

			searchers = new List<DirectoryCancellationSearcherBase>();
			this.suppressOperationCanceledException = suppressOperationCanceledException;

			foreach (var folder in folders)
			{
				searchers.Add(new DirectoryCancellationDelegateSearcher(folder, isValid, handlerOption, false, tokenSource.Token));
			}

			this.tokenSource = tokenSource;
		}

		#endregion

		/// <summary>
		/// Starts a directory search operation with realtime reporting using several threads in thread pool.
		/// </summary>
		public void StartSearch()
		{
			try
			{
				searchers.ForEach(s => s.StartSearch());
			}
			catch (OperationCanceledException)
			{
				OnSearchCompleted(true);
				if (!suppressOperationCanceledException) throw;
				return;
			}

			OnSearchCompleted(false);
		}

		/// <summary>
		/// Starts a directory search operation with realtime reporting using several threads in thread pool as an asynchronous operation.
		/// </summary>
		public Task StartSearchAsync()
		{
			return Task.Run(() => StartSearch(), tokenSource.Token);
		}

		/// <summary>
		/// Stops a directory search operation.
		/// </summary>
		public void StopSearch() => tokenSource.Cancel();

		#endregion
	}
}
