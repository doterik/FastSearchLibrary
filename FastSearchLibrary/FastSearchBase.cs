using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FastSearchLibrary
{
	/// <summary>Static helper methods for <see cref="FastSearchLibrary"/></summary>
	public class FastSearchBase
	{
		#region Checking methods
		private protected static void CheckFolder(string folder)
		{
			if (folder == null) throw new ArgumentNullException(nameof(folder), "Argument is null.");
			if (folder == string.Empty) throw new ArgumentException("Argument is not valid.", nameof(folder));

			var dir = new DirectoryInfo(folder);

			if (!dir.Exists) throw new ArgumentException("Argument does not represent an existing directory.", nameof(folder));
		}

		private protected static void CheckFolders(List<string> folders)
		{
			if (folders == null) throw new ArgumentNullException(nameof(folders), "Argument is null.");
			if (folders.Count == 0) throw new ArgumentException("Argument is an empty list.", nameof(folders));

			foreach (var folder in folders) CheckFolder(folder);
		}

		private protected static void CheckPattern(string pattern)
		{
			if (pattern == null) throw new ArgumentNullException(nameof(pattern), "Argument is null.");
			if (pattern == string.Empty) throw new ArgumentException("Argument is not valid.", nameof(pattern));
		}

		private protected static void CheckDelegate(Func<FileInfo, bool> isValid)
		{
			if (isValid == null) throw new ArgumentNullException(nameof(isValid), "Argument is null.");
		}

		private protected static void CheckDelegate(Func<DirectoryInfo, bool> isValid)
		{
			if (isValid == null) throw new ArgumentNullException(nameof(isValid), "Argument is null.");
		}

		private protected static void CheckTokenSource(CancellationTokenSource tokenSource)
		{
			if (tokenSource == null) throw new ArgumentNullException(nameof(tokenSource), @"Argument ""tokenSource"" is null.");
		}

		#endregion
	}
}
