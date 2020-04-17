using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace C8TypoEmu
{
    static class Disassembler
    {
        public static void DisassembleROM(byte[] ROM)
        {
            Byte[] OpCode = new Byte[2];

            for (short pc = 0x200; pc < 0x200 + ROM.Length; pc += 0x2)
            {
                OpCode[0] = ROM[pc - 0x200];
                OpCode[1] = ROM[pc - 0x1FF];
                var currentCode = DisassembleOpCode(OpCode, pc);
                Console.WriteLine($"{pc,0:X4} | {OpCode[0],0:X2}{OpCode[1],0:X2} | {currentCode.disassembled.PadRight(26)}-  {currentCode.description}");
            }
        }

        private static (string disassembled, string description) DisassembleOpCode(byte[] OpCode, short ProgramCounter)
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
                    {// Handle 00E-
                        case 0x0:
                        {
                            return ($"LD   V{x,0:X1}, V{y,0:X1}", $"Set V{x,0:X1} = V{y,0:X1}");
                        }
                    }
                    return ("", "Unknown 8xy-");
                }
                default:
                {
                    return ("", "Unknown instruction");
                }
            }
        }

        public static void BytestoBitmap(byte[] ROM, int width = 0xF)
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
            bitmap.Save("rom.bmp");
        }
    }
}