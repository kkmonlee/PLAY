using System;
using NAudio;
using NAudio.Wave;
using Timer = System.Timers.Timer;
using FlagLib.Extensions;

namespace PLAY.Core
{
    public sealed class AudioPlayer : IDisposable
    {
        private IWavePlayer _wavePlayer;
        private WaveChannel32 _inputStream;
        private readonly Timer _songFinishedTimer;

        // Get playback state
        public PlaybackState PlaybackState
        {
            get { return _wavePlayer.PlaybackState; }
        }

        // Get current song
        public Song LoadedSong { get; private set; }

        public float Volume
        {
            get { return _inputStream.Volume; }
            set { _inputStream.Volume = value; }
        }

        // Get current song time
        public TimeSpan CurrentTime
        {
            get { return _inputStream == null ? TimeSpan.Zero : _inputStream.CurrentTime; }
            set { _inputStream.CurrentTime = value; }
        }

        // Get total song length
        public TimeSpan TotalTime
        {
            get { return LoadedSong == null ? TimeSpan.Zero : _inputStream.TotalTime; }
        }

        // Handle x after song finishes
        public event EventHandler SongFinished;

        // Make another instance of this player
        public AudioPlayer()
        {
            Volume = 1.0f;
            _songFinishedTimer = new Timer {Interval = 250};
            _songFinishedTimer.Elapsed += songFinishedTimer_Elapsed;

        }

        // Loads a designated song
        public void Load(Song song)
        {
            Stop();
            RenewDevice();
            OpenFile(song.Path);
            LoadedSong = song;
        }

        // Play loaded song
        public void Play()
        {
            if (_wavePlayer == null || _inputStream == null || _wavePlayer.PlaybackState == PlaybackState.Playing) return;
            try
            {
                _wavePlayer.Play();
                _songFinishedTimer.Start();
            }
            catch (MmException)
            {
                // Errorino
            }
        }

        // Pause loaded song
        public void Pause()
        {
            if (_wavePlayer == null || _inputStream == null || _wavePlayer.PlaybackState == PlaybackState.Paused) return;
            _wavePlayer.Pause();
            _songFinishedTimer.Stop();
        }

        // Stop loaded song
        public void Stop()
        {
            if (_wavePlayer != null)
            {
                _wavePlayer.Stop();
            }

            if (_songFinishedTimer != null)
            {
                _songFinishedTimer.Stop();
            }

            CloseFile();
        }

        public void Dispose()
        {
            Stop();

            if (_wavePlayer != null)
            {
                _wavePlayer.Dispose();
                //_wavePlayer = null;
            }

            if (_inputStream != null)
            {
                _inputStream.Close();
            }

            if (_songFinishedTimer != null)
            {
                _songFinishedTimer.Dispose();
            }
        }

        // Close loaded file
        private void CloseFile()
        {
            if (_inputStream != null)
            {
                _inputStream.Dispose();
                //_inputStream = null;
            }

            LoadedSong = null;
        }

        // Opens a file
        private void OpenFile(string fileName)
        {
            CreateInputStream(fileName);
            _wavePlayer.Init(_inputStream);
        }

        // Create input stream
        private void CreateInputStream(string filename)
        {
            if (filename.EndsWith(".wav"))
            {
                _inputStream = OpenWavStream(filename);
            }
            else if (filename.EndsWith(".mp3"))
            {
                _inputStream = OpenMp3Stream(filename);
            }
            else
            {
                throw new InvalidOperationException("Unsupported audio format!");
            }
        }

        // Open MP3 stream
        private WaveChannel32 OpenMp3Stream(string fileName)
        {
            WaveStream mp3Stream = new Mp3FileReader(fileName);
            return new WaveChannel32(mp3Stream);
        }

        // Open WAV stream
        private WaveChannel32 OpenWavStream(string fileName)
        {
            WaveStream readerStream = new WaveFileReader(fileName);
            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm)
            {
                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                readerStream = new BlockAlignReductionStream(readerStream);
            }

            if (readerStream.WaveFormat.BitsPerSample == 16) return new WaveChannel32(readerStream) {Volume = Volume};
            var format = new WaveFormat(readerStream.WaveFormat.SampleRate, 16, readerStream.WaveFormat.Channels);
            readerStream = new WaveFormatConversionStream(format, readerStream);

            return new WaveChannel32(readerStream) {Volume = Volume};
        }

        // Verif
        private void RenewDevice()
        {
            if (_wavePlayer != null)
            {
                _wavePlayer.Dispose();
            }
            _wavePlayer = new WaveOut();
        }

        // songFinishedTimer tick event args
        private void songFinishedTimer_Elapsed(object sender, EventArgs e)
        {
            if (CurrentTime < TotalTime) return;
            Stop();
            SongFinished.RaiseSafe(this, e);
        }
    }
}
