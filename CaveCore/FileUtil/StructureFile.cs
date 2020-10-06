using CaveGame.Core.FileUtil;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CaveGame.Core.FileUtil
{
	[Serializable]
	public class StructureMetadata
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public string Author { get; set; }
		public string Name { get; set; }
	}

	[Serializable]
	public class Structure
	{
		[XmlElement("Layer")]
		public string[] LayerIDs { get; set; }
	}

	[Serializable]
	public class StructureFile : Configuration
	{
		StructureMetadata Metadata { get; set; }
		Structure Structure { get; set; }

		[XmlInclude(typeof(StructureFile))]
		public virtual void Save()
		{

		}



		public StructureFile(string filepath)
		{

		}

		public StructureFile(StructureMetadata metadata)
		{
			Metadata = metadata;
			Structure = new Structure
			{
				LayerIDs = new string[] { "Layer1" },
			};
		}
	}

}
