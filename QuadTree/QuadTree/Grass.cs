using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuadTree
{
    public class Grass
    {
        public static VertexPositionTexture[] verts = new VertexPositionTexture[4] {
            new VertexPositionTexture(new Vector3(-1.5f, 1f, 0), Vector2.Zero),
            new VertexPositionTexture(new Vector3(1.5f, 1f, 0), Vector2.UnitX),
            new VertexPositionTexture(new Vector3(-1.5f, 0, 0), Vector2.UnitY),
            new VertexPositionTexture(new Vector3(1.5f, 0, 0), Vector2.One)};

        public Vector3 position;
        public Texture2D texture;
        public Node node;
        public BoundingBox bbox;

        public Grass(Node node, Vector3 pos)
        {
            this.node = node;
            this.texture = Main.tallGrassTexture;
            this.position = pos;
            this.bbox = new BoundingBox(this.position - new Vector3(1, 0, 1), this.position + new Vector3(1, 1, 1));
        }

        public void Draw(GraphicsDevice device)
        {
            Main.grassEffect.Parameters["W"].SetValue(Matrix.CreateTranslation(this.position));
            Main.grassEffect.CurrentTechnique.Passes[0].Apply();
            device.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, verts, 0, 2);
        }
    }
}
