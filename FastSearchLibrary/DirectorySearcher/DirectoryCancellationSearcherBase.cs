#pragma warning disable IDE0022 // Use expression body for methods

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
	internal abstract class DirectoryCancellationSearcherBase
	{
		/// <summary>
		/// Determines where execute event DirectoriesFound handlers
		/// </summary>
		protected ExecuteHandlers HandlerOption { get; }
		private string Folder { get; }
		private ConcurrentBag<Task> TaskHandlers { get; }
		protected CancellationToken Token { get; }
		protected bool SuppressOperationCanceledException { get; set; }

		public DirectoryCancellationSearcherBase(string folder, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
		{
			Folder = folder;
			Token = token;
			HandlerOption = handlerOption;
			SuppressOperationCanceledException = suppressOperationCanceledException;
			TaskHandlers = new ConcurrentBag<Task>();
		}

		public event EventHandler<DirectoryEventArgs>? DirectoriesFound;

		public event EventHandler<SearchCompletedEventArgs>? SearchCompleted;

		protected virtual void OnDirectoriesFound(List<DirectoryInfo> directories)
		{
			if (DirectoriesFound != null)
			{
				var arg = new DirectoryEventArgs(directories);

				if (HandlerOption == ExecuteHandlers.InNewTask)
					TaskHandlers.Add(Task.Run(() => DirectoriesFound(this, arg), Token));
				else
					DirectoriesFound(this, arg);
			}
		}

		protected virtual void OnSearchCompleted(bool isCanceled)
		{
			if (SearchCompleted != null)
			{
				if (HandlerOption == ExecuteHandlers.InNewTask)
				{
					try
					{
						Task.WaitAll(TaskHandlers.ToArray());
					}
					catch (AggregateException ex)
					{
						if (!(ex.InnerException is TaskCanceledException)) throw;

						if (!isCanceled) isCanceled = true;
					}
				}

				var arg = new SearchCompletedEventArgs(isCanceled);
				SearchCompleted(this, arg);
			}
		}

		/// <summary>
		/// Starts a directory search operation with realtime reporting using several threads in thread pool.
		/// </summary>
		public virtual void StartSearch()
		{
			try
			{
				GetDirectoriesFast();
			}
			catch (OperationCanceledException)
			{
				OnSearchCompleted(true); // isCanceled == true

				if (!SuppressOperationCanceledException) Token.ThrowIfCancellationRequested();

				return;
			}

			OnSearchCompleted(false);
		}

		protected virtual void GetDirectoriesFast()
		{

			GetStartDirectories(Folder).AsParallel().WithCancellation(Token).ForAll((d1) =>
			{
				GetStartDirectories(d1.FullName).AsParallel().WithCancellation(Token).ForAll((d2) =>
				{
					GetDirectories(d2.FullName);
				});
			});
		}

		protected abstract void GetDirectories(string folder);
		protected abstract List<DirectoryInfo> GetStartDirectories(string folder);
	}
}
