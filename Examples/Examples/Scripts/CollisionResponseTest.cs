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

using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Assets;
using XNAFinalEngine.Components;
using XNAFinalEngine.Helpers;

namespace XNAFinalEngineExamples
{
    /// <summary>
    /// Example script that paints green one of the GOs that are touching the GO this script belongs to.
    /// </summary>
    class CollisionResponseTest : Script
    {
        #region Variables
       
        /// <summary>
        /// Stores the material of the last GO  that collided with us
        /// </summary>
        private Material lastCollidedGOMaterial;

        /// <summary>
        /// The material used to paint the GOs hit
        /// </summary>
        private static Material greenMaterial = new BlinnPhong { DiffuseColor = new Color(0.4f, 0.85f, 0.35f), SpecularIntensity = 0.3f, SpecularPower = 200 };

        #endregion

        #region Properties

        /// <summary>
        /// Used to show debug info on screen. Will be removed in the future
        /// </summary>
        public HudText DebugText { get; set; }

        #endregion

        public override void OnCollisionEnter(GameObject3D go, ContactCollection contacts)
        {
            DebugText.Text.ClearXbox();
            DebugText.Text.Append("White Box Info\n\n- Last event: Collision Enter");

            // Save the last collided game object's material
            lastCollidedGOMaterial = go.ModelRenderer.Material;

            // Paint it green
            go.ModelRenderer.Material = greenMaterial;
        }

        public override void OnCollisionExit(GameObject3D go, ContactCollection contacts)
        {
            DebugText.Text.ClearXbox();
            DebugText.Text.Append("White Box Info\n\n- Last event: Collision Exit");

            // Restore the last collided gameobject's original material            
            go.ModelRenderer.Material = lastCollidedGOMaterial;
        }

        public override void OnCollisionStay(GameObject3D go, ContactCollection contacts)
        {
            DebugText.Text.ClearXbox();
            DebugText.Text.Append("White Box Info\n\n- Last event: Collision Stay");
            //var thisGo = (GameObject3D) Owner;
            //thisGo.RigidBody.Entity.ApplyImpulse(thisGo.Transform.Position, Vector3.Up * 0.5f);
        }

    } // CollisionResponseTest
} // Nebula.Scripts
