﻿using Chip8Emulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Chip8Tests
{
    [TestClass]
    public class Chip8Tests
    {
        /// <summary>
        /// 0NNN Calls RCA 1802 program at address NNN.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void Opcode0NNNTest1()
        {
        }

        /// <summary>
        /// 00E0 Clears the screen.
        /// </summary>
        [TestMethod]
        public void Opcode00E0Test1()
        {
            byte[] rom = new byte[] { };
            Chip8 cpu = GetCpuInstance(rom);

            for (int x = 0; x < cpu.ScreenData.GetUpperBound(0); x++)
            {
                for (int y = 0; y < cpu.ScreenData.GetUpperBound(1); y++)
                {
                    Assert.AreEqual(0, cpu.ScreenData[x, y, 0]);
                }
            }
        }

        /// <summary>
        /// 00EE Returns from a subroutine.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void Opcode00EETest1()
        {
        }

        /// <summary>
        /// 1NNN Jumps to address NNN.
        /// </summary>
        [TestMethod]
        public void Opcode1NNNTest1()
        {
            byte[] rom = new byte[]
            {
                0x12,
                0x34
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x234, cpu.ProgramCounter);
        }

        [TestMethod]
        public void Opcode1NNNTest2()
        {
            byte[] rom = new byte[]
            {
                0x1F,
                0xFF
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0xFFF, cpu.ProgramCounter);
        }

        /// <summary>
        /// 2NNN Calls subroutine at NNN.
        /// </summary>
        [TestMethod]
        public void Opcode2NNNTest1()
        {
            byte[] rom = new byte[]
            {
                0x2F,
                0xFF
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0xFFF, cpu.ProgramCounter);
            Assert.AreEqual(0x202, cpu.Stack.Peek());
        }

        /// <summary>
        /// 3XNN Skips the next instruction if VX equals NN.
        /// </summary>
        [TestMethod]
        public void Opcode3XNNTest1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set V1 0xA1
                0xA1,
                0x62, // Set V2 0x00
                0x00,
                0x63, // Set V3 0x00
                0x00,
                0x31, // Skip if V1 == 0xA1
                0xA1,
                0x82, // Set V2 V1
                0x10,
                0x83, // Set V3 V1
                0x10
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0, cpu.Registers[2]);
            Assert.AreEqual(0xA1, cpu.Registers[3]);
        }

        /// <summary>
        /// 4XNN Skips the next instruction if VX doesn't equal NN.
        /// </summary>
        [TestMethod]
        public void Opcode4XNNTest1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set V1 0xA1
                0xA1,
                0x62, // Set V2 0x00
                0x00,
                0x63, // Set V3 0x00
                0x00,
                0x41, // Skip if V1 != 0xA1
                0xA1,
                0x82, // Set V2 V1
                0x10,
                0x83, // Set V3 V1
                0x10
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0xA1, cpu.Registers[2]);
            Assert.AreEqual(0xA1, cpu.Registers[3]);
        }

        /// <summary>
        /// 5XY0 Skips the next instruction if VX equals VY.
        /// </summary>
        [TestMethod]
        public void Opcode5XY0Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0xA1 to V1
                0xA1,
                0x62, // Set 0xA1 to V2
                0xA1,
                0x63, // Set 0x00 to V3
                0x00,
                0x51, // Skip if V1 == V2
                0x20,
                0x62, // Set 0xAA to V2
                0xAA,
                0x63, // Set 0xBB to V3
                0xBB
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0xA1, cpu.Registers[2]);
            Assert.AreEqual(0xBB, cpu.Registers[3]);
        }

        /// <summary>
        /// 6XNN Sets VX to NN.
        /// </summary>
        [TestMethod]
        public void Opcode6XNNTest1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set V1 0xAA
                0xAA,
                0x62, // Set V2 0xBB
                0xBB,
                0x65, // Set V5 0xEE
                0xEE
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0xAA, cpu.Registers[1]);
            Assert.AreEqual(0xBB, cpu.Registers[2]);
            Assert.AreEqual(0xEE, cpu.Registers[5]);
        }

        /// <summary>
        /// 7XNN Adds NN to VX.
        /// </summary>
        [TestMethod]
        public void Opcode7XNNTest1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set V1 0x00
                0x00,
                0x62, // Set V2 0xF1
                0xF1,
                0x63, // Set V5 0xFF
                0xFF,
                0x71, // Add 0xF to V1
                0x0F,
                0x72, // Add 0xF to V2
                0x0F,
                0x73, // Add 0xF to V3
                0x0F
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x0F, cpu.Registers[1]);
            Assert.AreEqual(0x00, cpu.Registers[2]);
            Assert.AreEqual(0x0E, cpu.Registers[3]);
        }

        /// <summary>
        /// 8XY0 Sets VX to the value of VY.
        /// </summary>
        [TestMethod]
        public void Opcode8XY0Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x11 to V1
                0x11,
                0x62, // Set 0x22 to V2
                0x22,
                0x82, // Set V1 = V2
                0x10
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x11, cpu.Registers[1]);
            Assert.AreEqual(0x11, cpu.Registers[2]);
        }

        /// <summary>
        /// 8XY1 Sets VX to VX or VY.
        /// </summary>
        [TestMethod]
        public void Opcode8XY1Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x01 to V1
                0x01,
                0x62, // Set 0x04 to V2
                0x04,
                0x81, // Set V1 = V1 | V2
                0x21
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x05, cpu.Registers[1]);
            Assert.AreEqual(0x04, cpu.Registers[2]);
        }

        /// <summary>
        /// 8XY2 Sets VX to VX and VY.
        /// </summary>
        [TestMethod]
        public void Opcode8XY2Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x05 to V1
                0x05,
                0x62, // Set 0x01 to V2
                0x01,
                0x81, // Set V1 = V1 & V2
                0x22
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x01, cpu.Registers[1]);
            Assert.AreEqual(0x01, cpu.Registers[2]);
        }

        /// <summary>
        /// 8XY3 Sets VX to VX xor VY.
        /// </summary>
        [TestMethod]
        public void Opcode8XY3Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x05 to V1
                0x05,
                0x62, // Set 0x01 to V2
                0x01,
                0x81, // Set V1 = V1 ^ V2
                0x23
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x04, cpu.Registers[1]);
            Assert.AreEqual(0x01, cpu.Registers[2]);
        }

        /// <summary>
        /// 8XY4 Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
        /// </summary>
        [TestMethod]
        public void Opcode8XY4Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0xFF to V1
                0xFF,
                0x62, // Set 0x01 to V2
                0x01,
                0x81, // Set V1 = V1 + V2
                0x24
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x00, cpu.Registers[0x01]);
            Assert.AreEqual(0x01, cpu.Registers[0x0F]);
        }

                /// <summary>
        /// 8XY4 Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
        /// </summary>
        [TestMethod]
        public void Opcode8XY4Test2()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0xFE to V1
                0xFE,
                0x62, // Set 0x01 to V2
                0x01,
                0x81, // Set V1 = V1 + V2
                0x24
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0xFF, cpu.Registers[0x01]);
            Assert.AreEqual(0x00, cpu.Registers[0x0F]);
        }

        /// <summary>
        /// 8XY5 VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
        /// </summary>
        [TestMethod]
        public void Opcode8XY5Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0xFE to V1
                0xFE,
                0x62, // Set 0x01 to V2
                0x01,
                0x81, // Set V1 = V1 - V2
                0x25
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0xFD, cpu.Registers[0x01]);
            Assert.AreEqual(0x01, cpu.Registers[0x0F]);
        }

        /// <summary>
        /// 8XY5 VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
        /// </summary>
        [TestMethod]
        public void Opcode8XY5Test2()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x0A to V1
                0x0A,
                0x62, // Set 0x0B to V2
                0x0B,
                0x81, // Set V1 = V1 - V2
                0x25
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0xFF, cpu.Registers[0x01]);
            Assert.AreEqual(0x00, cpu.Registers[0x0F]);
        }

        /// <summary>
        /// 8XY6 Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift.
        /// </summary>
        [TestMethod]
        public void Opcode8XY6Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x08 to V1
                0x08,
                0x81, // Shift V1
                0x06
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x04, cpu.Registers[0x01]);
            Assert.AreEqual(0x00, cpu.Registers[0x0F]);
        }

        /// <summary>
        /// 8XY6 Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift.
        /// </summary>
        [TestMethod]
        public void Opcode8XY6Test2()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x09 to V1
                0x09,
                0x81, // Shift V1 to right
                0x06
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x04, cpu.Registers[0x01]);
            Assert.AreEqual(0x01, cpu.Registers[0x0F]);
        }

        /// <summary>
        /// 8XY7 Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
        /// </summary>
        [TestMethod]
        public void Opcode8XY7Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x09 to V1
                0x09,
                0x62, // Set 0x0A to V2
                0x0A,
                0x81, // V1 = V2 - V1
                0x27
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x01, cpu.Registers[0x01]);
            Assert.AreEqual(0x01, cpu.Registers[0x0F]);
        }

        /// <summary>
        /// 8XY7 Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
        /// </summary>
        [TestMethod]
        public void Opcode8XY7Test2()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x09 to V1
                0x09,
                0x62, // Set 0x0A to V2
                0x08,
                0x81, // V1 = V2 - V1
                0x27
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0xFF, cpu.Registers[0x01]);
            Assert.AreEqual(0x00, cpu.Registers[0x0F]);
        }

        /// <summary>
        /// 8XYE Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift.
        /// </summary>
        [TestMethod]
        public void Opcode8XYETest1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x08 to V1
                0x08, // 00000100
                0x81, // Shift V1 to left
                0x0E
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x10, cpu.Registers[0x01]);
            Assert.AreEqual(0x00, cpu.Registers[0x0F]);
        }

        /// <summary>
        /// 8XYE Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift.
        /// </summary>
        [TestMethod]
        public void Opcode8XYETest2()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x88 to V1
                0x88, // 10000100
                0x81, // Shift V1 to left
                0x0E
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x10, cpu.Registers[0x01]);
            Assert.AreEqual(0x01, cpu.Registers[0x0F]);
        }

        /// <summary>
        /// 9XY0 Skips the next instruction if VX doesn't equal VY.
        /// </summary>
        [TestMethod]
        public void Opcode9XY0Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x08 to V1
                0x08,
                0x62, // Set 0x09 to V2
                0x09,
                0x63, // Set 0x08 to V3
                0x08,
                0x64, // Set 0x00 to V4
                0x00,
                0x65, // Set 0x00 to V5
                0x00,

                0x91, // Skip if V1 != V2
                0x20,
                0x64, // Set 0x01 to V4
                0x01,

                0x91, // Skip if V1 != V3
                0x30,
                0x65, // Set 0x01 to V5
                0x01
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x00, cpu.Registers[0x04]);
            Assert.AreEqual(0x01, cpu.Registers[0x05]);
        }

        /// <summary>
        /// ANNN Sets I to the address NNN.
        /// </summary>
        [TestMethod]
        public void OpcodeANNNTest1()
        {
            byte[] rom = new byte[]
            {
                0xA1, // Set 0x0123 to I
                0x23
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x0123, cpu.AddressI);
        }

        /// <summary>
        /// BNNN Jumps to the address NNN plus V0.
        /// </summary>
        [TestMethod]
        public void OpcodeBNNNTest1()
        {
            byte[] rom = new byte[]
            {
                0x60, // Set 0x02 to V0
                0x02,
                0xB1, // Set 0x0123 + V0 to ProgramCounter
                0x23
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x0125, cpu.ProgramCounter);
        }

        /// <summary>
        /// BNNN Jumps to the address NNN plus V0.
        /// </summary>
        [TestMethod]
        public void OpcodeBNNNTest2()
        {
            byte[] rom = new byte[]
            {
                0x60, // Set 0x02 to V0
                0x02,
                0xBF, // Set 0x0FFF + V0 to ProgramCounter
                0xFF
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x01, cpu.ProgramCounter);
        }

        /// <summary>
        /// CXNN Sets VX to a random number and NN.
        /// </summary>
        [TestMethod]
        public void OpcodeCXNNTest1()
        {
            byte[] rom = new byte[]
            {
                0xC1, // RND V1
                0xAA, //10101010
                0xC2, // RND V2
                0xAA, //10101010
                0xC3, // RND V3
                0xFF  //10101010
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.IsFalse(cpu.Registers[0x01] == cpu.Registers[0x02] 
                            && cpu.Registers[0x01] == cpu.Registers[0x03]);
            Assert.AreEqual(0x00, cpu.Registers[0x01] & 0x55 /* 01010101 */);
        }

        /// <summary>
        /// DXYN Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels...
        /// </summary>
        [TestMethod]
        [Ignore]
        public void OpcodeDXYNTest1()
        {
        }

        /// <summary>
        /// EX9E Skips the next instruction if the key stored in VX is pressed.
        /// </summary>
        [TestMethod]
        public void OpcodeEX9ETest1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x02 to V1
                0x02,
                0x62, // Set 0x05 to V2
                0x05,
                0xE1, // Skip
                0x9E,
                0x62, // Set 0x06 to V2
                0x06,
                0x61, // Set 0x01 to V1
                0x01
            };
            Chip8 cpu = GetCpuInstance(rom, 2);
            cpu.KeyPressed(0x02);

            ExecuteCommandsNTime(cpu, 3);

            Assert.AreEqual(0x01, cpu.Registers[0x01]);
            Assert.AreEqual(0x05, cpu.Registers[0x02]);
        }

        /// <summary>
        /// EXA1 Skips the next instruction if the key stored in VX isn't pressed.
        /// </summary>
        [TestMethod]
        public void OpcodeEXA1Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x02 to V1
                0x02,
                0x62, // Set 0x05 to V2
                0x05,
                0xE1, // Skip
                0xA1,
                0x62, // Set 0x06 to V2
                0x06,
                0x61, // Set 0x01 to V1
                0x01
            };
            Chip8 cpu = GetCpuInstance(rom, 2);
            cpu.KeyPressed(0x03);

            ExecuteCommandsNTime(cpu, 3);

            Assert.AreEqual(0x01, cpu.Registers[0x01]);
            Assert.AreEqual(0x05, cpu.Registers[0x02]);
        }

        /// <summary>
        /// FX07 Sets VX to the value of the delay timer.
        /// </summary>
        [TestMethod]
        public void OpcodeFX07Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x05 to V1
                0x05,
                0xF1, // Set V1 to timer value
                0x15,
                0x61, // Set 0x00 to V1
                0x00,
                0xF1, // Set timer value to V1
                0x07
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x05, cpu.Registers[0x01]);
        }

        /// <summary>
        /// FX0A A key press is awaited, and then stored in VX.
        /// </summary>
        [TestMethod]
        public void OpcodeFX0ATest1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x00 to V1
                0x00,
                0xF1, // Wait for pressed key
                0x0A
            };
            Chip8 cpu = GetCpuInstance(rom, 5);
            cpu.KeyPressed(0x0B);

            ExecuteCommandsNTime(cpu, 1);

            Assert.AreEqual(0x0B, cpu.Registers[0x01]);
        }

        /// <summary>
        /// FX15 Sets the delay timer to VX.
        /// </summary>
        [TestMethod]
        public void OpcodeFX15Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x05 to V1
                0x05,
                0xF1, // Set V1 to timer value
                0x15,
                0x61, // Set 0x00 to V1
                0x00,
                0xF1, // Set timer value to V1
                0x07
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x05, cpu.Registers[0x01]);
        }

        /// <summary>
        /// FX18 Sets the sound timer to VX.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void OpcodeFX18Test1()
        {
        }

        /// <summary>
        /// FX1E Adds VX to I.[3]
        /// </summary>
        [TestMethod]
        public void OpcodeFX1ETest1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x05 to V1
                0x05,
                0xA1, // Set 0x0123 to I
                0x23,
                0xF1, // Add
                0x1E
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x0128, cpu.AddressI);
        }

        /// <summary>
        /// FX29 Sets I to the location of the sprite for the character in VX.
        /// </summary>
        [TestMethod]
        public void OpcodeFX29Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0x05 to V1
                0x05,
                0xA1, // Set 0x0123 to I
                0x23,
                0xF1, // Add
                0x29
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x05 * 5, cpu.AddressI);
        }

        /// <summary>
        /// FX33 Stores the Binary-coded decimal representation of VX...
        /// </summary>
        [TestMethod]
        public void OpcodeFX33Test1()
        {
            byte[] rom = new byte[]
            {
                0x61, // Set 0xFE to V1
                0xFE,
                0xA1, // Set 0x0123 to I
                0x23,
                0xF1, // Store to memory
                0x33,
                0xF3, // Read from memory
                0x65
            };
            Chip8 cpu = GetCpuInstance(rom);

            Assert.AreEqual(0x02, cpu.Registers[0]);
            Assert.AreEqual(0x05, cpu.Registers[1]);
            Assert.AreEqual(0x04, cpu.Registers[2]);
        }

        /// <summary>
        /// FX55 Stores V0 to VX in memory starting at address I.[4]
        /// </summary>
        [TestMethod]
        public void OpcodeFX55Test1()
        {
            byte[] rom = new byte[]
            {
                // Init values
                0xA1, // Set 0x0123 to I
                0x23,
                0x60, // Set 0x01 to V0
                0x01,
                0x61, // Set 0x02 to V1
                0x02,
                0x62, // Set 0x03 to V2
                0x03,
                // Action
                0xF2, // Store to memory
                0x55,
                // Cleare registers
                0x60, // Set 0x00 to V0
                0x00,
                0x61, // Set 0x00 to V1
                0x00,
                0x62, // Set 0x00 to V2
                0x00,
                // Action
                0xA1, // Set 0x0123 to I
                0x23,
                0xF2, // Read from memory
                0x65
            };
            Chip8 cpu = GetCpuInstance(rom, 5);

            Assert.AreEqual(0x125, cpu.AddressI);

            ExecuteCommandsNTime(cpu, 5);

            Assert.AreEqual(0x01, cpu.Registers[0]);
            Assert.AreEqual(0x02, cpu.Registers[1]);
            Assert.AreEqual(0x03, cpu.Registers[2]);
            Assert.AreEqual(0x01, cpu.Memory[0x123]);
            Assert.AreEqual(0x02, cpu.Memory[0x124]);
            Assert.AreEqual(0x03, cpu.Memory[0x125]);
        }

        /// <summary>
        /// FX65 Fills V0 to VX with values from memory starting at address I.[4]
        /// </summary>
        [TestMethod]
        public void OpcodeFX65Test1()
        {
            byte[] rom = new byte[]
            {
                // Init values
                0xA1, // Set 0x0123 to I
                0x23,
                0x60, // Set 0x01 to V0
                0x01,
                0x61, // Set 0x02 to V1
                0x02,
                0x62, // Set 0x03 to V2
                0x03,
                // Action
                0xF2, // Store to memory
                0x55,
                // Cleare registers
                0x60, // Set 0x00 to V0
                0x00,
                0x61, // Set 0x00 to V1
                0x00,
                0x62, // Set 0x00 to V2
                0x00,
                // Action
                0xA1, // Set 0x0123 to I
                0x23,
                0xF2, // Read from memory
                0x65
            };
            Chip8 cpu = GetCpuInstance(rom, 5);

            Assert.AreEqual(0x125, cpu.AddressI);

            ExecuteCommandsNTime(cpu, 5);

            Assert.AreEqual(0x01, cpu.Registers[0]);
            Assert.AreEqual(0x02, cpu.Registers[1]);
            Assert.AreEqual(0x03, cpu.Registers[2]);
            Assert.AreEqual(0x01, cpu.Memory[0x123]);
            Assert.AreEqual(0x02, cpu.Memory[0x124]);
            Assert.AreEqual(0x03, cpu.Memory[0x125]);
        }

        private void ExecuteCommandsNTime(Chip8 cpu, int number)
        {
            for (int i = 0; i < number; i++)
                cpu.ExecuteNextOpcode();
        }

        private Chip8 GetCpuInstance(byte[] rom, int steps = 0)
        {
            Chip8 cpu = new Chip8();
            cpu.LoadRom(rom);

            if (steps == 0)
                steps = rom.Length / 2;

            ExecuteCommandsNTime(cpu, steps);

            return cpu;
        }
    }
}

/*

Opearation codes

0NNN	Calls RCA 1802 program at address NNN.
00E0	Clears the screen.
00EE	Returns from a subroutine.
1NNN	Jumps to address NNN.
2NNN	Calls subroutine at NNN.
3XNN	Skips the next instruction if VX equals NN.
4XNN	Skips the next instruction if VX doesn't equal NN.
5XY0	Skips the next instruction if VX equals VY.
6XNN	Sets VX to NN.
7XNN	Adds NN to VX.
8XY0	Sets VX to the value of VY.
8XY1	Sets VX to VX or VY.
8XY2	Sets VX to VX and VY.
8XY3	Sets VX to VX xor VY.
8XY4	Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
8XY5	VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
8XY6	Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift.[2]
8XY7	Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
8XYE	Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift.[2]
9XY0	Skips the next instruction if VX doesn't equal VY.
ANNN	Sets I to the address NNN.
BNNN	Jumps to the address NNN plus V0.
CXNN	Sets VX to a random number and NN.
DXYN	Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels.
        Each row of 8 pixels is read as bit-coded (with the most significant bit of each byte displayed on the left)
        starting from memory location I; I value doesn't change after the execution of this instruction. As described
        above, VF is set to 1 if any screen pixels are flipped from set to unset when the sprite is drawn, and to 0
        if that doesn't happen.
EX9E	Skips the next instruction if the key stored in VX is pressed.
EXA1	Skips the next instruction if the key stored in VX isn't pressed.
FX07	Sets VX to the value of the delay timer.
FX0A	A key press is awaited, and then stored in VX.
FX15	Sets the delay timer to VX.
FX18	Sets the sound timer to VX.
FX1E	Adds VX to I.[3]
FX29	Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are
        represented by a 4x5 font.
FX33	Stores the Binary-coded decimal representation of VX, with the most significant of three digits at
        the address in I, the middle digit at I plus 1, and the least significant digit at I plus 2.
FX55	Stores V0 to VX in memory starting at address I.[4]
FX65	Fills V0 to VX with values from memory starting at address I.[4]

*/