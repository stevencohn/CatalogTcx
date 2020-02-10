//************************************************************************************************
// Copyright © 2010 Steven M. Cohn. All Rights Reserved.
//************************************************************************************************

namespace CatalogTcx
{
	using System.Linq;
	using System.Management;

	
	internal static class WmiExtensions
	{

		/// <summary>
		/// Fetch the first item from the search result collection.
		/// </summary>
		/// <param name="searcher"></param>
		/// <returns></returns>

		public static ManagementObject First (this ManagementObjectSearcher searcher)
		{
			return searcher.Get().Cast<ManagementObject>().FirstOrDefault();
		}
	}
}
