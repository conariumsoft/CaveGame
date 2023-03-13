using System;
using System.Collections.Generic;
using System.IO;
using CaveGame.Common.World;

namespace CaveGame.Client
{
    
    public class SavedWorldManager
    {
        private static void CreateDirectoryIfNull(string filename)
        {
            if (!System.IO.Directory.Exists(filename))
                System.IO.Directory.CreateDirectory(filename);
        }
        
        public static List<WorldMetadata> GetWorldsOnFile()
        {
            CreateDirectoryIfNull("Worlds");
            List<WorldMetadata> results = new List<WorldMetadata>();
            foreach (var directory in Directory.EnumerateDirectories("Worlds"))
            {
                Console.WriteLine(directory);

                var worldMetadataFilePath = Path.Combine(directory, @"WorldMetadata.xml");
                var worldMetadataFileExists = File.Exists(worldMetadataFilePath);
                if (worldMetadataFileExists)
                    results.Add(WorldMetadata.LoadWorldData(directory));
            }
            return results;
        }

        public static void DeleteSave(WorldMetadata meta)
        {
            // TODO: Secure?
            Directory.Delete(Path.Combine("Worlds", meta.Name), true);
        }

        public static int CalculateHash(string read)
        {
            int hashedValue = 3074457;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457;
            }
            return hashedValue;
        }
    }
}
