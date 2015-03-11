using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuadTree
{
    class Particle
    {
        public static VertexPositionTexture[] verts = new VertexPositionTexture[4] {
            new VertexPositionTexture(new Vector3(-.5f, .5f, 0), Vector2.Zero),
            new VertexPositionTexture(new Vector3(.5f, .5f, 0), Vector2.UnitX),
            new VertexPositionTexture(new Vector3(-.5f, 0, 0), Vector2.UnitY),
            new VertexPositionTexture(new Vector3(.5f, 0, 0), Vector2.One)};

        public Texture2D texture;
        public Vector3 position;
        public Vector3 velocity;
        public float lifeTime;
        public World world;

        public Particle(World world, Vector3 pos, Texture2D tex, float life)
        {
            this.position = pos;
            this.texture = tex;
            this.lifeTime = life;
            this.world = world;
        }

        public void Update(GameTime gameTime)
        {
            this.lifeTime -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.position += this.velocity;

        }
    }
}
