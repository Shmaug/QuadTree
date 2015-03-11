using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuadTree
{
    public class World
    {
        public Node[,] nodes;
        public Camera camera;
        public Noise2D noise;
        public int builtNodes;
        public int nodeSize = 128;
        public int groundHeight = 0;
        public int showDistance = 5;
        public int grassDistanceSquared = 100 * 100;
        public int size = 512;
        public int seed;
        public Vector3 center
        {
            get
            {
                return (new Vector3(this.size * this.nodeSize * 0.5f, 0, this.size * this.nodeSize * 0.5f));
            }
        }
        public int numNodes
        {
            get
            {
                return this.nodes.Length;
            }
        }
        public Player[] players;
        public List<Fire> fires;
        public Vector4[] plights;
        public Vector4[] plightsCol;
        public int plightsDrawn;

        public World(int size, int seed, bool load = true)
        {
            this.seed = seed;
            plights = new Vector4[24];
            plightsCol = new Vector4[24];
            this.fires = new List<Fire>();
            this.size = size;
            this.noise = new Noise2D(seed);
            this.players = new Player[2];
            //this.players[0] = new Player(this, new Vector3(this.center.X, 0, this.center.Z), true);
            this.camera = new Camera(this.players[0]);
            this.camera.position = new Vector3(this.center.X, 0, this.center.Z);
            if (load)
                ThreadPool.QueueUserWorkItem(new WaitCallback(this.load), this.size);
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < this.players.Length; i++)
                if (players[i] != null)
                    players[i].Update(gameTime);
            this.camera.Update(gameTime);
        }

        public void Draw(GraphicsDevice device)
        {
            device.RasterizerState = new RasterizerState() { FillMode = (Main.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Q)) ? FillMode.WireFrame : FillMode.Solid };
           
            device.DepthStencilState = DepthStencilState.None;
            foreach (EffectPass pass in Main.skyEffect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in Main.skyModel.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = Main.skyEffect; ;
                        part.Effect.Parameters["W"].SetValue(
                            Matrix.CreateScale(200f) * Matrix.CreateTranslation(this.camera.position));
                        part.Effect.Parameters["VP"].SetValue(this.camera.viewMatrix * this.camera.projectionMatrix);
                        part.Effect.Parameters["camPos"].SetValue(this.camera.position);
                        part.Effect.Parameters["tex"].SetValue(Main.skyTexture);
                    }

                    mesh.Draw();
                }
            }
            device.DepthStencilState = DepthStencilState.Default;

            Main.texturedModelEffect.Parameters["View"].SetValue(this.camera.viewMatrix);
            Main.texturedModelEffect.Parameters["Proj"].SetValue(this.camera.projectionMatrix);

            for (int i = 0; i < this.plights.Length; i++)
                this.plights[i] = new Vector4(0, 0, 0, -1);
            int c = 0;
            foreach (Fire f in this.fires)
            {
                f.Draw(device);
                if (c < this.plights.Length)
                {
                    if (this.camera.frustum.Intersects(f.bbox))
                    {
                        this.plights[c] = new Vector4(f.position, f.radius);
                        this.plightsCol[c] = (new Color(200, 150, 100) * 0.5f).ToVector4();
                        c++;
                    }
                }
            }
            this.plightsDrawn = c;
            Main.grassEffect.Parameters["plights"].SetValue(this.plights);
            Main.grassEffect.Parameters["plightscol"].SetValue(this.plightsCol);
            Main.grassEffect.Parameters["numplights"].SetValue((uint)this.plightsDrawn);
            Main.grassEffect.Parameters["tex"].SetValue(Main.tallGrassTexture);
            Main.grassEffect.Parameters["VP"].SetValue(this.camera.viewMatrix * this.camera.projectionMatrix);
            Main.grassEffect.Parameters["rot"].SetValue(Matrix.CreateRotationY(this.camera.rotation.Y));

            Main.terrainEffect.Parameters["plights"].SetValue(this.plights);
            Main.terrainEffect.Parameters["plightscol"].SetValue(this.plightsCol);
            Main.terrainEffect.Parameters["numplights"].SetValue((uint)this.plightsDrawn);
            Main.terrainEffect.Parameters["ambientColor"].SetValue(new Vector4(1, 1, 1, 1));
            Main.terrainEffect.Parameters["World"].SetValue(Matrix.Identity);
            Main.terrainEffect.Parameters["View"].SetValue(this.camera.viewMatrix);
            Main.terrainEffect.Parameters["Proj"].SetValue(this.camera.projectionMatrix);
            Main.terrainEffect.Parameters["camPos"].SetValue(this.camera.position);
            Main.terrainEffect.Parameters["tex0"].SetValue(Main.sandTexture);
            Main.terrainEffect.Parameters["tex1"].SetValue(Main.grassTexture);
            Main.terrainEffect.Parameters["tex2"].SetValue(Main.rockTexture);
            Main.terrainEffect.Parameters["tex3"].SetValue(Main.snowTexture);

            Main.modelEffect.Parameters["plights"].SetValue(this.plights);
            Main.modelEffect.Parameters["plightscol"].SetValue(this.plightsCol);
            Main.modelEffect.Parameters["numplights"].SetValue((uint)this.plightsDrawn);
            Main.modelEffect.Parameters["View"].SetValue(this.camera.viewMatrix);
            Main.modelEffect.Parameters["Proj"].SetValue(this.camera.projectionMatrix);

            Main.texturedModelEffect.Parameters["plights"].SetValue(this.plights);
            Main.texturedModelEffect.Parameters["plightscol"].SetValue(this.plightsCol);
            Main.texturedModelEffect.Parameters["numplights"].SetValue((uint)this.plightsDrawn);

            this.DrawTerrain(device);

            for (int i = 0; i < this.players.Length; i++)
                if (players[i] != null)
                    players[i].Draw(device);

        }

        private void DrawTerrain(GraphicsDevice device)
        {
            // terrain
            int sX = (int)(this.camera.position.X / this.nodeSize);
            int sZ = (int)(this.camera.position.Z / this.nodeSize);
            if (sX < 0) sX = 0;
            if (sZ < 0) sZ = 0;

            if (Main.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.V))
                this.grassDistanceSquared = 1000;
            else
                this.grassDistanceSquared = 25000;

            bool drawGrass = Main.ks.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.E);

            RasterizerState tRS = new RasterizerState() { FillMode = (Main.ks.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Q)) ? FillMode.WireFrame : FillMode.Solid };
            RasterizerState gRS = new RasterizerState() { FillMode = FillMode.Solid, CullMode = CullMode.CullClockwiseFace };
            device.RasterizerState = tRS;
            device.DepthStencilState = DepthStencilState.Default;
            for (int x = sX - 5; x < sX + 5; x++)
            {
                for (int z = sZ - 5; z < sZ + 5; z++)
                {
                    if (x >= 0 && z >= 0 && x < this.size && z < this.size)
                    {
                        Node node = this.nodes[x, z];

                        if (node.built && node.getVerts() != null && node.getIndicies() != null)
                        {
                            if (node.getVerts().Length > 0 && node.getIndicies().Length > 0)
                            {
                                if (this.camera.frustum.Intersects(node.bbox))
                                {
                                    foreach (EffectPass p in Main.terrainEffect.CurrentTechnique.Passes)
                                    {
                                        p.Apply();
                                        device.DrawUserIndexedPrimitives<VertexMultitextured>(PrimitiveType.TriangleList, node.getVerts(), 0, node.getVerts().Length, node.getIndicies(), 0, node.getIndicies().Length / 3, VertexMultitextured.VertexDeclaration);
                                    }

                                    if (drawGrass)
                                        foreach (Grass g in node.grass)
                                            if (Vector2.DistanceSquared(new Vector2(g.position.X, g.position.Z), new Vector2(this.camera.position.X, this.camera.position.Z)) <= this.grassDistanceSquared && this.camera.frustum.Intersects(g.bbox))
                                                g.Draw(device);
                                }
                            }
                        }
                    }
                }
            }
            device.RasterizerState = tRS;
            device.DepthStencilState = DepthStencilState.Default;
        }

        public float getHeightAtPoint(float x, float z)
        {
            x = MathHelper.Clamp(x, 0, this.nodeSize * this.size);
            z = MathHelper.Clamp(z, 0, this.nodeSize * this.size);
            float y = 0;
            if (!float.IsNaN(x) && !float.IsNaN(z))
            {
                Node node = this.nodes[(int)(x / (this.nodeSize - 1)), (int)(z / (this.nodeSize - 1))];

                x -= node.position.X;
                z -= node.position.Z;

                y = node.heightMap[(int)x, (int)z];

                try
                {
                    x = (x == this.size - 1) ? (x - 0.001f) : x;
                    z = (z == this.size - 1) ? (z - 0.001f) : z;
                    int fX = (int)Math.Floor(x);
                    int fZ = (int)Math.Floor(z);
                    float sX = x - fX;
                    float sY = z - fZ;
                    float a = node.heightMap[fX, fZ];
                    float b = node.heightMap[fX + 1, fZ];
                    float c = node.heightMap[fX, fZ + 1];
                    float d = node.heightMap[fX + 1, fZ + 1];

                    y = (a * (1f - sX) + b * sX) * (1f - sY) + (c * (1f - sX) + d * sX) * (sY);
                }
                catch { }
            }

            return y;
        }


        public void load(object thrCon)
        {
            Random cRand = new Random();

            int size = (int)thrCon;
            this.size = size;
            this.nodes = new Node[size, size];
            float total = size * size;
            Main.loading = true;
            Main.statusText1 = "Generating world";
            Main.statusText2 = "Generating heightmap...";
            float counter = 0;
            this.builtNodes = 0;

            int s = size * this.nodeSize + 1;
            float[,] heights = DiamondSquare.DiamondSquareGrid(s, this.seed, 0, 1, 3f);

            //smoothing
            for (int x = 0; x < s; x++)
            {
                for (int y = 0; y < s; y++)
                {
                    float c = 0;
                    if (x > 0)
                    {
                        heights[x, y] += heights[x - 1, y];
                        c++;
                    }
                    if (x < s - 1)
                    {
                        heights[x, y] += heights[x + 1, y];
                        c++;
                    }
                    if (y > 0)
                    {
                        heights[x, y] += heights[x, y - 1];
                        c++;
                    }
                    if (y < s - 1)
                    {
                        heights[x, y] += heights[x, y + 1];
                        c++;
                    }
                    heights[x, y] /= c;
                    Main.statusText2 = "Smoothing heightmap " + (int)(((float)(x + y * s) / heights.Length) * 100f) + "%";
                }
            }
            for (int x = 0; x < s; x++)
            {
                for (int y = 0; y < s; y++)
                {
                    float c = 0;
                    if (x > 0)
                    {
                        heights[x, y] += heights[x - 1, y];
                        c++;
                    }
                    if (x < s - 1)
                    {
                        heights[x, y] += heights[x + 1, y];
                        c++;
                    }
                    if (y > 0)
                    {
                        heights[x, y] += heights[x, y - 1];
                        c++;
                    }
                    if (y < s - 1)
                    {
                        heights[x, y] += heights[x, y + 1];
                        c++;
                    }
                    heights[x, y] /= c;
                    Main.statusText2 = "Smoothing heightmap x2 " + (int)(((float)(x + y * s) / heights.Length) * 100f) + "%";
                }
            }

            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Main.statusText2 = "Creating nodes " + (counter / total) * 100f + "% (" + counter + "/" + total + ")";
                    this.nodes[x, y] = new Node(this, new Vector3(x * (this.nodeSize - 1), this.groundHeight, y * (this.nodeSize - 1)), this.nodeSize, false);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(this.nodes[x, y].buildDiamondSquare), heights);
                    counter++;
                }
            }
            while (this.builtNodes <= total)
            {
                Main.statusText2 = "Building nodes " + (int)((this.builtNodes / total) * 100f) + "% (" + this.builtNodes + "/" + total + ")";
                Thread.Sleep(100);
                if (this.builtNodes >= total)
                    break;
            }
            total = (this.nodeSize * this.nodeSize) * this.nodes.Length;
            Main.statusText1 = "Generating grass";
            /*Random rand = new Random();
            for (float i = 0; i < total; )
            {
                float gX = rand.Next(0, this.size * (this.nodeSize - 1));
                float gZ = rand.Next(0, this.size * (this.nodeSize - 1));

                float gY = this.getHeightAtPoint(gX, gZ) - 0.2f;

                this.nodes[(int)(gX / this.nodeSize), (int)(gZ / this.nodeSize)].grass.Add(new Grass(this.nodes[(int)(gX / this.nodeSize), (int)(gZ / this.nodeSize)], new Vector3(gX, gY, gZ)));

                Main.statusText2 = (int)((i / total) * 100f) + "%";

                i += rand.Next(30, 50);
            }*/
            Main.loading = false;
            Main.inGame = true;
            Console.WriteLine("Done");

            Color[] h = new Color[heights.Length];
            for (int x = 0; x < s; x++)
                for (int y = 0; y < s; y++)
                    h[x + y * s] = new Color(heights[x, y], heights[x, y], heights[x, y]);
            Texture2D tex = new Texture2D(Main.me.GraphicsDevice, s, s);
            tex.SetData<Color>(h);

            tex.SaveAsPng(System.IO.File.Create("C:\\Users\\Trevor\\Desktop\\map.png"), s, s);
        }
    }
}
