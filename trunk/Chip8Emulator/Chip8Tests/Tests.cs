using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Chip8Emulator;

namespace Chip8Tests
{
    [TestClass]
    public class Chip8Tests
    {
        [TestMethod]
        public void Opcode00EETest1()
        {
            byte[] rom = new byte[]
            {
                0x12,
                0x34
            };
            Chip8 cpu = GetCpuInstance(rom);
        }

        [TestMethod]
        public void Opcode1NNNTest1()
        {
            byte[] rom = new byte[]
            {
                0x12,
                0x34
            };
            Chip8 cpu = GetCpuInstance(rom);

            cpu.ExecuteNextOpcode();

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

            cpu.ExecuteNextOpcode();

            Assert.AreEqual(0xFFF, cpu.ProgramCounter);
        }

        [TestMethod]
        public void Opcode2NNNTest1()
        {
            byte[] rom = new byte[]
            {
                0x2F,
                0xFF
            };
            Chip8 cpu = GetCpuInstance(rom);

            cpu.ExecuteNextOpcode();

            Assert.AreEqual(0xFFF, cpu.ProgramCounter);
            Assert.AreEqual(0x202, cpu.Stack.Peek());
        }

        private Chip8 GetCpuInstance(byte[] rom)
        {
            Chip8 cpu = new Chip8();
            cpu.LoadRom(rom);
            return cpu;
        }
    }
}