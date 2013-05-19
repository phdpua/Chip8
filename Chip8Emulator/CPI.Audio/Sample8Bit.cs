using System;

namespace CPI.Audio
{
    /// <summary>
    /// Represents the data in a single sample (mono or stereo) in a 8-bit sound file.
    /// </summary>
    public struct Sample8Bit : IEquatable<Sample8Bit>
    {
        # region Private Fields

        byte _leftChannel;
        byte _rightChannel;
        readonly bool _stereo;

        # endregion

        # region Constructors

        /// <summary>
        /// Creates a new single-channel 8-bit sample.
        /// </summary>
        /// <param name="monoChannel">The 8-bit value for the single channel in this sample.</param>
        public Sample8Bit(byte monoChannel)
        {
            this._leftChannel = monoChannel;
            this._rightChannel = monoChannel;
            this._stereo = false;
        }

        /// <summary>
        /// Creates a new two-channel 8-bit sample.
        /// </summary>
        /// <param name="leftChannel">The 8-bit value for the left channel in this sample.</param>
        /// <param name="rightChannel">The 8-bit value for the right channel in this sample.</param>
        public Sample8Bit(byte leftChannel, byte rightChannel)
        {
            this._leftChannel = leftChannel;
            this._rightChannel = rightChannel;
            this._stereo = true;
        }

        /// <summary>
        /// Creates a new 8-bit sample with its value defaulted to 128.
        /// </summary>
        /// <param name="stereo">true for stereo; false for mono.</param>
        public Sample8Bit(bool stereo)
        {
            this._leftChannel = 128;
            this._rightChannel = 128;
            this._stereo = stereo;
        }

        # endregion

        # region Properties

        /// <summary>
        /// Gets or sets the left channel (for a stereo sample) and the single channel (for a mono sample).
        /// </summary>
        public byte LeftChannel
        {
            get
            {
                return this._leftChannel;
            }
            set
            {
                this._leftChannel = value;

                if (!this._stereo)
                    this._rightChannel = value;
            }
        }

        /// <summary>
        /// Gets or sets the right channel (for a stereo sample) and the single channel (for a mono sample).
        /// </summary>
        public byte RightChannel
        {
            get
            {
                return this._rightChannel;
            }
            set
            {
                this._rightChannel = value;

                if (!this._stereo)
                    this._leftChannel = value;
            }
        }

        /// <summary>
        /// Returns true if the sample is a stereo sample, or false if it's a mono sample.
        /// </summary>
        public bool Stereo
        {
            get
            {
                return this._stereo;
            }
        }

        # endregion

        # region Overridden Methods

        /// <summary>
        /// Checks to see whether a specified object is equal to this object.
        /// </summary>
        /// <param name="obj">An object to compare to the this object.</param>
        /// <returns>True if the objects are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return (obj is Sample8Bit && this.Equals((Sample8Bit)obj));
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>a 32-bit hash code.</returns>
        public override int GetHashCode()
        {
            int a = this._leftChannel;
            int b = this._rightChannel;

            return ((a << 24) | b);
        }

        # endregion

        #region IEquatable<Sample8Bit> Members

        /// <summary>
        /// Checks to see whether a specified Sample8Bit object is equal to this object.
        /// </summary>
        /// <param name="other">A Sample8Bit object to compare to the this object.</param>
        /// <returns>True if the objects are equal; false otherwise.</returns>
        public bool Equals(Sample8Bit other)
        {
            return this._leftChannel == other._leftChannel && this._rightChannel == other._rightChannel && this._stereo == other._stereo;
        }

        #endregion

        # region Operators

        /// <summary>
        /// Checks to see whether two Sample8Bit instances are equal.
        /// </summary>
        /// <param name="a">a Sample8Bit instance</param>
        /// <param name="b">a Sample8Bit instance</param>
        /// <returns>true if the instances are equal; false otherwise.</returns>
        public static bool operator ==(Sample8Bit a, Sample8Bit b)
        {
            return a.Equals(b);
        }

        /// <summary>
        /// Checks to see whether two Sample8Bit instances are unequal.
        /// </summary>
        /// <param name="a">a Sample8Bit instance</param>
        /// <param name="b">a Sample8Bit instance</param>
        /// <returns>true if the instances are unequal; false otherwise.</returns>
        public static bool operator !=(Sample8Bit a, Sample8Bit b)
        {
            return !a.Equals(b);
        }

        # endregion
    }
}
