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
using System.IO;
using CPI.Audio;

namespace Chip8Emulator
{
    public class SoundCard
    {
        private Stream soundStream;
        private SoundEffectInstance playerInstance;

        public SoundCard()
        {
            soundStream = new MemoryStream();
            using (var songWriter = new SongWriter(soundStream, 180, false))
            {
                songWriter.AddNote(600, 1);
            }
        }

        public void Beep()
        {
            //Generate sound stream
            if (playerInstance == null)
            {
                //Play sound stream
                soundStream.Position = 0;

                SoundEffect player = SoundEffect.FromStream(soundStream);
                playerInstance = player.CreateInstance();
                playerInstance.IsLooped = true;
            }
            else if (playerInstance.State != SoundState.Playing)
            {
                playerInstance.Play();
            }
        }

        public void Stop()
        {
            if (playerInstance == null)
                return;

            if (playerInstance.State != SoundState.Stopped)
                playerInstance.Stop();
        }
    }
}
