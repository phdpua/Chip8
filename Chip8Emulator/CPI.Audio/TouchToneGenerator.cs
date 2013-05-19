using System;
using System.IO;
using System.Collections.Generic;

namespace CPI.Audio
{
    /// <summary>
    /// Generates tones from a touch-tone phone.
    /// </summary>
    public class TouchToneGenerator : IDisposable
    {
        # region Static Fields

        private static readonly double[] columnTones = new double[] { 1209, 1336, 1477 };
        private static readonly double[] rowTones = new double[] { 697, 770, 852, 941 };
        private static readonly Dictionary<char, double> columnTone = new Dictionary<char, double>();
        private static readonly Dictionary<char, double> rowTone = new Dictionary<char, double>();
        private static readonly Dictionary<char, char> letterTranslations = new Dictionary<char, char>();

        # endregion

        # region Static Constructors

        static TouchToneGenerator()
        {
            columnTone['1'] = columnTones[0];
            columnTone['2'] = columnTones[1];
            columnTone['3'] = columnTones[2];
            columnTone['4'] = columnTones[0];
            columnTone['5'] = columnTones[1];
            columnTone['6'] = columnTones[2];
            columnTone['7'] = columnTones[0];
            columnTone['8'] = columnTones[1];
            columnTone['9'] = columnTones[2];
            columnTone['*'] = columnTones[0];
            columnTone['0'] = columnTones[1];
            columnTone['#'] = columnTones[2];

            rowTone['1'] = rowTones[0];
            rowTone['2'] = rowTones[0];
            rowTone['3'] = rowTones[0];
            rowTone['4'] = rowTones[1];
            rowTone['5'] = rowTones[1];
            rowTone['6'] = rowTones[1];
            rowTone['7'] = rowTones[2];
            rowTone['8'] = rowTones[2];
            rowTone['9'] = rowTones[2];
            rowTone['*'] = rowTones[3];
            rowTone['0'] = rowTones[3];
            rowTone['#'] = rowTones[3];

            letterTranslations['1'] = '1';
            letterTranslations['2'] = '2';
            letterTranslations['3'] = '3';
            letterTranslations['4'] = '4';
            letterTranslations['5'] = '5';
            letterTranslations['6'] = '6';
            letterTranslations['7'] = '7';
            letterTranslations['8'] = '8';
            letterTranslations['9'] = '9';
            letterTranslations['*'] = '*';
            letterTranslations['0'] = '0';
            letterTranslations['#'] = '#';

            letterTranslations['A'] = '2';
            letterTranslations['B'] = '2';
            letterTranslations['C'] = '2';
            letterTranslations['D'] = '3';
            letterTranslations['E'] = '3';
            letterTranslations['F'] = '3';
            letterTranslations['G'] = '4';
            letterTranslations['H'] = '4';
            letterTranslations['I'] = '4';
            letterTranslations['J'] = '5';
            letterTranslations['K'] = '5';
            letterTranslations['L'] = '5';
            letterTranslations['M'] = '6';
            letterTranslations['N'] = '6';
            letterTranslations['O'] = '6';
            letterTranslations['P'] = '7';
            letterTranslations['R'] = '7';
            letterTranslations['S'] = '7';
            letterTranslations['T'] = '8';
            letterTranslations['U'] = '8';
            letterTranslations['V'] = '8';
            letterTranslations['W'] = '9';
            letterTranslations['X'] = '9';
            letterTranslations['Y'] = '9';
        }

        # endregion

        # region Private Fields

        private readonly WaveWriter8Bit _writer;

        private readonly int _samplesPerTone;

        private readonly int _samplesPerPause;

        # endregion

        # region Constructors

        /// <summary>
        /// Instantiates a new TouchToneGenerator object.
        /// </summary>
        /// <param name="output">The Stream object to write the wave to.</param>
        /// <remarks>
        /// When this object is disposed, it will close its underlying stream.  To change this default behavior,
        /// you can call an overloaded constructor which takes a closeUnderlyingStream parameter.
        /// </remarks>
        public TouchToneGenerator(Stream output)
            : this(output, true) { }

        /// <summary>
        /// Instantiates a new TouchToneGenerator object.
        /// </summary>
        /// <param name="output">The Stream object to write the wave to.</param>
        /// <param name="closeUnderlyingStream">
        /// Determines whether to close the the stream that the TouchToneGenerator 
        /// is writing to when the TouchToneGenerator is closed.
        /// </param>
        public TouchToneGenerator(Stream output, bool closeUnderlyingStream)
        {
            _writer = new WaveWriter8Bit(output, 8000, false, closeUnderlyingStream);

            _samplesPerTone = _writer.SampleRate * 9 / 20;

            _samplesPerPause = _writer.SampleRate / 20;

        }

        # endregion

        # region Methods

        /// <summary>
        /// Generates touchtone phone tones for a string of digits.
        /// </summary>
        /// <param name="phoneNumber">
        /// A string of digits.
        /// Valid digits are: 1234567890*#ABCDEFGHIJKLMNOPRSTUVWXY
        /// Invalid digits will be silently ignored.
        /// </param>
        public void GenerateTones(string phoneNumber)
        {
            foreach (char digit in phoneNumber.ToUpper())
            {
                if (letterTranslations.ContainsKey(digit))
                    GenerateTone(letterTranslations[digit]);
            }
        }

        private void GenerateTone(char digit)
        {
            double columnFrequency = columnTone[digit];
            double rowFrequency = rowTone[digit];

            double samplesPerColumnCycle = _writer.SampleRate / columnFrequency;
            double samplesPerRowCycle = _writer.SampleRate / rowFrequency;


            Sample8Bit sample = new Sample8Bit(false);

            long startPoint = _writer.CurrentSample;

            // Write out the tone associated with the digit's column
            for (int currentSample = 0; currentSample < _samplesPerTone; currentSample++)
            {
                double sampleValue = Math.Sin(currentSample / samplesPerColumnCycle * 2 * Math.PI) * 60;

                sample.LeftChannel = (byte)(sampleValue + 128);

                _writer.Write(sample);
            }

            // go back to the starting point
            _writer.CurrentSample = startPoint;

            // Overlay the tone associated with the digit's row
            for (int currentSample = 0; currentSample < _samplesPerTone; currentSample++)
            {
                // Figure out what the sample was originally set to
                Sample8Bit originalSample = _writer.Read();
                _writer.CurrentSample--;

                double sampleValue = Math.Sin(currentSample / samplesPerRowCycle * 2 * Math.PI) * 60;

                sample.LeftChannel = (byte)(sampleValue + originalSample.LeftChannel);

                _writer.Write(sample);
            }

            // Insert a pause
            sample.LeftChannel = 128;

            for (int currentSample = 0; currentSample < _samplesPerPause; currentSample++)
            {
                _writer.Write(sample);
            }
        }

        /// <summary>
        /// Closes the TouchToneGenerator and saves the underlying stream.
        /// </summary>
        public void Close()
        {
            if (_writer != null)
                _writer.Close();
        }

        /// <summary>
        /// Closes the TouchToneGenerator and the underlying stream.
        /// </summary>
        /// <param name="disposing">true if called by the Dispose() method; false if called by a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        # endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes this object and cleans up any resources used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
