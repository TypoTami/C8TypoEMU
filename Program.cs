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

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            CurrentState.currentROM = File.ReadAllBytes("testRoms/test_opcode.ch8");

            Disassembler.DisassembleROM(CurrentState.currentROM);
        }
    }
}
