using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace QuadTree
{
    public class Noise2D
    {
        public int seed = 0;
        public int octaves = 5;
        public float amplitude = 0.5f;
        public float persistence = 0.3f;
        public float frequency = 0.015f;

        public Noise2D(int seed)
        {
            this.seed = seed;
        }

        public float getNoise(float x, float y)
        {
            //returns -1 to 1
            float total = 0f;
            float freq = this.frequency, amp = this.amplitude;
            for (int i = 0; i < this.octaves; ++i)
            {
                total = total + this.Smooth(x * freq, y * freq) * amp;
                freq *= 2;
                amp *= this.persistence;
            }
            if (total < -2.4f) total = -2.4f;
            else if (total > 2.4f) total = 2.4f;

            total += 2.4f;
            total /= 4.8f;

            return total;
        }

        private float NoiseGeneration(int x, int y)
        {
            int n = ((x + y * 57) << 13) ^ (x + y * 57);

            return (1.0f - ((n * (n * n * 15731 + 789221) + this.seed) & 0x7fffffff) / 1073741824f);
        }

        private float Interpolate(float x, float y, float a)
        {
            float value = (1 - (float)Math.Cos(a * Math.PI)) * 0.5f;
            return x * (1f - value) + y * value;
        }

        private float Smooth(float x, float y)
        {
            float n1 = this.NoiseGeneration((int)x, (int)y);
            float n2 = this.NoiseGeneration((int)x + 1, (int)y);
            float n3 = this.NoiseGeneration((int)x, (int)y + 1);
            float n4 = this.NoiseGeneration((int)x + 1, (int)y + 1);

            float i1 = this.Interpolate(n1, n2, x - (int)x);
            float i2 = this.Interpolate(n3, n4, x - (int)x);

            return this.Interpolate(i1, i2, y - (int)y);
        }
    }
}
