#if CLIENT
using CaveGame.Client;
#endif
using CaveGame.Core.Game.Entities;
using CaveGame.Core.Furniture;
using CaveGame.Core.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Inventory
{
	public abstract class FurnitureItem : Item
	{
		public override int MaxStack => 99;
		public virtual Texture2D ItemTexture { get; }

		public override void Draw(SpriteBatch sb, Vector2 position, float scale)
		{
			sb.Draw(ItemTexture, position, null, Color.White, 0, Vector2.Zero, scale * 2, SpriteEffects.None, 0);
		}
	}

	public class FurnaceItem : FurnitureItem
	{
#if CLIENT
		public override Texture2D ItemTexture => ItemTextures.Furnace;
#endif
		public override void OnClientLMBHeld(Player player, IGameClient client)
		{
			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());
			Point pos = new Point(
				(int)Math.Floor(mp.X / Globals.TileSize),
				(int)Math.Floor(mp.Y / Globals.TileSize)
			);

			if (Furnace.CanPlace(client.World, pos.X, pos.Y))
			{
				client.Send(new PlaceFurniturePacket((byte)FurnitureID.Furnace, 0, pos.X, pos.Y));
			}
		}


	}


	public class DoorItem : FurnitureItem
	{
#if CLIENT
		public override Texture2D ItemTexture => ItemTextures.Door;
#endif
		public override void OnClientLMBHeld(Player player, IGameClient client)
		{
			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());
			Point pos = new Point(
				(int)Math.Floor(mp.X / Globals.TileSize),
				(int)Math.Floor(mp.Y / Globals.TileSize)
			);

			if (WoodenDoor.CanPlace(client.World, pos.X, pos.Y))
			{
				client.Send(new PlaceFurniturePacket((byte)FurnitureID.WoodenDoor, 0, pos.X, pos.Y));
			}
		}


	}

	public class WorkbenchItem : FurnitureItem
	{
#if CLIENT
		public override Texture2D ItemTexture => ItemTextures.Workbench;
#endif
		public override void OnClientLMBHeld(Player player, IGameClient client)
		{
			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());
			Point pos = new Point(
				(int)Math.Floor(mp.X / Globals.TileSize),
				(int)Math.Floor(mp.Y / Globals.TileSize)
			);

			if (Furniture.Workbench.CanPlace(client.World, pos.X, pos.Y))
			{
				client.Send(new PlaceFurniturePacket((byte)FurnitureID.Workbench, 0, pos.X, pos.Y));
			}
		}

	}
}
