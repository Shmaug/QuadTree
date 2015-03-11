using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace QuadTree
{
    public class Main : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public static int screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        public static int screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        public static SpriteFont[] font = new SpriteFont[1];

        public static string statusText1 = "Press space to start!";
        public static string statusText2 = "Press F to connect!";
        public static bool loading = false;

        public static bool inGame = false;

        public static Effect terrainEffect;
        public static Effect grassEffect;
        public static Effect modelEffect;
        public static Effect texturedModelEffect;
        public static Effect skyEffect;

        public static TextureCube skyTexture;

        public static Texture2D sandTexture;
        public static Texture2D grassTexture;
        public static Texture2D rockTexture;
        public static Texture2D snowTexture;
        public static Texture2D tallGrassTexture;
        public static Texture2D fireTexture;
        public static Texture2D[] weaponTexture;

        public static Model skyModel;
        public static Model playerModel;
        public static Model fireModel;
        public static Model[] weaponModel;

        public static SoundEffect sandStepSound;

        public static World world;

        public static KeyboardState ks, lastks;
        public static MouseState ms, lastms;

        public static Main me;

        public static float fpstime = 0;
        public static int frames;
        public static float fps;

        public static bool lockMouse = false;

        public static string error;


        public Main()
        {
            me = this;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferMultiSampling = true;
            graphics.PreferredBackBufferWidth = screenWidth;
            graphics.PreferredBackBufferHeight = screenHeight;
            Content.RootDirectory = "Content";
            try
            {
                IntPtr hWnd = this.Window.Handle;
                var control = System.Windows.Forms.Control.FromHandle(hWnd);
                var form = control.FindForm();
                form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                form.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            }
            catch { }
        }

        protected override void Initialize()
        {

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            weaponModel = new Model[1];
            weaponModel[0] = Content.Load<Model>("wep/m9/M9");

            weaponTexture = new Texture2D[1];
            weaponTexture[0] = Content.Load<Texture2D>("wep/m9/tex");

            playerModel = Content.Load<Model>("mod/player");
            fireModel = Content.Load<Model>("mod/fire");
            skyModel = Content.Load<Model>("mod/skyCube");

            terrainEffect = Content.Load<Effect>("fx/terrain");
            grassEffect = Content.Load<Effect>("fx/grass");
            modelEffect = Content.Load<Effect>("fx/model");
            texturedModelEffect = Content.Load<Effect>("fx/texturedmodel");
            skyEffect = Content.Load<Effect>("fx/sky");

            skyTexture = Content.Load<TextureCube>("tex/sky");

            sandTexture = Content.Load<Texture2D>("tex/sand");
            grassTexture = Content.Load<Texture2D>("tex/grass");
            rockTexture = Content.Load<Texture2D>("tex/rock");
            snowTexture = Content.Load<Texture2D>("tex/snow");
            fireTexture = Content.Load<Texture2D>("tex/fireTex");
            tallGrassTexture = Content.Load<Texture2D>("grass");
            sandStepSound = Content.Load<SoundEffect>("aud/sand");

            for (int i = 0; i < font.Length; i++)
                font[i] = Content.Load<SpriteFont>("fon/f" + i);

            Mouse.SetPosition(Main.screenWidth / 2, Main.screenHeight / 2);
        }

        protected override void UnloadContent()
        {

        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            inGame = false;
            NetworkHost.shutdown();
            NetworkClient.shutdown();
            base.OnExiting(sender, args);
        }

        protected override void Update(GameTime gameTime)
        {
            Network.totalTime = gameTime.TotalGameTime.Milliseconds;
            ks = Keyboard.GetState();
            ms = Mouse.GetState();

            if ((ks.IsKeyDown(Keys.LeftAlt) || ks.IsKeyDown(Keys.RightAlt)) && ks.IsKeyDown(Keys.F4))
            {
                this.Exit();
            }

            if (inGame)
            {
                lockMouse = me.IsActive;
                IsMouseVisible = !me.IsActive;
                world.Update(gameTime);
            }
            else
            {
                if (!loading)
                {
                    if (ks.IsKeyDown(Keys.Space) && lastks.IsKeyUp(Keys.Space))
                    {
                        world = new World(10, 10, false);
                        ThreadPool.QueueUserWorkItem(new WaitCallback(world.load), 16);
                        NetworkHost.listenForConnections(7777);
                    }
                    else if (ks.IsKeyDown(Keys.F) && lastks.IsKeyUp(Keys.F))
                    {
                        //NetworkClient.tryConnect("68.7.50.111",7777); // jon
                        NetworkClient.tryConnect("98.176.183.63", 7777); // me
                        //NetworkClient.tryConnect("192.168.1.144", 7777); // dad
                        //NetworkClient.tryConnect("127.0.0.1", 7777); // local me
                    }
                }
            }
            fpstime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (fpstime >= 1f)
            {
                fps = frames;
                fpstime = 0;
                frames = 0;
            }
            lastks = ks;
            lastms = ms;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            frames++;
            if (!loading && inGame)
            {
                GraphicsDevice.Clear(Color.Black);
                world.Draw(GraphicsDevice);

                spriteBatch.Begin();
                spriteBatch.DrawString(font[0],
                    ""
                    , Vector2.One * 10f, Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                spriteBatch.End();
            }
            else
            {
                GraphicsDevice.Clear(Color.Black);
                spriteBatch.Begin();
                spriteBatch.DrawString(font[0], statusText1, new Vector2(screenWidth, screenHeight) * 0.5f, Color.White, 0f, font[0].MeasureString(statusText1) * new Vector2(0.5f, 1), 1f, SpriteEffects.None, 0f);
                spriteBatch.DrawString(font[0], statusText2, new Vector2(screenWidth, screenHeight) * 0.5f, Color.White, 0f, font[0].MeasureString(statusText2) * new Vector2(0.5f, -1), 1f, SpriteEffects.None, 0f);
                spriteBatch.End();
            }
            spriteBatch.Begin();
            spriteBatch.DrawString(font[0], "fps: " + fps, Vector2.One * 5f, Color.Blue, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
            spriteBatch.DrawString(font[0], "err: " + error, Vector2.UnitX * screenWidth * 0.25f, Color.Blue, 0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
