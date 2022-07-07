using Minesweeper.Properties;
using System;
using System.ComponentModel;
using System.Media;

namespace Minesweeper.Manager
{
    internal class SoundManager : IDisposable
    {
        private readonly SoundPlayer _soundPlayer;

        public SoundManager()
        {
            _soundPlayer = new SoundPlayer();

            _soundPlayer.LoadCompleted += SoundPlayer_LoadCompleted;

            _soundPlayer.StreamChanged += SoundPlayer_StreamChanged;
        }

        private void SoundPlayer_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
                _soundPlayer.Play();
            _soundPlayer.Stream.Dispose();
        }

        private void SoundPlayer_StreamChanged(object sender, EventArgs e)
        {
            _soundPlayer.LoadAsync();
        }

        public void PlayExplosion()
        {
            _soundPlayer.Stream = Resources.Explosion1;
        }

        public void Dispose()
        {
            _soundPlayer.Dispose();
        }
    }
}
