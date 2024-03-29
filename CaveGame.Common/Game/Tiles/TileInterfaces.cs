﻿using CaveGame.Common.Game.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common.Game.Tiles
{


	public interface IClickableTile
	{
		void OnClick(IGameWorld world, Player clicker);
	}

	public interface IWaterBreakable { }
	public interface INonMinable { }
	public interface INonSolid { }

	// Property interfaces
	public interface INonStandardCollision
	{
		bool CollisionCheck(IGameWorld world);
		void OnCollide();
	}

	public interface ILightEmitter
	{
		Light3 Light { get; }
	}

	public interface ILocalTileUpdate
	{
		void LocalTileUpdate(IGameWorld world, int x, int y);
	}
	public interface RandomTileTickable
	{
		void RandomTick(IGameWorld world, int x, int y);
	}
	public interface ITileUpdate
	{
		void TileUpdate(IGameWorld world, int x, int y);
	}
	public interface ISoil { }
	public interface IGas { }
	public interface ILiquid
	{
		float Viscosity { get; }
	}
	public interface IVegetation { }
	public interface IOre { }
	public interface IMineral { }
	public interface IPlatformTile { }
}
