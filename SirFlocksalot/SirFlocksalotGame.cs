using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace SirFlocksalot
{
    public class GameObject
    {
        public Vector2 Position = Vector2.Zero;
        public GameObject() { }
    }
    public class SirFlocksalotGame : Game
    {
        public static Point ScreenSize = new Point(1280, 720);
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont DebugFont;
        Texture2D BackgroundTexture;
        Texture2D RogueTexture;
        List<Texture2D> PetalTextures;
        Moon Moon;
        Flock Flock;
        float TimeModifier = 1.0f;
        public SirFlocksalotGame()
        {
            IsMouseVisible = true;
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = ScreenSize.X;
            graphics.PreferredBackBufferHeight = ScreenSize.Y;
            //IsFixedTimeStep = graphics.SynchronizeWithVerticalRetrace = false;
            Content.RootDirectory = "Content";
            PetalTextures = new List<Texture2D>();
            Moon = new Moon();
            Flock = new Flock();
        }
        protected override void Initialize()
        {
            base.Initialize();
        }
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            DebugFont = Content.Load<SpriteFont>("Fonts/Debug");
            Moon.Texture = Content.Load<Texture2D>("moon");
            BackgroundTexture = Content.Load<Texture2D>("stars");
            PetalTextures.Add(Content.Load<Texture2D>("petal"));
            PetalTextures.Add(Content.Load<Texture2D>("petal-blue"));
            PetalTextures.Add(Content.Load<Texture2D>("petal-yellow"));
            RogueTexture = Content.Load<Texture2D>("rogue");
            // Post Content Loading

            Flock.CreateFlock(PetalTextures, RogueTexture);
        }
        protected override void UnloadContent()
        {
        }
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            float DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            float CurrentTime = (float)gameTime.TotalGameTime.TotalSeconds;
            Moon.Update(DeltaTime);
            Flock.Update(CurrentTime, DeltaTime, TimeModifier);
            base.Update(gameTime);
        }


        protected override void Draw(GameTime gameTime)
        {
            float frameRate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;
            GraphicsDevice.Clear(new Color(0));
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
            spriteBatch.Draw(BackgroundTexture, new Rectangle(0, 0, ScreenSize.X, ScreenSize.Y), Color.White);
            Moon.Draw(spriteBatch);
            Flock.Draw(spriteBatch);
            spriteBatch.DrawString(DebugFont, string.Format("{0} (FPS)", frameRate), new Vector2(10, 10), Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
