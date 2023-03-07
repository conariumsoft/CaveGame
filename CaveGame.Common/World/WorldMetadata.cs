using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace CaveGame.Common.World
{
    public class WorldMetadata
	{
		public int Seed { get; set; }
		public string Name { get; set; }
		public string LastPlayTime { get; set; }
		public string LastVersionPlayedOn { get; set; }


        public static WorldMetadata LoadWorldData(string directory)
        {
            XmlDocument worldmetaXML = new XmlDocument();
            worldmetaXML.Load(Path.Combine(directory, @"WorldMetadata.xml"));

            WorldMetadata Metadata = new WorldMetadata
            {
                Name = worldmetaXML["Metadata"]["Name"]?.InnerText,
                LastPlayTime = worldmetaXML["Metadata"]["LastPlayed"]?.InnerText,
                Seed = Int32.Parse(worldmetaXML["Metadata"]["Seed"]?.InnerText),
				LastVersionPlayedOn = worldmetaXML["Metadata"]["LastVersion"]?.InnerText,

				
            };


            return Metadata;
        }
    }
}
