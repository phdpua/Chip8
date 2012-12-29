using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Graphics;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Chip8Emulator.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Chip8Emulator
{
    public partial class MainPage : UserControl, INotifyPropertyChanged
    {
        private const int FPU = 25;
        private const int operations = 500;

        private uint[] screenData;
        private SpriteBatch spriteBatch;

        public List<string> RomItems { get; set; }

        public string CurrentRom
        {
            get { return currentRom; }
            set
            {
                if (currentRom == value)
                    return;

                currentRom = value;

                LoadRom();
                OnPropertyChanged("CurrentRom");
            }
        }
        private string currentRom;

        private Texture2D texture;

        private Chip8 cpu;
        private DispatcherTimer timer;
        private WriteableBitmap bitmap;

        public MainPage()
        {
            if (!TestRenderMode())
                return;

            screenData = new uint[640 * 320];

            spriteBatch = new SpriteBatch(GraphicsDeviceManager.Current.GraphicsDevice);

            RomItems = new List<string>(RomLoader.GetRomList());

            texture = new Texture2D(GraphicsDeviceManager.Current.GraphicsDevice, 640, 320, false, SurfaceFormat.Color);

            InitializeComponent();

            this.KeyDown += new KeyEventHandler(RootVisual_KeyDown);
            this.KeyUp += new KeyEventHandler(RootVisual_KeyUp);

            cpu = new Chip8();
            timer = new DispatcherTimer();
            bitmap = new WriteableBitmap(640, 320);

            //screen.Source = bitmap;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / FPU);
        }

        private bool TestRenderMode()
        {
            StringBuilder sb = new StringBuilder();

            if (GraphicsDeviceManager.Current.RenderMode != RenderMode.Hardware)
            {
                switch (GraphicsDeviceManager.Current.RenderModeReason)
                {
                    case RenderModeReason.GPUAccelerationDisabled:
                    case RenderModeReason.SecurityBlocked:
                        sb.AppendLine("Cannot Load 3D. Please Follow instructions below");
                        sb.AppendLine("1. Right click on your Silverlight plug-in.");
                        sb.AppendLine("2. Click the 'Silverlight' option.");
                        sb.AppendLine("3. Go to the permissions tab.");
                        sb.AppendLine("4. Find the domain '" + Application.Current.Host.Source.AbsoluteUri.Substring(0, Application.Current.Host.Source.AbsoluteUri.IndexOf(Application.Current.Host.Source.LocalPath)) + "'");
                        sb.AppendLine("5. When you find the entry, select the item below it '3D Graphics: use blocked display drivers'.");
                        sb.AppendLine("6. Click 'Allow'");
                        sb.AppendLine("7. Click 'Ok'");
                        sb.AppendLine("8. Refresh Page");
                        break;
                    case RenderModeReason.Not3DCapable:
                        sb.AppendLine("Cannot Load 3D.");
                        sb.AppendLine("Your Graphics card does not support 3D");
                        break;
                    case RenderModeReason.TemporarilyUnavailable:
                        sb.AppendLine("Cannot Load 3D. Please Follow instructions below");
                        sb.AppendLine("1. Right click on your Silverlight plug-in.");
                        sb.AppendLine("2. Click the 'Silverlight' option.");
                        sb.AppendLine("3. Go to the permissions tab.");
                        sb.AppendLine("4. Find the domain '" + Application.Current.Host.Source.AbsoluteUri.Substring(0, Application.Current.Host.Source.AbsoluteUri.IndexOf(Application.Current.Host.Source.LocalPath)) + "'");
                        sb.AppendLine("5. When you find the entry, select the item below it '3D Graphics: use blocked display drivers'.");
                        sb.AppendLine("6. Click 'Allow'");
                        sb.AppendLine("7. Click 'Ok'");
                        break;
                }
            }

            return string.IsNullOrEmpty(sb.ToString());
        }

        private void RootVisual_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyHandler(e.Key, true);
        }

        private void RootVisual_KeyUp(object sender, KeyEventArgs e)
        {
            OnKeyHandler(e.Key, false);
        }

        private void LoadRom()
        {
            if (string.IsNullOrEmpty(CurrentRom))
                throw new InvalidOperationException("Invalide ROM name");

            if (timer.IsEnabled)
                timer.Stop();

            cpu.LoadRom(CurrentRom);

            timer.Start();
        }

        private void ProcessFrame()
        {
            for (int i = 0; i < operations / FPU; i++)
                cpu.ExecuteNextOpcode();

            //for (int x = 0; x < 640 - 1; x++)
            //{
            //    for (int y = 0; y < 320 - 1; y++)
            //    {
            //        bitmap.Pixels[x + y * 640] = (0xFF << 24)
            //            //| (cpu.ScreenData[y, x, 0] & 0xFF) << 16
            //            | (cpu.ScreenData[y, x, 0] & 0xFF) << 8;
            //            //| (cpu.ScreenData[y, x, 0] & 0xFF);
            //    }
            //}
            //bitmap.Invalidate();
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
                //keyPressed.Text = key.ToString();
            }
            else if (!isPressed && cpu.GetKeyPressed() != -1)
            {
                cpu.KeyReleased(keyCode);
                //keyPressed.Text = string.Empty;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            ProcessFrame();
        }

        private void DrawingSurface_Draw(object sender, DrawEventArgs e)
        {
            GraphicsDeviceManager.Current.GraphicsDevice.Textures[0] = null;

            for (int x = 0; x < 640 - 1; x++)
            {
                for (int y = 0; y < 320 - 1; y++)
                {
                    screenData[x + y * 640] = (UInt32)((0x50 << 24)
                        | (cpu.ScreenData[y, x, 0] & 0xFF) << 8);
                }
            }

            texture.SetData(screenData);

            spriteBatch.Begin();

            spriteBatch.Draw(texture, new Vector2(0, 0), new Color(255, 255, 255));

            spriteBatch.End();

            e.InvalidateSurface();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;

            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
