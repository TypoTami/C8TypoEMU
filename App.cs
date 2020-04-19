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
        Texture2D memoryMap;
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
            // this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 2d);
            // Load a rom
            // Emulator.currentROM = File.ReadAllBytes("testRoms/BREAKOUT.ch8");
            // Emulator.currentROM = File.ReadAllBytes("testRoms/INVADERS.ch8");
            // Emulator.currentROM = File.ReadAllBytes("testRoms/PONG.ch8");
            // Emulator.currentROM = File.ReadAllBytes("testRoms/Fishie.ch8");
            // Emulator.currentROM = File.ReadAllBytes("testRoms/test_opcode.ch8");
            Emulator.currentROM = File.ReadAllBytes("testRoms/BC_test.ch8");
            // Emulator.currentROM = File.ReadAllBytes("testRoms/drawZero.ch8");

            Disassembler.BytestoBitmap(Emulator.currentROM, 0xF, "rom.bmp");
            //Disassembler.DisassembleROM(Emulator.currentROM);

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

            // Setup display
            canvas = new Texture2D(GraphicsDevice, 64, 32, false, SurfaceFormat.Color);
            for (int i = 0; i < 64 * 32; i++)
            {
                Emulator.display[i] = 0xFF000000;
            }

            // Setup memoryMap
            memoryMap = new Texture2D(GraphicsDevice, 64, 64, false, SurfaceFormat.Color);
            for (int i = 0; i < 64 * 64; i++)
            {
                Disassembler.memoryMap[i] = 0xFF000000;
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
            $"executionPaused: {Emulator.executionPaused}", 
            $"pausedOn: {Emulator.pausedOn[0]:X2}{Emulator.pausedOn[1]:X2}",
            "",
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
            Disassembler.MapMemory(Emulator.memory);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            graphics.GraphicsDevice.Textures[0] = null;

            canvas.SetData<UInt32>(Emulator.display, 0, 64 * 32);
            memoryMap.SetData<UInt32>(Disassembler.memoryMap, 0, 64 * 64);
            
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

            spriteBatch.Draw(canvas, new Rectangle(0, 40, 1280, 640), Color.White);
            spriteBatch.Draw(memoryMap, new Rectangle(1280 - 64*4, 720 - 64*4, 64*4, 64*4), Color.White * 0.5f);

            Vector2 textLeftPoint = new Vector2(0,0);
            Vector2 textPosition = new Vector2(10,10);
            spriteBatch.DrawString(genericFont, info, textPosition, Color.Red, 0, textLeftPoint, 1.0f, SpriteEffects.None, 0.5f);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}