#if CLIENT
using CaveGame.Client;
#endif
using CaveGame.Core.Entities;
using CaveGame.Core.Furniture;
using CaveGame.Core.Network;
using CaveGame.Core.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Inventory
{
	public enum ItemID : short
	{
		CopperIngot, LeadIngot, TinIngot, ChromiumIngot, AluminiumIngot,
		IronIngot, NickelIngot, GoldIngot, TileItem, WallItem, 
	}

	public interface IItem
	{
		short ID { get; }
		int MaxStack { get; }
		string Name { get; }
#if CLIENT
		void Draw(SpriteBatch sb, Vector2 position, float scale);
#endif
	}

	

	public class Item : IItem
	{
		public short ID
		{
			get
			{
				var name = this.GetType().Name;
				return (short)Enum.Parse(typeof(ItemID), name);
			}
		}

		public virtual int MaxStack => 99;
		public virtual string Name => this.GetType().Name;
		public virtual void Draw(SpriteBatch sb, Vector2 position, float scale) { }
		public virtual void OnClientLMBDown(LocalPlayer player, IGameClient client) { }
		public virtual void OnClientLMBUp(LocalPlayer player, IGameClient client) {}
		public virtual void OnClientLMBHeld(LocalPlayer player, IGameClient client) { }
		public virtual void OnClientSelected(LocalPlayer player, IGameClient client) { }
		public virtual void OnServerUse(Player player, IGameWorld world) { }

	}

	public interface IGameClient
	{
		Camera2D Camera { get; }
		void Send(Packet p);
		IGameWorld World { get; }
	}

	public class TileRemoverItem : Item
	{
		public override void OnClientLMBHeld(LocalPlayer player, IGameClient client)
		{
			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			Point pos = new Point(
				(int)Math.Floor(mp.X / Globals.TileSize),
				(int)Math.Floor(mp.Y / Globals.TileSize)
			);
			int x = pos.X;
			int y = pos.Y;
			if (client.World.GetTile(x, y).ID != 0)
			{
				client.Send(new PlaceTilePacket(0, 0, 0, x, y));
				client.World.SetTile(x, y, new Air());
			}
		}
		public override void Draw(SpriteBatch sb, Vector2 position, float scale)
		{
#if CLIENT

			sb.Draw(ItemTextures.BigPickaxe, position, null, Color.Red, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
#endif
		}
	}

	public class WallRemoverItem : Item
	{
		public override void OnClientLMBHeld(LocalPlayer player, IGameClient client)
		{
			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			Point pos = new Point(
				(int)Math.Floor(mp.X / Globals.TileSize),
				(int)Math.Floor(mp.Y / Globals.TileSize)
			);
			int x = pos.X;
			int y = pos.Y;
			if (client.World.GetWall(x, y).ID != 0)
			{
				client.Send(new PlaceWallPacket(0, 0, 0, x, y));
				client.World.SetWall(x, y, new Walls.Air());
			}
		}
		public override void Draw(SpriteBatch sb, Vector2 position, float scale)
		{
#if CLIENT

			sb.Draw(ItemTextures.WallScraper, position, null, Color.Red, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
#endif
		}
	}

	public class WallItem : Item
	{
		public override int MaxStack => 999;

		public Walls.Wall Wall;

		public WallItem(Walls.Wall wall)
		{
			Wall = wall;
		}
		public override void OnClientLMBHeld(LocalPlayer player, IGameClient client)
		{
			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			Point pos = new Point(
				(int)Math.Floor(mp.X / Globals.TileSize),
				(int)Math.Floor(mp.Y / Globals.TileSize)
			);
			int x = pos.X;
			int y = pos.Y;
			if (client.World.GetTile(x, y).ID != Wall.ID)
			{
				client.Send(new PlaceWallPacket(Wall.ID, 0, 0, x, y));
				client.World.SetWall(x, y, Walls.Wall.FromID(Wall.ID));
			}
		}

		public override void Draw(SpriteBatch sb, Vector2 position, float scale)
		{
			Texture2D tex = null;
#if CLIENT
			tex = GameTextures.TileSheet;
#endif
			sb.Draw(tex, position, Wall.Quad, Wall.Color, 0, Vector2.Zero, scale * 2, SpriteEffects.None, 0);
		}

	}

	public class TileItem : Item
	{
		public override int MaxStack => 999;

		public Tile Tile;

		public TileItem(Tile tile)
		{
			Tile = tile;
		}
		public override void OnClientLMBHeld(LocalPlayer player, IGameClient client)
		{
			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			Point pos = new Point(
				(int)Math.Floor(mp.X / Globals.TileSize),
				(int)Math.Floor(mp.Y / Globals.TileSize)
			);
			int x = pos.X;
			int y = pos.Y;
			if (!client.World.IsCellOccupied(x, y))
			{
				client.Send(new PlaceTilePacket(Tile.ID, 0, 0, x, y));
				client.World.SetTile(x, y, Tile.FromID(Tile.ID));
			}
		}

		

		public override void Draw(SpriteBatch sb, Vector2 position, float scale)
		{
			Texture2D tex = null;
#if CLIENT
			tex = GameTextures.TileSheet;
#endif
			sb.Draw(tex, position, Tile.Quad, Tile.Color, 0, Vector2.Zero, scale*2, SpriteEffects.None, 0);
		}
	}


	public class BombItem : Item
	{
		public override int MaxStack => 99;


		public override void OnClientLMBDown(LocalPlayer player, IGameClient client)
		{
			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			var unitVec = (mp-player.Position);
			unitVec.Normalize();

			client.Send(new PlayerThrowItemPacket(ThrownItem.Bomb, Rotation.FromUnitVector(unitVec)));
		}



#if CLIENT
		public override void Draw(SpriteBatch sb, Vector2 position, float scale)
		{
			sb.Draw(ItemTextures.Bomb, position, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
		}
#endif
	}



	// items
	public abstract class Ingot : Item
	{
		public override int MaxStack => 99;

		public virtual Color Color { get; }

#if CLIENT
		public override void Draw(SpriteBatch sb, Vector2 position, float scale)
		{
			sb.Draw(ItemTextures.Ingot, position, null, Color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
		}
#endif
	}

	public class CopperIngot: Ingot {
		public override Color Color => new Color(1.0f, 0.45f, 0.0f);
	}
	public class LeadIngot : Ingot
	{
		public override Color Color => new Color(0.3f, 0.3f, 0.45f);
	}
	public class TinIngot : Ingot
	{
		public override Color Color => new Color(0.7f, 0.4f, 0.4f);
	}
	public class ChromiumIngot : Ingot
	{
		public override Color Color => new Color(0.5f, 1.0f, 1.0f);
	}
	public class AluminiumIngot: Ingot {
		public override Color Color => new Color(0.9f, 0.9f, 0.9f);
	}
	public class IronIngot: Ingot {
		public override Color Color => new Color(1.0f, 0.8f, 0.8f);
	}
	public class NickelIngot: Ingot {
		public override Color Color => new Color(1.0f, 0.5f, 0.5f);
	}
	public class GoldIngot: Ingot {
		public override Color Color => new Color(1.0f, 1.0f, 0.5f);
	}

	// Warlock
	public class MilesHat { }
	public class MilesBeard { }
	public class MilesRobe { }

	// Witch
	public class NatalieHat { }
	public class NatalieRobe { }
	public class NatalieLeggings { }
	public class HeatRune { }
	public class ElectricRune { }
	public class Necronomicon { }
	public class ResurrectionTome { }
	public class Lean { }
	public class ProgrammerSocks { }

	public class OwlEyePotion { }
	public class MetalDetector { }
	public class EmptyBook { }
	public class Paper { }
	public class Sugar { }
	public class Sulphur { }
	public class ThermiteMix { }
	public class SoulManifold { }
	public class EmptyBottle { }
	public class Fedora { }
	public class TopHat { }
	public class ArmyHelmet { }
	public class NordicHelm { }


	public class PhilosopherStone { }
	public class PoisonVial { }

	public class ShoeSpikes { }
	public class Jetpack { }
	public class ExoskelHead { }
	public class ExoskelTorso { }
	public class ExoskelLegs { }

	public class GrassSeeds { }
	public class Bucket { }
	public class WaterBucket { }
	public class MagmaBucket { }
	public class SludgeBucket { }
	public class LiquidOxygenTank { }
	public class LiquidNitrogenTank { }
	public class VRHeadset { }

	public class Transistor { }

	public class M1911 { }
	public class Uzi { }
	public class Revolver { }
	public class PlasmaRifle { }
	public class Vulkan { }

	public class ZeusCannon { }
	public class SquirtGun { }
	public class ProtonPump { }
	public class FreezeRay { }
	public class Flamethrower { }
	public class Molotov { }
	public class Grenade { }
	public class Dynamite { }
	public class Bomb { }
	public class StickyBomb { }
	public class Sparkler { }

	public class GravityBoots { }
	public class Aqualung { }

	public class ManletPotion { }
	public class HellwalkerPotion { }
	public class GlowingPotion { }
	public class SpeedPotion { }
	public class HP20Potion { }
	public class HP50Potion { }


	public class Glue { }
	//public class Wire { }
	// Furniture
	public class PlasticPrinter { }
	public class Anvil { }
	public class Campfire { }
	public class Furnace { }
	public class ArrowTurret { }
	public class MGTurret { }
	public class LaserTurret { }
	public class Teleporter { }
	public class Workbench
	{
		internal static bool CanPlace(IGameWorld world, int x, int y)
		{
			throw new NotImplementedException();
		}
	}
	public class AlchemyLab { }
}