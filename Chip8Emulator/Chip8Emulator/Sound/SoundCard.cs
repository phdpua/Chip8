using Microsoft.Xna.Framework.Audio;
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

namespace Chip8Emulator
{
    public class SoundCard
    {
        public void Beep()
        {
            //Generate sound stream

            //Play sound stream
            SoundEffect player = SoundEffect.FromStream(null);
        }
    }
}
