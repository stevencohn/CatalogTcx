//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace CatalogTcx
{
	using System.Collections.Generic;
	using System.Management;


	/// <summary>
	/// Manages USB devices on the current system.
	/// </summary>

	internal class UsbFactory
	{

		/// <summary>
		/// Gets a list of available USB disks on the current system.
		/// </summary>
		/// <returns>A List of UsbDisk instances.</returns>

		public IEnumerable<UsbDisk> GetAvailableDisks()
		{
			using var searcher = new ManagementObjectSearcher(
				"select DeviceID, Model from Win32_DiskDrive where InterfaceType='USB'").Get();

			// browse all USB WMI physical disks
			foreach (var o in searcher)
			{
				var drive = (ManagementObject)o;

				// associate physical disks with partitions
				using var partition = new ManagementObjectSearcher(
					$"associators of {{Win32_DiskDrive.DeviceID='{drive["DeviceID"]}'}} where AssocClass = Win32_DiskDriveToDiskPartition").First();

				if (partition == null) continue;

				// associate partitions with logical disks (drive letter volumes)
				using var logical = new ManagementObjectSearcher(
					$"associators of {{Win32_DiskPartition.DeviceID='{partition["DeviceID"]}'}} where AssocClass = Win32_LogicalDiskToPartition").First();

				if (logical == null) continue;

				// finally find the logical disk entry to determine the volume name
				using var volume = new ManagementObjectSearcher(
					$"select FreeSpace, Size, VolumeName from Win32_LogicalDisk where Name='{logical["Name"]}'").First();

				yield return new UsbDisk(logical["Name"].ToString())
				{
					Model = drive["Model"].ToString(),
					Volume = volume["VolumeName"].ToString(),
					FreeSpace = (ulong)volume["FreeSpace"],
					Size = (ulong)volume["Size"]
				};
			}
		}
	}
}
