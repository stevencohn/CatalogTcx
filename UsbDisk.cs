//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace CatalogTcx
{

	/// <summary>
	/// Represents the displayable information for a single USB disk.
	/// </summary>

	internal class UsbDisk
	{

		/// <summary>
		/// Initialize a new instance with the given values.
		/// </summary>
		/// <param name="name">The Windows drive letter assigned to this device.</param>

		public UsbDisk (string name)
		{
			Name = name;
			Model = string.Empty;
			Volume = string.Empty;
			FreeSpace = 0;
			Size = 0;
		}


		/// <summary>
		/// Gets the description of this disk.
		/// </summary>

		public string Description => 
			string.IsNullOrEmpty(Volume) ? Name : $"{Name} {Volume} ({Model})";


		/// <summary>
		/// Gets the available free space on the disk, specified in bytes.
		/// </summary>

		public ulong FreeSpace
		{
			get;
			internal set;
		}


		/// <summary>
		/// Get the model of this disk.  This is the manufacturer's name.
		/// </summary>
		/// <remarks>
		/// When this class is used to identify a removed USB device, the Model
		/// property is set to String.Empty.
		/// </remarks>

		public string Model
		{
			get;
			internal set;
		}


		/// <summary>
		/// Gets the name of this disk.  This is the Windows identifier, drive letter.
		/// </summary>

		public string Name
		{
			get;
		}


		/// <summary>
		/// Gets the total size of the disk, specified in bytes.
		/// </summary>

		public ulong Size
		{
			get;
			internal set;
		}


		/// <summary>
		/// Get the volume name of this disk.  This is the friently name ("Stick").
		/// </summary>
		/// <remarks>
		/// When this class is used to identify a removed USB device, the Volume
		/// property is set to String.Empty.
		/// </remarks>

		public string Volume
		{
			get;
			internal set;
		}
	}
}
