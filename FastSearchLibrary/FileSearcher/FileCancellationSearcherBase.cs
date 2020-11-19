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
		//protected CancellationToken Token { get; }
		protected bool AllowOperationCanceledException { get; }

		public FileCancellationSearcherBase(string folder, ExecuteHandlers handlerOption, bool allowOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption)
		{
			Token = token;
			AllowOperationCanceledException = allowOperationCanceledException;
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

		//protected override void GetFiles(string folder)
		//protected override List<DirectoryInfo> GetStartDirectories(string folder)

		public override void StartSearch()
		{
			try
			{
				GetFilesFast();
			}
			catch (OperationCanceledException)
			{
				OnSearchCompleted(true); // isCanceled == true

				if (AllowOperationCanceledException) Token.ThrowIfCancellationRequested();

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
