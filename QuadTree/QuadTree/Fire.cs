using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace QuadTree
{
    public class Fire
    {
        public Model model;
        public Vector3 position;
        public BoundingSphere bbox;
        public Matrix matrix
        {
            get
            {
                return Matrix.CreateScale(.2f) * Matrix.CreateTranslation(this.position);
            }
        }
        public World world;
        public float radius = 10f;

        public Fire(Vector3 pos, World world)
        {
            this.model = Main.fireModel;
            this.position = pos;
            this.world = world;
            this.bbox = new BoundingSphere(this.position, this.radius);
        }

        public void Draw(GraphicsDevice device)
        {
            Matrix[] transforms = new Matrix[this.model.Bones.Count];
            this.model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh m in this.model.Meshes)
            {
                foreach (ModelMeshPart part in m.MeshParts)
                {
                    part.Effect = Main.texturedModelEffect;

                    part.Effect.Parameters["World"].SetValue(transforms[m.ParentBone.Index] * this.matrix);
                    part.Effect.Parameters["tex"].SetValue(Main.fireTexture);
                    foreach (EffectPass p in part.Effect.CurrentTechnique.Passes)
                    {
                        m.Draw();
                        p.Apply();
                    }
                }
            }
        }
    }
}
