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
		public const string CurrentVersionString = "2.3.0";
		public const string CurrentVersionFullString = "2.3.0 Beta";
		public const int ProtocolVersion = 3;

		public static UpdateDescription[] UpdateLog =
		{
			new UpdateDescription
			{
				VersionString = "2.3.0",
				Date = "2020 XX XX",
				UpdateName = "Multiplayer Update 2.3",
				
				ChangeLog = new string[]{
					"+ Added Biomes",
					"+ Added Inventory",
					"+ Added Itemstacks",
					"+ Upgraded rendering code",
					"+ Added loading screen",
					"+ Added more settings",
					"+ Menus are now lua scripts.",
					"+ Many under-the-hood changes and upgrades",
					"+ Added summon command",
					"+ Added Entity Health Stat",
					"+ Added a crash report system",
					"+ Added Entity Status Effects",
					"+ Liquids slow entities down, some even float.",
					"+ Fixed numerous bugs"
				},
				Notes = new string[]{
					"This version will be distributed to playtesters only.",
					"2.3 will be released on Steam in one week."
				},
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
