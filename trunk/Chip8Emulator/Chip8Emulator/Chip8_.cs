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
using System.Collections.Generic;

namespace Chip8Emulator
{
    public class Chip8_
    {
        const int ROMSIZE = 0xFFF; // Размер ROM

        byte[] Memory = new byte[0xFFF]; // 0xFFF байтов памяти
        byte[] Registers = new byte[0x0F]; // 16 регистров по 1 байту
        UInt16 AddressI = new ushort(); // 16-битный адресный регистр I
        UInt16 ProgramCounter = new ushort(); // 16-битный программный счетчик
        Stack<UInt16> Stack = new Stack<ushort>(); // 16-битный стек

        byte[] KeyState = new byte[0x0F]; // Клавиатура
        byte DelayTimer = new byte(); // Таймер задержки
        byte SoundTimer = new byte(); // Звуковой таймер
        public byte[,] ScreenData = new byte[64, 32]; // Дисплей

        void CPUReset() // "Перезагружает" эмулятор очищая переменные
        {
            AddressI = 0;
            ProgramCounter = 0x200;
            for (int i = 0; i < Registers.GetLength(0); i++)
            {
                Registers[i] = 0;
            }
            for (int i = 0; i < Memory.GetLength(0); i++)
            {
                Memory[i] = 0;
            }
            for (int i = 0; i < KeyState.GetLength(0); i++)
            {
                KeyState[i] = 0;
            }
            DelayTimer = 0;
            SoundTimer = 0;
        }

        int GetKeyPressed() // Возвращает нажатую клавишу
        {
            int res = -1;

            for (int i = 0; i < 16; i++)
            {
                if (KeyState[i] > 0)
                    return i;
            }

            return res;
        }



        // Clear screen
        private void Opcode00E0()
        {
        }

        // Возвращение из метода
        private void Opcode00EE()
        {
            ProgramCounter = Stack.Pop();
        }

        //Jumps to address NNN.
        private void Opcode1NNN(UInt16 opcode)
        {
            ProgramCounter = (UInt16)(opcode & 0x0FFF);
        }

        // Вызов метода по адресу NNN
        private void Opcode2NNN(UInt16 opcode)
        {
            Stack.Push(ProgramCounter);
            ProgramCounter = (UInt16)(opcode & 0x0FFF);
        }

        // Пропускает следующую инструкцию если VX == NN
        private void Opcode3XNN(UInt16 opcode)
        {
            int nn = opcode & 0x00FF;
            int regx = opcode & 0x0F00;
            regx >>= 8;

            if (Registers[regx] == nn)
                ProgramCounter += 2;
        }

        // Пропускает следующую инструкцию если VX != NN
        private void Opcode4XNN(UInt16 opcode)
        {
            int nn = opcode & 0x00FF;
            int regx = opcode & 0x0F00;
            regx >>= 8;

            if (Registers[regx] != nn)
                ProgramCounter += 2;
        }

        // Пропускает следующую инструкцию если VX == VY
        private void Opcode5XY0(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            if (Registers[regx] == Registers[regy])
                ProgramCounter += 2;
        }

        // Присваивает VX значение nn
        private void Opcode6XNN(UInt16 opcode)
        {
            int nn = opcode & 0x00FF;
            int regx = opcode & 0x0F00;
            regx >>= 8;

            Registers[regx] = (byte)nn;
        }

        // Прибавляет NN к vx
        private void Opcode7XNN(UInt16 opcode)
        {
            int nn = opcode & 0x00FF;
            int regx = opcode & 0x0F00;
            regx >>= 8;

            Registers[regx] += (byte)nn;
        }

        // Присваивает vx значение vy
        private void Opcode8XY0(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            Registers[regx] = Registers[regy];
        }

        // VX = VX | VY
        void Opcode8XY1(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            Registers[regx] = (byte)(Registers[regx] | Registers[regy]);
        }

        // VX = VX & VY
        private void Opcode8XY2(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            Registers[regx] = (byte)(Registers[regx] & Registers[regy]);
        }

        // VX = VX xor VY
        private void Opcode8XY3(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            Registers[regx] = (byte)(Registers[regx] ^ Registers[regy]);
        }

        // Прибавляет vy к vx. Присваивает флагу значение 1 при переполнении, иначе 0
        void Opcode8XY4(UInt16 opcode)
        {
            //Registers[0xF] = 0;
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            int value = Registers[regx] + Registers[regy];

            if (value > 255)
                Registers[0xF] = 1;
            else
                Registers[0xF] = 0;

            Registers[regx] = (byte)(Registers[regx] + Registers[regy]);
        }

        // Вычитает vy из vx. Присваивает флагу значение 0 если vy > vx, иначе 1
        // VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
        private void Opcode8XY5(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            if (Registers[regx] < Registers[regy])
                Registers[0xF] = 0;
            else
                Registers[0xF] = 1;

            Registers[regx] = (byte)(Registers[regx] - Registers[regy]);
        }

        // Сдвигает VX вправо на 1 разряд. VF присваивается значение последнего значащего бита VX перед сдвигом.
        private void Opcode8XY6(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;

            Registers[0xF] = (byte)(Registers[regx] & 0x1);
            Registers[regx] >>= 1;
        }

        // Присваивает VX значение VY минус VX. VF присваивается значение 0 если VY < VX, иначе 1.
        // Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
        private void Opcode8XY7(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            if (Registers[regy] < Registers[regx])
                Registers[0xF] = 0;
            else
                Registers[0xF] = 1;

            Registers[regx] = (byte)(Registers[regy] - Registers[regx]);
        }

        // Сдвигает VX влево на 1 разряд. VF присваивается значение последнего значащего бита VX перед сдвигом.
        private void Opcode8XYE(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;

            Registers[0xF] = (byte)(Registers[regx] >> 7);
            Registers[regx] <<= 1;
        }

        // Пропускает следующую инструкцию если VX != VY
        void Opcode9XY0(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            if (Registers[regx] != Registers[regy])
                ProgramCounter += 2;
        }

        // Присваивает I значение nnn
        void OpcodeANNN(UInt16 opcode)
        {
            AddressI = (byte)(opcode & 0x0FFF);
        }

        // Переход к адресу NNN + V0
        void OpcodeBNNN(UInt16 opcode)
        {
            int nnn = opcode & 0x0FFF;
            ProgramCounter = (byte)(Registers[0] + nnn);
        }

        // Присваивает vx значение random + NN
        void OpcodeCXNN(UInt16 opcode)
        {
            Random rand = new Random();
            int nn = opcode & 0x00FF;
            int regx = opcode & 0x0F00;
            regx >>= 8;

            Registers[regx] = (byte)(rand.Next() & nn);
        }

        // Рисует спрайт в координатах (VX, VY) шириной 8 и высотой N пикселей.
        // VF присваивается 1 если пиксель переключается из 1 в 0 при отрисовке спрайта,
        // иначе присваивается 0
        void OpcodeDXYN(UInt16 opcode)
        {

        }

        // Пропускает следующую инструкцию, если нажата клавиша хранящаяся в VX.
        void OpcodeEX9E(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int key = Registers[regx];

            if (KeyState[key] == 1)
                ProgramCounter += 2;
        }

        // Пропускает следующую инструкцию, если не нажата клавиша хранящаяся в VX.
        void OpcodeEXA1(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int key = Registers[regx];

            if (KeyState[key] == 0)
                ProgramCounter += 2;
        }

        // Присваивает VX значение таймера задержки.
        void OpcodeFX07(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;

            Registers[regx] = DelayTimer;
        }

        // Ожидает нажатия клавиши, и сохраняет её в VX.
        void OpcodeFX0A(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;

            int keypressed = GetKeyPressed();

            if (keypressed == -1)
            {
                ProgramCounter -= 2;
            }
            else
            {
                Registers[regx] = (byte)keypressed;
            }
        }

        // Присваивает таймеру значение VX
        void OpcodeFX15(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;

            DelayTimer = Registers[regx];
        }

        // Присваивает звуковому таймеру значение VX
        void OpcodeFX18(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;

            SoundTimer = Registers[regx];
        }

        // Прибавляет vx к I
        void OpcodeFX1E(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;

            AddressI += Registers[regx];
        }

        //Присваивает I координаты спрайта символа в VX. Символы 0-F представлены спрайтами 4x5.
        void OpcodeFX29(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            AddressI = (UInt16)(Registers[regx] * 5);
        }

        //Сохраняет бинарно-закодированное десятичное представление VX в адресах I, I + 1, I + 2.
        void OpcodeFX33(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;

            int value = Registers[regx];

            int hundreds = value / 100;
            int tens = (value / 10) % 10;
            int units = value % 10;

            Memory[AddressI] = (byte)hundreds;
            Memory[AddressI + 1] = (byte)tens;
            Memory[AddressI + 2] = (byte)units;
        }

        // Сохраняет V0 в VX начиная с адреса I.
        void OpcodeFX55(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;

            for (int i = 0; i <= regx; i++)
            {
                Memory[AddressI + i] = Registers[i];
            }

            AddressI = (UInt16)(AddressI + regx + 1);
        }

        //Заполняет V0 в VX значениями из памяти начинающейся с адреса I.
        void OpcodeFX65(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;

            for (int i = 0; i <= regx; i++)
            {
                Registers[i] = Memory[AddressI + i];
            }

            AddressI = (byte)(AddressI + regx + 1);
        }
    }
}
