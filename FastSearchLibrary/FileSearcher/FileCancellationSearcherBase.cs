#pragma warning disable IDE0007 // Use implicit type

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
			var arg = new FileEventArgs(files); // TODO arg !?

			if (HandlerOption == ExecuteHandlers.InNewTask)
				TaskHandlers.Add(Task.Run(() => CallFilesFound(files), Token));
			else
				CallFilesFound(files);
		}

		protected override void OnSearchCompleted(bool isCanceled)
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

				CallSearchCompleted(isCanceled); // TODO =else
			}
			else
				CallSearchCompleted(isCanceled);
		}

		protected override void GetFilesFast()
		{
			List<DirectoryInfo> startDirs = GetStartDirectories(Folder);

			GetStartDirectories(Folder).AsParallel().WithCancellation(Token).ForAll((d1) =>
			{
				GetStartDirectories(d1.FullName).AsParallel().WithCancellation(Token).ForAll((d2) =>
				{
					GetFiles(d2.FullName);
				});
			});
		}
	}
}
