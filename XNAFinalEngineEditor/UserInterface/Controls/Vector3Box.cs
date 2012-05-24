
#region License
/*
Copyright (c) 2008-2012, Laboratorio de Investigaci�n y Desarrollo en Visualizaci�n y Computaci�n Gr�fica - 
                         Departamento de Ciencias e Ingenier�a de la Computaci�n - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

�	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

�	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

�	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, Jos� Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using Microsoft.Xna.Framework;
using Keys = Microsoft.Xna.Framework.Input.Keys;

#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// A composite control with 3 text box to modified vector3 data.
    /// </summary>
    public class Vector3Box : Control
    {
        
        #region Variables

        // Controls
        private readonly TextBox xTextBox, yTextBox, zTextBox;
        
        private Vector3 value;

        #endregion

        #region Properties
        
        /// <summary>
        /// Current value.
        /// </summary>
        public virtual Vector3 Value
        {
            get { return value; }
            set
            {
                if (value != Value)
                {
                    this.value = value;
                    OnValueChanged(new EventArgs());
                }
            }
        } // Value

        #endregion

        #region Events

        public event EventHandler ValueChanged;

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose managed resources.
        /// </summary>
        protected override void DisposeManagedResources()
        {
            // A disposed object could be still generating events, because it is alive for a time, in a disposed state, but alive nevertheless.
            ValueChanged = null;
            base.DisposeManagedResources();
        } // DisposeManagedResources

        #endregion

        #region Constructor

        /// <summary>
        /// A composite control with 3 text box to modified vector3 data.
        /// </summary>
        public Vector3Box()
        {
            Anchor = Anchors.Left | Anchors.Right | Anchors.Top;
            Width = 420;
            Height = 25;
            CanFocus = false;
            Passive = true;
            var xLabel = new Label
            {
                Parent = this,
                Width = 15,
                Height = 25,
                Text = "X"
            };
            xTextBox = new TextBox
            {
                Parent = this,
                Width = 100,
                Left = xLabel.Width,
                Text = "0",
            };
            var yLabel = new Label
            {
                Parent = this,
                Left = xTextBox.Left + xTextBox.Width +  8,
                Width = 15,
                Height = 25,
                Text = "Y"
            };
            yTextBox = new TextBox
            {
                Parent = this,
                Width = 100,
                Left = yLabel.Left + yLabel.Width,
                Text = "0",
            };
            var zLabel = new Label
            {
                Parent = this,
                Left = yTextBox.Left + yTextBox.Width + 8,
                Width = 15,
                Height = 25,
                Text = "Z"
            };
            zTextBox = new TextBox
            {
                Parent = this,
                Left = zLabel.Left + zLabel.Width,
                Width = 100,
                Text = "0",
            };
            
            #region Events

            KeyEventHandler keyHandler = delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Keys.Enter)
                {
                    try
                    {
                        Value = new Vector3((float)double.Parse(xTextBox.Text), (float)double.Parse(yTextBox.Text), (float)double.Parse(zTextBox.Text));
                    }
                    catch // If not numeric
                    {
                        xTextBox.Text = value.X.ToString();
                        yTextBox.Text = value.Y.ToString();
                        zTextBox.Text = value.Z.ToString();
                    }
                }
            };
            xTextBox.KeyDown += keyHandler;
            yTextBox.KeyDown += keyHandler;
            zTextBox.KeyDown += keyHandler;
            // For tabs and other not so common things.
            EventHandler focusHandler = delegate
            {
                try
                {
                    Value = new Vector3((float)double.Parse(xTextBox.Text), (float)double.Parse(yTextBox.Text), (float)double.Parse(zTextBox.Text));
                }
                catch // If not numeric
                {
                    xTextBox.Text = value.X.ToString();
                    yTextBox.Text = value.Y.ToString();
                    zTextBox.Text = value.Z.ToString();
                }
            };
            xTextBox.FocusLost += focusHandler;
            yTextBox.FocusLost += focusHandler;
            zTextBox.FocusLost += focusHandler;

            ValueChanged += delegate
            {
                xTextBox.Text = value.X.ToString();
                yTextBox.Text = value.Y.ToString();
                zTextBox.Text = value.Z.ToString();
            };
            
            #endregion
            
        } // Vector3Box

        #endregion

        #region Draw

        protected override void DrawControl(Rectangle rect)
        {
            // Only the children will be rendered.
        } // DrawControl

        #endregion

        #region On Value Changed

        protected virtual void OnValueChanged(EventArgs e)
        {
            if (ValueChanged != null) 
                ValueChanged.Invoke(this, e);
        } // OnValueChanged

        #endregion

    } // Vector3Box
} // XNAFinalEngine.UserInterface