using System;
using System.Runtime.InteropServices;

/// <summary>
/// A one-for-one set of C# wrappers that call SketchUp's C API functions.
/// </summary>
/// <remarks>
/// C# allows C functions to be called entirely with C# code. This library uses that
/// fact to allow a C# programmer who doesn't know, or want to work directly with, C
/// to call the SketchUp C API functions in a manner that closesly matches the way
/// the functions would be called in C. Thus, the SketchUp documentation on the C API
/// is still a good reference for C# programmers who use this library.
/// <para></para>
/// A higher level (and much simpler) interface to the SketchUp C API is provided by
/// the ExLumina.SketchUp.Factory namespace. Its functions can read and write SketchUp
/// models, but it provides more direct functions for creating faces, component
/// definitions, groups, instance, and texture mapping.
/// </remarks>
namespace ExLumina.SketchUp.API
{
    /// <summary>
    /// All C API function wrappers are methods of this class.
    /// </summary>
    public static partial class SU
    {
        private const string LIB = "SketchUpAPI";

        public const double MetersToInches = 39.37007874;

        public const int LengthFormat_Decimal = 0;
        public const int LengthFormat_Architectural = 1;
        public const int LengthFormat_Engineering = 2;
        public const int LengthFormat_Fractional = 3;

        public const int MaterialColorizeType_Shift = 0;
        public const int MaterialColorizeType_Tint = 1;

        public const int MaterialType_Colored = 0;
        public const int MaterialType_Textured = 1;
        public const int MaterialType_ColorizedTexture = 2;

        public const int ModelUnits_Inches = 0;
        public const int ModelUnits_Feet = 1;
        public const int ModelUnits_Millimeters = 2;
        public const int ModelUnits_Centimeters = 3;
        public const int ModelUnits_Meters = 4;

        public const int ModelVersion_SU3 = 0;
        public const int ModelVersion_SU4 = 1;
        public const int ModelVersion_SU5 = 2;
        public const int ModelVersion_SU6 = 3;
        public const int ModelVersion_SU7 = 4;
        public const int ModelVersion_SU8 = 5;
        public const int ModelVersion_SU2013 = 6;
        public const int ModelVersion_SU2014 = 7;
        public const int ModelVersion_SU2015 = 8;
        public const int ModelVersion_SU2016 = 9;
        public const int ModelVersion_SU2017 = 10;
        public const int ModelVersion_SU2018 = 11;
        public const int ModelVersion_SU2019 = 12;

        public const int ErrorNone = 0;
        public const int ErrorNullPointerInput = 1;
        public const int ErrorInvalidInput = 2;
        public const int ErrorNullPointerOutput = 3;
        public const int ErrorInvalidOutput = 4;
        public const int ErrorOverwriteValid = 5;
        public const int ErrorGeneric = 6;
        public const int ErrorSerialization = 7;
        public const int ErrorOutOfRange = 8;
        public const int ErrorNoData = 9;
        public const int ErrorInsufficientSize = 10;
        public const int ErrorUnknownException = 11;
        public const int ErrorModelInvalid = 12;
        public const int ErrorModelVersion = 13;
        public const int ErrorLayerLocked = 14;
        public const int ErrorDuplicate = 15;
        public const int ErrorPartialSuccess = 16;
        public const int ErrorUnsupported = 17;
        public const int ErrorInvalidArgument = 18;
        public const int ErrorEntityLocked = 19;
        public const int ErrorInvalidOperation = 20;

        public static readonly IntPtr Invalid = (IntPtr)0;

        /// <summary>
        /// Parent class to all reference wrapper classes.
        /// </summary>
        /// <remarks>
        /// To ensure type-safety, subclasses of this class wrap
        /// the C pointer for the various SketchUp data structures
        /// this library implements. Those subclasses generally
        /// add no code or members.
        /// <para></para>
        /// You will not need to subclass
        /// this class yourself.
        /// </remarks>
        public abstract class IntPtrRef
        {
            internal IntPtr intPtr;
        }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class CameraRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class ComponentDefinitionRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class ComponentInstanceRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class DrawingElementRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class EdgeRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class EntitiesRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class FaceRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class GeometryInputRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class GroupRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class ImageRepRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class LoopInputRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class LoopRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class MaterialRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class ModelRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class OptionsManagerRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class OptionsProviderRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class StringRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class StylesRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class TextureRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class TextureWriterRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class TypedValueRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class UVHelperRef : IntPtrRef { }

        /// <summary>
        /// This class wraps the API structure of the same name.
        /// </summary>
        public class VertexRef : IntPtrRef { }

        static void ThrowOut(
            int errCode,
            string msg)
        {
            string suMsg;

            switch (errCode)
            {
                case ErrorNone:
                    return;

                case ErrorNullPointerInput:
                    suMsg = "A pointer for a required input was NULL.";
                    break;

                case ErrorInvalidInput:
                    suMsg = "An API object input to the function was not created properly.";
                    break;

                case ErrorNullPointerOutput:
                    suMsg = "A pointer for a required output was NULL.";
                    break;

                case ErrorInvalidOutput:
                    suMsg = "An API object to be written with output from the function was not created properly.";
                    break;

                case ErrorOverwriteValid:
                    suMsg = "Indicates that an input object reference already references an object where it was expected to be SU_INVALID.";
                    break;

                case ErrorGeneric:
                    suMsg = "Indicates an unspecified error.";
                    break;

                case ErrorSerialization:
                    suMsg = "Indicate an error occurred during loading or saving of a file.";
                    break;

                case ErrorOutOfRange:
                    suMsg = "An input contained a value that was outside the range of allowed values.";
                    break;

                case ErrorNoData:
                    suMsg = "The requested operation has no data to return to the user. This usually occurs when a request is made for data that is only available conditionally.";
                    break;

                case ErrorInsufficientSize:
                    suMsg = "Indicates that the size of an output parameter is insufficient.";
                    break;

                case ErrorUnknownException:
                    suMsg = "An unknown exception occurred.";
                    break;

                case ErrorModelInvalid:
                    suMsg = "The model requested is invalid and cannot be loaded.";
                    break;

                case ErrorModelVersion:
                    suMsg = "The model cannot be loaded or saved due to an invalid version";
                    break;

                case ErrorLayerLocked:
                    suMsg = "The layer that is being modified is locked.";
                    break;

                case ErrorDuplicate:
                    suMsg = "The user requested an operation that would result in duplicate data.";
                    break;

                case ErrorPartialSuccess:
                    suMsg = "The requested operation was not fully completed but it returned an intermediate successful result.";
                    break;

                case ErrorUnsupported:
                    suMsg = "The requested operation is not supported.";
                    break;

                case ErrorInvalidArgument:
                    suMsg = "An argument contains invalid information.";
                    break;

                case ErrorEntityLocked:
                    suMsg = "The entity being modified is locked.";
                    break;

                case ErrorInvalidOperation:
                    suMsg = "The requested operation is invalid.";
                    break;

                default:
                    suMsg = "Unrecognized error code.";
                    break;
            }

            throw new SketchUpException($"Error {errCode}\n" + msg + "\n" + suMsg, errCode);
        }
    }
}
