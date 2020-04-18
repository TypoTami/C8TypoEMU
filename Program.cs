using System;
using System.IO;

namespace C8TypoEmu
{
    class Program
    {
        private static class CurrentState
        {
            // http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#2.2

            static public byte[] registers = new byte[0x10]; // 16, 8-Bit registers - V0 to VF (VF doubles as a flag for some instructions)
            static public short registerI; // Generally used to store memory addresses
            static public short programCounter = 0x200; // 16-Bit program counter. Starts at 0x200
            static public byte stackPointer; // Stack pointer. Self explanatory
            static public short[] stack = new short[0x10]; // Holds up to 16, 16-Bit memory addresses that the interpreter should return to after subroutines
            static public byte delayTimer = 0x00;
            static public byte soundTimer = 0x00;
            static public byte[] memory = new byte[4096];
            static public byte[] currentROM;
        }

        static readonly byte[,] font = new byte[16,5] // This is our default font.
        {
            {0xF0, 0x90, 0x90, 0x90, 0xF0}, // 0
            {0x20, 0x60, 0x20, 0x20, 0x70}, // 1
            {0xF0, 0x10, 0xF0, 0x80, 0xF0}, // 2
            {0xF0, 0x10, 0xF0, 0x10, 0xF0}, // 3
            {0x90, 0x90, 0xF0, 0x10, 0x10}, // 4
            {0xF0, 0x80, 0xF0, 0x10, 0xF0}, // 5
            {0xF0, 0x80, 0xF0, 0x90, 0xF0}, // 6
            {0xF0, 0x10, 0x20, 0x40, 0x40}, // 7
            {0xF0, 0x90, 0xF0, 0x90, 0xF0}, // 8
            {0xF0, 0x90, 0xF0, 0x10, 0xF0}, // 9
            {0xF0, 0x90, 0xF0, 0x90, 0x90}, // A
            {0xE0, 0x90, 0xE0, 0x90, 0xE0}, // B
            {0xF0, 0x80, 0x80, 0x80, 0xF0}, // C
            {0xE0, 0x90, 0x90, 0x90, 0xE0}, // D
            {0xF0, 0x80, 0xF0, 0x80, 0xF0}, // E
            {0xF0, 0x80, 0xF0, 0x80, 0x80}  // F
        };

        static void Main(string[] args)
        {
            //CurrentState.currentROM = File.ReadAllBytes("testRoms/Fishie.ch8");
            //CurrentState.currentROM = File.ReadAllBytes("testRoms/test_opcode.ch8");
            CurrentState.currentROM = File.ReadAllBytes("testRoms/BLINKY.ch8");

            Disassembler.BytestoBitmap(CurrentState.currentROM, 0xF, "rom.bmp");

            //Disassembler.DisassembleROM(CurrentState.currentROM);

            Init();

            using(var app = new App())
            app.Run();
        }

        static void Init()
        {
            // Put font into memory
            int pos = 0x00;
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    CurrentState.memory[pos] = font[i,j];
                    pos++;
                }
            }
            // Put rom into memory
            pos = 0x200;
            for (int i = 0; i < CurrentState.currentROM.Length; i++)
            {
                CurrentState.memory[pos] = CurrentState.currentROM[i];
                pos++;
            }

            Disassembler.BytestoBitmap(CurrentState.memory, 0x3F, "memory.bmp");
        }
    }
}
