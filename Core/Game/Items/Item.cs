
using CaveGame.Core.Game.Entities;
using CaveGame.Core.Furniture;
using CaveGame.Core.Network;
using CaveGame.Core.Game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using CaveGame.Core.FileUtil;
using System.Linq;
using CaveGame.Core.Game.Walls;
using DataManagement;
using CaveGame.Core.Generic;
using CaveGame.Core.Inventory;

namespace CaveGame.Core.Game.Items

{
	using JsonString = String;
	public enum ItemID : short
	{
		CopperIngot, LeadIngot, TinIngot, ChromiumIngot, AluminiumIngot,
		IronIngot, NickelIngot, GoldIngot, TileItem, WallItem, 
	}

	public enum ItemTag
    {
		Armor, Potion
    }

	public interface IArmorItem
	{
		int Defense { get; }
	}

	public interface IItem
	{
		short ID { get; }
		int MaxStack { get; }
		string Name { get; }

		void Draw(GraphicsEngine GFX, Vector2 position, float scale);

	}

	public class Item : IItem
	{
		public virtual List<ItemTag> Tags => new List<ItemTag>();
		public short ID
		{
			get
			{
				var name = this.GetType().Name;
				return (short)Enum.Parse(typeof(ItemID), name);
			}
		}

		public virtual string Namespace => "cavegame";
		public virtual string[] Tooltip { get; }
		public virtual int MaxStack => 99;
		public virtual string Name => this.GetType().Name;
		public virtual string DisplayName => Name;

		public virtual Texture2D Texture { get; }

		public bool MouseDown { get; set; }
		public void Draw(GraphicsEngine GFX, Texture2D texture, Vector2 position, float scale)
		{
			GFX.Sprite(texture, position, null, Color.Gray, Rotation.Zero, Vector2.Zero, scale, SpriteEffects.None, 0);
		} // pseudo- default draw
		public virtual void Draw(GraphicsEngine GFX, Vector2 position, float scale) => Draw(GFX, Texture, position, scale);
		public virtual void OnClientLMBDown(Player player, IGameClient client, ItemStack stack)
        {
			MouseDown = true;
        }
		public virtual void OnClientLMBUp(Player player, IGameClient client, ItemStack stack)
        {
			MouseDown = false;
        }
		public virtual void OnClientLMBHeld(Player player, IGameClient client, ItemStack stack, GameTime gt) { }
		public virtual void OnClientSelected(Player player, IGameClient client) { }
		public virtual void OnServerUse(Player player, IGameWorld world) { }

		public virtual void OnClientDraw(GraphicsEngine GFX) {}

		public virtual Metabinary GetMetadataComplex()
        {
			var tag = new Metabinary { Name = "item" };
			var itemmods = new ComplexTag { Name = "attachments" };
			itemmods.AddInt("poison", 2);
			itemmods.AddInt("dulled", 41);
			tag.AddComplex(itemmods);
			tag.AddString("namespace", Namespace);
			tag.AddString("itemid", Name);

			
			return tag;
        }

		public static bool TryFromName(string name, out Item item)
		{
			item = null;
			if (name.StartsWith("TileItem"))
			{
				var tilename = name.Substring(9);
				item = new TileItem(Tile.FromName(tilename));
				return true;
			}
			if (name.StartsWith("WallItem"))
			{
				var wallname = name.Substring(9);
				item = new WallItem(Wall.FromName(wallname));
				return true;
			}

			var basetype = typeof(Item);
			var types = basetype.Assembly.GetTypes().Where(type => type.IsSubclassOf(basetype));


			foreach (var type in types)
			{
				if (name == type.Name)
                {
					item =  (Item)type.GetConstructor(Type.EmptyTypes).Invoke(null);
					return true;
				}
					
			}
			return false;
		}

		public static Item FromName(string name)
		{
			if (name.StartsWith("TileItem"))
            {
				var tilename = name.Substring(9);
				return new TileItem(Tile.FromName(tilename));
            }
			if (name.StartsWith("WallItem"))
            {
				var wallname = name.Substring(9);
				return new WallItem(Wall.FromName(wallname));
            }

			var basetype = typeof(Item);
			var types = basetype.Assembly.GetTypes().Where(type => type.IsSubclassOf(basetype));


			foreach (var type in types)
			{
				if (name == type.Name)
					return (Item)type.GetConstructor(Type.EmptyTypes).Invoke(null);
			}
			throw new Exception("ID not valid! "+name);
		}

		public static Item FromMetadataComplex(ComplexTag tag)
        {
			Item item = FromName(tag.GetValue<string>("itemid"));

			if (item is TileItem tileItem)
				tileItem.Tile = Tile.FromID(tag.GetValue<short>("tileitem_id"));
			if (item is WallItem wallItem)
				wallItem.Wall = Wall.FromID(tag.GetValue<short>("wallitem_id"));


			return item;
        }

	}


	public class Potion : Item
	{

		public override string[] Tooltip => new string[]{
			""
		};
		public override string DisplayName => base.DisplayName;
		public override Texture2D Texture => GraphicsEngine.Instance.Potion;
		public virtual void OnDrink(Player player)=> player.AddEffect(new StatusEffects.Burning(10));

		public override void OnClientLMBDown(Player player, IGameClient client, ItemStack stack)
		{
			base.OnClientLMBDown(player, client, stack); 
			stack.Quantity--;
			OnDrink(player);
		}
	}

	public class Speed : Potion
	{
		public override string DisplayName => "Speed Potion";
		public override void OnDrink(Player player) => player.AddEffect(new StatusEffects.AmphetamineRush(10));
	}

	public class GlowPotion : Potion
	{

	}

	public class GenericPickaxe : Item
	{
		public virtual float SwingTime => 0.15f;
		public virtual byte Strength => 2;
		public virtual Color Tint => Color.Gray;

		public virtual float Size => 1.0f;

		public float swingingTimer;


		public override void OnClientLMBHeld(Player player, IGameClient client, ItemStack stack, GameTime gt)
		{
			swingingTimer += gt.GetDelta();

			if (swingingTimer >= SwingTime)
            {
				swingingTimer = 0;
				MouseState mouse = Mouse.GetState();

				var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

				Point pos = new Point(
					(int)Math.Floor(mp.X / Globals.TileSize),
					(int)Math.Floor(mp.Y / Globals.TileSize)
				);
				int x = pos.X;
				int y = pos.Y;

				var tile = client.World.GetTile(x, y);
				if (tile is INonMinable)
					return;

				if (client.World.GetTile(x, y).ID != 0)
				{
					client.Send(new DamageTilePacket(new Point(x, y), Strength));
					//client.Send(new PlaceTilePacket(0, 0, 0, x, y));
					//client.World.SetTile(x, y, new Tiles.Air());
				}
			}
			base.OnClientLMBHeld(player, client, stack, gt);
		}
		public override void Draw(GraphicsEngine GFX, Vector2 position, float scale)
		{
			GFX.Sprite(GFX.PickaxeNew, position+new Vector2(8*scale, 8*scale), null, Tint, Rotation.Zero, new Vector2(8, 8), scale* Size, SpriteEffects.None, 0);
		}

	}

	public class CopperPickaxe : GenericPickaxe
    {
        public override string DisplayName => "Copper Pickaxe";
        public override Color Tint => new Color(1.0f, 0.7f, 0.2f);
        public override float SwingTime => 0.2f;
        public override byte Strength => 2;
        public override float Size => 0.8f;
    }

	public class LeadPickaxe : GenericPickaxe
	{
		public override string DisplayName => "Lead Pickaxe";
		public override Color Tint => new Color(0.3f, 0.3f, 0.4f);
		public override float SwingTime => 0.4f;
		public override byte Strength => 12;
		public override float Size => 1.25f;
	}

	public class IronPickaxe : GenericPickaxe
	{
		public override string DisplayName => "Iron Pickaxe";
		public override Color Tint => new Color(0.75f, 0.5f, 0.4f);
		public override float SwingTime => 0.3f;
		public override byte Strength => 6;
	}


	// TODO: Make walls break
	public class GenericWallScraper : Item
	{
		public override void OnClientLMBHeld(Player player, IGameClient client, ItemStack stack, GameTime gt)
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
				client.World.SetWall(x, y, new Game.Walls.Air());
			}
            base.OnClientLMBHeld(player, client, stack, gt);
		}
		public override void Draw(GraphicsEngine GFX, Vector2 position, float scale) => Draw(GFX, GFX.WallScraper, position, scale);
	}

	public class WallItem : Item
	{
		public override int MaxStack => 999;

		public override string DisplayName => Wall.WallName + " Wall";
		public override string Name => "WallItem:" + Wall.WallName;
		public Wall Wall;


		public WallItem() : base() { }

		public WallItem(Wall wall)
		{
			Wall = wall;
		}
		public override void OnClientLMBHeld(Player player, IGameClient client, ItemStack stack, GameTime gt)
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
				// TODO: Play sound
				stack.Quantity--;
				client.Send(new PlaceWallPacket(Wall.ID, 0, 0, x, y));
				client.World.SetWall(x, y, Game.Walls.Wall.FromID(Wall.ID));
			}
            base.OnClientLMBHeld(player, client, stack, gt);
		}

		public override Metabinary GetMetadataComplex()
		{
			Metabinary fatass = base.GetMetadataComplex();
			fatass.AddShort("wallitem_id", Wall.ID);
			return fatass;
		}

		public override void Draw(GraphicsEngine GFX, Vector2 position, float scale)
		{
			GFX.Sprite(GFX.TileSheet, position + (new Vector2(1.5f, 1.5f) * scale * 1.5f), Wall.Quad, Wall.Color, Rotation.Zero, Vector2.Zero, scale * 1.5f, SpriteEffects.None, 0);
		}

	}

	public class TileItem : Item
	{
		public override int MaxStack => 999;
		public override string DisplayName => Tile.TileName + " Block";

		public override string Name => "TileItem:" + Tile.TileName;

		public Tile Tile;


		public TileItem() : base() { }

		public TileItem(Tile tile)
		{
			Tile = tile;
		}

		public static TileItem Of<T>() where T: Tile, new()
		{
			return new TileItem { Tile = new T() };
		}
		public static TileItem Of<T>(T Tobj) where T : Tile, new()
		{
			return new TileItem { Tile = Tobj };
		}
		public override void OnClientLMBHeld(Player player, IGameClient client, ItemStack stack, GameTime gt)
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
				if (client.World.GetTile(x, y).ID != Tile.ID)
                {
					stack.Quantity--;

					client.Send(new PlaceTilePacket(Tile.ID, 0, 0, x, y));
					client.World.SetTile(x, y, Tile.FromID(Tile.ID));
				}
            }
            base.OnClientLMBHeld(player, client, stack, gt);
		}

        public override Metabinary GetMetadataComplex()
        {
			Metabinary fatass = base.GetMetadataComplex();
			fatass.AddShort("tileitem_id", Tile.ID);
			return fatass;
        }

        public override void Draw(GraphicsEngine GFX, Vector2 position, float scale)
		{
			GFX.Sprite(GFX.TileSheet, position+(new Vector2(1.5f, 1.5f)*scale*1.5f), Tile.Quad, Tile.Color, Rotation.Zero, Vector2.Zero, scale*1.5f, SpriteEffects.None, 0);
		}
	}

	public class BowItem : Item {
        public override int MaxStack => 1;
        public override void OnClientLMBDown(Player player, IGameClient client, ItemStack stack)
        {
			// TODO: Find and comsume arrows in inventory
			bool hasArrows = true;


			if (!hasArrows)
				return;

			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			var unitLookVector = player.Position.LookAt(mp);

			client.Send(new PlayerThrowItemPacket(ThrownItem.Arrow, Rotation.FromUnitVector(unitLookVector)));

            base.OnClientLMBDown(player, client, stack);
        }
        public override void Draw(GraphicsEngine GFX, Vector2 position, float scale) => Draw(GFX, GFX.BowSprite, position, scale);
	}

	public class BombItem : Item
	{
		public override int MaxStack => 99;
		public override void OnClientLMBDown(Player player, IGameClient client, ItemStack stack)
		{
			stack.Quantity--;

			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());

			var unitVec = (mp-player.Position);
			unitVec.Normalize();

			client.Send(new PlayerThrowItemPacket(ThrownItem.Bomb, Rotation.FromUnitVector(unitVec)));
            base.OnClientLMBDown(player, client, stack);
		}

		public override void Draw(GraphicsEngine GFX, Vector2 position, float scale) => Draw(GFX, GFX.BombSprite, position, scale);
	}

	

	public class SplatterItem : Item
    {
		public override void Draw(GraphicsEngine GFX, Vector2 position, float scale) => Draw(GFX, GFX.Bong, position, scale);
		public override int MaxStack => 99;

		float gelta;

        public override void OnClientLMBHeld(Player player, IGameClient client, ItemStack stack, GameTime gt)
        {
			gelta += gt.GetDelta();

			if (gelta < 0.05)
				return;

			gelta = 0;

			MouseState mouse = Mouse.GetState();

			var mp = client.Camera.ScreenToWorldCoordinates(mouse.Position.ToVector2());


			var unitVec = (mp - player.Position);
			unitVec.Normalize();

			var result = client.World.TileRaycast(player.Position, Rotation.FromUnitVector(unitVec));

			if (result.Hit)
			{
				Vector2 rounded = result.Intersection.ToPoint().ToVector2();


				client.World.ParticleSystem.Add(new TileBloodSplatterParticle { Position = rounded, Face = result.Face});
			}
		}
    }

	// items
	public abstract class Ingot : Item
	{
		public override int MaxStack => 99;

		public virtual Color Color { get; }

		public override void Draw(GraphicsEngine GFX, Vector2 position, float scale)
		{
			GFX.Sprite(GFX.Ingot, position, null, Color, Rotation.Zero, Vector2.Zero, scale, SpriteEffects.None, 0);
		}
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