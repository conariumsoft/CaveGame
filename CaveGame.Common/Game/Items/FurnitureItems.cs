#if CLIENT
using CaveGame.Client;
#endif
using CaveGame.Common.Game.Entities;
using CaveGame.Common.Game.Furniture;
using CaveGame.Common.Network;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using CaveGame.Common.Extensions;
using CaveGame.Common.Game.Inventory;

namespace CaveGame.Common.Game.Items
{
	public abstract class FurnitureItem : Item
	{
		public override int MaxStack => 99;

		public void Draw(GraphicsEngine GFX, Texture2D ItemTexture, Vector2 position, float scale)
		{
			GFX.Sprite(ItemTexture, position, null, Color.White, Rotation.Zero, Vector2.Zero, scale * 2, SpriteEffects.None, 0);
		}
		public override void Draw(GraphicsEngine GFX, Vector2 position, float scale) { }
	}

	public class FurnaceItem : FurnitureItem
	{
		public override void Draw(GraphicsEngine GFX, Vector2 position, float scale) => Draw(GFX, GFX.Furnace, position, scale);
        public override void OnClientLMBHeld(Player player, IGameClient client, ItemStack stack, GameTime gt)
		{
			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());
			Point pos = mp.ToTileCoords();

			if (Furnace.CanPlace(client.World, pos.X, pos.Y))
			{
				stack.Quantity--;
				client.Send(new PlaceFurniturePacket((byte)FurnitureID.Furnace, 0, pos.X, pos.Y));
			}
		}
	}


	public class DoorItem : FurnitureItem
	{
		public override void Draw(GraphicsEngine GFX, Vector2 position, float scale) => Draw(GFX, GFX.Door, position, scale);
		public override void OnClientLMBHeld(Player player, IGameClient client, ItemStack stack, GameTime gt)
		{
			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());
			Point pos = mp.ToTileCoords();

			if (WoodenDoor.CanPlace(client.World, pos.X, pos.Y))
			{
				stack.Quantity--;
				client.Send(new PlaceFurniturePacket((byte)FurnitureID.WoodenDoor, 0, pos.X, pos.Y));
			}
		}


	}

	public class WorkbenchItem : FurnitureItem
	{
		public override void Draw(GraphicsEngine GFX, Vector2 position, float scale) => Draw(GFX, GFX.Workbench, position, scale);
		public override void OnClientLMBHeld(Player player, IGameClient client, ItemStack stack, GameTime gt)
		{
			MouseState mouse = Mouse.GetState();
			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());
			Point pos = new Point(
				(int)Math.Floor(mp.X / Globals.TileSize),
				(int)Math.Floor(mp.Y / Globals.TileSize)
			);
			if (Furniture.Workbench.CanPlace(client.World, pos.X, pos.Y))
			{
				stack.Quantity--;
				client.Send(new PlaceFurniturePacket((byte)FurnitureID.Workbench, 0, pos.X, pos.Y));
			}
		}

	}
}
