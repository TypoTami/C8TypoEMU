using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace C8TypoEmu
{
    static class Disassembler
    {
        static public UInt32[] memoryMap = new UInt32[64 * 64];
        public static void DisassembleROM(byte[] ROM)
        {
            byte[] OpCode = new byte[2];

            for (short pc = 0; pc < ROM.Length; pc += 2)
            {
                OpCode[0] = ROM[pc];
                OpCode[1] = ROM[pc + 1];
                var currentCode = DisassembleOpCode(OpCode, pc);
                Console.WriteLine($"{pc,0:X4} | {OpCode[0],0:X2}{OpCode[1],0:X2} | {currentCode.disassembled.PadRight(26)}-  {currentCode.description}");
            }
        }

        public static (string disassembled, string description) DisassembleOpCode(byte[] OpCode, short ProgramCounter)
        {
            byte firstNibble = (byte)(OpCode[0] >> 4);
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
                        {
                            return ("CLS", "Clear the display");
                        }
                        case 0xEE:
                        {
                            return ("RET", "Return from a subroutine.");
                        }
                        default:
                        {// Handle 0nnn
                            return ("SYS  addr", "");
                        }
                    }
                }
                case 0x1:
                {// Handle 1nnn
                    return ($"JP   #{addr,0:X3}", $"Jump to location at: #{addr,0:X3}");
                }
                case 0x2:
                {// Handle 2nnn
                    return ($"CALL #{addr,0:X3}", $"Call subroutine at:  #{addr,0:X3}");
                }
                case 0x3:
                {// Handle 3xkk
                    return ($"SE   V{x,0:X1}, ${kk,0:X2}", $"Skip next instruction if V{x,0:X1} == ${kk,0:X2}");
                }
                case 0x4:
                {// Handle 4xkk
                    return ($"SNE  V{x,0:X1}, ${kk,0:X2}", $"Skip next instruction if V{x,0:X1} != ${kk,0:X2}");
                }
                case 0x5:
                {// Handle 5xy0
                    return ($"SE   V{x,0:X1}, V{y,0:X1}", $"Skip next instruction if V{x,0:X1} != V{y,0:X1}");
                }
                case 0x6:
                {// Handle 6xkk
                    return ($"LD   V{x,0:X1}, ${kk,0:X2}", $"Set V{x,0:X1} = ${kk,0:X2}");
                }
                case 0x7:
                {// Handle 7xkk 
                    return ($"ADD  V{x,0:X1}, ${kk,0:X2}", $"Set V{x,0:X1} = V{x,0:X1} + ${kk,0:X2}");
                }
                case 0x8:
                {// Handle 8xy-
                    switch(lastNibble) 
                    {
                        case 0x0:
                        {// Handle 8xy0
                            return ($"LD   V{x,0:X1}, V{y,0:X1}", $"Set V{x,0:X1} = V{y,0:X1}");
                        }
                        case 0x1:
                        {// Handle 8xy1
                            return ($"OR   V{x,0:X1}, V{y,0:X1}", $"Set V{x,0:X1} = V{x,0:X1} OR V{y,0:X1}");
                        }
                        case 0x2:
                        {// Handle 8xy2
                            return ($"AND  V{x,0:X1}, V{y,0:X1}", $"Set V{x,0:X1} = V{x,0:X1} AND V{y,0:X1}");
                        }
                        case 0x3:
                        {// Handle 8xy3
                            return ($"XOR  V{x,0:X1}, V{y,0:X1}", $"Set V{x,0:X1} = V{x,0:X1} XOR V{y,0:X1}");
                        }
                        case 0x4:
                        {// Handle 8xy4
                            return ($"ADD  V{x,0:X1}, V{y,0:X1}", $"Set V{x,0:X1} = V{x,0:X1} + V{y,0:X1}, If the result is greater than 8 bits: VF = 1");
                        }
                        case 0x5:
                        {// Handle 8xy5
                            return ($"SUB  V{x,0:X1}, V{y,0:X1}", $"Set V{x,0:X1} = V{x,0:X1} - V{y,0:X1}, If Vx > Vy, VF is set to 1. Then Vx = Vy - Vx");
                        }
                        case 0x6:
                        {// Handle 8xy6
                            return ($"SHR  V{x,0:X1}", $"Set V{x,0:X1} SHR 1, If least-significant bit of Vx is 1: VF = 1. Otherwise 0");
                        }
                        case 0x7:
                        {// Handle 8xy7
                            return ($"SUBN V{x,0:X1}, V{y,0:X1}", $"Set V{x,0:X1} = V{y,0:X1} - V{x,0:X1}, VF = NOT borrow ,If Vx > Vy, VF is set to 1. Then Vx = Vy - Vx");
                        }
                        case 0xE:
                        {// Handle 8xyE
                            return ($"SHL  V{x,0:X1}", $"Set V{x,0:X1} SHL 1, If most-significant bit of Vx is 1: VF = 1. Otherwise 0");
                        }
                    }
                    return ("", "Unknown 8xy-");
                }
                case 0x9:
                {// Handle 9xy0  
                    return ($"ADD  V{x,0:X1}, ${kk,0:X2}", $"Set V{x,0:X1} = V{x,0:X1} + ${kk,0:X2}");
                }
                case 0xA:
                {// Handle Annn   
                    return ($"LD   I, ${addr,0:X3}", $"Set I = ${addr,0:X3}");
                }
                case 0xB:
                {// Handle Bnnn   
                    return ($"JP   V0, ${addr,0:X3}", $"Jump to address PC = V0 + ${addr,0:X3}");
                }
                case 0xC:
                {// Handle Cxkk    
                    return ($"RND  V{x,0:X1}, ${kk,0:X2}", $"Set V{x,0:X1} = RND AND ${addr,0:X3}");
                }
                case 0xD:
                {// Handle Dxyn     
                    return ($"DRW  V{x,0:X1}, V{y,0:X1}, #{lastNibble,0:X1}", $"Draw 8 wide sprite at V{x,0:X1}, V{y,0:X1} of height #{lastNibble,0:X1} pixels");
                }
                case 0xE:
                {// Handle Ex--
                    switch (OpCode[1])
                    {
                        case 0x9E:
                        {// Handle Ex9E
                            return ($"SKP  V{x,0:X1}", $"Skip next instruction if key with the value of Vx is pressed");
                        }
                        case 0xA1:
                        {// Handle ExA1
                            return ($"SKNP V{x,0:X1}", $"Skip next instruction if key with the value of Vx is NOT pressed");
                        }
                    }
                    return ("", "Unknown Ex--");
                }
                case 0xF:
                {// Handle Fx--
                    switch (OpCode[1])
                    {
                        case 0x07:
                        {// Handle Fx07
                            return ($"LD   V{x,0:X1}, DT", $"Set V{x,0:X1} = Delay timer");
                        }
                        case 0x0A:
                        {// Handle Fx0A
                            return ($"LD   V{x,0:X1}, K", $"Wait for a keypress. Then Set V{x,0:X1} = Value of key pressed");
                        }
                        case 0x15:
                        {// Handle Fx15
                            return ($"LD   DT, V{x,0:X1}", $"Set Delay timer = V{x,0:X1}");
                        }
                        case 0x18:
                        {// Handle Fx18
                            return ($"LD   ST, V{x,0:X1}", $"Set Sound timer = V{x,0:X1}");
                        }
                        case 0x1E:
                        {// Handle Fx1E
                            return ($"ADD  I, V{x,0:X1}", $"Set I = I + V{x,0:X1}");
                        }
                        case 0x29:
                        {// Handle Fx29
                            return ($"LD   F, V{x,0:X1}", $"Set I = location of sprite for digit V{x,0:X1}");
                        }
                        case 0x33:
                        {// Handle Fx33
                            return ($"LD   B, V{x,0:X1}", $"Store BCD representation of V{x,0:X1} in memory locations: I, I+1, I+2");
                        }
                        case 0x55:
                        {// Handle Fx55
                            return ($"LD   [I], V{x,0:X1}", $"Store registers V0 though V{x,0:X1} in memory starting at I");
                        }
                        case 0x65:
                        {// Handle Fx65
                            return ($"LD   V{x,0:X1}, [I]", $"Read registers V0 though V{x,0:X1} in memory starting at I");
                        }
                    }
                    return ("", "Unknown Fx--");
                }
                default:
                {
                    return ("", "Unknown instruction");
                }
            }
        }

        public static void BytestoBitmap(byte[] ROM, int width = 0xF, string output = "output.bmp")
        {
            width += 1;
            int height = (int)Math.Ceiling((double)ROM.Length / (double)width);
            var bitmap = new Image<Rgba32>(width, height);
            
            int i = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width & i < ROM.Length; x++)
                {
                    bitmap[x,y] = new Rgba32 (ROM[i], ROM[i], ROM[i]);
                    i++;
                }
            }
            bitmap.Save(output);
        }
        public static void MapMemory(byte[] memory, int programCounter)
        {
            for (int i = 0; i < 4096; i++)
            {
                memoryMap[i] = 0xFF000000 + memory[i];
            }
            memoryMap[programCounter] = memoryMap[programCounter] + 0x0000FF00;
        }
    }
}