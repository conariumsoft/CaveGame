using CaveGame.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Client.DebugTools
{
    public class ChunkGridLineRenderer
    {
		public void Draw(GraphicsEngine gfx, Camera2D Camera)
		{
			int camChunkX = (int)Math.Floor(Camera.ScreenCenterToWorldSpace.X / (Globals.ChunkSize * Globals.TileSize));
			int camChunkY = (int)Math.Floor(Camera.ScreenCenterToWorldSpace.Y / (Globals.ChunkSize * Globals.TileSize));

			int rendr = 4;

			for (int x = -rendr; x <= rendr; x++)
			{
				gfx.Line(Color.White, new Vector2(
					(camChunkX + x) * (Globals.ChunkSize * Globals.TileSize),
					(camChunkY - rendr) * (Globals.ChunkSize * Globals.TileSize)
				), new Vector2(
					(camChunkX + x) * (Globals.ChunkSize * Globals.TileSize),
					(camChunkY + rendr) * (Globals.ChunkSize * Globals.TileSize)
				), 0.5f);
			}

			for (int y = -rendr; y <= rendr; y++)
			{
				gfx.Line(Color.White, new Vector2(
					(camChunkX - rendr) * (Globals.ChunkSize * Globals.TileSize),
					(camChunkY + y) * (Globals.ChunkSize * Globals.TileSize)
				), new Vector2(
					(camChunkX + rendr) * (Globals.ChunkSize * Globals.TileSize),
					(camChunkY + y) * (Globals.ChunkSize * Globals.TileSize)
				), 0.5f);
			}

			for (int x = -rendr; x <= rendr; x++)
			{
				for (int y = -rendr; y <= rendr; y++)
				{
					var pos = new Vector2(camChunkX + x, camChunkY + y) * (Globals.ChunkSize * Globals.TileSize);
					gfx.Text(gfx.Fonts.Arial8, (camChunkX + x) + ", " + (camChunkY + y), pos, new Color(1, 1, 1, 0.5f));
				}
			}
		}
	}
}
