﻿#pragma warning disable IDE0003 // Remove qualification
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

		protected CancellationToken token;

		protected bool SuppressOperationCanceledException { get; set; }

		public FileCancellationSearcherBase(string folder, ExecuteHandlers handlerOption, bool suppressOperationCanceledException, CancellationToken token)
			: base(folder, handlerOption)
		{
			this.token = token;
			this.SuppressOperationCanceledException = suppressOperationCanceledException;
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
										 
				if (!SuppressOperationCanceledException)
					token.ThrowIfCancellationRequested();

				return;
			}

			OnSearchCompleted(false); 
		}


		protected override void OnFilesFound(List<FileInfo> files)
		{
			var arg = new FileEventArgs(files);

			if (HandlerOption == ExecuteHandlers.InNewTask)
				taskHandlers.Add(Task.Run(() => CallFilesFound(files), token));
			else
				CallFilesFound(files);
		}


		protected override void OnSearchCompleted(bool isCanceled)
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

				CallSearchCompleted(isCanceled);           
			}
			else
				CallSearchCompleted(isCanceled);
		}


		protected override void GetFilesFast()
		{
			List<DirectoryInfo> startDirs = GetStartDirectories(folder);

			startDirs.AsParallel().WithCancellation(token).ForAll((d) =>
			{
				GetStartDirectories(d.FullName).AsParallel().WithCancellation(token).ForAll((dir) =>
				{
					GetFiles(dir.FullName);
				});
			});
		}

	}
}
