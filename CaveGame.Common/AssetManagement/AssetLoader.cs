using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CaveGame.Common
{
	public class AssetLoader
	{
		private Texture2D FromStream(GraphicsDevice GraphicsProcessor, Stream DataStream)
		{

			Texture2D Asset = Texture2D.FromStream(GraphicsProcessor, DataStream);
			// Fix Alpha Premultiply
			Color[] data = new Color[Asset.Width * Asset.Height];
			Asset.GetData(data);
			for (int i = 0; i != data.Length; ++i)
				data[i] = Color.FromNonPremultiplied(data[i].ToVector4());
			Asset.SetData(data);
			return Asset;
		}

		public Texture2D LoadTexture(GraphicsDevice device, string filepath) => FromStream(device, TitleContainer.OpenStream(filepath));


	}
}
