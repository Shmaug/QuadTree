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
    public class Camera
    {
        public Vector3 position;
        public Vector3 rotation;
        public AudioListener ears;

        public Vector3 viewVector;

        public Matrix viewMatrix;
        public Matrix projectionMatrix;

        public BoundingFrustum frustum;

        public Player player;

        public Camera(Player player)
        {
            this.ears = new AudioListener();
            this.player = player;
            this.position = Vector3.Zero;
            this.rotation = Vector3.Zero;
            this.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(50f), (float)Main.screenWidth / (float)Main.screenHeight, 0.01f, 10000f);
            this.frustum = new BoundingFrustum(this.viewMatrix * this.projectionMatrix);
        }

        public void Update(GameTime gameTime)
        {
            Matrix rotationMatrix = Matrix.CreateRotationX(this.rotation.X) * Matrix.CreateRotationY(this.rotation.Y) * Matrix.CreateRotationZ(this.rotation.Z);

            if (Main.ks.IsKeyUp(Keys.LeftAlt) && Main.lockMouse)
            {
                this.rotation.Y -= (Main.ms.X - Main.screenWidth / 2) * (float)gameTime.ElapsedGameTime.TotalSeconds * 0.05f;
                this.rotation.X -= (Main.ms.Y - Main.screenHeight / 2) * (float)gameTime.ElapsedGameTime.TotalSeconds * 0.05f;

                this.rotation.X = MathHelper.Clamp(this.rotation.X, -MathHelper.PiOver2, MathHelper.PiOver2);

                Mouse.SetPosition(Main.screenWidth / 2, Main.screenHeight / 2);
            }

            if (this.player == null)
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
                if (Main.ks.IsKeyDown(Keys.Space))
                    moveVector.Y = 0.5f;
                else if (Main.ks.IsKeyDown(Keys.LeftControl))
                    moveVector.Y = -0.5f;

                moveVector *= (float)gameTime.ElapsedGameTime.TotalSeconds * (Main.ks.IsKeyDown(Keys.LeftShift) ? 150f : 75f);

                moveVector =  Vector3.Transform(moveVector, rotationMatrix);

                this.position += moveVector;

                this.ears.Velocity = moveVector;
            }
            else
            {
                this.player.headRot = this.rotation;
                this.position = this.player.position + new Vector3(0, .75f, 0) + Vector3.Transform(new Vector3(this.player.headSway, this.player.headBob, 0), Matrix.CreateRotationY(this.rotation.Y));
                this.ears.Velocity = this.player.velocity;
            }

            this.ears.Position = this.position;

            Vector3 lookAt = Vector3.Transform(Vector3.Forward, rotationMatrix);
            Vector3 up = Vector3.Transform(Vector3.Up, rotationMatrix);
            this.viewMatrix = Matrix.CreateLookAt(this.position, this.position + lookAt, up);

            this.frustum = new BoundingFrustum(this.viewMatrix * this.projectionMatrix);
        }
    }
}