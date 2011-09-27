
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

namespace XNAFinalEngine.UserInterface
{

    public delegate void DeviceEventHandler(DeviceEventArgs e);
    public delegate void SkinEventHandler(EventArgs e);

    public delegate void EventHandler(object sender, EventArgs e);
    public delegate void MouseEventHandler(object sender, MouseEventArgs e);
    public delegate void KeyEventHandler(object sender, KeyEventArgs e);
    public delegate void DrawEventHandler(object sender, DrawEventArgs e);
    public delegate void MoveEventHandler(object sender, MoveEventArgs e);
    public delegate void ResizeEventHandler(object sender, ResizeEventArgs e);
    public delegate void WindowClosingEventHandler(object sender, WindowClosingEventArgs e);
    public delegate void WindowClosedEventHandler(object sender, WindowClosedEventArgs e);
    public delegate void ConsoleMessageEventHandler(object sender, ConsoleMessageEventArgs e);

} // XNAFinalEngine.UI
