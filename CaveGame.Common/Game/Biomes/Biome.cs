using CaveGame.Common.Noise;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Common.Game.Biomes
{
	public abstract class Biome
	{

		public int Seed => Generator.Seed;
		public OctaveNoise Octave4 => Generator.Octave4;
		public OctaveNoise Octave2 => Generator.Octave2;
		public OctaveNoise Octave3 => Generator.Octave3;
		public OctaveNoise Octave8 => Generator.Octave8;
		public SimplexNoise Simplex => Generator.Simplex;


		protected Generator Generator;



		public Biome(Generator generator)
		{
			Generator = generator;
		}

		public abstract float GetHeight(int x);
		public abstract void SurfacePass(ref Chunk chunk, int chunkX, int chunkY);
		public abstract void StructurePass(IGameWorld world, int x, int y);
	}
}
