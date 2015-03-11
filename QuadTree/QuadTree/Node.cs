using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuadTree
{
    public struct VertexMultitextured
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
        public Vector4 texWeights;

        public static int SizeInBytes = (3 + 3 + 2 + 4) * sizeof(float);
        public static VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement[]
        {
         new VertexElement( 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0 ),
         new VertexElement( sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0 ),
         new VertexElement( sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0 ),
         new VertexElement( sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1 ),
        });
    }
    public class Node
    {
        public World world;
        public Matrix matrix;
        public Vector3 position;
        public int size;
        public bool built = false;
        public bool visible = false;
        public float[,] heightMap;
        public BoundingBox bbox;

        public List<Grass> grass;

        public VertexMultitextured[] verts;
        public int[] indicies;

        public Node(World world, Vector3 position, int size, bool build = true)
        {
            this.grass = new List<Grass>();
            this.world = world;
            this.matrix = Matrix.CreateTranslation(position);
            this.size = size;
            this.position = position;
            if (build)
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.build));
        }

        public VertexMultitextured[] getVerts()
        {
            return this.verts;
        }

        public int[] getIndicies()
        {
            return this.indicies;
        }

        public Vector3 getCenter()
        {
            return this.matrix.Translation + new Vector3(this.size, 0, this.size) / 2f;
        }

        public Vector3 getPointClosestToPoint(Vector3 point)
        {
            return new Vector3(
                MathHelper.Clamp(point.X, this.position.X, this.position.X + this.size),
                this.position.Y,
                MathHelper.Clamp(point.Z, this.position.Z, this.position.Z + this.size));

        }

        public void buildDiamondSquare(object threadContext)
        {
            float[,] heights = (float[,])threadContext;
            float max = 0;
            float min = 0;
            int s = this.world.size * this.world.nodeSize;
            for (int x = 0; x < s; x++)
                for (int y = 0; y < s; y++)
                    if (heights[x, y] < min)
                        min = heights[x, y];
                    else if (heights[x, y] > max)
                        max = heights[x, y];
            min *= this.size;
            max *= this.size;
            float range = max - min;

            this.heightMap = new float[this.size, this.size];

            // verts
            this.verts = new VertexMultitextured[this.size * this.size + 1];
            for (int x = 0; x < this.size; x++)
            {
                for (int y = 0; y < this.size; y++)
                {
                    int i = x + y * this.size;
                    float h = heights[((int)this.position.X + x), ((int)this.position.Z + y)] * this.size;
                    this.verts[i] = new VertexMultitextured() { Position = new Vector3(this.position.X + x, h, this.position.Z + y), TextureCoordinate = new Vector2(this.position.X + x, this.position.Y + y) / 20f, Normal = Vector3.Up, texWeights = Vector4.Zero };

                    this.verts[i].texWeights.X = MathHelper.Clamp(1f - Math.Abs(h) / (range * .3f), 0, 1);
                    this.verts[i].texWeights.Y = MathHelper.Clamp(1f - Math.Abs(h - (max * 0.5f)) / (range * .25f), 0, 1);
                    this.verts[i].texWeights.Z = MathHelper.Clamp(1f - Math.Abs(h - (max * 0.7f)) / (range * .42f), 0, 1);
                    this.verts[i].texWeights.W = MathHelper.Clamp(1f - Math.Abs(h - max) / (range * .1f), 0, 1);

                    float total = 0f;
                    total += this.verts[i].texWeights.X;
                    total += this.verts[i].texWeights.Y;
                    total += this.verts[i].texWeights.Z;
                    total += this.verts[i].texWeights.W;

                    this.verts[i].texWeights.X /= total;
                    this.verts[i].texWeights.Y /= total;
                    this.verts[i].texWeights.Z /= total;
                    this.verts[i].texWeights.W /= total;
                    total = 0f;
                    total += this.verts[i].texWeights.X;
                    total += this.verts[i].texWeights.Y;
                    total += this.verts[i].texWeights.Z;
                    total += this.verts[i].texWeights.W;

                    this.heightMap[x, y] = h;
                }
            }

            // indicies
            this.indicies = new int[(this.size - 1) * (this.size - 1) * 6];
            int counter = 0;
            for (int x = 0; x < this.size - 1; x++)
            {
                for (int y = 0; y < this.size - 1; y++)
                {
                    this.indicies[counter++] = x + y * size;
                    this.indicies[counter++] = (x + 1) + y * size;
                    this.indicies[counter++] = (x + 1) + (y + 1) * size;

                    this.indicies[counter++] = x + y * size;
                    this.indicies[counter++] = (x + 1) + (y + 1) * size;
                    this.indicies[counter++] = x + (y + 1) * size;
                }
            }

            // normals
            for (int i = 0; i < this.indicies.Length / 3; i++)
            {
                int i1 = this.indicies[i * 3];
                int i2 = this.indicies[i * 3 + 1];
                int i3 = this.indicies[i * 3 + 2];

                Vector3 s1 = this.verts[i1].Position - this.verts[i3].Position;
                Vector3 s2 = this.verts[i1].Position - this.verts[i2].Position;
                Vector3 norm = Vector3.Cross(s1, s2);

                this.verts[i1].Normal += norm;
                this.verts[i2].Normal += norm;
                this.verts[i3].Normal += norm;
            }

            // normalize normals
            for (int i = 0; i < this.verts.Length; i++)
            {
                this.verts[i].Normal.Normalize();

            }
            this.bbox = new BoundingBox(this.position - new Vector3(0, 1000f, 0), this.position + new Vector3(this.size, 1000f, this.size));

            this.world.builtNodes++;
            this.built = true;
        }

        private void build(object threadContext)
        {
            // verts
            this.heightMap = new float[this.size, this.size];
            this.verts = new VertexMultitextured[this.size * this.size + 1];
            for (int x = 0; x < this.size; x++)
            {
                for (int y = 0; y < this.size; y++)
                {
                    this.verts[x + y * this.size] = new VertexMultitextured() { Position = new Vector3(this.position.X + x, this.world.noise.getNoise(x + this.position.X, y + this.position.Z) * this.size, this.position.Z + y), TextureCoordinate = new Vector2(this.position.X + x, this.position.Y + y) / 20f, Normal = Vector3.Up };
                    this.verts[x + y * this.size].texWeights = new Vector4(1, 0, 0, 0);
                    this.heightMap[x, y] = this.verts[x + y * this.size].Position.Y;
                }
            }
            
            // indicies
            this.indicies = new int[(this.size - 1) * (this.size - 1) * 6];
            int counter = 0;
            for (int x = 0; x < this.size - 1; x++)
            {
                for (int y = 0; y < this.size - 1; y++)
                {
                    this.indicies[counter++] = x + y * size;
                    this.indicies[counter++] = (x + 1) + y * size;
                    this.indicies[counter++] = (x + 1) + (y + 1) * size;

                    this.indicies[counter++] = x + y * size;
                    this.indicies[counter++] = (x + 1) + (y + 1) * size;
                    this.indicies[counter++] = x + (y + 1) * size;
                }
            }

            // normals
            for (int i = 0; i < this.indicies.Length / 3; i++)
            {
                int i1 = this.indicies[i * 3];
                int i2 = this.indicies[i * 3 + 1];
                int i3 = this.indicies[i * 3 + 2];

                Vector3 s1 = this.verts[i1].Position - this.verts[i3].Position;
                Vector3 s2 = this.verts[i1].Position - this.verts[i2].Position;
                Vector3 norm = Vector3.Cross(s1, s2);

                this.verts[i1].Normal += norm;
                this.verts[i2].Normal += norm;
                this.verts[i3].Normal += norm;
            }
            float high = 0;
            float low = 0;

            // normalize normals
            for (int i = 0; i < this.verts.Length; i++)
            {
                if (this.verts[i].Position.Y > high)
                    high = this.verts[i].Position.Y;
                if (this.verts[i].Position.Y < low)
                    low = this.verts[i].Position.Y;
                this.verts[i].Normal.Normalize();
            }
            this.bbox = new BoundingBox(this.position - new Vector3(0, low, 0), this.position + new Vector3(this.size, high, this.size));

            this.world.builtNodes++;
            this.built = true;
        }
    }
}
