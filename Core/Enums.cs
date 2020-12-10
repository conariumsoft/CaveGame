using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core
{
	public enum Direction : byte
	{
		Left,
		Right,
		Up,
		Down,
	}


	/// <summary>
	/// Cardinal Directions. Generally used for surface face of a tile.
	/// </summary>
	public enum Face
	{
		Top,
		Bottom,
		Left,
		Right
	}

	public enum Compass
    {
		North = 0,
		Northeast = 45,
		East = 90,
		Southeast = 135,
		South = 180,
		Southwest = 225,
		West = 270,
		Northwest = 315
    }

	public static class CardinalDirectionExtension
    {
		public static Rotation ToRotation(this Compass dir) => Rotation.FromDeg((int)dir);
    }


	/// <summary>
	/// Applied to the following:
	/// IEntity, Explosion, and certain tile classes
	/// </summary>
	public interface IDamageSource { }

	public enum DamageType
	{
		Frostbite,
		Fire,
		Lava,
		BluntForceTrauma,
		PunctureTrauma,
		Electrocution,
		LacerationTrauma,
		Neurotoxin,
		Poison,
		Explosion,
		Psionic,
		Asphyxiation,
		ActOfGod,
	}



}
