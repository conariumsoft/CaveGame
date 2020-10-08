using CaveGame.Core.FileUtil;
using CaveGame.Core.Tiles;
using CaveGame.Core.Walls;
using System;
using System.Collections.Generic;
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
		public string Name { get; set; }
		public string Notes { get; set; }
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
		private string path => StructureFile.FolderRoot+ Structure.Metadata.Name + "/" + LayerID;

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
			Tiles = new CaveGame.Core.Tiles.Tile[Structure.Metadata.Width, Structure.Metadata.Height];
			Walls = new CaveGame.Core.Walls.Wall[Structure.Metadata.Width, Structure.Metadata.Height];
			Furniture = new List<Furniture>();
		}

		public Layer() {
			Visible = true;
		}

		private void CreateFolder()
		{
			Directory.CreateDirectory(path);
		}

		public void LoadTiles() {
			if (!File.Exists(path + "/tiles.l"))
				throw new Exception("Tile Layer does not exist!");


			byte[] TileData = File.ReadAllBytes(path+"/tiles.l");

			int index = 0;
			for (int x = 0; x < Structure.Metadata.Width; x++)
			{
				for (int y = 0; y < Structure.Metadata.Height; y++)
				{
					Tiles[x,y] = Tile.FromID(TileData[index]);
					index++;
				}
			}

		}

		public void LoadWalls()
		{
			if (!File.Exists(path + "/walls.l"))
				throw new Exception("Tile Layer does not exist! " + path);


			byte[] WallData = File.ReadAllBytes(path + "/walls.l");

			int index = 0;
			for (int x = 0; x < Structure.Metadata.Width; x++)
			{
				for (int y = 0; y < Structure.Metadata.Height; y++)
				{
					Walls[x,y] = Wall.FromID(WallData[index]);
					index++;
				}
			}
		}

		public void SaveTiles()
		{
			CreateFolder();

			byte[] TileData = new byte[Structure.Metadata.Width * Structure.Metadata.Height];
				
				

			int index = 0;
			for (int x = 0; x < Structure.Metadata.Width; x++)
			{
				for (int y = 0; y < Structure.Metadata.Height; y++)
				{
					TileData[index] = Tiles[x,y].ID;
					index++;
				}
			}

			File.WriteAllBytes(path + "/tiles.l", TileData);
		}

		public void SaveWalls()
		{
			CreateFolder();

			byte[] WallData = new byte[Structure.Metadata.Width * Structure.Metadata.Height];

			int index = 0;
			for (int x = 0; x < Structure.Metadata.Width; x++)
			{
				for (int y = 0; y < Structure.Metadata.Height; y++)
				{
					WallData[index] = Walls[x,y].ID;
					index++;
				}
			}

			File.WriteAllBytes(path + "/walls.l", WallData);
		}
	}

	[Serializable]
	public class StructureFile : Configuration
	{
		[XmlIgnore]
		public static string FolderRoot = "ExtractedStructures/";

		public StructureMetadata Metadata { get; set; }

		[XmlArray("Structure")]
		[XmlArrayItem("Layer")]
		public List<Layer> Layers;

		[XmlInclude(typeof(StructureFile))]
		public override void Save()
		{
			if (!Directory.Exists(FolderRoot))
				Directory.CreateDirectory(FolderRoot);


			if (!Directory.Exists(FolderRoot+Metadata.Name))
				Directory.CreateDirectory(FolderRoot+Metadata.Name);

			XmlSerializer writer = new XmlSerializer(typeof(StructureFile));

			using (var stream = File.Open(FolderRoot+Metadata.Name + "/structure.xml", FileMode.OpenOrCreate, FileAccess.Write))
				writer.Serialize(stream, this);


			foreach(Layer layer in Layers)
			{
				layer.SaveTiles();
				layer.SaveWalls();
			}

			Directory.CreateDirectory("Structures");

			if (File.Exists("Structures/" + Metadata.Name + ".structure"))
				File.Delete("Structures/" + Metadata.Name + ".structure");
			ZipFile.CreateFromDirectory(FolderRoot + Metadata.Name, "Structures/" + Metadata.Name + ".structure");
		}


		public static StructureFile LoadStructure(string filepath)
		{

			if (!File.Exists("Structures/" + filepath + ".structure"))
				throw new Exception();

			ZipFile.ExtractToDirectory("Structures/" + filepath + ".structure", FolderRoot + filepath);

			XmlSerializer reader = new XmlSerializer(typeof(StructureFile));
			StructureFile structure;

			if (!Directory.Exists(filepath))
				throw new Exception("Structure folder not found!");

			if (!File.Exists(FolderRoot+filepath + "/structure.xml"))
				throw new Exception("Corrupted structure file: 'structue.xml' is missing");

			using (var stream = File.Open(FolderRoot+filepath + "/structure.xml", FileMode.Open, FileAccess.Read))
				structure = (StructureFile)reader.Deserialize(stream);


			foreach (Layer layer in structure.Layers)
			{
				layer.Structure = structure;
				layer.LoadTiles();
				layer.LoadWalls();
			}

			return structure;				
		}



		public StructureFile(StructureMetadata metadata)
		{
			Metadata = metadata;
			Layers = new List<Layer>();
		}

		public StructureFile() { }
	}

}
