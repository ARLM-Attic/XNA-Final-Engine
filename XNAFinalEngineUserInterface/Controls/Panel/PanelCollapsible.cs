
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
#endregion

namespace XNAFinalEngine.UserInterface
{

    /// <summary>
    /// Panel collapsible.
    /// </summary>
    public class PanelCollapsible : Panel
    {

        #region Variables

        private readonly TreeButton treeButton;

        #endregion

        #region Constructor

        /// <summary>
        /// Panel Collapsible.
        /// </summary>
        public PanelCollapsible()
        {
            CanFocus = false;
            Passive = false;
            BackgroundColor = Color.Transparent;

            // This is the control that manages the collapse functionality.
            treeButton = new TreeButton();
            Add(treeButton, false);
            treeButton.Width = Width;
            TextChanged += delegate { treeButton.Text = Text; };
            treeButton.Anchor = Anchors.Left | Anchors.Right | Anchors.Top;
            treeButton.CanFocus = false;

            // The client area is lowered to make place to the previous control.
            Margins m = ClientMargins;
            m.Top += 20;
            ClientMargins = m;
            
            // If the control is collaped or expanded...
            treeButton.CheckedChanged += delegate
            {
                int differencial;
                if (treeButton.Checked)
                {
                    // Only show the tree button.
                    ClientArea.Visible = false;
                    differencial = -Height + 20;
                    Height = 20; 
                }
                else
                {
                    // Show the client are.
                    ClientArea.Visible = true;
                    AdjustHeightFromChildren();
                    differencial = Height - 20;
                }
                if (Parent != null)
                {
                    // Move up or down the controls that are below this control
                    foreach (var childControl in Parent.ChildrenControls)
                    {
                        if (childControl.Top > Top && childControl.Anchor == Anchors.Top)
                        {
                            childControl.Top += differencial; 
                        }
                    }
                }
            };
            treeButton.Checked = false;
        } // PanelCollapsible

        #endregion

        #region Add and Remove

        internal override void Add(Control control, bool client)
        {
            base.Add(control, client);
            if (treeButton != null && !treeButton.Checked)
            {
                AdjustHeightFromChildren();
            }
        } // Add

        internal override void Remove(Control control)
        {
            base.Remove(control);
            if (treeButton != null && !treeButton.Checked)
            {
                AdjustHeightFromChildren();
            }
        } // Remove

        #endregion

        #region Draw Control

        /// <summary>
        /// Prerender the control into the control's render target.
        /// </summary>
        protected override void DrawControl(Rectangle rect)
        {
            // We only want to render the children.
        } // DrawControl

        #endregion

    } // PanelCollapsible
} // XNAFinalEngine.UserInterface
