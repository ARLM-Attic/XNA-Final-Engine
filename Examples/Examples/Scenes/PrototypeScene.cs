
#region Using directives
using XNAFinalEngine.Editor;
using XNAFinalEngine.Components;
using XNAFinalEngine.Assets;
using Microsoft.Xna.Framework;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.UserInterface;
using Size = XNAFinalEngine.Helpers.Size;
#endregion

namespace XNAFinalEngineExamples
{

    /// <summary>
    /// Prototype Scene.
    /// </summary>
    public class PrototypeScene : EditableScene
    {

        #region Variables

        private static GameObject3D hangar, hangar2, hangar3, hangar4, hangar5, hangar6, hangar7, hangar8, hangar9, hangar10, hangar11,
                                    miniSection1_1, miniSection1_2, miniSection1_3, miniSection1_4, miniSection1_5, miniSection1_6, miniSection1_7,
                                    miniSection2_1, miniSection2_2, miniSection2_3, miniSection2_4, miniSection2_5, miniSection2_6,
                                    miniSection3_1, miniSection3_2, miniSection3_3, miniSection3_4, miniSection3_5, miniSection3_6, miniSection3_7, miniSection3_8, miniSection3_9, miniSection3_10, miniSection3_11, miniSection3_12, miniSection3_13, miniSection3_14,
                                    yhallway1, yhallway2, yhallway3, yhallway4, yhallway5, yhallway6, yhallway7, yhallway8,
                                    longHallway1, longHallway2, longHallway3, longHallway4, longHallway5, longHallway6, longHallway7, longHallway8, longHallway9, longHallway10,
                                    maintBay01_1, maintBay01_2, maintBay01_3, maintBay01_4, maintBay01_5, maintBay01_6, maintBay01_7, maintBay01_8, maintBay01_9, maintBay01_10,
                                    guardRoom1, guardRoom2, guardRoom3, guardRoom4, guardRoom5, guardRoom6, guardRoom7, guardRoom8, guardRoom9, guardRoom10,
                                    maintHallway1, maintHallway2, maintHallway3, maintHallway4, maintHallway5, maintHallway6, maintHallway7, maintHallway8, maintHallway9,
                                    maintBay02_1, maintBay02_2, maintBay02_3, maintBay02_4, maintBay02_5, maintBay02_6, maintBay02_7, maintBay02_8, maintBay02_9, maintBay02_10, maintBay02_11, maintBay02_12, maintBay02_13, maintBay02_14, maintBay02_15,
                                    MiniPartHanger1, MiniPartHanger2,
                                    collide1, collide2, collide3, collide4,
                                    camera, directionalLight;

        #region Lights

        private static GameObject3D hangarLight1, hangarLight2, hangarLight3, hangarLight4, hangarLight5, hangarLight6, hangarLight7, hangarLight8, hangarLight9, hangarLight10;
        
        #endregion

        #endregion

        #region Load

        /// <summary>
        /// Load the resources.
        /// </summary>
        /// <remarks>Remember to call the base implementation of this method at the end.</remarks>
        public override void Load()
        {
            // Call it before anything.
            base.Load();            

            #region Camera

            camera = new GameObject3D();
            camera.AddComponent<Camera>();
            camera.AddComponent<SoundListener>();
            camera.Camera.RenderTargetSize = Size.FullScreen;
            camera.Camera.FarPlane = 5000;
            camera.Camera.NearPlane = 0.1f;
            camera.Transform.LookAt(new Vector3(5, 0, 15), Vector3.Zero, Vector3.Up);            
            ScriptCustomCamera script = (ScriptCustomCamera)camera.AddComponent<ScriptCustomCamera>();
            script.SetPosition(new Vector3(249f, 263f, 278f), new Vector3(249f, 256f, 220f));
            camera.Camera.ClearColor = Color.Black;
            camera.Camera.FieldOfView = 180 / 6f;
            camera.Camera.PostProcess = new PostProcess();
            camera.Camera.PostProcess.ToneMapping.ToneMappingFunction = ToneMapping.ToneMappingFunctionEnumerate.FilmicALU;
            camera.Camera.PostProcess.MLAA.EdgeDetection = MLAA.EdgeDetectionType.Color;
            camera.Camera.PostProcess.MLAA.ThresholdColor = 0.1f;
            camera.Camera.PostProcess.Bloom.Threshold = 1;
            camera.Camera.AmbientLight = new AmbientLight
            {
                SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeMap(new TextureCube("FactoryCatwalkRGBM", true, 50)),
                //SphericalHarmonicLighting = SphericalHarmonicL2.GenerateSphericalHarmonicFromCubeMap(new TextureCube("Colors", false)),
                Color = new Color(15, 15, 15),
                Intensity = 0.12f,
                AmbientOcclusionStrength = 6f
            };            
            camera.Camera.Sky = new Skybox { TextureCube = new TextureCube("sky", true, 50) };
            camera.Camera.AmbientLight.AmbientOcclusion = new HorizonBasedAmbientOcclusion
            {
                NumberSteps = 8, // Don't change this.
                NumberDirections = 12, // Don't change this.
                Radius = 0.000003f, // Bigger values produce more cache misses and you don’t want GPU cache misses, trust me.
                LineAttenuation = 0.75f,
                Contrast = 1.0f,
                AngleBias = 1.25f,
                Quality = HorizonBasedAmbientOcclusion.QualityType.HighQuality,
                TextureSize = Size.TextureSize.HalfSize,
            };
            camera.Camera.PostProcess.FilmGrain.Enabled = true;
            camera.Camera.PostProcess.FilmGrain.Strength = 0.15f;

            #endregion

            #region Models

            #region Hangar

            hangar = new GameObject3D(new FileModel("Hangar/31"),
                                                 new BlinnPhong
                                                 {

                                                 });
            hangar.Transform.Position = new Vector3(0, 0, 0);
            hangar.Transform.Rotate(new Vector3(0, 0.1f, 0));

            hangar2 = new GameObject3D(new FileModel("Hangar/32"),
                                     new BlinnPhong
                                     {

                                     });
            hangar3 = new GameObject3D(new FileModel("Hangar/33"),
                         new BlinnPhong
                         {

                         });

            hangar4 = new GameObject3D(new FileModel("Hangar/34"),
                         new BlinnPhong
                         {

                         });
            hangar5 = new GameObject3D(new FileModel("Hangar/35"),
             new BlinnPhong
             {

             });

            hangar6 = new GameObject3D(new FileModel("Hangar/36"),
             new BlinnPhong
             {

             });

            hangar7 = new GameObject3D(new FileModel("Hangar/37"),
             new BlinnPhong
             {

             });

            hangar8 = new GameObject3D(new FileModel("Hangar/38"),
             new BlinnPhong
             {

             });

            hangar9 = new GameObject3D(new FileModel("Hangar/39"),
             new BlinnPhong
             {

             });

            hangar10 = new GameObject3D(new FileModel("Hangar/310"),
            new BlinnPhong
            {

            });

            hangar11 = new GameObject3D(new FileModel("Hangar/311"),
            new BlinnPhong
            {

            });

            #endregion
            
            #region Mini Section 1

            miniSection1_1 = new GameObject3D(new FileModel("MiniSection1/41"),
            new BlinnPhong
            {

            });

            miniSection1_2 = new GameObject3D(new FileModel("MiniSection1/42"),
            new BlinnPhong
            {

            });

            miniSection1_3 = new GameObject3D(new FileModel("MiniSection1/43"),
            new BlinnPhong
            {

            });

            miniSection1_4 = new GameObject3D(new FileModel("MiniSection1/44"),
            new BlinnPhong
            {

            });

            miniSection1_5 = new GameObject3D(new FileModel("MiniSection1/45"),
            new BlinnPhong
            {

            });

            miniSection1_6 = new GameObject3D(new FileModel("MiniSection1/46"),
            new BlinnPhong
            {

            });

            miniSection1_7 = new GameObject3D(new FileModel("MiniSection1/47"),
            new BlinnPhong
            {

            });

            #endregion

            #region Mini Section 2

            miniSection2_1 = new GameObject3D(new FileModel("MiniSection2/51"),
            new BlinnPhong
            {

            });

            miniSection2_2 = new GameObject3D(new FileModel("MiniSection2/52"),
            new BlinnPhong
            {

            });

            miniSection2_3 = new GameObject3D(new FileModel("MiniSection2/53"),
            new BlinnPhong
            {

            });

            miniSection2_4 = new GameObject3D(new FileModel("MiniSection2/54"),
            new BlinnPhong
            {

            });

            miniSection2_5 = new GameObject3D(new FileModel("MiniSection2/55"),
            new BlinnPhong
            {

            });

            miniSection2_6 = new GameObject3D(new FileModel("MiniSection2/56"),
            new BlinnPhong
            {

            });

            #endregion

            #region Mini Section 3

            miniSection3_1 = new GameObject3D(new FileModel("MiniSection3/61"),
            new BlinnPhong
            {

            });

            miniSection3_2 = new GameObject3D(new FileModel("MiniSection3/62"),
            new BlinnPhong
            {

            });

            miniSection3_3 = new GameObject3D(new FileModel("MiniSection3/63"),
            new BlinnPhong
            {

            });

            miniSection3_4 = new GameObject3D(new FileModel("MiniSection3/64"),
            new BlinnPhong
            {

            });

            miniSection3_5 = new GameObject3D(new FileModel("MiniSection3/65"),
            new BlinnPhong
            {

            });

            miniSection3_6 = new GameObject3D(new FileModel("MiniSection3/66"),
            new BlinnPhong
            {

            });

            miniSection3_7 = new GameObject3D(new FileModel("MiniSection3/67"),
            new BlinnPhong
            {

            });

            miniSection3_8 = new GameObject3D(new FileModel("MiniSection3/68"),
            new BlinnPhong
            {

            });

            miniSection3_9 = new GameObject3D(new FileModel("MiniSection3/69"),
            new BlinnPhong
            {

            });

            miniSection3_10 = new GameObject3D(new FileModel("MiniSection3/610"),
            new BlinnPhong
            {

            });

            miniSection3_11 = new GameObject3D(new FileModel("MiniSection3/611"),
            new BlinnPhong
            {

            });

            miniSection3_12 = new GameObject3D(new FileModel("MiniSection3/612"),
            new BlinnPhong
            {

            });

            miniSection3_13 = new GameObject3D(new FileModel("MiniSection3/613"),
            new BlinnPhong
            {

            });

            miniSection3_14 = new GameObject3D(new FileModel("MiniSection3/614"),
            new BlinnPhong
            {

            });

            #endregion

            #region YHallway
            yhallway1 = new GameObject3D(new FileModel("YHallway/71"),
            new BlinnPhong
            {

            });

            yhallway2 = new GameObject3D(new FileModel("YHallway/72"),
            new BlinnPhong
            {

            });

            yhallway3 = new GameObject3D(new FileModel("YHallway/73"),
            new BlinnPhong
            {

            });

            yhallway4 = new GameObject3D(new FileModel("YHallway/74"),
            new BlinnPhong
            {

            });

            yhallway5 = new GameObject3D(new FileModel("YHallway/75"),
            new BlinnPhong
            {

            });

            yhallway6 = new GameObject3D(new FileModel("YHallway/76"),
            new BlinnPhong
            {

            });

            yhallway7 = new GameObject3D(new FileModel("YHallway/77"),
            new BlinnPhong
            {

            });

            yhallway8 = new GameObject3D(new FileModel("YHallway/78"),
            new BlinnPhong
            {

            });

            #endregion

            #region Long Hallway

            longHallway1 = new GameObject3D(new FileModel("LongHallway/81"),
            new BlinnPhong
            {

            });

            longHallway2 = new GameObject3D(new FileModel("LongHallway/82"),
            new BlinnPhong
            {

            });

            longHallway3 = new GameObject3D(new FileModel("LongHallway/83"),
            new BlinnPhong
            {

            });

            longHallway4 = new GameObject3D(new FileModel("LongHallway/84"),
            new BlinnPhong
            {

            });

            longHallway5 = new GameObject3D(new FileModel("LongHallway/85"),
            new BlinnPhong
            {

            });

            longHallway6 = new GameObject3D(new FileModel("LongHallway/86"),
            new BlinnPhong
            {

            });

            longHallway7 = new GameObject3D(new FileModel("LongHallway/87"),
            new BlinnPhong
            {

            });

            longHallway8 = new GameObject3D(new FileModel("LongHallway/88"),
            new BlinnPhong
            {

            });

            longHallway9 = new GameObject3D(new FileModel("LongHallway/89"),
            new BlinnPhong
            {

            });

            longHallway10 = new GameObject3D(new FileModel("LongHallway/810"),
            new BlinnPhong
            {

            });
            #endregion

            #region Maint Bay 01

            maintBay01_1 = new GameObject3D(new FileModel("MaintBay01/91"),
            new BlinnPhong
            {

            });

            maintBay01_2 = new GameObject3D(new FileModel("MaintBay01/92"),
            new BlinnPhong
            {

            });

            maintBay01_3 = new GameObject3D(new FileModel("MaintBay01/93"),
            new BlinnPhong
            {

            });

            maintBay01_4 = new GameObject3D(new FileModel("MaintBay01/94"),
            new BlinnPhong
            {

            });

            maintBay01_5 = new GameObject3D(new FileModel("MaintBay01/95"),
            new BlinnPhong
            {

            });

            maintBay01_6 = new GameObject3D(new FileModel("MaintBay01/96"),
            new BlinnPhong
            {

            });

            maintBay01_7 = new GameObject3D(new FileModel("MaintBay01/97"),
            new BlinnPhong
            {

            });

            maintBay01_8 = new GameObject3D(new FileModel("MaintBay01/98"),
            new BlinnPhong
            {

            });

            maintBay01_9 = new GameObject3D(new FileModel("MaintBay01/99"),
            new BlinnPhong
            {

            });

            maintBay01_10 = new GameObject3D(new FileModel("MaintBay01/910"),
            new BlinnPhong
            {

            });
            #endregion

            #region Guard Room

            guardRoom1 = new GameObject3D(new FileModel("GuardRoom/t101"),
            new BlinnPhong
            {

            });

            guardRoom2 = new GameObject3D(new FileModel("GuardRoom/t102"),
            new BlinnPhong
            {

            });

            guardRoom3 = new GameObject3D(new FileModel("GuardRoom/t103"),
            new BlinnPhong
            {

            });

            guardRoom4 = new GameObject3D(new FileModel("GuardRoom/t104"),
            new BlinnPhong
            {

            });

            guardRoom5 = new GameObject3D(new FileModel("GuardRoom/t105"),
            new BlinnPhong
            {

            });

            guardRoom6 = new GameObject3D(new FileModel("GuardRoom/t106"),
            new BlinnPhong
            {

            });

            guardRoom7 = new GameObject3D(new FileModel("GuardRoom/t107"),
            new BlinnPhong
            {

            });

            guardRoom8 = new GameObject3D(new FileModel("GuardRoom/t108"),
            new BlinnPhong
            {

            });

            guardRoom9 = new GameObject3D(new FileModel("GuardRoom/t109"),
            new BlinnPhong
            {

            });

            guardRoom10 = new GameObject3D(new FileModel("GuardRoom/t1010"),
            new BlinnPhong
            {

            });
            #endregion

            #region Maint Hallway
            maintHallway1 = new GameObject3D(new FileModel("MaintHallway/t111"),
            new BlinnPhong
            {

            });

            maintHallway2 = new GameObject3D(new FileModel("MaintHallway/t112"),
            new BlinnPhong
            {

            });

            maintHallway3 = new GameObject3D(new FileModel("MaintHallway/t113"),
            new BlinnPhong
            {

            });

            maintHallway4 = new GameObject3D(new FileModel("MaintHallway/t114"),
            new BlinnPhong
            {

            });

            maintHallway5 = new GameObject3D(new FileModel("MaintHallway/t115"),
            new BlinnPhong
            {

            });

            maintHallway6 = new GameObject3D(new FileModel("MaintHallway/t116"),
            new BlinnPhong
            {

            });

            maintHallway7 = new GameObject3D(new FileModel("MaintHallway/t117"),
            new BlinnPhong
            {

            });

            maintHallway8 = new GameObject3D(new FileModel("MaintHallway/t118"),
            new BlinnPhong
            {

            });

            maintHallway9 = new GameObject3D(new FileModel("MaintHallway/t119"),
            new BlinnPhong
            {

            });
            #endregion

            #region Maint Bay 02
            maintBay02_1 = new GameObject3D(new FileModel("MaintBay02/t121"),
            new BlinnPhong
            {

            });

            maintBay02_2 = new GameObject3D(new FileModel("MaintBay02/t122"),
            new BlinnPhong
            {

            });

            maintBay02_3 = new GameObject3D(new FileModel("MaintBay02/t123"),
            new BlinnPhong
            {

            });

            maintBay02_4 = new GameObject3D(new FileModel("MaintBay02/t124"),
            new BlinnPhong
            {

            });

            maintBay02_5 = new GameObject3D(new FileModel("MaintBay02/t125"),
            new BlinnPhong
            {

            });

            maintBay02_6 = new GameObject3D(new FileModel("MaintBay02/t126"),
            new BlinnPhong
            {

            });

            maintBay02_7 = new GameObject3D(new FileModel("MaintBay02/t127"),
            new BlinnPhong
            {

            });

            maintBay02_8 = new GameObject3D(new FileModel("MaintBay02/t128"),
            new BlinnPhong
            {

            });
            maintBay02_9 = new GameObject3D(new FileModel("MaintBay02/t129"),
            new BlinnPhong
            {

            });
            maintBay02_10 = new GameObject3D(new FileModel("MaintBay02/t1211"),
            new BlinnPhong
            {

            });

            maintBay02_11 = new GameObject3D(new FileModel("MaintBay02/t1212"),
            new BlinnPhong
            {

            });

            maintBay02_12 = new GameObject3D(new FileModel("MaintBay02/t1213"),
            new BlinnPhong
            {

            });

            maintBay02_13 = new GameObject3D(new FileModel("MaintBay02/t1214"),
            new BlinnPhong
            {

            });

            maintBay02_14 = new GameObject3D(new FileModel("MaintBay02/t1215"),
            new BlinnPhong
            {

            });

            maintBay02_15 = new GameObject3D(new FileModel("MaintBay02/t1216"),
            new BlinnPhong
            {

            });
            #endregion

            #region MiniPartHanger
            MiniPartHanger1 = new GameObject3D(new FileModel("MiniPartHanger/t131"),
            new BlinnPhong
            {

            });

            MiniPartHanger2 = new GameObject3D(new FileModel("MiniPartHanger/t132"),
            new BlinnPhong
            {

            });
            #endregion

            #region Collide
            /*
            collide1 = new GameObject3D(new FileModel("Collide/t141"),
            new BlinnPhong
            {

            });

            collide2 = new GameObject3D(new FileModel("Collide/t142"),
            new BlinnPhong
            {

            });

            collide3 = new GameObject3D(new FileModel("Collide/t143"),
            new BlinnPhong
            {

            });

            collide4 = new GameObject3D(new FileModel("Collide/t144"),
            new BlinnPhong
            {

            });
            */
            #endregion
             
            #endregion

            #region Lights

            #region Hangar Lights

            PointLight pl;
            SpotLight sl;

            hangarLight1 = new GameObject3D();
            hangarLight1.Transform.Position = new Vector3(224.07f, 259.80f, 259.48f);
            hangarLight1.Transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            sl = (SpotLight)hangarLight1.AddComponent<SpotLight>();
            sl.DiffuseColor = new Color(1.0f, 1.0f, 1.0f);
            sl.Intensity = 4f;
            sl.Range = 30f;
            sl.OuterConeAngle = 120f;
            sl.InnerConeAngle = 105f;
            /*sl.LightMaskTexture = new Texture("LightMasks\\Crysis2TestLightMask");
            sl.Shadow = new BasicShadow
            {
                Filter = Shadow.FilterType.PCF3x3,
                LightDepthTextureSize = Size.Square1024X1024,
                TextureSize = Size.TextureSize.FullSize
            };*/
                        
            hangarLight2 = new GameObject3D();
            hangarLight2.Transform.Position = new Vector3(219.63f, 259.80f, 252.15f);
            hangarLight2.Transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            sl = (SpotLight)hangarLight2.AddComponent<SpotLight>();
            sl.DiffuseColor = new Color(1.0f, 1.0f, 1.0f);
            sl.Intensity = 4f;
            sl.Range = 30f;
            sl.OuterConeAngle = 120f;
            sl.InnerConeAngle = 105f;      
            
            hangarLight3 = new GameObject3D();
            hangarLight3.Transform.Position = new Vector3(232.40f, 259.80f, 263.69f);
            hangarLight3.Transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            sl = (SpotLight)hangarLight3.AddComponent<SpotLight>();
            sl.DiffuseColor = new Color(1.0f, 1.0f, 1.0f);
            sl.Intensity = 4f;
            sl.Range = 30f;
            sl.OuterConeAngle = 120f;
            sl.InnerConeAngle = 105f;         

            hangarLight4 = new GameObject3D();
            hangarLight4.Transform.Position = new Vector3(242.07f, 259.80f, 264.54f);
            hangarLight4.Transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            sl = (SpotLight)hangarLight4.AddComponent<SpotLight>();
            sl.DiffuseColor = new Color(1.0f, 1.0f, 1.0f);
            sl.Intensity = 4f;
            sl.Range = 30f;
            sl.OuterConeAngle = 120f;
            sl.InnerConeAngle = 105f;

            hangarLight5 = new GameObject3D();
            hangarLight5.Transform.Position = new Vector3(251.24f, 259.80f, 262.19f);
            hangarLight5.Transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            sl = (SpotLight)hangarLight5.AddComponent<SpotLight>();
            sl.DiffuseColor = new Color(1.0f, 1.0f, 1.0f);
            sl.Intensity = 4f;
            sl.Range = 30f;
            sl.OuterConeAngle = 120f;
            sl.InnerConeAngle = 105f;

            hangarLight6 = new GameObject3D();
            hangarLight6.Transform.Position = new Vector3(259.07f, 259.80f, 256.77f);
            hangarLight6.Transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            sl = (SpotLight)hangarLight6.AddComponent<SpotLight>();
            sl.DiffuseColor = new Color(1.0f, 1.0f, 1.0f);
            sl.Intensity = 4f;
            sl.Range = 30f;
            sl.OuterConeAngle = 120f;
            sl.InnerConeAngle = 105f;

            hangarLight7 = new GameObject3D();
            hangarLight7.Transform.Position = new Vector3(260.21f, 259.80f, 246.62f);
            hangarLight7.Transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            sl = (SpotLight)hangarLight7.AddComponent<SpotLight>();
            sl.DiffuseColor = new Color(1.0f, 1.0f, 1.0f);
            sl.Intensity = 4f;
            sl.Range = 30f;
            sl.OuterConeAngle = 120f;
            sl.InnerConeAngle = 105f;

            hangarLight8 = new GameObject3D();
            hangarLight8.Transform.Position = new Vector3(266.83f, 259.80f, 233.59f);
            hangarLight8.Transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            sl = (SpotLight)hangarLight8.AddComponent<SpotLight>();
            sl.DiffuseColor = new Color(1.0f, 1.0f, 1.0f);
            sl.Intensity = 4f;
            sl.Range = 30f;
            sl.OuterConeAngle = 120f;
            sl.InnerConeAngle = 105f;


            hangarLight9 = new GameObject3D();
            hangarLight9.Transform.Position = new Vector3(235.99f, 265.80f, 241.55f);
            hangarLight9.Transform.Rotate(new Vector3(-90.0f, 0.0f, 0.0f));
            sl = (SpotLight)hangarLight9.AddComponent<SpotLight>();
            sl.DiffuseColor = new Color(224, 244, 255);
            sl.Intensity = 8f;
            sl.Range = 28f;
            sl.OuterConeAngle = 115f;
            sl.InnerConeAngle = 100f;

            hangarLight10 = new GameObject3D(); 
            hangarLight10.Transform.Position = new Vector3(243.37f, 256.23f, 242.903f);            
            pl = (PointLight) hangarLight10.AddComponent<PointLight>();            
            pl.DiffuseColor = new Color(0.0f, 1.0f, 0.0f);
            pl.Intensity = 0.5f;
            pl.Range = 8f; 


            #endregion

            #endregion

            #region Statistics

            GameObject2D statistics = new GameObject2D();
            statistics.AddComponent<ScriptStatisticsDrawer>();

            #endregion

            PostProcessWindow.Show(camera.Camera.PostProcess);

        } // Load

        #endregion

        #region Update Tasks

        /// <summary>
        /// Tasks executed during the update.
        /// This is the place to put the application logic.
        /// </summary>
        public override void UpdateTasks()
        {
            base.UpdateTasks();
        } // UpdateTasks

        #endregion

        #region Render Tasks

        /// <summary>
        /// Tasks before the engine render.
        /// Some tasks are more related to the frame rendering than the update,
        /// or maybe the update frequency is too high to waste time in this kind of tasks,
        /// for that reason the pre render task exists.
        /// For example, is more correct to update the HUD information here because is related with the rendering.
        /// </summary>
        public override void PreRenderTasks()
        {

        } // PreRenderTasks

        /// <summary>
        /// Tasks after the engine render.
        /// Probably you won’t need to place any task here.
        /// </summary>
        public override void PostRenderTasks()
        {

        } // PostRenderTasks

        #endregion

    } // EmptyScene
} // XNAFinalEngineExamples