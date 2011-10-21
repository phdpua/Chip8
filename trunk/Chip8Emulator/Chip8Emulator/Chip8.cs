using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Threading;

namespace Chip8Emulator
{
    public class Chip8
    {
        private const int ROMSIZE = 0xFFF; // Размер ROM

        public byte[] Memory = new byte[0xFFF]; // 0xFFF байтов памяти
        public byte[] Registers = new byte[16]; // 16 регистров по 1 байту
        public UInt16 AddressI = new ushort(); // 16-битный адресный регистр I
        public UInt16 ProgramCounter = new ushort(); // 16-битный программный счетчик
        public Stack<UInt16> Stack = new Stack<ushort>(); // 16-битный стек

        public byte[] KeyState = new byte[16]; // Клавиатура
        byte DelayTimer = new byte(); // Таймер задержки
        byte SoundTimer = new byte(); // Звуковой таймер
        public byte[,,] ScreenData = new byte[320, 640, 3]; // Дисплей

        private DispatcherTimer delayTimer;

        public Chip8()
        {
            delayTimer = new DispatcherTimer();
            delayTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 60);
            delayTimer.Tick += new EventHandler(delayTimer_Tick);
            delayTimer.Start();
        }

        private void delayTimer_Tick(object sender, EventArgs e)
        {
            if (DelayTimer > 0)
                DelayTimer--;

            if (SoundTimer > 0)
                SoundTimer--;
        }

        public void CPUReset() // "Перезагружает" эмулятор очищая переменные
        {
            AddressI = 0;
            ProgramCounter = 0x200;
            Array.Clear(Registers, 0, Registers.Length);
            Array.Clear(Memory, 0, Memory.Length);
            Array.Clear(KeyState, 0, KeyState.Length);
            DelayTimer = 0;
            SoundTimer = 0;

            Opcode00E0();

            LoadFonts();
        }

        public bool LoadRom(string romName) // Загружает программу в "память"
        {
            CPUReset();

            try
            {
                Assembly myAssembly = Assembly.GetExecutingAssembly();
                using (BinaryReader br = new BinaryReader(
                    myAssembly.GetManifestResourceStream("Chip8Emulator.Roms." + romName)))
                {
                    for (int i = 0x200; i < ROMSIZE; i++)
                    {
                        byte b = br.ReadByte();
                        Memory[i] = b;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public bool LoadRom(byte[] rom)
        {
            CPUReset();
            Opcode00E0();

            try
            {
                Array.Copy(rom, 0, Memory, 0x200, rom.Length);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public void KeyPressed(int key) // Клавиша нажата
        {
            if (key >= 0 && key < 16)
                KeyState[key] = 1;
        }

        public void KeyReleased(int key) // Клавиша отпущена
        {
            if (key >= 0 && key < 16)
                KeyState[key] = 0;
        }

        public int GetKeyPressed() // Возвращает нажатую клавишу
        {
            int res = -1;

            for (int i = 0; i < 16; i++)
            {
                if (KeyState[i] > 0)
                    return i;
            }

            return res;
        }

        private UInt16 GetNextOpcode() // Возвращает следующий оп-код
        {
            UInt16 res = 0;
            res = Memory[ProgramCounter];
            res <<= 8;
            res |= Memory[ProgramCounter + 1];
            ProgramCounter += 2;
            return res;
        }

        public void ExecuteNextOpcode() // Определяет и выполняет оп-код
        {
            UInt16 opcode = GetNextOpcode();
            switch (opcode & 0xF000)
            {
                case 0x0000: DecodeOpcode0(opcode); break;
                case 0x1000: Opcode1NNN(opcode); break;
                case 0x2000: Opcode2NNN(opcode); break;
                case 0x3000: Opcode3XNN(opcode); break;
                case 0x4000: Opcode4XNN(opcode); break;
                case 0x5000: Opcode5XY0(opcode); break;
                case 0x6000: Opcode6XNN(opcode); break;
                case 0x7000: Opcode7XNN(opcode); break;
                case 0x8000: DecodeOpcode8(opcode); break;
                case 0x9000: Opcode9XY0(opcode); break;
                case 0xA000: OpcodeANNN(opcode); break;
                case 0xB000: OpcodeBNNN(opcode); break;
                case 0xC000: OpcodeCXNN(opcode); break;
                case 0xD000: OpcodeDXYN(opcode); break;
                case 0xE000: DecodeOpcodeE(opcode); break;
                case 0xF000: DecodeOpcodeF(opcode); break;
                default: break;
            }
        }

        private void DecodeOpcode8(UInt16 opcode)
        {
            switch (opcode & 0xF)
            {
                case 0x0: Opcode8XY0(opcode); break;
                case 0x1: Opcode8XY1(opcode); break;
                case 0x2: Opcode8XY2(opcode); break;
                case 0x3: Opcode8XY3(opcode); break;
                case 0x4: Opcode8XY4(opcode); break;
                case 0x5: Opcode8XY5(opcode); break;
                case 0x6: Opcode8XY6(opcode); break;
                case 0x7: Opcode8XY7(opcode); break;
                case 0xE: Opcode8XYE(opcode); break;
                default: break;
            }
        }

        private void DecodeOpcode0(UInt16 opcode)
        {
            switch (opcode & 0xF)
            {
                case 0x0: Opcode00E0(); break;
                case 0xE: Opcode00EE(); break;
                default: break;
            }
        }

        private void DecodeOpcodeE(UInt16 opcode)
        {
            switch (opcode & 0xF)
            {
                case 0xE: OpcodeEX9E(opcode); break;
                case 0x1: OpcodeEXA1(opcode); break;
                default: break;
            }
        }

        private void DecodeOpcodeF(UInt16 opcode)
        {
            switch (opcode & 0xFF)
            {
                case 0x07: OpcodeFX07(opcode); break;
                case 0x0A: OpcodeFX0A(opcode); break;
                case 0x15: OpcodeFX15(opcode); break;
                case 0x18: OpcodeFX18(opcode); break;
                case 0x1E: OpcodeFX1E(opcode); break;
                case 0x29: OpcodeFX29(opcode); break;
                case 0x33: OpcodeFX33(opcode); break;
                case 0x55: OpcodeFX55(opcode); break;
                case 0x65: OpcodeFX65(opcode); break;
                default: break;
            }
        }

        // "Очищает" дисплей
        private void Opcode00E0()
        {
            Array.Clear(ScreenData, 0, ScreenData.Length);
        }

        // Возвращение из метода
        private void Opcode00EE()
        {
            if (Stack.Count > 0)
            {
                ProgramCounter = Stack.Pop();
            }
        }

        // Переход к адресу NNN
        private void Opcode1NNN(UInt16 opcode)
        {
            ProgramCounter = (UInt16)(opcode & 0x0FFF);
        }

        // Вызов метода по адресу NNN
        private void Opcode2NNN(UInt16 opcode)
        {
            if (Stack.Count < 16)
            {
                Stack.Push(ProgramCounter);
                ProgramCounter = (UInt16)(opcode & 0x0FFF);
            }
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

        // Прибавляет NN к VX
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
        private void Opcode8XY1(UInt16 opcode)
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
        private void Opcode8XY4(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            int value = Registers[regx] + Registers[regy];

            Registers[0x0F] = (byte)((value > 255) ? 1 : 0);

            Registers[regx] = (byte)(value);
        }

        // Вычитает vy из vx. Присваивает флагу значение 1 если vy > vx, иначе 0
        private void Opcode8XY5(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            Registers[0x0F] = (byte)((Registers[regx] < Registers[regy]) ? 0 : 1);
            Registers[regx] = (byte)(Registers[regx] - Registers[regy]);
        }

        // Сдвигает VX вправо на 1 разряд. VF присваивается значение последнего значащего бита VX перед сдвигом.
        private void Opcode8XY6(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;

            Registers[0x0F] = (byte)(Registers[regx] & 0x01);
            Registers[regx] >>= 1;
        }

        // Присваивает VX значение VY минус VX. VF присваивается значение 0 если VY < VX, иначе 1.
        private void Opcode8XY7(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            Registers[0x0F] = (byte)((Registers[regy] < Registers[regx]) ? 0 : 1);
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
        private void Opcode9XY0(UInt16 opcode)
        {
            int regx = opcode & 0x0F00;
            regx >>= 8;
            int regy = opcode & 0x00F0;
            regy >>= 4;

            if (Registers[regx] != Registers[regy])
                ProgramCounter += 2;
        }

        // Присваивает I значение nnn
        private void OpcodeANNN(UInt16 opcode)
        {
            AddressI = (UInt16)(opcode & 0x0FFF);
        }

        // Переход к адресу NNN + V0
        private void OpcodeBNNN(UInt16 opcode)
        {
            int nnn = opcode & 0x0FFF;
            ProgramCounter = (UInt16)((Registers[0] + nnn) & 0x0FFF);
        }

        // Присваивает vx значение random + NN
        private void OpcodeCXNN(UInt16 opcode)
        {
            Random rand = new Random();
            rand.Next(255);
            int nn = opcode & 0x00FF;
            int regx = opcode & 0x0F00;
            regx >>= 8;

            Registers[regx] = (byte)(rand.Next(255) & nn);
        }

        // Рисует спрайт в координатах (VX, VY) шириной 8 и высотой N пикселей.
        // VF присваивается 1 если пиксель переключается из 1 в 0 при отрисовке спрайта,
        // иначе присваивается 0
        private void OpcodeDXYN(UInt16 opcode)
        {
            const int SCALE = 10;
            int regx = opcode & 0x0F00;
            regx = regx >> 8;
            int regy = opcode & 0x00F0;
            regy = regy >> 4;

            int coordx = Registers[regx] * SCALE;
            int coordy = Registers[regy] * SCALE;
            int height = opcode & 0x000F;

            Registers[0x0F] = 0;

            for (int yline = 0; yline < height; yline++)
            {
                // Данные спрайта, хранящиеся в Memory[AddressI]
                // Каждая строка обозначена как AddressI + yline
                byte data = (Memory[AddressI + yline]);

                // Выполняется для каждого из 8 пикселей в строке
                int xpixel = 0;
                int xpixelinv = 7;
                for (xpixel = 0; xpixel < 8; xpixel++, xpixelinv--)
                {

                    // Значение пикселя 1? Если да то переключается его состояние
                    int mask = 1 << xpixelinv;
                    if ((data & mask) != 0)
                    {
                        int x = (xpixel * SCALE) + coordx;
                        int y = (yline * SCALE) + coordy;

                        int colour = 255;

                        // Обнаружена коллизия
                        if (ScreenData[y, x, 0] == 255)
                        {
                            colour = 0;
                            Registers[0x0F] = 1;
                        }

                        // Цвет пикселя
                        for (int i = 0; i < SCALE; i++)
                        {
                            for (int j = 0; j < SCALE; j++)
                            {
                                ScreenData[y + i, x + j, 0] = (byte)colour;
                                ScreenData[y + i, x + j, 1] = (byte)colour;
                                ScreenData[y + i, x + j, 2] = (byte)colour;
                            }
                        }
                    }
                }
            }
        }

        //	Пропускает следующую инструкцию, если нажата клавиша хранящаяся в VX.
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

            AddressI = (UInt16)(AddressI + regx + 1);
        }

        private void LoadFonts()
        {
            byte[] font = new byte[]
            {
                0xF0, 0x90, 0x90, 0x90, 0xF0,   //0
                0x20, 0x60, 0x20, 0x20, 0x70,   //1
                0xF0, 0x10, 0xF0, 0x80, 0xF0,   //2
                0xF0, 0x10, 0xF0, 0x10, 0xF0,   //3
                0x90, 0x90, 0xF0, 0x10, 0x10,   //4
                0xF0, 0x80, 0xF0, 0x10, 0xF0,   //5
                0xF0, 0x80, 0xF0, 0x90, 0xF0,   //6
                0xF0, 0x10, 0x20, 0x40, 0x40,   //7
                0xF0, 0x90, 0xF0, 0x90, 0xF0,   //8
                0xF0, 0x90, 0xF0, 0x10, 0xF0,   //9
                0xF0, 0x90, 0xF0, 0x90, 0x90,   //A
                0xE0, 0x90, 0xE0, 0x90, 0xE0,   //B
                0xF0, 0x80, 0x80, 0x80, 0xF0,   //C
                0xE0, 0x90, 0x90, 0x90, 0xE0,   //D
                0xF0, 0x80, 0xF0, 0x80, 0xF0,   //E
                0xF0, 0x80, 0xF0, 0x80, 0x80    //F
            };
            Array.Copy(font, Memory, font.Length);
        }
    }
}
