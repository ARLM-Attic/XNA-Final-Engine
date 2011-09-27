
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
#endregion

namespace XNAFinalEngine.UserInterface
{

    #region Enumerators

    public enum Message
    {
        Click,
        MouseDown,
        MouseUp,
        MousePress,
        MouseMove,
        MouseOver,
        MouseOut,
        KeyDown,
        KeyUp,
        KeyPress,
    } // Message

    public enum ControlState
    {
        Enabled,
        Hovered,
        Pressed,
        Focused,
        Disabled
    } // ControlState

    public enum Alignment
    {
        None,
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    } // Alignment

    public enum ModalResult
    {
        None,
        Ok,
        Cancel,
        Yes,
        No,
        Abort,
        Retry,
        Ignore
    } // ModalResult

    public enum Orientation
    {
        Horizontal,
        Vertical
    } // Orientation

    public enum ScrollBarType
    {
        None,
        Vertical,
        Horizontal,
        Both
    } // ScrollBarsType

    [Flags]
    public enum Anchors
    {
        None = 0x00,
        Left = 0x01,
        Top = 0x02,
        Right = 0x04,
        Bottom = 0x08,
        Horizontal = Left | Right,
        Vertical = Top | Bottom,
        All = Left | Top | Right | Bottom
    } // Anchors

    /// <summary>
    /// Specifies how an image is positioned within a control.
    /// </summary>
    public enum SizeMode
    {
        Normal,
        Auto,
        Centered,
        Stretched
    } // SizeMode

    #endregion

    #region Structs

    public struct Margins
    {

        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        /// <summary>
        /// Top + Bottom.
        /// </summary>
        public int Vertical   { get { return (Top + Bottom); } }

        /// <summary>
        /// Left + Right.
        /// </summary>
        public int Horizontal { get { return (Left + Right); } }

        public Margins(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        } // Margins

    }  // Margins

    public struct Size
    {
        public int Width;
        public int Height;

        public static Size Zero { get { return new Size(0, 0); } }

        public Size(int width, int height)
        {
            Width = width;
            Height = height;
        } // Size
        
    } // Size

    #endregion

} // XNAFinalEngine.UserInterface
