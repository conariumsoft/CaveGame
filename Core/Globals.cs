using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
	public class UpdateDescription
	{
		public string Date;
		public string VersionString;
		public string UpdateName;
		public string[] ChangeLog;
		public string[] Notes;
		
	}

	public static class Globals
	{
		public const int TileSize = 8;
		public const int ChunkSize = 32;
		public const string CurrentVersionString = "2.2.0";
		public const string CurrentVersionFullString = "2.2.0 Beta";
		public const int ProtocolVersion = 2;

		public static UpdateDescription[] UpdateLog =
		{
			new UpdateDescription
			{
				VersionString = "2.2.0",
				Date = "2020 November 13",
				UpdateName = "Multiplayer Update 2",
				Notes = new string[]{
					"f",

				},
				ChangeLog = new string[]{
					"+ Added Entity Health Stat",
					"+ Added Entity Status Effects",
					"+ Liquids slow entities down, some even float.",
					"+ Lava damages entities and inflicts Burning debuff",
					"+ Fixed some UI bugs",

				}
			},
			new UpdateDescription
			{
				VersionString = "2.1.0",
				Date = "2020 October 31",
				UpdateName = "Open Multiplayer Test",
				Notes = new string[]{
					"Changes haven't been tracked to this point, detailed update logs will come hereafter.",
				},
				ChangeLog = new string[]{
					"+ Added CaveGame"
				}
			},
		};
	}
}
