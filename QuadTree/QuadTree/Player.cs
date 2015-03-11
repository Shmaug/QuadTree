using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace QuadTree
{
    public class Player
    {
        public World world;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 headRot;
        public AudioEmitter footStepEmitter;
        private SoundEffectInstance footstepSound;
        private bool stepC;
        private float bobV;
        private float swayV;
        public float headBob;
        public float headSway;
        public bool onGround;
        public Weapon weapon;
        public float legRot;
        private float legRotD = MathHelper.ToRadians(4);

        public bool localPlayer;
        public Vector3 goalPos;
        public Vector3 goalHeadRot;
        public float lerpTime;
        public float totalLerpTime;

        public Player(World world, Vector3 pos, bool local = true)
        {
            this.world = world;
            this.position = pos;
            this.velocity = Vector3.Zero;
            this.footStepEmitter = new AudioEmitter();
            this.footstepSound = Main.sandStepSound.CreateInstance();
            this.footstepSound.IsLooped = false;
            this.localPlayer = local;
        }

        public void Update(GameTime gameTime)
        {
            #region movement
            if (this.localPlayer)
            {
                Vector3 moveVector = Vector3.Zero;
                if (Main.ks.IsKeyDown(Keys.W))
                    moveVector.Z = -1;
                else if (Main.ks.IsKeyDown(Keys.S))
                    moveVector.Z = 1;
                if (Main.ks.IsKeyDown(Keys.A))
                    moveVector.X = -1;
                else if (Main.ks.IsKeyDown(Keys.D))
                    moveVector.X = 1;

                if (moveVector != Vector3.Zero)
                    moveVector.Normalize();

                moveVector *= 10f;
                if (Main.ks.IsKeyDown(Keys.LeftShift))
                    moveVector *= 1.5f;

                this.footStepEmitter.Position = this.position;
                this.footStepEmitter.Velocity = this.velocity;
                this.footstepSound.Apply3D(this.world.camera.ears, this.footStepEmitter);

                // calculate head bob
                if (moveVector.X != 0 || moveVector.Z != 0 && onGround)
                {
                    float len = MathHelper.ToRadians(moveVector.Length());

                    this.bobV += len;
                    this.swayV += len;

                    if (this.bobV > MathHelper.TwoPi)
                    {
                        this.stepC = false;
                        this.bobV -= MathHelper.TwoPi;
                    }
                    this.headBob = (float)Math.Sin(this.bobV) * 0.2f;
                    this.headSway = (float)Math.Cos(this.swayV * 0.5f) * 0.4f;

                    this.legRot += this.legRotD;
                    if (this.legRot > MathHelper.ToRadians(40) || this.legRot < MathHelper.ToRadians(-40))
                        this.legRot *= -1;

                    if (this.bobV > MathHelper.Pi * 1.5f && !this.stepC)
                    {
                        this.footstepSound.Pitch = new Random().Next((int)((len / 15f) * 5f), 10) / 10f;
                        this.footstepSound.Play();
                        this.stepC = true;
                    }
                }
                else
                {
                    this.legRot = 0;
                    this.bobV = 0;
                    this.stepC = false;
                    this.swayV = 0;
                    this.headBob = MathHelper.Lerp(this.headBob, 0, 0.1f);
                    this.headSway = MathHelper.Lerp(this.headSway, 0, 0.1f);
                    this.footstepSound.Stop();
                }

                Matrix rotationMatrix = Matrix.CreateRotationY(this.world.camera.rotation.Y);
                moveVector = Vector3.Transform(moveVector, rotationMatrix);

                this.velocity.X = moveVector.X;
                this.velocity.Z = moveVector.Z;

                if (this.onGround && Main.ks.IsKeyDown(Keys.Space))
                    this.velocity.Y += 20f;
            }
            this.velocity.Y -= 1f;

            this.position += this.velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            this.position.X = MathHelper.Clamp(this.position.X, 0, this.world.size * (this.world.nodeSize - 1));
            this.position.Z = MathHelper.Clamp(this.position.Z, 0, this.world.size * (this.world.nodeSize - 1));

            bool g = this.onGround;

            this.onGround = false;
            float y = this.world.getHeightAtPoint(this.position.X, this.position.Z);
            if (this.position.Y < y + 1.8f)
            {
                this.position.Y = y + 1.5f;
                this.velocity.Y = 0;
                this.onGround = true;
            }
            #endregion
            if (this.localPlayer)
            {
                if (Main.ks.IsKeyDown(Keys.F) && Main.lastks.IsKeyUp(Keys.F))
                {
                    this.weapon = new Weapon(this.position, this.world, 0, this);
                }
                if (Main.ks.IsKeyDown(Keys.T) && Main.lastks.IsKeyUp(Keys.T))
                {
                    Fire f = new Fire(this.position, this.world);
                    f.position.Y = this.world.getHeightAtPoint(f.position.X, f.position.Z);
                    this.world.fires.Add(f);
                }
            }
            else
            {
                if (Vector3.DistanceSquared(this.position, this.goalPos) > 100)
                {
                    this.position = this.goalPos;
                    this.lerpTime = 0;
                }
                else
                {
                    this.position = this.goalPos;
                    //this.position = Vector3.Lerp(this.position, this.goalPos, 1f - this.lerpTime/this.totalLerpTime);
                }
                this.lerpTime -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (this.lerpTime < 0) this.lerpTime = 0;
            }
            if (this.weapon != null)
                this.weapon.Update(gameTime);
        }

        public void Draw(GraphicsDevice device)
        {
            if (this.weapon != null)
                this.weapon.Draw(device);
            if (!this.localPlayer)
            {
                foreach (ModelMesh m in Main.playerModel.Meshes)
                {
                    foreach (ModelMeshPart part in m.MeshParts)
                    {
                        Vector4 col = new Vector4(1, 1, 1, 1);
                        part.Effect = Main.modelEffect;
                        Matrix mat = Matrix.Identity;
                        switch (m.Name)
                        {
                            case "Head":
                                mat = Matrix.CreateScale(.25f, .25f, .25f) * Matrix.CreateTranslation(0, .75f, 0);
                                col = new Vector4(.5f, .35f, .25f, 1f);
                                break;
                            case "Torso":
                                mat = Matrix.CreateScale(.5f, .5f, .25f);
                                col = new Vector4(.5f, .1f, .1f, 1f);
                                break;
                            case "Right Arm":
                                mat = Matrix.CreateScale(.25f, .5f, .25f);
                                float rot = this.legRot * -.5f;
                                if (this.weapon != null)
                                    rot = this.headRot.X + MathHelper.PiOver2;
                                mat *= Matrix.CreateTranslation(-.25f, -.25f, 0) * (Matrix.CreateRotationX(rot) * Matrix.CreateRotationY(.25f)) * Matrix.CreateTranslation(.25f, .25f, 0);
                                mat *= Matrix.CreateTranslation(.75f, 0, 0);
                                col = new Vector4(.5f, .1f, .1f, 1f);
                                break;
                            case "Left Arm":
                                mat = Matrix.CreateScale(.25f, .5f, .25f);
                                mat *= Matrix.CreateTranslation(.25f, -.25f, 0) * Matrix.CreateRotationX(this.legRot * .5f) * Matrix.CreateTranslation(-.25f, .25f, 0);
                                mat *= Matrix.CreateTranslation(-.75f, 0, 0);
                                col = new Vector4(.5f, .1f, .1f, 1f);
                                break;
                            case "Right Leg":
                                mat = Matrix.CreateScale(.25f, .5f, .25f) * Matrix.CreateTranslation(0, -.5f, 0) * Matrix.CreateRotationX(legRot) * Matrix.CreateTranslation(.25f, -.5f, 0);
                                col = new Vector4(.1f, .1f, .2f, 1f);
                                break;
                            case "Left Leg":
                                mat = Matrix.CreateScale(.25f, .5f, .25f) * Matrix.CreateTranslation(0, -.5f, 0) * Matrix.CreateRotationX(-legRot) * Matrix.CreateTranslation(-.25f, -.5f, 0);
                                col = new Vector4(.1f, .1f, .2f, 1f);
                                break;
                        }

                        mat *= Matrix.CreateRotationY(this.headRot.Y);
                        part.Effect.Parameters["ambientColor"].SetValue(col);
                        part.Effect.Parameters["World"].SetValue(mat * Matrix.CreateTranslation(this.position));
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
}
