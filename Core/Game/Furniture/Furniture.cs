using CaveGame.Core.Game.Entities;
using CaveGame.Core.Inventory;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CaveGame.Core.Furniture
{
	public enum FurnitureID : byte
	{
		Workbench, WoodenDoor, TrapDoor, Furnace, Anvil, OakBookshelf, OakBed, Chest
	}
	public interface ILayerOccupant
	{
		byte Damage { get; set; }
		byte Opacity { get; }
		byte Hardness { get; }
		Rectangle Quad { get; }
		string Namespace { get; }
	}
	public interface IFurniture
	{

	}


	public abstract class FurnitureTile
	{

		public int FurnitureNetworkID { get; set; }
		public Point Position { get; set; } // Top Left Tile Corner

		public virtual Point TopLeft => Position;

		public abstract Vector2 BoundingBox { get; }
		public abstract Color Color { get; }
		public abstract Rectangle Quad { get; }
		public abstract FurnitureID ID { get; }
		public abstract Point OccupationBox { get; }

		public FurnitureTile() {
		}

		public static FurnitureTile FromID(byte b)
		{
			var basetype = typeof(FurnitureTile);
			var types = basetype.Assembly.GetTypes().Where(type => type.IsSubclassOf(basetype));

			foreach (var type in types)
			{
				bool exists = Enum.TryParse(type.Name, out FurnitureID id);
				if (exists && id == (FurnitureID)b)
					return (FurnitureTile)type.GetConstructor(Type.EmptyTypes).Invoke(null);
			}
			throw new Exception("ID not valid!");
		}

		

		public virtual void OnCollide(){}
		public virtual void OnPlayerInteracts(Player player, IGameWorld world, IGameClient client) { }
		public virtual void Draw(Texture2D tilesheet, SpriteBatch sb)
		{
			sb.Draw(tilesheet, Position.ToVector2()*Globals.TileSize, Quad, Color);
		}
		public virtual void OnTileUpdate(IGameWorld world, int x, int y) { }
	}

	public class Furnace : FurnitureTile
	{
		public override Vector2 BoundingBox => new Vector2(16, 16);

		public override Color Color => Color.White;

		public override Rectangle Quad => new Rectangle(56, 88, 16, 16);

		public override FurnitureID ID => FurnitureID.Furnace;

		public override Point OccupationBox => new Point(2, 2);


		public static bool CanPlace(IGameWorld world, int x, int y)
		{
			if (world.IsCellOccupied(x, y))
				return false;
			if (world.IsCellOccupied(x + 1, y))
				return false;
			if (world.IsCellOccupied(x, y+1))
				return false;
			if (world.IsCellOccupied(x + 1, y+1))
				return false;
			if ((world.GetTile(x, y + 2) is INonSolid))
				return false;
			if ((world.GetTile(x + 1, y + 2) is INonSolid))
				return false;

			return true;
		}
	}

	public class Chest : FurnitureTile
	{
		public override Vector2 BoundingBox => new Vector2(16, 16);

		public override Color Color => Color.White;

		public override Rectangle Quad => new Rectangle(112, 96, 16, 16);

		public override FurnitureID ID => FurnitureID.Chest;

		public override Point OccupationBox => new Point(2, 2);

		public static bool CanPlace(IGameWorld world, int x, int y)
		{
			if (world.IsCellOccupied(x, y))
				return false;
			if (world.IsCellOccupied(x + 1, y))
				return false;
			if (world.IsCellOccupied(x, y + 1))
				return false;
			if (world.IsCellOccupied(x + 1, y + 1))
				return false;
			if ((world.GetTile(x, y + 2) is INonSolid))
				return false;
			if ((world.GetTile(x + 1, y + 2) is INonSolid))
				return false;

			return true;
		}
	}
	public class Bookshelf : FurnitureTile
	{
		public override Vector2 BoundingBox => new Vector2(16, 24);

		public override Color Color => Color.White;

		public override Rectangle Quad => new Rectangle(96, 104, 16, 24);

		public override FurnitureID ID => FurnitureID.OakBookshelf;

		public override Point OccupationBox => new Point(2, 3);

		public static bool CanPlace(IGameWorld world, int x, int y)
		{
			if (world.IsCellOccupied(x, y))
				return false;
			if (world.IsCellOccupied(x + 1, y))
				return false;
			if (world.IsCellOccupied(x, y + 1))
				return false;
			if (world.IsCellOccupied(x + 1, y + 1))
				return false;
			if (world.IsCellOccupied(x, y + 2))
				return false;
			if (world.IsCellOccupied(x + 1, y + 2))
				return false;
			if ((world.GetTile(x, y + 3) is INonSolid))
				return false;
			if ((world.GetTile(x + 1, y + 3) is INonSolid))
				return false;

			return true;
		}

	}

	public class PaintingFurniture : FurnitureTile
	{
		public override Vector2 BoundingBox { get; }
		public override Color Color { get; }
		public override Rectangle Quad { get; }
		public override FurnitureID ID { get; }
		public override Point OccupationBox { get; }
	}

	public class ForestPainting : PaintingFurniture
	{

	}
	public class Campfire : FurnitureTile
	{
		public override Vector2 BoundingBox => throw new NotImplementedException();

		public override Color Color => Color.White;

		public override Rectangle Quad => throw new NotImplementedException();

		public override FurnitureID ID => throw new NotImplementedException();

		public override Point OccupationBox => throw new NotImplementedException();
	}

	public class Workbench : FurnitureTile
	{

		public override FurnitureID ID => FurnitureID.Workbench;
		public override Vector2 BoundingBox => new Vector2(8, 0);
		public override Color Color => TileDefinitions.OakPlank.Color;
		public override Rectangle Quad => new Rectangle(56, 104, 16, 8);

		public override Point OccupationBox => new Point(2, 1);

		public static bool CanPlace(IGameWorld world, int x, int y)
		{
			if (world.IsCellOccupied(x, y))
				return false;
			if (world.IsCellOccupied(x+1, y))
				return false;
			if ((world.GetTile(x, y +1) is INonSolid))
				return false;
			if ((world.GetTile(x+1, y +1) is INonSolid))
				return false;

			return true;
		}

		public override void OnTileUpdate(IGameWorld world, int x, int y) {
			if (world.IsTile<INonSolid>(Position.X, Position.Y+1))
			{
				world.RemoveFurniture(this);
				return;
			}
				

			if (world.IsTile<INonSolid>(Position.X+1, Position.Y + 1))
			{
				world.RemoveFurniture(this);
				return;
			}

		}
	}

	public enum DoorState : byte
	{
		Closed,
		OpenLeft,
		OpenRight
	}

	public class WoodenDoor : FurnitureTile
	{
		public override Point TopLeft
		{
			get
			{
				if (State == DoorState.OpenRight)
					return Position;
				if (State == DoorState.OpenLeft)
					return Position-new Point(1, 0);

				return Position;
			}
		} 
		public override Point OccupationBox {
			get {
				if (State == DoorState.OpenRight)
					return new Point(2, 3);
				if (State == DoorState.OpenLeft)
					return new Point(2, 3);

				return new Point(1, 3);
			}
		}
		public override FurnitureID ID => FurnitureID.WoodenDoor;

		public override Vector2 BoundingBox => new Vector2(3, 12);

		public override Color Color => TileDefinitions.OakPlank.Color;

		public override Rectangle Quad => OpenDoorQuad;

		public DoorState State { get; set; }

		protected static Rectangle OpenDoorQuad = new Rectangle(16, 104, 16, 24);
		protected static Rectangle ClosedDoorQuad = new Rectangle(32, 104, 8, 24);

		public WoodenDoor()
		{
			State = DoorState.Closed;
		}

		public override void Draw(Texture2D tilesheet, SpriteBatch sb)
		{
			var pos = Position.ToVector2() * Globals.TileSize;

			if (State == DoorState.Closed)
				sb.Draw(tilesheet, pos, ClosedDoorQuad, Color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
			if (State == DoorState.OpenLeft)
				sb.Draw(tilesheet, pos-new Vector2(8, 0), OpenDoorQuad, Color, 0, Vector2.Zero, 1, SpriteEffects.FlipHorizontally, 0);
			if (State == DoorState.OpenRight)
				sb.Draw(tilesheet, pos, OpenDoorQuad, Color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
		}

		public static bool CanPlace(IGameWorld world, int x, int y)
		{
			if (world.IsCellOccupied(x, y))
				return false;
			if (world.IsCellOccupied(x, y+1))
				return false;
			if (world.IsCellOccupied(x, y+2))
				return false;
			if ((world.GetTile(x, y - 1) is INonSolid))
				return false;
			if ((world.GetTile(x, y + 3) is INonSolid))
				return false;

			return true;
		}

		public override void OnPlayerInteracts(Core.Game.Entities.Player player, IGameWorld world, IGameClient client)
		{
			var mp = client.Camera.ScreenToWorldCoordinates(Mouse.GetState().Position.ToVector2());
			Point pos = new Point(
				(int)Math.Floor(mp.X / Globals.TileSize),
				(int)Math.Floor(mp.Y / Globals.TileSize)
			);

			if (State == DoorState.Closed)
			{
				if (player.Facing == HorizontalDirection.Left)
				{
					if (world.IsCellOccupied(pos.X - 1, pos.Y))
						return;
					if (world.IsCellOccupied(pos.X - 1, pos.Y + 1))
						return;
					if (world.IsCellOccupied(pos.X - 1, pos.Y + 2))
						return;

					State = DoorState.OpenLeft;
					client.Send(new Network.OpenDoorPacket(FurnitureNetworkID, player.Facing));
				} else
				{
					if (world.IsCellOccupied(pos.X + 1, pos.Y))
						return;
					if (world.IsCellOccupied(pos.X + 1, pos.Y + 1))
						return;
					if (world.IsCellOccupied(pos.X + 1, pos.Y + 2))
						return;

					State = DoorState.OpenRight;
					client.Send(new Network.OpenDoorPacket(FurnitureNetworkID, player.Facing));
				}
			}
			else
			{
				client.Send(new Network.CloseDoorPacket(FurnitureNetworkID));
				State = DoorState.Closed;
			}
		}
	}
}
