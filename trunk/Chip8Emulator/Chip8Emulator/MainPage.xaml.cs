using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Chip8Emulator
{
    public partial class MainPage : UserControl
    {
        private const int FPU = 30;
        private const int operations = 5000;

        private Chip8 cpu;
        private DispatcherTimer timer;
        private WriteableBitmap bitmap;

        public MainPage()
        {
            InitializeComponent();

            this.KeyDown += new KeyEventHandler(RootVisual_KeyDown);
            this.KeyUp += new KeyEventHandler(RootVisual_KeyUp);

            cpu = new Chip8();
            timer = new DispatcherTimer();
            bitmap = new WriteableBitmap(640, 320);

            screen.Source = bitmap;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / FPU);

            LoadRom();
        }

        void RootVisual_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyHandler(e.Key, true);
        }

        void RootVisual_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyHandler(e.Key, false);
        }

        private void LoadRom()
        {
            if (timer.IsEnabled)
                timer.Stop();

            //cpu.LoadRom("INVADERS");
            cpu.LoadRom("BLINKY");

            timer.Start();
        }

        private void ProcessFrame()
        {
            for (int i = 0; i < operations / FPU; i++)
                cpu.ExecuteNextOpcode();

            for (int x = 0; x < 640 - 1; x++)
            {
                for (int y = 0; y < 320 - 1; y++)
                {
                    bitmap.Pixels[x + y * 640] = (0xFF << 24)
                        //| (cpu.ScreenData[y, x, 0] & 0xFF) << 16
                        | (cpu.ScreenData[y, x, 0] & 0xFF) << 8;
                        //| (cpu.ScreenData[y, x, 0] & 0xFF);
                }
            }
            bitmap.Invalidate();
        }

        private void OnKeyHandler(Key key, bool isPressed)
        {
            int keyCode = -1;

            switch (key)
            {
                case Key.D0:
                    keyCode = 0x0;
                    break;
                case Key.D1:
                    keyCode = 0x1;
                    break;
                case Key.D2:
                    keyCode = 0x2;
                    break;
                case Key.D3:
                    keyCode = 0x3;
                    break;
                case Key.D4:
                    keyCode = 0x4;
                    break;
                case Key.D5:
                    keyCode = 0x5;
                    break;
                case Key.D6:
                    keyCode = 0x6;
                    break;
                case Key.D7:
                    keyCode = 0x7;
                    break;
                case Key.D8:
                    keyCode = 0x8;
                    break;
                case Key.D9:
                    keyCode = 0x9;
                    break;
                case Key.A:
                    keyCode = 0xA;
                    break;
                case Key.B:
                    keyCode = 0xB;
                    break;
                case Key.C:
                    keyCode = 0xC;
                    break;
                case Key.D:
                    keyCode = 0xD;
                    break;
                case Key.E:
                    keyCode = 0xE;
                    break;
                case Key.F:
                    keyCode = 0xF;
                    break;
                default:
                    return;
            }

            if (isPressed && cpu.GetKeyPressed() == -1)
            {
                cpu.KeyPressed(keyCode);
                keyPressed.Text = key.ToString();
            }
            else if (!isPressed && cpu.GetKeyPressed() != -1)
            {
                cpu.KeyReleased(keyCode);
                keyPressed.Text = string.Empty;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            ProcessFrame();
        }
    }
}
