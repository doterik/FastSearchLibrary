﻿#pragma warning disable IDE0003 // Remove qualification
#pragma warning disable IDE0007 // Use implicit type

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
		protected ExecuteHandlers HandlerOption { get; set; }

		private readonly string folder;

		private readonly ConcurrentBag<Task> taskHandlers;

		protected CancellationToken token;

		protected bool SuppressOperationCanceledException { get; set; }

		public DirectoryCancellationSearcherBase(string folder, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
		{
			this.folder = folder;
			this.token = token;
			this.HandlerOption = handlerOption;
			this.SuppressOperationCanceledException = suppressOperationCanceledException;
			taskHandlers = new ConcurrentBag<Task>();
		}


		public event EventHandler<DirectoryEventArgs> DirectoriesFound;

		public event EventHandler<SearchCompletedEventArgs> SearchCompleted;


		protected virtual void OnDirectoriesFound(List<DirectoryInfo> directories)
		{
			EventHandler<DirectoryEventArgs> handler = DirectoriesFound;

			if (handler != null)
			{
				var arg = new DirectoryEventArgs(directories);

				if (HandlerOption == ExecuteHandlers.InNewTask)
					taskHandlers.Add(Task.Run(() => DirectoriesFound(this, arg), token));
				else
					handler(this, arg);
			}
		}


		protected virtual void OnSearchCompleted(bool isCanceled)
		{
			EventHandler<SearchCompletedEventArgs> handler = SearchCompleted;

			if (handler != null)
			{
				if (HandlerOption == ExecuteHandlers.InNewTask)
				{
					try
					{
						Task.WaitAll(taskHandlers.ToArray());
					}
					catch (AggregateException ex)
					{
						if (!(ex.InnerException is TaskCanceledException))
							throw;

						if (!isCanceled)
							isCanceled = true;
					}
				}

				var arg = new SearchCompletedEventArgs(isCanceled);

				handler(this, arg);
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

				if (!SuppressOperationCanceledException)
					token.ThrowIfCancellationRequested();

				return;
			}

			OnSearchCompleted(false);
		}



		protected virtual void GetDirectoriesFast()
		{
			List<DirectoryInfo> startDirs = GetStartDirectories(folder);

			startDirs.AsParallel().WithCancellation(token).ForAll((d) =>
			{
				GetStartDirectories(d.FullName).AsParallel().WithCancellation(token).ForAll((dir) =>
				{
					GetDirectories(dir.FullName);
				});
			});
		}


		protected abstract void GetDirectories(string folder);

		protected abstract List<DirectoryInfo> GetStartDirectories(string folder);

	}
}
