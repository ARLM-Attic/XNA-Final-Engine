
#region License
/*

 Based in the project Neoforce Controls (http://neoforce.codeplex.com/)
 GNU Library General Public License (LGPL)

-----------------------------------------------------------------------------------------------------------------------------------------------
Modified by: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Input;
using XNAFinalEngine.Assets;
using XNAFinalEngine.EngineCore;
using Keyboard = XNAFinalEngine.Input.Keyboard;
#endregion

namespace XNAFinalEngine.UserInterface
{

    #region Enumerators

    public enum TextBoxMode
    {
        Normal,
        Password,
        Multiline
    } // TextBoxMode

    #endregion

    /// <summary>
    /// Text Box.
    /// </summary>
    public class TextBox : ClipControl
    {

        #region Structs

        /// <summary>
        /// Text Selection.
        /// </summary>
        private struct Selection
        {
            /// <summary>
            /// Selection start position. Undefined value is -1.
            /// </summary>
            private int start;

            /// <summary>
            /// Selection end position. Undefined value is -1.
            /// </summary>
            private int end;

            /// <summary>
            /// Selection start position. Undefined value is -1.
            /// </summary>
            public int Start
            {
                get
                {
                    if (start > end && start != -1 && end != -1) 
                        return end;
                    return start;
                }
                set { start = value; }
            } // Start

            /// <summary>
            /// Selection end position. Undefined value is -1.
            /// </summary>
            public int End
            {
                get
                {
                    if (end < start && start != -1 && end != -1) 
                        return start;
                    return end;
                }
                set { end = value; }
            } // End

            /// <summary>
            /// Is empty?
            /// </summary>
            public bool IsEmpty
            {
                get { return Start == -1 && End == -1; }
            } // IsEmpty

            /// <summary>
            /// Selection Length.
            /// </summary>
            public int Length
            {
                get { return IsEmpty ? 0 : (End - Start); }
            } // Length

            public Selection(int start, int end)
            {
                this.start = start;
                this.end = end;
            } // Selection

            /// <summary>
            /// Empty the selection.
            /// </summary>
            public void Clear()
            {
                Start = -1;
                End = -1;
            } // Clear
        } // Selection

        #endregion

        #region Variables

        /// <summary>
        /// Cursor's position.
        /// </summary>
        private int positionX;
        private int positionY;

        /// <summary>
        /// Scrollbar Type.
        /// </summary>
        private ScrollBarType scrollBarType = ScrollBarType.Both;

        /// <summary>
        /// Scrollbars.
        /// </summary>
        private readonly ScrollBar horizontalScrollBar;
        private readonly ScrollBar verticalScrollBar;

        /// <summary>
        /// Password Character.
        /// </summary>
        private char passwordCharacter = '�';

        /// <summary>
        /// Caret navigation or text cursor visible.
        /// </summary>
        private bool caretVisible = true;

        /// <summary>
        /// Auto select text when focus is gained. For example when you press the tab key until the control is reached.
        /// </summary>
        private bool autoSelection = true;

        /// <summary>
        /// Text Box Mode (Normal, Password, Multiline).
        /// </summary>
        private TextBoxMode mode = TextBoxMode.Normal;
        
        /// <summary>
        /// Text is read only?
        /// </summary>
        private bool readOnly;

        /// <summary>
        /// Draw Borders?
        /// </summary>
        private bool drawBorders = true;

        private bool showCursor;
        private double flashTime;
        private string shownText = "";
        private Selection selection = new Selection(-1, -1);
        private List<string> lines = new List<string>();
        private int linesDrawn;
        private int charsDrawn;
        private Font font;
        private bool wordWrap;
        private const string Separator = "\n";
        private string text = "";
        private string buffer = "";

        /// <summary>
        /// The text that the control had when it gained focus.
        /// </summary>
        private string initialText;

        #endregion

        #region Properties

        #region Position

        /// <summary>
        /// Cursor's X position.
        /// </summary>
        private int PositionX
        {
            get { return positionX; }
            set
            {
                positionX = value;
                if (positionX < 0)
                    positionX = 0;
                if (positionX > Lines[PositionY].Length) 
                    positionX = Lines[PositionY].Length;
            }
        } // positionX

        /// <summary>
        /// Cursor's Y position.
        /// </summary>
        private int PositionY
        {
            get { return positionY; }
            set
            {
                positionY = value;

                if (positionY < 0) 
                    positionY = 0;
                if (positionY > Lines.Count - 1) 
                    positionY = Lines.Count - 1;
                if (positionX > Lines[PositionY].Length)
                    positionX = Lines[PositionY].Length;
            }
        } // PositionY

        /// <summary>
        /// Cursor's position.
        /// </summary>
        private int Position
        {
            get { return GetPos(PositionX, PositionY); }
            set
            {
                PositionY = GetPosY(value);
                PositionX = GetPosX(value);
            }
        } // Position

        #endregion

        /// <summary>
        ///  Scrollbar Type (None, Vertical, Horizontal, Both).
        /// </summary>
        public virtual ScrollBarType ScrollBars
        {
            get { return scrollBarType; }
            set
            {
                scrollBarType = value;
                SetupScrollBars();
            }
        } // ScrollBars

        /// <summary>
        /// Password Character. Default: �
        /// </summary>
        public virtual char PasswordCharacter
        {
            get { return passwordCharacter; }
            set { passwordCharacter = value; if (ClientArea != null) ClientArea.Invalidate(); }
        } // PasswordCharacter

        /// <summary>
        /// Caret navigation or text cursor visible.
        /// </summary>
        public virtual bool CaretVisible
        {
            get { return caretVisible; }
            set { caretVisible = value; }
        } // CaretVisible

        /// <summary>
        /// Text Box Mode (Normal, Password, Multiline).
        /// </summary>
        public virtual TextBoxMode Mode
        {
            get { return mode; }
            set
            {
                if (value != TextBoxMode.Multiline)
                {
                    Text = Text.Replace(Separator, "");
                }
                mode = value;
                selection.Clear();

                if (ClientArea != null) ClientArea.Invalidate();
                SetupScrollBars();
            }
        } // TextBoxMode

        /// <summary>
        /// Text is read only?
        /// </summary>
        public virtual bool ReadOnly
        {
            get { return readOnly; }
            set { readOnly = value; }
        } // ReadOnly

        /// <summary>
        /// Draw Borders?
        /// </summary>
        public virtual bool DrawBorders
        {
            get { return drawBorders; }
            set { drawBorders = value; if (ClientArea != null) ClientArea.Invalidate(); }
        } // DrawBorders

        #region Selection

        /// <summary>
        /// Selected Text.
        /// </summary>
        public virtual string SelectedText
        {
            get
            {
                if (selection.IsEmpty)
                {
                    return "";
                }
                return Text.Substring(selection.Start, selection.Length);
            }
        } // SelectedText

        /// <summary>
        /// Selection Start.
        /// </summary>
        public virtual int SelectionStart
        {
            get
            {
                if (selection.IsEmpty)
                    return Position;
                return selection.Start;
            }
            set
            {
                Position = value;
                if (Position < 0) Position = 0;
                if (Position > Text.Length) Position = Text.Length;
                selection.Start = Position;
                if (selection.End == -1) selection.End = Position;
                ClientArea.Invalidate();
            }
        } // SelectionStart

        /// <summary>
        /// Auto select text when focus is gained. For example when you press the tab key until the control is reached, or when you click the control.
        /// </summary>
        public virtual bool AutoSelection
        {
            get { return autoSelection; }
            set { autoSelection = value; }
        } // AutoSelection

        /// <summary>
        /// Selection Length.
        /// </summary>
        public virtual int SelectionLength
        {
            get { return selection.Length; }
            set
            {
                if (value == 0)
                {
                    selection.End = selection.Start;
                }
                else if (selection.IsEmpty)
                {
                    selection.Start = 0;
                    selection.End = value;
                }
                else if (!selection.IsEmpty)
                {
                    selection.End = selection.Start + value;
                }

                if (!selection.IsEmpty)
                {
                    if (selection.Start < 0) selection.Start = 0;
                    if (selection.Start > Text.Length) selection.Start = Text.Length;
                    if (selection.End < 0) selection.End = 0;
                    if (selection.End > Text.Length) selection.End = Text.Length;
                }
                ClientArea.Invalidate();
            }
        } // SelectionLength

        #endregion

        /// <summary>
        /// Text.
        /// </summary>
        public override string Text
        {
            get { return text; }
            set
            {
                if (mode != TextBoxMode.Multiline && value != null)
                {
                    value = value.Replace(Separator, "");
                }

                text = value;

                if (!Suspended) OnTextChanged(new EventArgs());

                lines = SplitLines(text);
                if (ClientArea != null) ClientArea.Invalidate();

                SetupScrollBars();
                ProcessScrolling();
            }
        } // Text

        private List<string> Lines
        {
            get { return lines; }
            set { lines = value; }
        } // Lines

        #endregion

        #region Constructor

        /// <summary>
        /// Text Box.
        /// </summary>
        public TextBox()
        {
            CheckLayer(SkinInformation, "Cursor");

            SetDefaultSize(128, 20);
            Lines.Add("");

            verticalScrollBar = new ScrollBar(Orientation.Vertical)
            {
                Range = 1,
                PageSize = 1,
                Value = 0,
                Anchor = Anchors.Top | Anchors.Right | Anchors.Bottom,
                Visible = false
            };
            horizontalScrollBar = new ScrollBar(Orientation.Horizontal)
            {
                Range = ClientArea.Width,
                PageSize = ClientArea.Width,
                Value = 0,
                Anchor = Anchors.Right | Anchors.Left | Anchors.Bottom,
                Visible = false
            };
        } // TextBox

        #endregion

        #region Init

        protected internal override void Init()
        {
            base.Init();
            ClientArea.Draw += ClientAreaDraw;
            verticalScrollBar.ValueChanged += ScrollBarValueChanged;
            horizontalScrollBar.ValueChanged += ScrollBarValueChanged;
            Add(verticalScrollBar, false);
            Add(horizontalScrollBar, false);
            FocusGained += delegate { initialText = Text; };
        } // Init

        protected internal override void InitSkin()
        {
            base.InitSkin();
            SkinInformation = new SkinControl(Skin.Controls["TextBox"]);

            #if (WINDOWS)
                Cursor = Skin.Cursors["Text"].Cursor;
            #endif

            font = (SkinInformation.Layers["Control"].Text != null) ? SkinInformation.Layers["Control"].Text.Font.Font : null;
        } // InitSkin

        #endregion
        
        #region Draw

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            if (drawBorders)
            {
                base.DrawControl(rect);
            }
        } // DrawControl

        /// <summary>
        /// Determine Pages.
        /// </summary>
        private void DeterminePages()
        {
            if (ClientArea != null)
            {
                int sizey = font.LineSpacing;
                linesDrawn = ClientArea.Height / sizey;
                if (linesDrawn > Lines.Count) linesDrawn = Lines.Count;

                charsDrawn = ClientArea.Width - 1;
            }
        } // DeterminePages

        /// <summary>
        /// Get Maximum Line.
        /// </summary>
        private string GetMaxLine()
        {
            int max = 0;
            int x = 0;

            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i].Length > max)
                {
                    max = Lines[i].Length;
                    x = i;
                }
            }
            return Lines.Count > 0 ? Lines[x] : "";
        } // GetMaxLine

        /// <summary>
        /// Draw the text.
        /// </summary>
        private void ClientAreaDraw(object sender, DrawEventArgs e)
        {
            Color col = SkinInformation.Layers["Control"].Text.Colors.Enabled;
            SkinLayer cursor = SkinInformation.Layers["Cursor"];
            Alignment al = mode == TextBoxMode.Multiline ? Alignment.TopLeft : Alignment.MiddleLeft;
            Rectangle r = e.Rectangle;
            bool drawsel = !selection.IsEmpty;
            string tmpText;

            font = (SkinInformation.Layers["Control"].Text != null) ? SkinInformation.Layers["Control"].Text.Font.Font : null;

            if (Text != null && font != null)
            {
                DeterminePages();

                if (mode == TextBoxMode.Multiline)
                {
                    shownText = Text;
                    tmpText = Lines[PositionY];
                }
                else if (mode == TextBoxMode.Password)
                {
                    shownText = "";
                    foreach (char character in Text)
                    {
                        shownText = shownText + passwordCharacter;
                    }
                    tmpText = shownText;
                }
                else
                {
                    shownText = Text;
                    tmpText = Lines[PositionY];
                }

                if (TextColor != UndefinedColor && ControlState != ControlState.Disabled)
                {
                    col = TextColor;
                }

                if (mode != TextBoxMode.Multiline)
                {
                    linesDrawn = 0;
                    verticalScrollBar.Value = 0;
                }

                if (drawsel)
                {
                    DrawSelection(r);
                }

                int sizey = font.LineSpacing;

                if (showCursor && caretVisible)
                {
                    Vector2 size = Vector2.Zero;
                    if (PositionX > 0 && PositionX <= tmpText.Length)
                    {
                        size = font.MeasureString(tmpText.Substring(0, PositionX));
                    }
                    if (size.Y == 0)
                    {
                        size = font.MeasureString(" ");
                        size.X = 0;
                    }

                    int m = r.Height - font.LineSpacing;

                    Rectangle rc = new Rectangle(r.Left - horizontalScrollBar.Value + (int)size.X, r.Top + m / 2, cursor.Width, font.LineSpacing);

                    if (mode == TextBoxMode.Multiline)
                    {
                        rc = new Rectangle(r.Left + (int)size.X - horizontalScrollBar.Value, r.Top + (int)((PositionY - verticalScrollBar.Value) * font.LineSpacing), cursor.Width, font.LineSpacing);
                    }
                    cursor.Alignment = al;
                    Renderer.DrawLayer(cursor, rc, col, 0);
                }

                for (int i = 0; i < linesDrawn + 1; i++)
                {
                    int ii = i + verticalScrollBar.Value;
                    if (ii >= Lines.Count || ii < 0) break;

                    if (Lines[ii] != "")
                    {
                        if (mode == TextBoxMode.Multiline)
                        {
                            Renderer.DrawString(font.XnaSpriteFont, Lines[ii], r.Left - horizontalScrollBar.Value, r.Top + (i * sizey), col);
                        }
                        else
                        {
                            Rectangle rx = new Rectangle(r.Left - horizontalScrollBar.Value, r.Top, r.Width, r.Height);
                            Renderer.DrawString(font.XnaSpriteFont, shownText, rx, col, al, false);
                        }
                    }
                }
            }
        } // ClientArea_Draw

                private void DrawSelection(Rectangle rect)
        {
            if (!selection.IsEmpty)
            {
                int s = selection.Start;
                int e = selection.End;

                int sl = GetPosY(s);
                int el = GetPosY(e);
                int sc = GetPosX(s);
                int ec = GetPosX(e);

                int hgt = font.LineSpacing;

                int start = sl;
                int end = el;

                if (start < verticalScrollBar.Value) start = verticalScrollBar.Value;
                if (end > verticalScrollBar.Value + linesDrawn) end = verticalScrollBar.Value + linesDrawn;

                for (int i = start; i <= end; i++)
                {
                    Rectangle r = Rectangle.Empty;

                    if (mode == TextBoxMode.Normal)
                    {
                        int m = ClientArea.Height - font.LineSpacing;
                        r = new Rectangle(rect.Left - horizontalScrollBar.Value + (int)font.MeasureString(Lines[i].Substring(0, sc)).X, rect.Top + m / 2,
                                         (int)font.MeasureString(Lines[i].Substring(0, ec + 0)).X - (int)font.MeasureString(Lines[i].Substring(0, sc)).X, hgt);
                    }
                    else if (sl == el)
                    {
                        r = new Rectangle(rect.Left - horizontalScrollBar.Value + (int)font.MeasureString(Lines[i].Substring(0, sc)).X, rect.Top + (i - verticalScrollBar.Value) * hgt,
                                          (int)font.MeasureString(Lines[i].Substring(0, ec + 0)).X - (int)font.MeasureString(Lines[i].Substring(0, sc)).X, hgt);
                    }
                    else
                    {
                        if (i == sl) r = new Rectangle(rect.Left - horizontalScrollBar.Value + (int)font.MeasureString(Lines[i].Substring(0, sc)).X, rect.Top + (i - verticalScrollBar.Value) * hgt, (int)font.MeasureString(Lines[i]).X - (int)font.MeasureString(Lines[i].Substring(0, sc)).X, hgt);
                        else if (i == el) r = new Rectangle(rect.Left - horizontalScrollBar.Value, rect.Top + (i - verticalScrollBar.Value) * hgt, (int)font.MeasureString(Lines[i].Substring(0, ec + 0)).X, hgt);
                        else r = new Rectangle(rect.Left - horizontalScrollBar.Value, rect.Top + (i - verticalScrollBar.Value) * hgt, (int)font.MeasureString(Lines[i]).X, hgt);
                    }

                    Renderer.Draw(Skin.Images["Control"].Texture.XnaTexture, r, Color.FromNonPremultiplied(160, 160, 160, 128));
                }
            }
        } // DrawSelection

        #endregion

        #region Process Scrolling

        private int GetStringWidth(string text, int count)
        {
            if (count > text.Length) count = text.Length;
            return (int)font.MeasureString(text.Substring(0, count)).X;
        } // GetStringWidth

        private void ProcessScrolling()
        {
            if (verticalScrollBar != null && horizontalScrollBar != null)
            {
                verticalScrollBar.PageSize = linesDrawn;
                horizontalScrollBar.PageSize = charsDrawn;

                if (horizontalScrollBar.PageSize > horizontalScrollBar.Range) horizontalScrollBar.PageSize = horizontalScrollBar.Range;

                if (PositionY >= verticalScrollBar.Value + verticalScrollBar.PageSize)
                {
                    verticalScrollBar.Value = (PositionY + 1) - verticalScrollBar.PageSize;
                }
                else if (PositionY < verticalScrollBar.Value)
                {
                    verticalScrollBar.Value = PositionY;
                }

                if (GetStringWidth(Lines[PositionY], PositionX) >= horizontalScrollBar.Value + horizontalScrollBar.PageSize)
                {
                    horizontalScrollBar.Value = (GetStringWidth(Lines[PositionY], PositionX) + 1) - horizontalScrollBar.PageSize;
                }
                else if (GetStringWidth(Lines[PositionY], PositionX) < horizontalScrollBar.Value)
                {
                    horizontalScrollBar.Value = GetStringWidth(Lines[PositionY], PositionX) - horizontalScrollBar.PageSize;
                }
            }
        } // ProcessScrolling

        #endregion

        #region Update

        protected internal override void Update()
        {
            base.Update();

            bool showCursorTemp = showCursor;

            showCursor = Focused;

            if (Focused)
            {
                flashTime += Time.GameDeltaTime;
                showCursor = flashTime < 0.5;
                if (flashTime > 1) flashTime = 0;
            }
            if (showCursorTemp != showCursor) ClientArea.Invalidate();
        } // Update

        #endregion

        #region Auxilliary Methods.

        /// <summary>
        /// Find previous word from current position.
        /// </summary>
        private int FindPreviousWord(string text)
        {
            bool letter = false;

            int p = Position - 1;
            if (p < 0) p = 0;
            if (p >= text.Length) p = text.Length - 1;
            
            for (int i = p; i >= 0; i--)
            {
                if (char.IsLetterOrDigit(text[i]))
                {
                    letter = true;
                    continue;
                }
                if (letter && !char.IsLetterOrDigit(text[i]))
                {
                    return i + 1;
                }
            }

            return 0;
        } // FindPreviousWord

        /// <summary>
        /// Find next word from current position.
        /// </summary>
        private int FindNextWord(string text)
        {
            bool space = false;

            for (int i = Position; i < text.Length - 1; i++)
            {
                if (!char.IsLetterOrDigit(text[i]))
                {
                    space = true;
                    continue;
                }
                if (space && char.IsLetterOrDigit(text[i]))
                {
                    return i;
                }
            }

            return text.Length;
        } // FindNextWord

        private int GetPosY(int pos)
        {
            if (pos >= Text.Length) return Lines.Count - 1;

            int p = pos;
            for (int i = 0; i < Lines.Count; i++)
            {
                p -= Lines[i].Length + Separator.Length;
                if (p < 0)
                {
                    return i;
                }
            }
            return 0;
        }

        private int GetPosX(int pos)
        {
            if (pos >= Text.Length) return Lines[Lines.Count - 1].Length;

            int p = pos;
            for (int i = 0; i < Lines.Count; i++)
            {
                p -= Lines[i].Length + Separator.Length;
                if (p < 0)
                {
                    p = p + Lines[i].Length + Separator.Length;
                    return p;
                }
            }
            return 0;
        }

        private int GetPos(int x, int y)
        {
            int p = 0;

            for (int i = 0; i < y; i++)
            {
                p += Lines[i].Length + Separator.Length;
            }
            p += x;

            return p;
        }

        private int CharAtPos(Point pos)
        {
            int x = pos.X;
            int y = pos.Y;
            int px = 0;
            int py = 0;

            if (mode == TextBoxMode.Multiline)
            {
                py = verticalScrollBar.Value + (int)((y - ClientTop) / font.LineSpacing);
                if (py < 0) py = 0;
                if (py >= Lines.Count) py = Lines.Count - 1;
            }
            else
            {
                py = 0;
            }

            string str = mode == TextBoxMode.Multiline ? Lines[py] : shownText;

            if (!string.IsNullOrEmpty(str))
            {
                for (int i = 1; i <= Lines[py].Length; i++)
                {
                    Vector2 v = font.MeasureString(str.Substring(0, i)) - (font.MeasureString(str[i - 1].ToString()) / 3);
                    if (x <= (ClientLeft + (int)v.X) - horizontalScrollBar.Value)
                    {
                        px = i - 1;
                        break;
                    }
                }
                if (x > ClientLeft + ((int)font.MeasureString(str).X) - horizontalScrollBar.Value - (font.MeasureString(str[str.Length - 1].ToString()).X / 3)) px = str.Length;
            }

            return GetPos(px, py);
        }

        #endregion

        #region On Mouse Down

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            flashTime = 0;

            Position = CharAtPos(e.Position);
            selection.Clear();

            if (e.Button == MouseButton.Left && caretVisible && mode != TextBoxMode.Password)
            {
                selection.Start = Position;
                selection.End = Position;
            }
            ClientArea.Invalidate();
        } // OnMouseDown

        #endregion

        #region On Mouse Move

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.Button == MouseButton.Left && !selection.IsEmpty && mode != TextBoxMode.Password && selection.Length < Text.Length)
            {
                int pos = CharAtPos(e.Position);
                selection.End = CharAtPos(e.Position);
                Position = pos;

                ClientArea.Invalidate();

                ProcessScrolling();
            }
        } // OnMouseMove

        #endregion

        #region On Mouse Up

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (e.Button == MouseButton.Left && !selection.IsEmpty && mode != TextBoxMode.Password)
            {
                if (selection.Length == 0) selection.Clear();
            }
        } // OnMouseUp

        #endregion

        #region On Key Press

        protected override void OnKeyPress(KeyEventArgs e)
        {
            flashTime = 0;
            
            if (!e.Handled)
            {
                if (e.Key == Keys.Escape)
                {
                    Text = initialText;
                    Focused = false;
                }
                if (e.Key == Keys.A && e.Control && mode != TextBoxMode.Password)
                {
                    SelectAll();
                }
                if (e.Key == Keys.Up)
                {
                    e.Handled = true;

                    if (e.Shift && selection.IsEmpty && mode != TextBoxMode.Password)
                    {
                        selection.Start = Position;
                    }
                    if (!e.Control)
                    {
                        PositionY -= 1;
                    }
                }
                else if (e.Key == Keys.Down)
                {
                    e.Handled = true;
                    if (e.Shift && selection.IsEmpty && mode != TextBoxMode.Password)
                    {
                        selection.Start = Position;
                    }
                    if (!e.Control)
                    {
                        PositionY += 1;
                    }
                }
                else if (e.Key == Keys.Back && !readOnly)
                {
                    e.Handled = true;
                    if (!selection.IsEmpty)
                    {
                        Text = Text.Remove(selection.Start, selection.Length);
                        Position = selection.Start;
                    }
                    else if (Text.Length > 0 && Position > 0)
                    {
                        Position -= 1;
                        Text = Text.Remove(Position, 1);
                    }
                    selection.Clear();
                }
                else if (e.Key == Keys.Delete && !readOnly)
                {
                    e.Handled = true;
                    if (!selection.IsEmpty)
                    {
                        Text = Text.Remove(selection.Start, selection.Length);
                        Position = selection.Start;
                    }
                    else if (Position < Text.Length)
                    {
                        Text = Text.Remove(Position, 1);
                    }
                    selection.Clear();
                }
                else if (e.Key == Keys.Left)
                {
                    e.Handled = true;
                    if (e.Shift && selection.IsEmpty && mode != TextBoxMode.Password)
                    {
                        selection.Start = Position;
                    }
                    if (!e.Control)
                    {
                        Position -= 1;
                    }
                    if (e.Control)
                    {
                        Position = FindPreviousWord(shownText);
                    }
                }
                else if (e.Key == Keys.Right)
                {
                    e.Handled = true;
                    if (e.Shift && selection.IsEmpty && mode != TextBoxMode.Password)
                    {
                        selection.Start = Position;
                    }
                    if (!e.Control)
                    {
                        Position += 1;
                    }
                    if (e.Control)
                    {
                        Position = FindNextWord(shownText);
                    }
                }
                else if (e.Key == Keys.Home)
                {
                    e.Handled = true;
                    if (e.Shift && selection.IsEmpty && mode != TextBoxMode.Password)
                    {
                        selection.Start = Position;
                    }
                    if (!e.Control)
                    {
                        PositionX = 0;
                    }
                    if (e.Control)
                    {
                        Position = 0;
                    }
                }
                else if (e.Key == Keys.End)
                {
                    e.Handled = true;
                    if (e.Shift && selection.IsEmpty && mode != TextBoxMode.Password)
                    {
                        selection.Start = Position;
                    }
                    if (!e.Control)
                    {
                        PositionX = Lines[PositionY].Length;
                    }
                    if (e.Control)
                    {
                        Position = Text.Length;
                    }
                }
                else if (e.Key == Keys.PageUp)
                {
                    e.Handled = true;
                    if (e.Shift && selection.IsEmpty && mode != TextBoxMode.Password)
                    {
                        selection.Start = Position;
                    }
                    if (!e.Control)
                    {
                        PositionY -= linesDrawn;
                    }
                }
                else if (e.Key == Keys.PageDown)
                {
                    e.Handled = true;
                    if (e.Shift && selection.IsEmpty && mode != TextBoxMode.Password)
                    {
                        selection.Start = Position;
                    }
                    if (!e.Control)
                    {
                        PositionY += linesDrawn;
                    }
                }
                else if (e.Key == Keys.Enter && mode == TextBoxMode.Multiline && !readOnly)
                {
                    e.Handled = true;
                    Text = Text.Insert(Position, Separator);
                    PositionX = 0;
                    PositionY += 1;
                }
                else if (e.Key == Keys.Tab)
                {
                }
                else if (!readOnly && !e.Control)
                {
                    string c = Keyboard.KeyToString(e.Key, e.Shift, e.Caps);
                    if (selection.IsEmpty)
                    {
                        Text = Text.Insert(Position, c);
                        if (c != "") PositionX += 1;
                    }
                    else
                    {
                        if (Text.Length > 0)
                        {
                            Text = Text.Remove(selection.Start, selection.Length);
                            Text = Text.Insert(selection.Start, c);
                            Position = selection.Start + 1;
                        }
                        selection.Clear();
                    }
                }

                if (e.Shift && !selection.IsEmpty)
                {
                    selection.End = Position;
                }

                #region Copy Paste

                // Windows only because it uses the Clipboard class. Of course this could be implemented manually in the XBOX 360 if you want it.
                #if (WINDOWS)
                if (e.Control && e.Key == Keys.C && mode != TextBoxMode.Password)
                {
                    System.Windows.Forms.Clipboard.Clear();
                    if (mode != TextBoxMode.Password && !selection.IsEmpty)
                    {
                        System.Windows.Forms.Clipboard.SetText((Text.Substring(selection.Start, selection.Length)).Replace("\n", Environment.NewLine));
                    }
                }
                else if (e.Control && e.Key == Keys.V && !readOnly && mode != TextBoxMode.Password)
                {
                    string t = System.Windows.Forms.Clipboard.GetText().Replace(Environment.NewLine, "\n");
                    if (selection.IsEmpty)
                    {
                        Text = Text.Insert(Position, t);
                        Position = Position + t.Length;
                    }
                    else
                    {
                        Text = Text.Remove(selection.Start, selection.Length);
                        Text = Text.Insert(selection.Start, t);
                        PositionX = selection.Start + t.Length;
                        selection.Clear();
                    }
                }
                #endif

                #endregion

                if ((!e.Shift && !e.Control) || Text.Length <= 0)
                {
                    selection.Clear();
                }

                if (e.Control && e.Key == Keys.Down)
                {
                    e.Handled = true;
                    HandleGuide(PlayerIndex.One);
                }
                flashTime = 0;
                if (ClientArea != null) ClientArea.Invalidate();

                DeterminePages();
                ProcessScrolling();
            }
            base.OnKeyPress(e);
        } // OnKeyPress

        #endregion

        #region Handle Guide

        private void HandleGuide(PlayerIndex pi)
        {
            if (!Guide.IsVisible)
            {
                Guide.BeginShowKeyboardInput(pi, "Enter Text", "", Text, GetText, pi.ToString());
            }
        } // HandleGuide

        private void GetText(IAsyncResult result)
        {
            string res = Guide.EndShowKeyboardInput(result);
            Text = res != null ? res : "";
            Position = text.Length;
        } // GetText

        #endregion

        #region Setup Scroll Bars

        private void SetupScrollBars()
        {
            DeterminePages();

            if (verticalScrollBar != null) verticalScrollBar.Range = Lines.Count;
            if (horizontalScrollBar != null)
            {
                horizontalScrollBar.Range = (int)font.MeasureString(GetMaxLine()).X;
                if (horizontalScrollBar.Range == 0) horizontalScrollBar.Range = ClientArea.Width;
            }

            if (verticalScrollBar != null)
            {
                verticalScrollBar.Left = Width - 16 - 2;
                verticalScrollBar.Top = 2;
                verticalScrollBar.Height = Height - 4 - 16;

                if (Height < 50 || (scrollBarType != ScrollBarType.Both && scrollBarType != ScrollBarType.Vertical)) verticalScrollBar.Visible = false;
                else if ((scrollBarType == ScrollBarType.Vertical || scrollBarType == ScrollBarType.Both) && mode == TextBoxMode.Multiline) verticalScrollBar.Visible = true;
            }
            if (horizontalScrollBar != null)
            {
                horizontalScrollBar.Left = 2;
                horizontalScrollBar.Top = Height - 16 - 2;
                horizontalScrollBar.Width = Width - 4 - 16;

                if (Width < 50 || wordWrap || (scrollBarType != ScrollBarType.Both && scrollBarType != ScrollBarType.Horizontal)) horizontalScrollBar.Visible = false;
                else if ((scrollBarType == ScrollBarType.Horizontal || scrollBarType == ScrollBarType.Both) && mode == TextBoxMode.Multiline && !wordWrap) horizontalScrollBar.Visible = true;
            }

            AdjustMargins();

            if (verticalScrollBar != null) verticalScrollBar.PageSize = linesDrawn;
            if (horizontalScrollBar != null) horizontalScrollBar.PageSize = charsDrawn;
        } // SetupScrollBars

        #endregion

        #region Adjust Margins

        protected override void AdjustMargins()
        {
            if (horizontalScrollBar != null && !horizontalScrollBar.Visible)
            {
                verticalScrollBar.Height = Height - 4;
                ClientMargins = new Margins(ClientMargins.Left, ClientMargins.Top, ClientMargins.Right, SkinInformation.ClientMargins.Bottom);
            }
            else
            {
                ClientMargins = new Margins(ClientMargins.Left, ClientMargins.Top, ClientMargins.Right, 18 + SkinInformation.ClientMargins.Bottom);
            }

            if (verticalScrollBar != null && !verticalScrollBar.Visible)
            {
                horizontalScrollBar.Width = Width - 4;
                ClientMargins = new Margins(ClientMargins.Left, ClientMargins.Top, SkinInformation.ClientMargins.Right, ClientMargins.Bottom);
            }
            else
            {
                ClientMargins = new Margins(ClientMargins.Left, ClientMargins.Top, 18 + SkinInformation.ClientMargins.Right, ClientMargins.Bottom);
            }
            base.AdjustMargins();
        } // AdjustMargins

        #endregion

        #region On Resize

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            selection.Clear();
            SetupScrollBars();
        } // OnResize

        #endregion

        #region Select All

        public virtual void SelectAll()
        {
            if (text.Length > 0)
            {
                selection.Start = 0;
                selection.End = Text.Length;
            }
        } // SelectAll

        #endregion

        #region Split Lines

        private List<string> SplitLines(string text)
        {
            if (buffer != text)
            {
                buffer = text;
                List<string> list = new List<string>();
                string[] s = text.Split(new char[] { Separator[0] });
                list.Clear();

                list.AddRange(s);

                if (positionY < 0) positionY = 0;
                if (positionY > list.Count - 1) positionY = list.Count - 1;

                if (positionX < 0) positionX = 0;
                if (positionX > list[PositionY].Length) positionX = list[PositionY].Length;

                return list;
            }
            return lines;
        } // SplitLines

        #endregion

        #region Scrollbar Value Changed

        void ScrollBarValueChanged(object sender, EventArgs e)
        {
            ClientArea.Invalidate();
        } // scrollBarValueChanged

        #endregion

        #region On Focus Lost, Gained

        /// <summary>
        /// If the control lost focus then...
        /// </summary>
        protected override void OnFocusLost()
        {
            selection.Clear();
            ClientArea.Invalidate();
            base.OnFocusLost();
        } // OnFocusLost

        /// <summary>
        /// If the control gained focus then...
        /// </summary>
        protected override void OnFocusGained()
        {
            if (!readOnly && autoSelection)
            {
                SelectAll();
                ClientArea.Invalidate();
            }
            base.OnFocusGained();
        } // OnFocusGained

        #endregion

    } // TextBox
} // XNAFinalEngine.UserInterface