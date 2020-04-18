using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace C8TypoEmu
{
    public class App : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D canvas;
        SpriteFont genericFont;
        String info;

        public App()
        {
            Content.RootDirectory = "Content";

            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720,
            };
            graphics.ApplyChanges();
        }

        protected override void Initialize()
        {
            //this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d);
            // Load a rom
            // Emulator.currentROM = File.ReadAllBytes("testRoms/BREAKOUT.ch8");
            // Emulator.currentROM = File.ReadAllBytes("testRoms/Fishie.ch8");
            Emulator.currentROM = File.ReadAllBytes("testRoms/test_opcode.ch8");
            // Emulator.currentROM = File.ReadAllBytes("testRoms/drawZero.ch8");

            Disassembler.BytestoBitmap(Emulator.currentROM, 0xF, "rom.bmp");
            Disassembler.DisassembleROM(Emulator.currentROM);

            // Put font into memory
            int pos = 0x00;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    Emulator.memory[pos] = Emulator.font[i,j];
                    pos++;
                }
            }
            // Put rom into memory
            pos = 0x200;
            for (int i = 0; i < Emulator.currentROM.Length; i++)
            {
                Emulator.memory[pos] = Emulator.currentROM[i];
                pos++;
            }

            Disassembler.BytestoBitmap(Emulator.memory, 0x3F, "memory.bmp");

            // Setup display
            canvas = new Texture2D(GraphicsDevice, 64, 32, false, SurfaceFormat.Color);
            for (int i = 0; i < 64 * 32; i++)
            {
                Emulator.display[i] = 0xFF000000;
            }

            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            genericFont = Content.Load<SpriteFont>("GenericFont");

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            string stackArray = "";
            for (int i = 0; i < 16; i++)
            {
                stackArray = stackArray + " " + Emulator.stack[i].ToString("X4");
            }

            info = String.Join(
            Environment.NewLine,
            $"PC: {Emulator.programCounter:X4}",
            $"DT: {Emulator.delayTimer:X2} ST: {Emulator.soundTimer:X2}",
            $"SP: {Emulator.stackPointer:X2}",
            $"ST: [{stackArray} ]",
            "",
            $"I : {Emulator.registerI:X4}",
            "",
            $"V0: {Emulator.registers[0x0]:X2} V8: {Emulator.registers[0x8]:X2}",
            $"V1: {Emulator.registers[0x1]:X2} V9: {Emulator.registers[0x9]:X2}",
            $"V2: {Emulator.registers[0x2]:X2} VA: {Emulator.registers[0xA]:X2}",
            $"V3: {Emulator.registers[0x3]:X2} VB: {Emulator.registers[0xB]:X2}",
            $"V4: {Emulator.registers[0x4]:X2} VC: {Emulator.registers[0xC]:X2}",
            $"V5: {Emulator.registers[0x5]:X2} VD: {Emulator.registers[0xD]:X2}",
            $"V6: {Emulator.registers[0x6]:X2} VE: {Emulator.registers[0xE]:X2}",
            $"V7: {Emulator.registers[0x7]:X2} VF: {Emulator.registers[0xF]:X2}");

            Emulator.ExecuteNextOpCode();
            Emulator.IncrementTimers();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            graphics.GraphicsDevice.Textures[0] = null;

            canvas.SetData<UInt32>(Emulator.display, 0, 64 * 32);
            
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            spriteBatch.Draw(canvas, new Rectangle(0, 0, 1280, 720), Color.White);

            Vector2 textLeftPoint = new Vector2(0,0);
            Vector2 textPosition = new Vector2(10,10);
            spriteBatch.DrawString(genericFont, info, textPosition, Color.Red, 0, textLeftPoint, 1.0f, SpriteEffects.None, 0.5f);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}