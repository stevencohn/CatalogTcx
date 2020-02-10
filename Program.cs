//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace CatalogTcx
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;


	class Program
	{
		/// <summary>
		/// blah blah blah
		/// </summary>
		/// <param name="args">blah blah blah</param>

		static void Main(string[] args)
		{
			var path = (args.Length == 0 ? "." : args[0]);

			// scan for tcx files in the current or specified path
			Console.WriteLine();
			Console.WriteLine("... scanning files in current directory");

			var count = ScanFiles(path, path, true);
			Console.WriteLine("... found: " + count.ToString());

			// determine if a Garmin USB is mounted
			var disks = new UsbFactory().GetAvailableDisks();
			if ((disks != null) && (disks.Count > 0))
			{
				var disk = disks.FirstOrDefault(
					e => e.Model.StartsWith("Garmin", StringComparison.InvariantCulture));

				if (disk != null)
				{
					// scan for tcx files on the Garmin, disk.Name would be simply like "G:"

					// Edge 820 stores fit files in Garmin\Activities
					var sourcePath = disk.Name + @"\Garmin\Activities";
					if (!Directory.Exists(sourcePath))
					{
						// Edge 705 stores tcx files in Garmin\History
						sourcePath = disk.Name + @"\Garmin\History";
					}

					if (Directory.Exists(sourcePath))
					{
						Console.WriteLine();
						Console.WriteLine("... scanning files in " + sourcePath);
						count = ScanFiles(sourcePath, path, false);
						Console.WriteLine("... found: " + count);
					}
				}
			}
			else
			{
				Console.WriteLine();
				Console.WriteLine("... Garmin not found");
			}

			// scan for Zwift files
			Console.WriteLine();
			Console.WriteLine("... scanning for Zwift files");
			count += ScanZwift(path, true);
			Console.WriteLine("... found: " + count);

			Console.WriteLine();
			Console.Write("... Press any key: ");
			Console.ReadKey();
		}


		private static int ScanFiles(string sourcePath, string targetPath, bool clean)
		{
			var count = 0;
			var dirnam = Path.GetDirectoryName(targetPath);
			if (dirnam == null) return 0;

			foreach (var filnam in GetFiles(sourcePath, new[] { ".fit", ".tcx" }))
			{
				var ext = Path.GetExtension(filnam);
				if (ext.Equals(".fit"))
				{
					// TODO: convert fit to tcx
					// TODO: set filnam to converted tcx file
				}

				// open each TCX file and look for its activity start time

				var root = XElement.Load(filnam);
				var ns = root.GetDefaultNamespace();

				var lap = (from e in root
							   .Element(ns + "Activities")?
							   .Element(ns + "Activity")?
							   .Elements(ns + "Lap")
						   where e.Attribute("StartTime") != null
						   select e).FirstOrDefault();

				var startTime = lap?.Attribute("StartTime");
				if (startTime == null) continue;

				// parse the start time so we can reformat into a filename
				var dttm = DateTime.Parse(startTime.Value);

				// the year is also used as a directory name
				var year = dttm.Year.ToString("0000");

				// build the final file name
				var stamp = $"{year}{dttm.Month:00}{dttm.Day:00}_{dttm.Hour:00}{dttm.Minute:00}.tcx";

				// ensure the archive directory (year name) exists
				var dirpath = Path.Combine(dirnam, year);
				if (!Directory.Exists(dirpath))
				{
					Directory.CreateDirectory(dirpath);
				}

				var target = Path.Combine(dirpath, stamp);
				if (File.Exists(target))
				{
					// delete target so there's no problem overwriting it
					File.Delete(target);
				}

				// save as formatted XML
				root.Save(target, SaveOptions.None);
				Console.WriteLine("    " + Path.GetFileName(filnam) + " --> " + Path.GetFileName(target));
				count++;

				// set the timestamps of the file to the activity start time
				File.SetCreationTime(target, dttm);
				File.SetLastWriteTime(target, dttm);

				if (clean)
				{
					// delete the source
					File.Delete(filnam);
				}
			}

			return count;
		}

		// gpsbabel -t -i garmin_fit 
		//     -f C:/Users/steven/Documents/Zwift/Activities/2016-12-08-18-33-29.fit 
		//     -o gtrnctr,course=0,sport=Biking
		//     -F C:/Users/steven/Desktop/qwe.tcx

		private static int ScanZwift(string targetPath, bool clean)
		{
			var dirnam = Path.GetDirectoryName(targetPath);
			if (dirnam == null) return 0;

			var sourcePath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
				@"Zwift\Activities");

			if (!Directory.Exists(sourcePath)) return 0;

			// the year is also used as target directory name
			var year = DateTime.Now.Year.ToString("0000");
			// ensure the archive directory (year name) exists
			var dirpath = Path.Combine(targetPath, year);
			if (!Directory.Exists(dirpath))
			{
				Directory.CreateDirectory(dirpath);
			}

			var count = 0;

			foreach (var filnam in GetFiles(sourcePath, new[] { ".fit" }))
			{
				var name = Path.GetFileName(filnam);
				if (name != null)
				{
					var target = Path.Combine(dirpath, name);
					if (File.Exists(target))
					{
						// delete target so there's no problem overwriting it
						File.Delete(target);
					}

					if (clean)
					{
						File.Move(filnam, target);
					}
					else
					{

						File.Copy(filnam, target);
					}

					count++;

					// set the timestamps of the file to the activity start time
					//File.SetCreationTime(target, dttm);
					//File.SetLastWriteTime(target, dttm);
				}
			}

			return count;
		}


		private static IEnumerable<string> GetFiles(string path, string[] types)
		{
			return Directory.GetFiles(path)
				.Where(file => types.Any(t => t.Equals(Path.GetExtension(file)))).ToList();
		}
	}
}
