using System;
using System.Linq;
using System.Collections;

namespace C8TypoEmu
{
    static class Emulator
    {
        // http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#2.2
        static public byte[] registers = new byte[0x10]; // 16, 8-Bit registers - V0 to VF (VF doubles as a flag for some instructions)
        static public short registerI = 0x000; // Generally used to store memory addresses
        static public short programCounter = 0x200; // 16-Bit program counter. Starts at 0x200
        static public byte stackPointer; // Stack pointer. Self explanatory
        static public short[] stack = new short[16]; // Holds up to 16, 16-Bit memory addresses that the interpreter should return to after subroutines
        static public byte delayTimer = 0x00;
        static public byte soundTimer = 0x00;
        static public byte[] memory = new byte[4096];
        static public byte[] currentROM;
        static public readonly byte[,] font = new byte[16,5] // This is our default font.
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
        static public UInt32[] display = new UInt32[64 * 32];

        static public bool DrawPixel(int x, int y, UInt32 colour = 0xFFFFFFFF)
        {
            bool collision = false;
            if (Emulator.display[x+(64*y)] == colour)
            {
                collision = true;
            }
            else
            {
                Emulator.display[x+(64*y)] = colour;
            }
            return collision;            
        }

        static public void IncrementTimers()
        {
            if (delayTimer != 0)
            {
                delayTimer--;
            }
            if (soundTimer != 0)
            {
                soundTimer--;
            }
        }

        static public void ExecuteNextOpCode()
        {
            Byte[] OpCode = new Byte[2];
            OpCode[0] = memory[programCounter];
            programCounter++;
            OpCode[1] = memory[programCounter];
            programCounter++;

            // Split the opcode into a bunch of pieces
            byte firstNibble = (byte)(OpCode[0] >> 4); // Highest 4 bits of the instruction
            byte lastNibble = (byte)((byte)OpCode[1] & (byte)0xF); // Lowest 4 bits of the instruction.
            short addr = (short)(((OpCode[0] & 0xF) << 0x8) | OpCode[1]); // Lowest 12 bits of the instruction. (C# does not have byte literals. Why!)
            byte x = (byte)(OpCode[0] & 0xF); // Lower 4 bits of the high byte of the instruction.
            byte y = (byte)(OpCode[1] >> 4); // Upper 4 bits of the low byte of the instruction.
            byte kk = OpCode[1]; // Lowest 8 bits of the instruction.

            switch (firstNibble)
            {
                case 0x0: 
                {// Handle 0---
                    switch(OpCode[1]) 
                    {// Handle 00E-
                        case 0xE0:
                        {// Handle 00E0 - CLS
                            for (int i = 0; i < 64 * 32; i++)
                            {
                                display[i] = 0xFF000000;
                            }
                            break;
                        }
                        case 0xEE:
                        {// Handle 00EE - RET
                            programCounter = stack[stackPointer];
                            if (stackPointer != 0)
                            {
                                stackPointer--;
                            }
                            break;
                        }
                        default:
                        {// Handle 0nnn - SYS addr (Ignored)
                            break;
                        }
                    }
                    break;
                }
                case 0x1: 
                {// Handle 1nnn - JP addr
                    programCounter = addr;
                    break;
                }
                case 0x2:
                {// Handle 2nnn - CALL addr
                    stack[stackPointer] = programCounter;
                    stackPointer++;
                    programCounter = addr;
                    break;
                }
                case 0x3:
                {// Handle 3xkk - SE Vx, byte
                    if (registers[x] == kk)
                    {
                        programCounter += 2;
                    }
                    break;
                }
                case 0x4:
                {// Handle 4xkk - SE Vx, byte
                    if (registers[x] != kk)
                    {
                        programCounter += 2;
                    }
                    break;
                }
                case 0x5:
                {// Handle 5xy0 - SE Vx, Vy
                    if (registers[x] == registers[y])
                    {
                        programCounter += 2;
                    }
                    break;
                }
                case 0x6:
                {// Handle 6xkk - LD Vx, byte
                    registers[x] = kk;
                    break;
                }
                case 0x7:
                {// Handle 7xkk - ADD Vx, byte
                    registers[x] = (byte)(registers[x] + kk);
                    break;
                }
                case 0x8:
                {// Handle 8xy-
                    switch(lastNibble) 
                    {
                        case 0x0:
                        {// Handle 8xy0 - LD Vx, Vy
                            registers[x] = registers[y];
                            break;
                        }
                        case 0x1:
                        {// Handle 8xy1 - OR Vx, Vy
                            registers[x] = (byte)(registers[x] | registers[y]);
                            break;
                        }
                        case 0x2:
                        {// Handle 8xy2 - AND Vx, Vy
                            registers[x] = (byte)(registers[x] & registers[y]);
                            break;
                        }
                        case 0x3:
                        {// Handle 8xy3 - XOR Vx, Vy
                            registers[x] = (byte)(registers[x] ^ registers[y]);
                            break;
                        }
                        case 0x4:
                        {// Handle 8xy4 - ADD Vx, Vy
                            int sum = registers[x] + registers[y];
                            if (sum > 0xFF)
                            {
                                registers[x] = 0xFF;
                                registers[0xF] = 1;
                            }
                            break;
                        }
                        case 0x5:
                        {// Handle 8xy5 - SUB Vx, Vy
                            if (registers[x] > registers[y])
                            {
                                registers[x] = (byte)(registers[x] - registers[y]);
                                registers[0xF] = 1;
                            }
                            else 
                            {
                                registers[x] = (byte)(registers[x] - registers[y]);
                                registers[0xF] = 0;
                            }
                            break;
                        }
                        case 0x6:
                        {// Handle 8xy6 - SHR Vx
                            BitArray currentByte = new BitArray(new byte[] { registers[x] });
                            if (currentByte.Get(0) == true)
                            {
                                registers[0xF] = 1;
                            }
                            else
                            {
                                registers[0xF] = 0;
                            }
                            registers[x] = (byte)(registers[x] >> 1);
                            break;
                        }
                        case 0x7:
                        {// Handle 8xy7 - SUBN Vx, Vy
                            if (registers[y] > registers[x])
                            {
                                registers[x] = (byte)(registers[y] - registers[x]);
                                registers[0xF] = 1;
                            }
                            else 
                            {
                                registers[x] = (byte)(registers[y] - registers[x]);
                                registers[0xF] = 0;
                            }
                            break;
                        }
                        case 0xE:
                        {// Handle 8xyE - SHL Vx
                            BitArray currentByte = new BitArray(new byte[] { registers[x] });
                            if (currentByte.Get(7) == true)
                            {
                                registers[0xF] = 1;
                            }
                            else
                            {
                                registers[0xF] = 0;
                            }
                            registers[x] = (byte)(registers[x] << 1);
                            break;
                        }
                    }
                    break;
                }
                case 0xA:
                {// Handle Annn - LD I, addr
                    registerI = addr;
                    break;
                }
                case 0xD:
                {// Handle Dxyn - DRW Vx, Vy, nibble
                    short iOffset = registerI;
                    BitArray currentLine;

                    for (int line = 0; line < lastNibble; line++)
                    {
                        currentLine = new BitArray(new byte[] { memory[line + registerI] });
                        for (int row = 8; row > 0; row--)
                        {
                            if (currentLine[row - 1] == true)
                            {
                                if (DrawPixel(row + x - 4, line + y) == true)
                                {
                                    registers[0xF] = 0x1;
                                }
                            }
                        }
                    }
                    break;
                }
                default:
                {
                    break;
                }
            }
        }
    }
}

