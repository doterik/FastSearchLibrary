#pragma warning disable IDE0022 // Use expression body for methods

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
	/// <summary>Represents a class for fast file search in multiple directories.</summary>
	public class FileSearcherMultiple : FileBase
	{
		private readonly List<FileCommonSearcherBase> searchers;
		private readonly CancellationTokenSource tokenSource;
		private readonly bool allowOperationCanceledException;

		/// <summary>Event fires when next portion of files is found. Event handlers are not thread safe.</summary>

		public event EventHandler<FileEventArgs>? FilesFound
		{
			add { searchers.ForEach((s) => s.FilesFound += value); }
			remove { searchers.ForEach((s) => s.FilesFound -= value); }
		}

		/// <summary>Event fires when search process is completed or stopped.</summary>

		public event EventHandler<SearchCompletedEventArgs>? SearchCompleted;

		/// <summary>Calls a SearchCompleted event.</summary>
		/// <param name="isCanceled">Determines whether search process canceled.</param>
		protected virtual void OnSearchCompleted(bool isCanceled)
		{
			SearchCompleted?.Invoke(this, new SearchCompletedEventArgs(isCanceled));
		}

		#region FileCancellationPatternSearcher constructors

		/// <summary>Initializes a new instance of the <see cref="FileSearcherMultiple"/> class.</summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="tokenSource">Instance of <see cref="CancellationTokenSource"/> for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcherMultiple(List<string> folders, CancellationTokenSource tokenSource) : this(folders, "*", tokenSource) { }

		/// <summary>Initializes a new instance of the <see cref="FileSearcherMultiple"/> class.</summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <param name="tokenSource">Instance of <see cref="CancellationTokenSource"/> for search process cancellation possibility.</param>
		/// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
		/// <param name="allowOperationCanceledException">if set to <c>true</c> [allow operation canceled exception].</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcherMultiple(
			List<string> folders,
			string pattern,
			CancellationTokenSource tokenSource,
			ExecuteHandlers handlerOption = ExecuteHandlers.InCurrentTask,
			bool allowOperationCanceledException = false)
		{
			CheckFolders(folders);
			CheckPattern(pattern);
			CheckTokenSource(tokenSource);

			searchers = new List<FileCommonSearcherBase>();
			this.tokenSource = tokenSource;
			this.allowOperationCanceledException = allowOperationCanceledException;

			foreach (var folder in folders)
			{
				searchers.Add(new FileCancellationSearcher(folder, pattern, tokenSource.Token, handlerOption, false));
			}
		}

		#endregion

		#region FileCancellationDelegateSearcher constructor

		/// <summary>Initializes a new instance of the <see cref="FileSearcherMultiple"/> class.</summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="isValid">The delegate that determines algorithm of file selection.</param>
		/// <param name="tokenSource">Instance of <see cref="CancellationTokenSource"/> for search process cancellation possibility.</param>
		/// <param name="handlerOption">Specifies where FilesFound event handlers are executed.</param>
		/// <param name="allowOperationCanceledException">if set to <c>true</c> [allow operation canceled exception].</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public FileSearcherMultiple(
			List<string> folders,
			Func<FileInfo, bool> isValid,
			CancellationTokenSource tokenSource,
			ExecuteHandlers handlerOption = ExecuteHandlers.InCurrentTask,
			bool allowOperationCanceledException = false)
		{
			CheckFolders(folders);
			CheckDelegate(isValid);
			CheckTokenSource(tokenSource);

			searchers = new List<FileCommonSearcherBase>();
			this.tokenSource = tokenSource;
			this.allowOperationCanceledException = allowOperationCanceledException;

			foreach (var folder in folders)
			{
				searchers.Add(new FileCancellationSearcher(folder, isValid, tokenSource.Token, handlerOption, false));
			}
		}

		#endregion

		/// <summary>Starts a file search operation with realtime reporting using several threads in thread pool.</summary>
		public void StartSearch()
		{
			try
			{
				searchers.ForEach(s => s.StartSearch());
			}
			catch (OperationCanceledException)
			{
				OnSearchCompleted(true);
				if (allowOperationCanceledException) throw;
				return;
			}

			OnSearchCompleted(false);
		}

		/// <summary>Starts a file search operation with realtime reporting using several threads in thread pool as an asynchronous operation.</summary>
		public Task StartSearchAsync()
		{
			return Task.Run(() => StartSearch(), tokenSource.Token);
		}

		/// <summary>Stops a file search operation.</summary>
		public void StopSearch() => tokenSource.Cancel();
	}
}
