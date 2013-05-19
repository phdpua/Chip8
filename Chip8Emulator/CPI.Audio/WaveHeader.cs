using System;
using System.IO;

namespace CPI.Audio
{
    /// <summary>
    /// Represents the Header information in a PCM wave file.
    /// </summary>
    internal class WaveHeader
    {
        # region Constants

        /// <summary>
        /// The size of the wave header.
        /// </summary>
        public const int HeaderSize = 44;

        # endregion

        # region Private Fields

        readonly int _sampleRate;
        readonly short _bitsPerSample;
        readonly bool _stereo;
        long _numberOfSamples;
        readonly int _bytesPerSample;

        # endregion

        # region Constructors

        /// <summary>
        /// Instantiates a new wave header.
        /// </summary>
        /// <param name="sampleRate">Specifies the sample rate (number of samples per second) of the wave file.</param>
        /// <param name="bitsPerSample">Specifies whether this is an 8-bit or 16-bit wave file.</param>
        /// <param name="stereo">Specifies whether this is a mono or stereo wave file.</param>
        public WaveHeader(int sampleRate, short bitsPerSample, bool stereo)
        {
            if (sampleRate <= 0)
                throw new ArgumentOutOfRangeException("sampleRate", "Sample Rate must be greater than zero.");

            if (bitsPerSample != 8 && bitsPerSample != 16)
                throw new ArgumentException("Invalid Bits Per Sample specified.  Currently supported values are [8, 16].", "bitsPerSample");

            _sampleRate = sampleRate;
            _bitsPerSample = bitsPerSample;
            _stereo = stereo;

            _bytesPerSample = _bitsPerSample / 8;
            if (_stereo)
                _bytesPerSample *= 2;
        }

        # endregion

        # region Properties

        /// <summary>
        /// Gets the sample rate (number of samples per second) of the wave file.
        /// </summary>
        public int SampleRate
        {
            get
            {
                return _sampleRate;
            }
        }

        /// <summary>
        /// Gets the size in bits for a single channel of a sample.
        /// Supported values are 8 and 16.
        /// </summary>
        public short BitsPerSample
        {
            get
            {
                return _bitsPerSample;
            }
        }

        /// <summary>
        /// Gets a boolean value which is true if the wave file is stereo, or false if it's mono.
        /// </summary>
        public bool Stereo
        {
            get
            {
                return _stereo;
            }
        }

        /// <summary>
        /// Gets the number of bytes needed to store all channels of a single sample.
        /// </summary>
        public int BytesPerSample
        {
            get
            {
                return _bytesPerSample;
            }
        }

        /// <summary>
        /// Gets or sets the number of samples that the wave file contains.
        /// </summary>
        public long NumberOfSamples
        {
            get
            {
                return _numberOfSamples;
            }
            set
            {
                if (value < 0 || value * _bytesPerSample > uint.MaxValue)
                    throw new ArgumentOutOfRangeException("value", "NumberOfSamples cannot be less than zero or greater than int.MaxValue * bytesPerSample.");

                _numberOfSamples = value;
            }
        }


        # endregion

        # region Methods

        /// <summary>
        /// Writes the wave header to a BinaryWriter object.
        /// </summary>
        /// <param name="writer">A BinaryWriter object to write the header to.</param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(0x46464952); // "RIFF" in ASCII
            writer.Write((int)(HeaderSize + (_numberOfSamples * _bitsPerSample * (_stereo ? 2 : 1) / 8)) - 8);
            writer.Write(0x45564157); // "WAVE" in ASCII

            writer.Write(0x20746d66); // "fmt " in ASCII
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)(_stereo ? 2 : 1));
            writer.Write(_sampleRate);
            writer.Write(_sampleRate * (_stereo ? 2 : 1) * _bitsPerSample / 8);
            writer.Write((short)((_stereo ? 2 : 1) * BitsPerSample / 8));
            writer.Write(_bitsPerSample);

            writer.Write(0x61746164); // "data" in ASCII
            writer.Write((int)(_numberOfSamples * _bitsPerSample * (_stereo ? 2 : 1) / 8));
        }

        # endregion
    }
}
