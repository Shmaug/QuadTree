using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace QuadTree
{
    public class DiamondSquare
    {
        private static int RandRange(Random r, int rMin, int rMax)
        {
            return rMin + r.Next() * (rMax - rMin);
        }

        private static double RandRange(Random r, double rMin, double rMax)
        {
            return rMin + r.NextDouble() * (rMax - rMin);
        }

        private static float RandRange(Random r, float rMin, float rMax)
        {
            return rMin + (float)r.NextDouble() * (rMax - rMin);
        }

        private static bool pow2(int a)
        {
            return (a & (a - 1)) == 0;
        }

        /*
         *	Generates a grid of VectorPositionColor elements as a 2D greyscale representation of terrain by the
         *	Diamond-square algorithm: http://en.wikipedia.org/wiki/Diamond-square_algorithm
         * 
         *	Arguments: 
         *		int size - the width or height of the grid being passed in.  Should be of the form (2 ^ n) + 1
         *		int seed - an optional seed for the random generator
         *		float rMin/rMax - the min and max height values for the terrain (defaults to 0 - 255 for greyscale)
         *		float noise - the roughness of the resulting terrain
         * */
        public static float[,] DiamondSquareGrid(int size, int seed = 0, float rMin = 0, float rMax = 255, float noise = 0.0f)
        {
            // Fail if grid size is not of the form (2 ^ n) - 1 or if min/max values are invalid
            int s = size - 1;
            if (!pow2(s) || rMin >= rMax)
                return null;

            float modNoise = 0.0f;

            // init the grid
            float[,] grid = new float[size,size];

            // Seed the first four corners
            Random rand = (seed == 0 ? new Random() : new Random(seed));
            grid[0,0] = RandRange(rand, rMin, rMax);
            grid[s,0] = RandRange(rand, rMin, rMax);
            grid[0,s] = RandRange(rand, rMin, rMax);
            grid[s,s] = RandRange(rand, rMin, rMax);

            /*
             * Use temporary named variables to simplify equations
             * 
             * s0 . d0. s1
             *  . . . . . 
             * d1 . cn. d2
             *  . . . . . 
             * s2 . d3. s3
             * 
             * */
            float s0, s1, s2, s3, d0, d1, d2, d3, cn;

            for (int i = s; i > 1; i /= 2)
            {
                // reduce the random range at each step
                modNoise = (rMax - rMin) * noise * ((float)i / s);

                // diamonds
                for (int y = 0; y < s; y += i)
                {
                    for (int x = 0; x < s; x += i)
                    {
                        s0 = grid[x,y];
                        s1 = grid[x + i,y];
                        s2 = grid[x,y + i];
                        s3 = grid[x + i,y + i];

                        // cn
                        grid[x + (i / 2),y + (i / 2)] = ((s0 + s1 + s2 + s3) / 4.0f)
                            + RandRange(rand, -modNoise, modNoise);
                    }
                }

                // squares
                for (int y = 0; y < s; y += i)
                {
                    for (int x = 0; x < s; x += i)
                    {
                        s0 = grid[x,y];
                        s1 = grid[x + i,y];
                        s2 = grid[x,y + i];
                        s3 = grid[x + i,y + i];
                        cn = grid[x + (i / 2),y + (i / 2)];

                        d0 = y <= 0 ? (s0 + s1 + cn) / 3.0f : (s0 + s1 + cn + grid[x + (i / 2),y - (i / 2)]) / 4.0f;
                        d1 = x <= 0 ? (s0 + cn + s2) / 3.0f : (s0 + cn + s2 + grid[x - (i / 2),y + (i / 2)]) / 4.0f;
                        d2 = x >= s - i ? (s1 + cn + s3) / 3.0f :
                            (s1 + cn + s3 + grid[x + i + (i / 2),y + (i / 2)]) / 4.0f;
                        d3 = y >= s - i ? (cn + s2 + s3) / 3.0f :
                            (cn + s2 + s3 + grid[x + (i / 2),y + i + (i / 2)]) / 4.0f;

                        grid[x + (i / 2),y] = d0 + RandRange(rand, -modNoise, modNoise);
                        grid[x,y + (i / 2)] = d1 + RandRange(rand, -modNoise, modNoise);
                        grid[x + i,y + (i / 2)] = d2 + RandRange(rand, -modNoise, modNoise);
                        grid[x + (i / 2),y + i] = d3 + RandRange(rand, -modNoise, modNoise);
                    }
                }
            }

            return grid;
        }
    }
}