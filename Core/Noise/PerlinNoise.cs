﻿using DataManagement;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaveGame.Core.Noise
{

	class PerlinNoise
	{
		public const double OFFSET = 0.5;
		private const int GradientSizeTable = 256;
		private readonly double[] _gradients = new double[GradientSizeTable * 3];

        /* Borrowed from Darwyn Peachey (see references above).
		 The gradient table is indexed with an XYZ triplet, which is first turned
		 into a single random index using a lookup in this table. The table simply
		 contains all numbers in [0..255] in random order. */

        private readonly byte[] _perm =
        {
            225, 155, 210, 108, 175, 199, 221, 144, 203, 116, 70, 213, 69, 158, 33, 252, 5,
            82, 173, 133, 222, 139, 174, 27, 9, 71, 90, 246, 75, 130, 91, 191, 169, 138, 2, 151, 194, 235, 81, 7, 25,
            113, 228, 159, 205, 253, 134, 142, 248, 65, 224, 217, 22, 121, 229, 63, 89, 103, 96, 104, 156, 17, 201, 129,
            36, 8, 165, 110, 237, 117, 231, 56, 132, 211, 152, 20, 181, 111, 239, 218, 170, 163, 51, 172, 157, 47, 80,
            212, 176, 250, 87, 49, 99, 242, 136, 189, 162, 115, 44, 43, 124, 94, 150, 16, 141, 247, 32, 10, 198, 223,
            255, 72, 53, 131, 84, 57, 220, 197, 58, 50, 208, 11, 241, 28, 3, 192, 62, 202, 18, 215, 153, 24, 76, 41, 15,
            179, 39, 46, 55, 6, 128, 167, 23, 188, 106, 34, 187, 140, 164, 73, 112, 182, 244, 195, 227, 13, 35, 77, 196,
            185, 26, 200, 226, 119, 31, 123, 168, 125, 249, 68, 183, 230, 177, 135, 160, 180, 12, 1, 243, 148, 102, 166,
            38, 238, 251, 37, 240, 126, 64, 74, 161, 40, 184, 149, 171, 178, 101, 66, 29, 59, 146, 61, 254, 107, 42, 86,
            154, 4, 236, 232, 120, 21, 233, 209, 45, 98, 193, 114, 78, 19, 206, 14, 118, 127, 48, 79, 147, 85, 30, 207,
            219, 54, 88, 234, 190, 122, 95, 67, 143, 109, 137, 214, 145, 93, 92, 100, 245, 0, 216, 186, 60, 83, 105, 97,
            204, 52
        };

        private readonly Random _random;

        public PerlinNoise(Random rand)
		{
            _random = rand;
            InitGradients();
		}

        public PerlinNoise(int seed)
		{
            _random = new Random(seed);
            InitGradients();
		}

        public PerlinNoise(long seed)
        {
            _random = new Random((int)seed);
            InitGradients();
        }

        public static DoubleRange Interval = new DoubleRange(-0.5, 0.5);

        /// <summary>
        /// Returns a noise value in [-0.5, 0.5] range.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        /// 


        public double Noise(double x, double y, double z)
        {
            /* The main noise function. Looks up the pseudorandom gradients at the nearest
                lattice points, dots them with the input vector, and interpolates the
                results to produce a single output value in [-0.5, 0.5] range. */
            var ix = (int)Math.Floor(x);
            double fx0 = x - ix;
            double fx1 = fx0 - 1;
            double wx = this.Smooth(fx0);

            var iy = (int)Math.Floor(y);
            double fy0 = y - iy;
            double fy1 = fy0 - 1;
            double wy = this.Smooth(fy0);

            var iz = (int)Math.Floor(z);
            double fz0 = z - iz;
            double fz1 = fz0 - 1;
            double wz = this.Smooth(fz0);

            double vx0 = this.Lattice(ix, iy, iz, fx0, fy0, fz0);
            double vx1 = this.Lattice(ix + 1, iy, iz, fx1, fy0, fz0);
            double vy0 = this.Lerp(wx, vx0, vx1);

            vx0 = this.Lattice(ix, iy + 1, iz, fx0, fy1, fz0);
            vx1 = this.Lattice(ix + 1, iy + 1, iz, fx1, fy1, fz0);
            double vy1 = this.Lerp(wx, vx0, vx1);

            double vz0 = this.Lerp(wy, vy0, vy1);

            vx0 = this.Lattice(ix, iy, iz + 1, fx0, fy0, fz1);
            vx1 = this.Lattice(ix + 1, iy, iz + 1, fx1, fy0, fz1);
            vy0 = this.Lerp(wx, vx0, vx1);

            vx0 = this.Lattice(ix, iy + 1, iz + 1, fx0, fy1, fz1);
            vx1 = this.Lattice(ix + 1, iy + 1, iz + 1, fx1, fy1, fz1);
            vy1 = this.Lerp(wx, vx0, vx1);

            double vz1 = this.Lerp(wy, vy0, vy1);
            return Math.Min(0.5, Math.Max(-0.5, this.Lerp(wz, vz0, vz1)));
        }

        private int Index(int ix, int iy, int iz)
        {
            // Turn an XYZ triplet into a single gradient table index.
            return this.Permutate(ix + this.Permutate(iy + this.Permutate(iz)));
        }

        private void InitGradients()
        {
            for (int i = 0; i < GradientSizeTable; i++)
            {
                double z = 1f - 2f * this._random.NextDouble();
                double r = Math.Sqrt(1f - z * z);
                double theta = 2 * Math.PI * this._random.NextDouble();
                this._gradients[i * 3] = r * Math.Cos(theta);
                this._gradients[i * 3 + 1] = r * Math.Sin(theta);
                this._gradients[i * 3 + 2] = z;
            }
        }


        private double Lattice(int ix, int iy, int iz, double fx, double fy, double fz)
        {
            // Look up a random gradient at [ix,iy,iz] and dot it with the [fx,fy,fz] vector.
            int index = this.Index(ix, iy, iz);
            int g = index * 3;
            return this._gradients[g] * fx + this._gradients[g + 1] * fy + this._gradients[g + 2] * fz;
        }

        private double Lerp(double t, double value0, double value1)
        {
            // Simple linear interpolation.
            return value0 + t * (value1 - value0);
        }

        private int Permutate(int x)
        {
            const int mask = GradientSizeTable - 1;
            return this._perm[x & mask];
        }

        private double Smooth(double x)
        {
            /* Smoothing curve. This is used to calculate interpolants so that the noise
                doesn't look blocky when the frequency is low. */
            return x * x * (3 - 2 * x);
        }

    }
}
