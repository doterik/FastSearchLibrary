#pragma warning disable IDE0022 // Use expression body for methods

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FastSearchLibrary
{
	internal abstract class FileCancellationSearcherBase : FileSearcherBase
	{
		protected CancellationToken Token { get; }
		protected bool SuppressOperationCanceledException { get; }

		public FileCancellationSearcherBase(string folder, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption)
		{
			Token = token;
			SuppressOperationCanceledException = suppressOperationCanceledException;
		}

		protected override void GetFilesFast() /* .WithCancellation(Token) */
		{
			GetStartDirectories(Folder).AsParallel().WithCancellation(Token).ForAll((d1) =>
			{
				GetStartDirectories(d1.FullName).AsParallel().WithCancellation(Token).ForAll((d2) =>
				{
					GetFiles(d2.FullName);
				});
			});
		}

//sync
		protected override void GetFiles(string folder)
		{
			Token.ThrowIfCancellationRequested();

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
				Token.ThrowIfCancellationRequested();

				GetFiles(d.FullName);
			}

			Token.ThrowIfCancellationRequested();

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

		protected override List<DirectoryInfo> GetStartDirectories(string folder)
		{
			Token.ThrowIfCancellationRequested();

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

		public override void StartSearch()
		{
			try
			{
				GetFilesFast();
			}
			catch (OperationCanceledException)
			{
				OnSearchCompleted(true); // isCanceled == true

				if (!SuppressOperationCanceledException) Token.ThrowIfCancellationRequested();

				return;
			}

			OnSearchCompleted(false);
		}

		protected override void OnFilesFound(List<FileInfo> files)
		{
			if (files.Count == 0) return;

			if (HandlerOption == ExecuteHandlers.InNewTask)
			{
				TaskHandlers.Add(Task.Run(() => CallFilesFound(files), Token)); /* ,Token */
			}
			else
			{
				CallFilesFound(files);
			}
		}

		protected override void OnSearchCompleted(bool isCanceled)
		{
			if (HandlerOption == ExecuteHandlers.InNewTask)
			{
				try
				{
					Task.WaitAll(TaskHandlers.ToArray());
				}
				catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
				{
					isCanceled = true;
				}
			}

			CallSearchCompleted(isCanceled); // else
		}
	}
}
