using CaveGame.Core.Game.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Game.Tiles
{


	public abstract class Liquid : Tile, ILiquid, INonSolid, INonMinable
	{

		public abstract float Viscosity { get; }

		private bool EvaporationCheck(IGameWorld world, int x, int y)
		{
			if (TileState < 1 && RNG.NextDouble() > 0.1)
			{
				world.SetTileNoLight(x, y, new Air());
				return true;
			}
			return false;
		}

		protected virtual void FlowBelow<TLiquid>(IGameWorld world, int x, int y) where TLiquid : Liquid, new()
		{
			var below = world.GetTile(x, y + 1);

			if (below is IWaterBreakable s)
			{

				world.SetTileNoLight(x, y + 1, new TLiquid { TileState = this.TileState });
				world.SetTileNoLight(x, y, new Air());
				TileState = 0;
			}

			if (below is TLiquid wbelow)
			{
				var most = 8 - below.TileState;

				byte taken = (byte)Math.Min(most, TileState);
				TileState -= taken;
				below.TileState += taken;
				world.DoUpdatePropogation(x, y);
				world.DoUpdatePropogation(x, y + 1);
			}
		}

		public void LiquidUpdate<TLiquid>(IGameWorld world, int x, int y) where TLiquid : Liquid, new()
		{
			EvaporationCheck(world, x, y);

			FlowBelow<TLiquid>(world, x, y);


			var left = world.GetTile(x - 1, y);
			var right = world.GetTile(x + 1, y);

			if (left is IWaterBreakable && right is IWaterBreakable && TileState >= 2)
			{
				if (TileState % 2 == 0)
				{
					world.SetTileNoLight(x - 1, y, new TLiquid { TileState = 1 });
					world.SetTileNoLight(x + 1, y, new TLiquid { TileState = 1 });
					TileState -= 2;
				}
			}
			else if (left is IWaterBreakable && TileState > 0)
			{


				world.SetTileNoLight(x - 1, y, new TLiquid { TileState = 1 });
				world.DoUpdatePropogation(x, y);
				TileState--;
			}
			else if (right is IWaterBreakable && TileState > 0)
			{

				world.SetTileNoLight(x + 1, y, new TLiquid { TileState = 1 });
				world.DoUpdatePropogation(x, y);
				TileState--;

			}

			if (left is TLiquid wleft && TileState > wleft.TileState)
			{
				if (TileState > wleft.TileState + 1)
				{
					TileState--;
					wleft.TileState++;
				}
				else if (TileState - wleft.TileState == 1)
				{
					if (RNG.NextDouble() > 0.5)

						wleft.TileState = TileState;
					else
						TileState = wleft.TileState;
				}
				if (TileState > wleft.TileState + 1)
				{
					TileState--;
					wleft.TileState++;
				}
				else if (TileState - wleft.TileState == 1)
				{
					if (RNG.NextDouble() > 0.5)

						wleft.TileState = TileState;
					else
						TileState = wleft.TileState;
				}
				world.DoUpdatePropogation(x, y);
				world.DoUpdatePropogation(x - 1, y);

				//world.SetTileNetworkUpdated(x, y);
				//world.SetTileNetworkUpdated(x - 1, y);
			}

			if (right is TLiquid wright && TileState > wright.TileState)
			{
				if (TileState > wright.TileState + 1)
				{
					TileState--;
					wright.TileState++;
				}
				else if (TileState - wright.TileState == 1)
				{
					if (RNG.NextDouble() > 0.5)

						wright.TileState = TileState;
					else
						TileState = wright.TileState;
				}
				if (TileState > wright.TileState + 1)
				{
					TileState--;
					wright.TileState++;
				}
				else if (TileState - wright.TileState == 1)
				{
					if (RNG.NextDouble() > 0.5)

						wright.TileState = TileState;
					else
						TileState = wright.TileState;
				}
				world.DoUpdatePropogation(x, y);
				world.DoUpdatePropogation(x + 1, y);
				//world.SetTileNetworkUpdated(x, y);
				//world.SetTileNetworkUpdated(x + 1, y);
			}
			EvaporationCheck(world, x, y);
		}
	}

	public class Lava : Liquid, ITileUpdate, ILocalTileUpdate, ILightEmitter
	{
		public override byte Opacity => 3;
		public override Color Color => Color.Red;
		public override void Drop(IGameServer server, IGameWorld world, Point tilePosition) { }

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 light)
		{
			var rect = new Rectangle(0, 15 * Globals.TileSize, Globals.TileSize, TileState);
			sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, (y * Globals.TileSize) + (8 - TileState)), rect, light.MultiplyAgainst(Color.Red));
		}

		public override float Viscosity => 1.06f;
		public Lava() { TileState = 8; }

		public Light3 Light => new Light3(32, 4, 4);

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			//LiquidUpdate<Lava>(world, x, y);
		}

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			var below = world.GetTile(x, y + 1);

			if (below is Water _)
			{
				world.SetTile(x, y + 1, new Obsidian());
				TileState--;
			}
			var left = world.GetTile(x - 1, y);

			if (left is Water _)
			{
				world.SetTile(x - 1, y, new Obsidian());
				TileState--;
			}
			var right = world.GetTile(x + 1, y);

			if (right is Water _)
			{
				world.SetTile(x + 1, y, new Obsidian());
				TileState--;
			}
			var top = world.GetTile(x, y - 1);

			if (top is Water _)
			{
				world.SetTile(x, y - 1, new Obsidian());
				TileState--;
			}

			LiquidUpdate<Lava>(world, x, y);
		}
	}

	public class Water : Liquid, ILiquid, ITileUpdate, ILocalTileUpdate, INonMinable
	{
		public override byte Opacity => 2;
		public override Color Color => Color.Blue;
		public override float Viscosity => 1.04f;
		public Water() { TileState = 8; }

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 light)
		{

			float brug = Math.Max(0.5f, (1 - (light.Blue / 16.0f)) * 2.0f);

			var rect = new Rectangle(0, 15 * Globals.TileSize, Globals.TileSize, TileState);
			sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, (y * Globals.TileSize) + (8 - TileState)), rect, light.MultiplyAgainst(Color.Blue) * brug);
		}

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			//LiquidUpdate<Water>(world, x, y);
		}

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			LiquidUpdate<Water>(world, x, y);
		}
	}
	public class Sludge : Liquid, ILightEmitter, ILocalTileUpdate
	{
		public override Color Color => Color.Green;
		public override float Viscosity => 2.0f;
		public Sludge() { TileState = 8; }
		public Light3 Light => new Light3(14, 32, 8);

		public override void Draw(Texture2D tilesheet, SpriteBatch sb, int x, int y, Light3 light)
		{

			var rect = new Rectangle(0, 15 * Globals.TileSize, Globals.TileSize, TileState);
			sb.Draw(tilesheet, new Vector2(x * Globals.TileSize, (y * Globals.TileSize) + (8 - TileState)), rect, light.MultiplyAgainst(new Color(0.6f, 0.9f, 0.3f)));
		}

		public void LocalTileUpdate(IGameWorld world, int x, int y)
		{
			LiquidUpdate<Sludge>(world, x, y);
		}
	}
	public class Ectoplasm { }
	public class Oil { }
	public class LiquidOxygen { }
	public class LiquidNitrogen { }
}
