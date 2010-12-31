
#region Using directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.UI;
using XNAFinalEngine.Sounds;
using XNAFinalEngine.Input;
#endregion

namespace XNAFinalEngine.Scenes
{

    /// <summary>
    /// Escena simple que muestra una caja texturada sobre un piso, un conjunto de luces afectandolos (vicibles al usuario)
    /// y la posibilidad de activar sombras y SSAO sobre la marcha.
    /// También permite modificar las transformaciones de los objetos y mover la camara al estilo XSI.
    /// Translate Manipulator: V
    /// Rotate Manipulator: C
    /// Scale Manipulator: X
    /// Camera Movement: S + Left Mouse, S + Right Mouse, Mouse Wheel
    /// Undo: CTRL-Z
    /// </summary>
    public class SceneTutorialTexturePicker : Scene
    {

        #region Variables

        #region Lights

        /// <summary>
        /// Scene's lights
        /// </summary>
        private Light pointLight1, directionalLight, spotLight;

        #endregion

        #region Objects

        private GraphicObject cube, sphere, cone, plane;

        private ContainerObject allObjects;

        #endregion

        #endregion

        #region Load

        /// <summary>
        /// Inicialización de la escena.
        /// </summary>
        public override void Load()
        {
            // Que camara quiero utilizar
            ApplicationLogic.Camera = new XSICamera(new Vector3(0, 0.5f, 0), 15, 0.35f, 0.3f);
            
            #region Load the models

            cube = new GraphicObject(new Box(2), new Blinn(Color.White));
            plane = new GraphicObject(new Graphics.Plane(20), new Ocean());
            
            #endregion

            #region Build the container object

            allObjects = new ContainerObject();
            allObjects.AddObject(plane);
            //allObjects.AddObject(cube);

            #endregion

            #region Lights

            AmbientLight.LightColor = new Color(0, 0, 0);

            spotLight = new SpotLight(new Vector3(-5, 5, 0), new Vector3(1, -1, 0), new Color(100, 100, 150), 30, 1);
            directionalLight = new Graphics.DirectionalLight(new Vector3(1.0f, 1.0f, 1.0f), new Color(100, 150, 100));
            pointLight1 = new PointLight(new Vector3(2, 5, 20), new Color(150, 100, 100));

            allObjects.AssociateLight(pointLight1);
            allObjects.AssociateLight(directionalLight);
            allObjects.AssociateLight(spotLight);
            
            #endregion

            #region Picker

            GizmoManipulator.AddObject(cube);

            #endregion

            plane.Material.OpenConfigurationWindow();

        } // Load

        #endregion

        #region Update

        /// <summary>
        /// Update de la escena. Este código se ejecuta justo antes de habilitar el renderizado de la escena.
        /// </summary>
        public override void Update()
        {
            //GizmoManipulator.ManipulateScene();
        } // Update

        #endregion

        #region Render

        /// <summary>
        /// Renderizado de la escena.
        /// </summary>
        public override void Render()
        {
            EngineManager.ClearTargetAndDepthBuffer(new Color(20, 50, 120));
                
            allObjects.Render();

            EngineManager.ClearDepthBuffer();
            
            #region UI     

            //GizmoManipulator.Render();
            
            #endregion

        } // Render

        #endregion

        #region UnloadContent

        /// <summary>
        /// Al finalizar el uso de la escena se liberan los recursos.
        /// </summary>
        public override void UnloadContent()
        {

        } // UnloadContent

        #endregion

    } // SceneTutorialTexturePicker
} // XNAFinalEngine.Scenes
