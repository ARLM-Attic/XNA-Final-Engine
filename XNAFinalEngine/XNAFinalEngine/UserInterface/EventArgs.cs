
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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
#endregion

namespace XNAFinalEngine.UI
{

    #region EventArgs

    public class EventArgs : System.EventArgs
    {

        public bool Handled;

    } //EventArgs

    #endregion

    #region KeyEventArgs

    public class KeyEventArgs : EventArgs
    {

        #region Variables
                
        public Keys Key = Keys.None;
        public bool Control;
        public bool Shift;
        public bool Alt;
        public bool Caps;

        #endregion

        #region Constructors
                
        public KeyEventArgs() { } 
                
        public KeyEventArgs(Keys key)
        {
            Key = key;
            Control = false;
            Shift = false;
            Alt = false;
            Caps = false;
        } // KeyEventArgs
        
        public KeyEventArgs(Keys key, bool control, bool shift, bool alt, bool caps)
        {
            Key = key;
            Control = control;
            Shift = shift;
            Alt = alt;
            Caps = caps;
        } // KeyEventArgs
        
        #endregion

    } // KeyEventArgs

    #endregion

    #region MouseEventArgs

    public class MouseEventArgs : EventArgs
    {

        #region Variables

        public MouseState State;
        public MouseButton Button = MouseButton.None;
        public Point Position = new Point(0, 0);
        public Point Difference = new Point(0, 0);
        
        #endregion

        #region Constructors

        public MouseEventArgs() { }

        public MouseEventArgs(MouseState state, MouseButton button, Point position)
        {
            State = state;
            Button = button;
            Position = position;
        } // MouseEventArgs

        #endregion

    } // MouseEventArgs

    #endregion

    #region DrawEventArgs

    public class DrawEventArgs : EventArgs
    {

        #region Variables
                
        public Rectangle Rectangle = Rectangle.Empty;
        public GameTime GameTime;
        
        #endregion

        #region Constructors

        public DrawEventArgs() { }
        
        public DrawEventArgs(Rectangle rectangle, GameTime gameTime)
        {
            Rectangle = rectangle;
            GameTime = gameTime;
        }  // DrawEventArgs

        #endregion

    } // DrawEventArgs

    #endregion

    #region ResizeEventArgs

    public class ResizeEventArgs : EventArgs
    {

        #region Variables

        public int Width;
        public int Height;
        public int OldWidth;
        public int OldHeight;

        #endregion

        #region Constructors

        public ResizeEventArgs() { }
        
        public ResizeEventArgs(int width, int height, int oldWidth, int oldHeight)
        {
            Width = width;
            Height = height;
            OldWidth = oldWidth;
            OldHeight = oldHeight;
        } // ResizeEventArgs

        #endregion

    } // ResizeEventArgs

    #endregion

    #region MoveEventArgs

    public class MoveEventArgs : EventArgs
    {

        #region Variables

        public int Left;
        public int Top;
        public int OldLeft;
        public int OldTop;

        #endregion

        #region Constructors

        public MoveEventArgs() { }
        
        public MoveEventArgs(int left, int top, int oldLeft, int oldTop)
        {
            Left = left;
            Top = top;
            OldLeft = oldLeft;
            OldTop = oldTop;
        } // MoveEventArgs

        #endregion

    } // MoveEventArgs

    #endregion

    #region DeviceEventArgs

    public class DeviceEventArgs : EventArgs
    {

        #region Variables

        public PreparingDeviceSettingsEventArgs DeviceSettings;
        
        #endregion

        #region Constructors
        
        public DeviceEventArgs() { }
        
        public DeviceEventArgs(PreparingDeviceSettingsEventArgs deviceSettings)
        {
            DeviceSettings = deviceSettings;
        } // DeviceEventArgs

        #endregion

    } // DeviceEventArgs

    #endregion

    #region WindowClosingEventArgs

    public class WindowClosingEventArgs : EventArgs
    {

        public bool Cancel;

    } // WindowClosingEventArgs

    #endregion

    #region WindowClosedEventArgs

    public class WindowClosedEventArgs : EventArgs
    {

        public bool Dispose;

    } // WindowClosedEventArgs

    #endregion

    #region ConsoleMessageEventArgs

    public class ConsoleMessageEventArgs : EventArgs
    {

        #region Variables
        
        public ConsoleMessage Message;        

        #endregion
        
        #region Constructors
        
        public ConsoleMessageEventArgs() { }
        
        public ConsoleMessageEventArgs(ConsoleMessage message)
        {
            Message = message;
        } // ConsoleMessageEventArgs

        #endregion

    } // ConsoleMessageEventArgs

    #endregion

} // XNAFinalEngine.UI