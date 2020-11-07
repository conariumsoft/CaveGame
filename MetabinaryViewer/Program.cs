using CaveGame.Core;
using CaveGame.Core.FileUtil;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace MetabinaryViewer
{
	class Program {
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{

				//	return;
			}
			//byte[] data = File.ReadAllBytes(args[0]);
			MetabinaryFile loadedFile = new MetabinaryFile { Name = "networked_item_concept" };//MetabinaryFile.Deserialize(data);
			loadedFile.AddShort("item_id", 34);
			loadedFile.AddByte("state", 34);
			loadedFile.AddShort("amount", 999);
			loadedFile.AddShort("durability", 4219);
			loadedFile.AddString("creator",  "jms");
			loadedFile.AddString("custom_name",  "Prized Golden Vape");
			var thig = new ComplexTag { Name = "spells" };
			thig.AddInt("MidasTouch", 12);
			thig.AddInt("ShadowCasting", 6969 );
			thig.AddInt("StealHealth", 222);

			loadedFile.AddComplex(thig);

			MetabinaryFile.Debug(loadedFile, 0);

			var bytedata = loadedFile.Serialize();
			Console.WriteLine("networkdata: {0} bytes", bytedata.Length);
			Console.WriteLine(bytedata.DumpHex());
		}
	}
}
