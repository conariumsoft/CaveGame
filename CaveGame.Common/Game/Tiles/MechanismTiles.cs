using CaveGame.Common.Game.Entities;
using CaveGame.Common.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common.Game.Tiles
{
	public interface IPowerSource { }
	public interface IConductive { 
		bool Powered { get; set; }
		void Power(IConductive source);
	}
	public interface IPartiallyConductive
	{
		void IsFaceConductive(Face face);
	}

	public class Wire : Tile, ITileUpdate, IWaterBreakable, INonSolid, IConductive
	{

		public void Power(IPowerSource source) { } 
		public IConductive PowerSource { get; set; }
		public bool Powered
		{
			get => TileState.Get(0);
			set => TileState.SetF(0, value);
		}

		public bool StateChanged
		{
			get => TileState.Get(1);
			set => TileState.SetF(1, value);
		}


		public void Propagate(IGameWorld world, int x, int y)
		{
			if (world.GetTile(x, y-1) is IConductive wireAbove && !wireAbove.Powered)
			{
				wireAbove.Power(this);
			}
			if (world.GetTile(x, y + 1) is IConductive wireBelow && !wireBelow.Powered)
			{
				wireBelow.Power(this);
			}
			if (world.GetTile(x-1, y) is IConductive wireLeft && !wireLeft.Powered)
			{
				wireLeft.Power(this);

			}
			if (world.GetTile(x + 1, y) is IConductive wireRight && !wireRight.Powered)
			{
				wireRight.Power(this);
			}
		}


		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (PowerSource != null && PowerSource.Powered == false)
			{
				Powered = false;
				world.SetTileUpdated(x, y);
			}
		}

		public void Power(IConductive source)
		{
			Powered = true;
			PowerSource = source;
		}
	}

	public class TNT : Tile, ITileUpdate, IConductive
	{
		public override Rectangle Quad => TileMap.TNT;
		public override byte Hardness => 1;

		public bool Powered { get; set; }

		public void TileUpdate(IGameWorld world, int x, int y) {

			if (Powered)
			{
				world.SetTile(x, y, new Air());
				world.Explosion(new Explosion { 
					BlastPressure = 1,
					BlastRadius = 1,
					Position = new Vector2(x+0.5f, y+0.5f)*8,
				}, true, true);
			}
		}

		public void Power(IConductive source)
		{
			Powered = true;
		}

	}
	public class Switch : Tile, ITileUpdate, IClickableTile, IPowerSource
	{
		public static Rectangle FlickedOff = new Rectangle(12*8, 0, 8, 8);
		public static Rectangle FlickedOn = new Rectangle(12*8, 1*8, 8, 8);


		public override void Draw(GraphicsEngine GFX, int x, int y, Light3 color)
		{
			Rectangle sprite = FlickedOff;
			if (On)
				sprite = FlickedOn;



			GFX.Sprite(
					texture: GFX.TileSheet,
					position: new Vector2(x * Globals.TileSize, y * Globals.TileSize),
					quad: sprite, color.MultiplyAgainst(Color),
					rotation: Rotation.Zero,
					origin: Vector2.Zero,
					scale: 1,
					efx: SpriteEffects.None,
					layer: 0
			);
		}

		public bool On
		{
			get => TileState.Get(0);
			set => TileState.SetF(0, value);
		}

		public override Rectangle Quad => TileMap.SwitchOff;


		public void PowerAdjacent(IGameWorld world, int x, int y)
		{
			if (world.GetTile(x, y-1) is IConductive ductAbove && !ductAbove.Powered)
			{

			}
		}

		public void OnClick(IGameWorld world, Player clicker)
		{
			On = !On;
			
		}

		public void TileUpdate(IGameWorld world, int x, int y)
		{
			if (On)
				PowerAdjacent(world, x, y);
			if (!On)
			{

			}

		}


	}
	public class ANDGate { }
	public class ORGate { }
	public class XORGate { }
	public class Diode { }
	public class NORGate { }
	public class XANDGate { }
	public class Pump { }
	public class Pipe { }
	public class Trapdoor { }
}
