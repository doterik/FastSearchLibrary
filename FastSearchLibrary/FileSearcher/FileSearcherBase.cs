//#pragma warning disable IDE0003 // Remove qualification
//#pragma warning disable IDE0007 // Use implicit type

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
			GetStartDirectories(Folder).AsParallel().ForAll((d) =>
			{
				GetStartDirectories(d.FullName).AsParallel().ForAll((dir) =>
				{
					GetFiles(dir.FullName);
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
			if (FilesFound != null)
			{
				var arg = new FileEventArgs(files);
				FilesFound(this, arg);
			}
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
			if (SearchCompleted != null)
			{
				var arg = new SearchCompletedEventArgs(isCanceled);
				SearchCompleted(this, arg);
			}
		}


		protected abstract void GetFiles(string folder);


		protected abstract List<DirectoryInfo> GetStartDirectories(string folder);


		public abstract void StartSearch();
	}
}
