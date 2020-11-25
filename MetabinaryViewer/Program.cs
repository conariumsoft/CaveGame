
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using DataManagement;
using System.IO;

namespace MetabinaryViewer
{
    class Program
    {

        static void ExamineBinary(string binName)
        {
            byte[] data = File.ReadAllBytes(binName);
            Metabinary binary = Metabinary.Deserialize(data);
            Metabinary.DebugText(binary);
        }

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                ExamineBinary(args[0]);
                return;
                //	return;
            }
            //byte[] data = File.ReadAllBytes(args[0]);
            Metabinary loadedFile = new Metabinary { Name = "networked_item_concept" };//MetabinaryFile.Deserialize(data);
            loadedFile.AddShort("item_id", 34);
            loadedFile.AddByte("state", 34);
            loadedFile.AddShort("amount", 999);
            loadedFile.AddShort("durability", 4219);
            
            loadedFile.AddString("custom_name", "Prized Golden Vape");
            var thig = new ComplexTag { Name = "spells" };
            thig.AddInt("MidasTouch", 12);
            thig.AddInt("ShadowCasting", 6969);
            thig.AddInt("StealHealth", 222);

            loadedFile.AddComplex(thig);
            var thig2 = new ComplexTag { Name = "data2" };
            thig.AddInt("MidasTouch", 12);
            thig.AddInt("ShadowCasting", 6969);
            thig.AddInt("StealHealth", 222);

            loadedFile.AddComplex(thig);
            var thig3 = new ComplexTag { Name = "data3" };
            var t4 = new ComplexTag { Name = "subdata" };


            loadedFile.AddComplex(thig);
            //loadedFile.AddInt("FUCKER", 42069);
            loadedFile.AddString("WTF", "lolololol");
            loadedFile.AddString("creator", "jms");
            loadedFile.AddByte("jimjones", 45);
            loadedFile.AddByte("jimjones2", 45);
            loadedFile.AddByte("jimjones3", 45);
            loadedFile.AddByte("jonesy", 45);

            Metabinary.DebugText(loadedFile, 0);

            var bytedata = loadedFile.Serialize();
            Console.WriteLine("networkdata: {0} bytes", bytedata.Length);
            Console.WriteLine(bytedata.DumpHex());
            Metabinary.DebugText(Metabinary.Deserialize(bytedata));
            File.WriteAllBytes("bigboy.metabin", bytedata);
        }
    }
}
