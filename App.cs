using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

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
        private int clockspeed;
        private int framerate;
        // MAXIMUM BODGE ENGAGE
        private bool singleStep = false;
        private bool showDebug = false;
        private KeyboardState currentState;
        private KeyboardState oldState;
        private WaveOutEvent waveOut = new WaveOutEvent();

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
            // Load a rom
            Emulator.currentROM = File.ReadAllBytes(Program.romToLoad);

            // Setup display
            canvas = new Texture2D(GraphicsDevice, 64, 32, false, SurfaceFormat.Color);

            // Setup Beep
            waveOut.Init(new SignalGenerator() {
                Gain = 0.2,
                Frequency = 500,
                Type = SignalGeneratorType.Square}
            );
            
            // Setup memoryMap
            memoryMap = new Texture2D(GraphicsDevice, 64, 64, false, SurfaceFormat.Color);
            for (int i = 0; i < 64 * 64; i++)
            {
                Disassembler.memoryMap[i] = 0xFF000000;
            }

            Emulator.Init();

            // Set clockspeed (Hz)
            clockspeed = 400;
            // Set framerate (fps)
            framerate = 60;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / (double)framerate);

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
            currentState = Keyboard.GetState();
                        
            // Player controls
            Array.Clear(Emulator.keyboard, 0, 16);
            if (currentState.IsKeyDown(Keys.D1))
                Emulator.keyboard[0x1] = true;
            if (currentState.IsKeyDown(Keys.D2))
                Emulator.keyboard[0x2] = true;
            if (currentState.IsKeyDown(Keys.D3))
                Emulator.keyboard[0x3] = true;
            if (currentState.IsKeyDown(Keys.D4))
                Emulator.keyboard[0xC] = true;
            if (currentState.IsKeyDown(Keys.Q))
                Emulator.keyboard[0x4] = true;
            if (currentState.IsKeyDown(Keys.W))
                Emulator.keyboard[0x5] = true;
            if (currentState.IsKeyDown(Keys.E))
                Emulator.keyboard[0x6] = true;
            if (currentState.IsKeyDown(Keys.R))
                Emulator.keyboard[0xD] = true;
            if (currentState.IsKeyDown(Keys.A))
                Emulator.keyboard[0x7] = true;
            if (currentState.IsKeyDown(Keys.S))
                Emulator.keyboard[0x8] = true;
            if (currentState.IsKeyDown(Keys.D))
                Emulator.keyboard[0x9] = true;
            if (currentState.IsKeyDown(Keys.F))
                Emulator.keyboard[0xE] = true;
            if (currentState.IsKeyDown(Keys.Z))
                Emulator.keyboard[0xA] = true;
            if (currentState.IsKeyDown(Keys.X))
                Emulator.keyboard[0x0] = true;
            if (currentState.IsKeyDown(Keys.C))
                Emulator.keyboard[0xB] = true;
            if (currentState.IsKeyDown(Keys.V))
                Emulator.keyboard[0xF] = true;

            // System keys
            // Quit
            if (currentState.IsKeyDown(Keys.Escape))
                Exit();
            // Toggle showDebug
            if (currentState.IsKeyDown(Keys.F9) & oldState.IsKeyUp(Keys.F9))
                showDebug = !showDebug;
            // Clockspeed and framerate adjustment
            if (currentState.IsKeyDown(Keys.F1) & oldState.IsKeyUp(Keys.F1))
                clockspeed *= 2;
            if (currentState.IsKeyDown(Keys.F2) & oldState.IsKeyUp(Keys.F2) & clockspeed > 25)
                clockspeed /= 2;
            if (currentState.IsKeyDown(Keys.F3) & oldState.IsKeyUp(Keys.F3))
                framerate += 5;
            if (currentState.IsKeyDown(Keys.F4) & oldState.IsKeyUp(Keys.F4) & framerate > 5)
                framerate -= 5;
            // Toggling single stepping
            if (currentState.IsKeyDown(Keys.F5) & oldState.IsKeyUp(Keys.F5))
                singleStep = !singleStep;
            // Reload rom
            if (currentState.IsKeyDown(Keys.F10) & oldState.IsKeyUp(Keys.F10))
            {
                Emulator.Init();
            }

            Emulator.IncrementTimers();

            int opcodesPerFrame = clockspeed / framerate;
            if (singleStep == false)
            {
                for (int i = 0; i < opcodesPerFrame; i++)
                {
                    Emulator.ExecuteNextOpCode();
                }
            }
            else if (currentState.IsKeyDown(Keys.F6) & oldState.IsKeyUp(Keys.F6) )
            {
                Emulator.ExecuteNextOpCode();
            }

            oldState = currentState;

            string stackArray = "";
            string keyArray = "";
            for (int i = 0; i < 16; i++)
            {
                stackArray = stackArray + " " + Emulator.stack[i].ToString("X4");
                keyArray = keyArray + " " + Emulator.keyboard[i].ToString();
            }

            if (Emulator.soundTimer > 0)
            {
                waveOut.Play();
            }
            else
            {
                waveOut.Stop();
            }

            if (showDebug == true)
            {
                byte[] OpCode = new byte[2] { Emulator.memory[Emulator.programCounter], Emulator.memory[Emulator.programCounter + 1] };
                var currentOpCode = Disassembler.DisassembleOpCode(OpCode, Emulator.programCounter);
                Disassembler.MapMemory(Emulator.memory, Emulator.programCounter);
                info = String.Join(
                Environment.NewLine,
                $"Framerate: {framerate}, Clockspeed: {clockspeed:d4}Hz, opcodesPerFrame: {opcodesPerFrame:d2}",
                "",
                $"executionPaused: {Emulator.executionPaused}", 
                $"pausedOn: {Emulator.pausedOn[0]:X2}{Emulator.pausedOn[1]:X2}",
                $"keyboard: [{keyArray} ]",
                "",
                $"currentOpCode: {OpCode[0]:X2}{OpCode[1]:X2} - {currentOpCode.disassembled}",
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
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.DarkSlateGray);
            graphics.GraphicsDevice.Textures[0] = null;

            canvas.SetData<UInt32>(Emulator.display, 0, 64 * 32);
            memoryMap.SetData<UInt32>(Disassembler.memoryMap, 0, 64 * 64);
            
            spriteBatch.Begin(samplerState: SamplerState.PointClamp, blendState: BlendState.AlphaBlend);

            spriteBatch.Draw(canvas, new Rectangle(0, 40, 1280, 640), Color.White);

            if (showDebug == true){
                spriteBatch.Draw(memoryMap, new Rectangle(1280 - 64*5, 720 - 64*5, 64*5, 64*5), Color.White * 0.75f);
                Vector2 textLeftPoint = new Vector2(0,0);
                Vector2 textPosition = new Vector2(10,10);
                spriteBatch.DrawString(genericFont, info, textPosition, Color.Red, 0, textLeftPoint, 1.0f, SpriteEffects.None, 0.5f);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}