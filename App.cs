using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C8TypoEmu
{
    public class App : Game
    {
    SpriteBatch spriteBatch;
    Texture2D texture2D;

        public App()
        {
        Content.RootDirectory = "Content";

        var graphics = new GraphicsDeviceManager(this)
                {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720,
        };
        graphics.ApplyChanges();
    }

    protected override void LoadContent()
        {
        spriteBatch = new SpriteBatch(GraphicsDevice);

        texture2D = Content.Load<Texture2D>("ProfilePic");

        base.LoadContent();
    }

    protected override void Draw(GameTime gameTime)
        {
        spriteBatch.Begin();

        spriteBatch.Draw(texture2D, Vector2.Zero, Color.White);

        spriteBatch.End();
    }
    }
}