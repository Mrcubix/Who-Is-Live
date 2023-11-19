using System;
using System.Text;
using WhoIsLive.Lib.Interfaces;

namespace WhoIsLive.UX.Cryptography
{
    /// <summary>
    ///   Default Obfuscator, use Base64 encoding.
    /// </summary>
    public class DefaultObfuscator : IObfuscator
    {
        #region Methods

        /// <summary>
        ///   Obfuscate the data using Base64 encoding.
        /// </summary>
        /// <param name="data">The data to obfuscate.</param>
        /// <returns>The obfuscated data.</returns>
        public byte[] Obfuscate(byte[] data) => Encoding.UTF8.GetBytes(Convert.ToBase64String(data));

        /// <summary>
        ///   Unobfuscate the data using Base64 encoding.
        /// </summary>
        /// <param name="data">The data to unobfuscate.</param>
        /// <returns>The unobfuscated data.</returns>
        public byte[] DeObfuscate(byte[] data) => Convert.FromBase64String(Encoding.UTF8.GetString(data));

        #endregion
    }
}