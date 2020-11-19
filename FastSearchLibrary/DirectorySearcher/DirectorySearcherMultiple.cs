#pragma warning disable IDE0052 // Remove unread private members

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
	/// <summary>Represents a class for fast directory search in multiple directories.</summary>
	public class DirectorySearcherMultiple : FileBase
	{
		#region Instance members

		private readonly List<DirectoryCancellationSearcherBase> searchers;
		private readonly CancellationTokenSource tokenSource;
		private readonly ExecuteHandlers handlerOption; // Unread member!
		private readonly bool allowOperationCanceledException;

		/// <summary>Event fires when next portion of directories is found. Event handlers are not thread safe.</summary>

		public event EventHandler<DirectoryEventArgs> DirectoriesFound
		{
			add { searchers.ForEach((s) => s.DirectoriesFound += value); }
			remove { searchers.ForEach((s) => s.DirectoriesFound -= value); }
		}

		/// <summary>Event fires when search process is completed or stopped.</summary>

		public event EventHandler<SearchCompletedEventArgs>? SearchCompleted;

		/// <summary>Calls a SearchCompleted event.</summary>
		/// <param name="isCanceled">Determines whether search process canceled.</param>
		protected virtual void OnSearchCompleted(bool isCanceled) => SearchCompleted?.Invoke(this, new SearchCompletedEventArgs(isCanceled));

		#region DirectoryCancellationPatternSearcher constructors

		/// <summary>Initializes a new instance of the <see cref="DirectorySearcherMultiple"/> class.</summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="tokenSource">Instance of <see cref="CancellationTokenSource"/> for search process cancellation possibility.</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public DirectorySearcherMultiple(List<string> folders, CancellationTokenSource tokenSource) : this(folders, "*", tokenSource) { }

		/// <summary>Initializes a new instance of the <see cref="DirectorySearcherMultiple"/> class.</summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="pattern">The search pattern.</param>
		/// <param name="tokenSource">Instance of <see cref="CancellationTokenSource"/> for search process cancellation possibility.</param>
		/// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
		/// <param name="allowOperationCanceledException">if set to <c>true</c> [allow operation canceled exception].</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public DirectorySearcherMultiple(
			List<string> folders,
			string pattern,
			CancellationTokenSource tokenSource,
			ExecuteHandlers handlerOption = ExecuteHandlers.InCurrentTask,
			bool allowOperationCanceledException = false)
		{
			CheckFolders(folders);
			CheckPattern(pattern);
			CheckTokenSource(tokenSource);

			searchers = new List<DirectoryCancellationSearcherBase>();
			this.tokenSource = tokenSource;
			this.handlerOption = handlerOption;
			this.allowOperationCanceledException = allowOperationCanceledException;

			foreach (var folder in folders) // TODO: allowOperationCanceledException = false !?
			{
				searchers.Add(new DirectoryCancellationSearcher(folder, pattern, tokenSource.Token, handlerOption, false));
			}

		}

		#endregion

		#region DirectoryCancellationDelegateSearcher constructor

		/// <summary>Initializes a new instance of the <see cref="DirectorySearcherMultiple"/> class.</summary>
		/// <param name="folders">Start search directories.</param>
		/// <param name="isValid">The delegate that determines algorithm of directory selection.</param>
		/// <param name="tokenSource">Instance of <see cref="CancellationTokenSource"/> for search process cancellation possibility.</param>
		/// <param name="handlerOption">Specifies where DirectoriesFound event handlers are executed.</param>
		/// <param name="allowOperationCanceledException">if set to <c>true</c> [allow operation canceled exception].</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public DirectorySearcherMultiple(
			List<string> folders,
			Func<DirectoryInfo, bool> isValid,
			CancellationTokenSource tokenSource,
			ExecuteHandlers handlerOption = ExecuteHandlers.InCurrentTask,
			bool allowOperationCanceledException = false)
		{
			CheckFolders(folders);
			CheckDelegate(isValid);
			CheckTokenSource(tokenSource);

			searchers = new List<DirectoryCancellationSearcherBase>();
			this.tokenSource = tokenSource;
			this.handlerOption = handlerOption;
			this.allowOperationCanceledException = allowOperationCanceledException;

			foreach (var folder in folders) // TODO: allowOperationCanceledException = false !?
			{
				searchers.Add(new DirectoryCancellationSearcher(folder, isValid, tokenSource.Token, handlerOption, false));
			}

		}

		#endregion

		/// <summary>Starts a directory search operation with realtime reporting using several threads in thread pool.</summary>
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

		/// <summary>Starts a directory search operation with realtime reporting using several threads in thread pool as an asynchronous operation.</summary>
		public Task StartSearchAsync() => Task.Run(() => StartSearch(), tokenSource.Token);

		/// <summary>Stops a directory search operation.</summary>
		public void StopSearch() => tokenSource.Cancel();

		#endregion
	}
}
