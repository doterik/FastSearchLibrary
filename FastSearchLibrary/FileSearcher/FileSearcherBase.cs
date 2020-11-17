#pragma warning disable IDE0022 // Use expression body for methods

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
	internal abstract class FileSearcherBase
	{
		/// <summary>
		/// Specifies where FilesFound event handlers are executed.
		/// </summary>
		protected ExecuteHandlers HandlerOption { get; }
		protected string Folder { get; }
		protected ConcurrentBag<Task> TaskHandlers { get; }

		public FileSearcherBase(string folder, ExecuteHandlers handlerOption)
		{
			Folder = folder;
			HandlerOption = handlerOption;
			TaskHandlers = new ConcurrentBag<Task>();
		}

		public event EventHandler<FileEventArgs>? FilesFound;

		public event EventHandler<SearchCompletedEventArgs>? SearchCompleted;

		protected virtual void GetFilesFast()
		{
			GetStartDirectories(Folder).AsParallel().ForAll((d1) =>
			{
				GetStartDirectories(d1.FullName).AsParallel().ForAll((d2) =>
				{
					GetFiles(d2.FullName);
				});
			});

			OnSearchCompleted(false);
		}

		protected virtual void OnFilesFound(List<FileInfo> files)
		{
			if (HandlerOption == ExecuteHandlers.InNewTask)
			{
				TaskHandlers.Add(Task.Run(() => CallFilesFound(files)));
			}
			else
			{
				CallFilesFound(files);
			}
		}

		protected virtual void CallFilesFound(List<FileInfo> files)
		{
			FilesFound?.Invoke(this, new FileEventArgs(files));
		}

		protected virtual void OnSearchCompleted(bool isCanceled)
		{
			if (HandlerOption == ExecuteHandlers.InNewTask)
			{
				Task.WaitAll(TaskHandlers.ToArray());
			}

			CallSearchCompleted(isCanceled);
		}

		protected virtual void CallSearchCompleted(bool isCanceled)
		{
			SearchCompleted?.Invoke(this, new SearchCompletedEventArgs(isCanceled));
		}

		protected abstract void GetFiles(string folder);
		protected abstract List<DirectoryInfo> GetStartDirectories(string folder);
		public abstract void StartSearch();
	}
}
