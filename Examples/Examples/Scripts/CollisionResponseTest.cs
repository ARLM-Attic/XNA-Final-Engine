#region License

/*
Copyright (c) 2008-2013, Schefer, Gustavo Martín.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

• Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

• Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

• The names of its contributors cannot be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schefer, Gustavo Martín (gusschefer@hotmail.com)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

using System.Text;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.Helpers;

namespace Nebula.Scripts
{
    /// <summary>
    /// Example script that paints green one of the GOs that are touching the GO this script belongs to.
    /// </summary>
    class CollisionResponseTest : Script
    {
        #region Variables

        /// <summary>
        /// Stores the last GO that collided with us
        /// </summary>
        private GameObject3D lastCollidedGO;
        
        /// <summary>
        /// Stores the material of the last GO  that collided with us
        /// </summary>
        private Material lastCollidedGOMaterial;

        /// <summary>
        /// The material used to paint the GOs hit
        /// </summary>
        private static Material greenMaterial = new BlinnPhong { DiffuseColor = new Color(0.0f, 0.85f, 0.0f) };

        private int enterCount = 0;
        private int exitCount = 0;
        private StringBuilder lastEvent = new StringBuilder();

        #endregion

        #region Properties

        /// <summary>
        /// Used to show debug info on screen. Will be removed in the future
        /// </summary>
        public HudText DebugText { get; set; }

        #endregion

        public override void OnCollisionEnter(BEPUphysics.Collidables.MobileCollidables.EntityCollidable collisionInfo)
        {
            enterCount++;
            lastEvent.ClearXbox();
            lastEvent.Append("CollisionEnter");

            // Paint green the GO that was hit
            // We only check ONE contact pair, NOT ALL of them. The GO could be in contact with two or more game objects.
            // We paint the first one and ingore the rest.
            GameObject3D go = collisionInfo.Pairs[0].EntityA.Tag as GameObject3D;
            if (go != null)
            {
                if (go == this.Owner)
                    go = collisionInfo.Pairs[0].EntityB.Tag as GameObject3D;

                // Save the go that was hit and the material that it has
                lastCollidedGO = go;
                lastCollidedGOMaterial = go.ModelRenderer.Material;

                // Paint it green
                go.ModelRenderer.Material = greenMaterial;
            }
        } // OnCollisionEnter
        

        public override void OnCollisionExit(BEPUphysics.Collidables.MobileCollidables.EntityCollidable collisionInfo)
        {
            exitCount++;
            lastEvent.ClearXbox();
            lastEvent.Append("CollisionExit");

            // Restore the last collided gameobject's original material
            if (lastCollidedGO != null)
                lastCollidedGO.ModelRenderer.Material = lastCollidedGOMaterial;
        } // OnCollisionExit


        public override void OnCollisionStay(BEPUphysics.Collidables.MobileCollidables.EntityCollidable collisionInfo)
        {
            lastEvent.ClearXbox();
            lastEvent.Append("CollisionStay");
        } // OnCollisionStay


        public override void Update()
        {
            DebugText.Text.ClearXbox();
            DebugText.Text.Append("White Box Info\n\n");
            DebugText.Text.Append("- OnCollisionEnter count: ");
            DebugText.Text.Append(enterCount);
            DebugText.Text.Append("\n- OnCollisionExit count: ");
            DebugText.Text.Append(exitCount);
            DebugText.Text.Append("\n- Last event: ");
            DebugText.Text.Append(lastEvent);
        } // Update

    } // CollisionResponseTest
} // Nebula.Scripts
