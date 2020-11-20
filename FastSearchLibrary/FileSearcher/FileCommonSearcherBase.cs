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
	internal abstract class FileCommonSearcherBase
	{
		/// <summary>
		/// Specifies where FilesFound event handlers are executed.
		/// </summary>
		protected ExecuteHandlers HandlerOption { get; }
		protected string Folder { get; }
		protected ConcurrentBag<Task> TaskHandlers { get; }

		private protected string Pattern { get; set; } = string.Empty; // FileCommonSearcher, FileCancellationSearcher
		private protected Func<FileInfo, bool>? IsValid { get; set; }  // FileCommonSearcher, FileCancellationSearcher
		private protected CancellationToken Token { get; set; }        // FileCancellationSearcherBase

		public FileCommonSearcherBase(string folder, ExecuteHandlers handlerOption)
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

		/// <summary>
		/// Starts a file search operation with realtime reporting using several threads in thread pool.
		/// </summary>
		public virtual void StartSearch() => GetFilesFast();

		protected virtual void OnFilesFound(List<FileInfo> files)
		{
			if (files.Count == 0) return;

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

		protected virtual void GetFiles(string folder)
		{
			if (Token.CanBeCanceled) Token.ThrowIfCancellationRequested();

			DirectoryInfo dirInfo;
			DirectoryInfo[] directories;
			try
			{
				dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (directories.Length == 0)
				{
					if (Pattern != string.Empty)
					{
						OnFilesFound(dirInfo.GetFiles(Pattern).ToList()); // 'pattern'
					}
					else if (IsValid != null)
					{
						OnFilesFound(dirInfo.GetFiles().Where(file => IsValid(file)).ToList()); // 'isValid'
					}

					return;
				}
			}
			catch (UnauthorizedAccessException) { return; }
			catch (PathTooLongException) { return; }
			catch (DirectoryNotFoundException) { return; }

			foreach (var d in directories)
			{
				if (Token.CanBeCanceled) Token.ThrowIfCancellationRequested();

				GetFiles(d.FullName);
			}

			if (Token.CanBeCanceled) Token.ThrowIfCancellationRequested();

			try
			{
				if (Pattern != string.Empty)
				{
					OnFilesFound(dirInfo.GetFiles(Pattern).ToList()); // 'pattern'
				}
				else if (IsValid != null)
				{
					OnFilesFound(dirInfo.GetFiles().Where(file => IsValid(file)).ToList()); // 'isValid'
				}
			}
			catch (UnauthorizedAccessException) { }
			catch (PathTooLongException) { }
			catch (DirectoryNotFoundException) { }
		}

		protected virtual List<DirectoryInfo> GetStartDirectories(string folder)
		{
			if (Token.CanBeCanceled) Token.ThrowIfCancellationRequested();

			DirectoryInfo[] directories;
			try
			{
				var dirInfo = new DirectoryInfo(folder);
				directories = dirInfo.GetDirectories();

				if (Pattern != string.Empty)
				{
					OnFilesFound(dirInfo.GetFiles(Pattern).ToList()); // 'pattern'
				}
				else if (IsValid != null)
				{
					OnFilesFound(dirInfo.GetFiles().Where(file => IsValid(file)).ToList()); // 'isValid'
				}

				if (directories.Length > 1) return new List<DirectoryInfo>(directories);
				if (directories.Length == 0) return new();
			}
			catch (UnauthorizedAccessException) { return new(); }
			catch (PathTooLongException) { return new(); }
			catch (DirectoryNotFoundException) { return new(); }

			return GetStartDirectories(directories[0].FullName); // directories.Length == 1
		}
	}
}
