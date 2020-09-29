using System;

namespace ExLumina.SketchUp.API
{
    /// <summary>
    /// This exception is thrown whenever a SketchUp C API function
    /// returns an error.
    /// </summary>
    public class SketchUpException : Exception
    {
        /// <summary>
        /// Contains the SketchUp C API error code returned.
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Captures a SketchUp C API function's error code.
        /// </summary>
        /// <remarks>
        /// The original error code value, the explanatory text from the SketchUp C API
        /// documentation, and an additional string passed to this exception's
        /// constructor, become the Message property of this exception. You can also
        /// obtain the SketchUp C API error value as an integer from this class's
        /// ErrorCode property.
        /// </remarks>
        /// <param name="s">A message to accompany the exception.</param>
        /// <param name="errorCode">The SketchUp C API function's return value.</param>
        public SketchUpException(string s, int errorCode) : base(s)
        {
            ErrorCode = errorCode;
        }
    }
}
