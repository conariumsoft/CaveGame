using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Furniture
{
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
	public class Furniture
	{
		public Point Position { get; set; } // Top Left Tile Corner

		public virtual void Draw()
		{

		}
	}
}
