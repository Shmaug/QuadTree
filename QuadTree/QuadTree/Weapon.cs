using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace QuadTree
{
    public class Weapon
    {
        public Model model;
        public Texture2D texture;
        public Vector3 position;
        public Vector3 rotation;
        public int ammo;
        private int clipSize;
        private int maxAmmo;
        public Player owner;
        public int type;
        public string name;
        public World world;

        public Weapon(Vector3 pos, World world, int type, Player owner)
        {
            this.world = world;
            this.position = pos;
            this.type = type;
            this.owner = owner;
            this.defaults();
        }

        public void Update(GameTime gameTime)
        {
            if (this.owner != null)
            {
                Vector3 off = new Vector3(.5f, 0, -1);
                if (this.owner.localPlayer)
                    off = new Vector3(this.owner.headSway + .5f, this.owner.headBob, -1);
                this.position = this.owner.position + new Vector3(0, 0.5f, 0) + Vector3.Transform(off, Matrix.CreateRotationX(this.owner.headRot.X) * Matrix.CreateRotationY(this.owner.headRot.Y));
                
                this.rotation = this.owner.headRot;
                this.rotation.X *= -1;
                this.rotation.Y += MathHelper.Pi;
            }
        }

        public void Draw(GraphicsDevice device)
        {
            Matrix world = Matrix.CreateRotationX(this.rotation.X) * Matrix.CreateRotationY(this.rotation.Y) * Matrix.CreateRotationZ(this.rotation.Z) * Matrix.CreateTranslation(this.position);
            Matrix[] transforms = new Matrix[this.model.Bones.Count];
            this.model.CopyAbsoluteBoneTransformsTo(transforms);
            
            foreach (ModelMesh m in this.model.Meshes)
            {
                foreach (ModelMeshPart part in m.MeshParts)
                {
                    part.Effect = Main.texturedModelEffect;

                    part.Effect.Parameters["World"].SetValue(transforms[m.ParentBone.Index] * world);
                    part.Effect.Parameters["tex"].SetValue(this.texture);

                    foreach (EffectPass p in part.Effect.CurrentTechnique.Passes)
                    {
                        m.Draw();
                        p.Apply();
                    }
                }
            }
        }

        private void defaults()
        {
            switch (this.type)
            {
                case 0: //m9
                    {
                        this.model = Main.weaponModel[0];
                        this.texture = Main.weaponTexture[0];
                        this.maxAmmo = 24;
                        this.clipSize = 12;
                        this.ammo = 12;
                        this.name = "M9";
                        break;
                    }
            }
        }
    }
}
