using CaveGame.Core.FileUtil;
using CaveGame.Core.Game.Tiles;
using CaveGame.Core.Game.Walls;
using DataManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
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
		public string File { get; set; }
		public string Name { get; set; }
		public string Notes { get; set; }
		public int EditorVersion { get; set; }
		[XmlArray("Layers")]
		[XmlArrayItem("ID")]
		public List<string> LayerList { get; set; }
	}

	public class Furniture
	{

	}



	public class Layer
	{
		[XmlIgnore]
		public bool Visible { get; set; }
		[XmlIgnore]
		public StructureFile Structure { get; set; }
		[XmlIgnore]

		public string LayerID { get; set; }
		public string Notes { get; set; }
		[XmlIgnore]
		public Tile[,] Tiles;
		[XmlIgnore]
		public Wall[,] Walls;

		[XmlArray("FurnitureList")]
		[XmlArrayItem("Furniture")]
		public List<Furniture> Furniture;

		public Layer(StructureFile file) {
			
			Structure = file;
			Tiles = new CaveGame.Core.Game.Tiles.Tile[Structure.Metadata.Width, Structure.Metadata.Height];
			Walls = new CaveGame.Core.Game.Walls.Wall[Structure.Metadata.Width, Structure.Metadata.Height];
			Furniture = new List<Furniture>();
		}

		public Layer() {
			Visible = true;
		}


		public void LoadTiles(byte[] TileData) {

			int index = 0;
			for (int x = 0; x < Structure.Metadata.Width; x++)
			{
				for (int y = 0; y < Structure.Metadata.Height; y++)
				{
					Tile t = Tile.FromID(TileData.ReadShort(index));
					Tiles[x,y] = t;
					index+=2;
				}
			}

		}

		public void LoadWalls(byte[] WallData)
		{

			int index = 0;
			for (int x = 0; x < Structure.Metadata.Width; x++)
			{
				for (int y = 0; y < Structure.Metadata.Height; y++)
				{
					Wall w = Wall.FromID(WallData.ReadShort(index));
					Walls[x,y] = w;
					index+=2;
				}
			}
		}

		public byte[] SaveTiles()
		{

			byte[] TileData = new byte[Structure.Metadata.Width * Structure.Metadata.Height*2];
				

			int index = 0;
			for (int x = 0; x < Structure.Metadata.Width; x++)
			{
				for (int y = 0; y < Structure.Metadata.Height; y++)
				{
					TileData.WriteShort(index, Tiles[x,y].ID);
					index+=2;
				}
			}

			return TileData;
		}

		public byte[] SaveWalls()
		{

			byte[] WallData = new byte[Structure.Metadata.Width * Structure.Metadata.Height*2];

			int index = 0;
			for (int x = 0; x < Structure.Metadata.Width; x++)
			{
				for (int y = 0; y < Structure.Metadata.Height; y++)
				{
					WallData.WriteShort(index, Walls[x,y].ID);
					index+=2;
				}
			}

			return WallData;
		}
	}

	[Serializable]
	public class StructureFile : Configuration
	{
		[XmlIgnore]
		public string Filepath;

		public StructureMetadata Metadata { get; set; }

		[XmlIgnore]
		public List<Layer> Layers;

		[XmlInclude(typeof(StructureFile))]
		public override void Save()
		{

			using (FileStream fToOpen = new FileStream(Filepath, FileMode.Create))
			{
				using (ZipArchive archive = new ZipArchive(fToOpen, ZipArchiveMode.Update)) {
					ZipArchiveEntry structureXmlEntry = archive.CreateEntry("structure.xml");

					XmlSerializer writer = new XmlSerializer(typeof(StructureFile));
					using (var stream = new StreamWriter(structureXmlEntry.Open()))
						writer.Serialize(stream, this);

					XmlSerializer furnitureWriter = new XmlSerializer(typeof(List<Furniture>));

					foreach (Layer layer in Layers)
					{

						using (var tstream = new BinaryWriter(archive.CreateEntry("layers/" + layer.LayerID + "/tiles.l").Open()))
							tstream.Write(layer.SaveTiles());
						using (var wstream = new BinaryWriter(archive.CreateEntry("layers/" + layer.LayerID + "/walls.l").Open()))
							wstream.Write(layer.SaveWalls());
						using (var fstream = new StreamWriter(archive.CreateEntry("layers/" + layer.LayerID + "/furniture.xml").Open()))
							furnitureWriter.Serialize(fstream, layer.Furniture);
					}
				}
			}
		}

		

		public static StructureFile LoadStructure(string filepath)
		{

			string tempDirectory = Path.GetTempPath()+"structure\\";
			if (Directory.Exists(tempDirectory))
				Directory.Delete(tempDirectory, true);
			ZipFile.ExtractToDirectory(filepath, tempDirectory);


			XmlSerializer reader = new XmlSerializer(typeof(StructureFile));
			StructureFile structure;

			using (var stream = File.Open(tempDirectory + "/structure.xml", FileMode.Open, FileAccess.Read))
				structure = (StructureFile)reader.Deserialize(stream);
			XmlSerializer furnitureReader = new XmlSerializer(typeof(List<Furniture>));

			foreach (string directory in Directory.GetDirectories(tempDirectory+"layers"))
			{
				Layer layer = new Layer(structure);
				layer.Visible = true;
				layer.Structure = structure;
				layer.LayerID = directory.Replace(tempDirectory + "layers", "");
				layer.LoadTiles(File.ReadAllBytes(directory+"/tiles.l"));
				layer.LoadWalls(File.ReadAllBytes(directory+"/walls.l"));

				using (var stream = File.Open(directory + "/furniture.xml", FileMode.Open, FileAccess.Read))
					layer.Furniture = (List<Furniture>)furnitureReader.Deserialize(stream);

				structure.Layers.Add(layer);

			}
			structure.Filepath = filepath;
			Directory.Delete(Path.GetTempPath() + "structure\\", true);
			return structure;				
		}



		public StructureFile(StructureMetadata metadata)
		{
			Metadata = metadata;
			Layers = new List<Layer>();
		}

		public StructureFile() {
			Layers = new List<Layer>();
		}
	}

}
