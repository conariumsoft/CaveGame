using System;
using System.IO;
using System.Xml.Serialization;

namespace CaveGame.Core.FileUtil
{
	[Serializable]
	public abstract class Configuration 
	{
		[XmlIgnore]
		public string filePath;

		public virtual void FillDefaults() { }


		[XmlInclude(typeof(Configuration))]
		public virtual void Save() {

			using (var writer = new System.IO.StreamWriter(filePath))
			{
				var serializer = new XmlSerializer(this.GetType());
				serializer.Serialize(writer, this);
				writer.Flush();
			}
		}

		public static T Load<T>(string filepath, bool savedefaults = false) where T : Configuration, new()  {
			
			XmlSerializer ser = new XmlSerializer(typeof(T));

			T config;

			if (File.Exists(filepath))
			{
				using (var stream = File.Open(filepath, FileMode.Open, FileAccess.Read))
					config = (T)ser.Deserialize(stream);
			} else
			{
				config = new T();
				config.FillDefaults();
				if (savedefaults)
				{
					config.filePath = filepath;
					config.Save();
				}
			}
			config.filePath = filepath;

			return config;
		}
	}
}
