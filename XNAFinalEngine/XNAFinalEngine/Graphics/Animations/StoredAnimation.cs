// Cuando el motor estaba hecho en DirectX se implemento la posibilidad de cargar animaciones de un archivo.
// Estara era la clase, funcionaba bien.

#region Using directives

#endregion

/*
namespace FinalEngine.GraphicObjects
{
    /// <summary>
    /// 
    /// </summary>
    public class StoredAnimation : Animation
    {

        #region Variables
                
        /// <summary>
        /// Container for all frames. The animation to realice.
        /// </summary>
        private AnimationRootFrame rootFrame;
        
        #endregion

        #region Constructor

        /// <summary>
        /// Creates the stored animation.
        /// </summary>
        public StoredAnimation(int dur, String animationFilename)
        {
            duration = dur;            
            elapsed = 0;            
            string fullFilename = Directories.ModelsDirectory + "\\" + animationFilename + ".x";
            if (File.Exists(fullFilename) == false)
            {
                throw new Exception("Failed to load model: File " + animationFilename + " does not exists!");
            }
            // Load the animation
            try
            {
                rootFrame = Mesh.LoadHierarchyFromFile(fullFilename, MeshFlags.Managed, DirectXForm.DirectXDevice,
                                                       new CustomAllocateHierarchy(), null);                
            } // try
            catch { throw new Exception("Unable to load the animated object"); }

        }
        #endregion

        #region Load Animation

        #region CustomFrame

        /// <summary>
        /// Custom frame class for animation.
        /// </summary>
        public class CustomFrame : Frame
        {
            public CustomFrame()
                : base()
            {
            } // CustomFrame()

            public CustomFrame(string setName)
                : base()
            {
                this.Name = setName;
            } // CustomFrame(name)
        } // class CustomFrame

        #endregion

        #region CustomAllocateHierarchy
        /// <summary>
        /// Custom allocate hierarchy
        /// </summary>
        public class CustomAllocateHierarchy : AllocateHierarchy
        {
            /// <summary>
            /// Create custom allocate hierarchy
            /// </summary>
            public CustomAllocateHierarchy()
                : base()
            {
            } // CustomAllocateHierarchy()

            /// <summary>
            /// Create frame
            /// </summary>
            public override Frame CreateFrame(string name)
            {
                return new CustomFrame(name);
            } // CreateFrame(name)

            /// <summary>
            /// Create mesh container
            /// </summary>
            public override MeshContainer CreateMeshContainer(string name, MeshData meshData,
                                                              ExtendedMaterial[] materials,
                                                              EffectInstance[] effectInstances,
                                                              GraphicsStream adjacency,
                                                              SkinInformation skinInfo)
            {
                return new CustomMeshContainer(name, meshData, materials, effectInstances, adjacency, skinInfo);
            } // CreateMeshContainer(name, meshData, materials)
        } // class CustomAllocateHierarchy

        #endregion

        #region CustomMeshContainer

        /// <summary>
        /// Custom mesh container. We dont need other information besides the animation and the simple dummy mesh.
        /// When find the dummy we incounter the frame that supouse to recide the real model.
        /// </summary>
        public class CustomMeshContainer : MeshContainer
        {            
            private Frame[] frameMatrices = null;

            private int numberOfAttributes = 0;

            /// <summary>
            /// Create custom mesh container
            /// </summary>
            public CustomMeshContainer(string setName, MeshData setMeshData, ExtendedMaterial[] setMaterials,
                                       EffectInstance[] setEffectInstances, GraphicsStream setAdjacency,
                                       SkinInformation setSkinInfo)
                : base()
            {
                // Ignore non meshes or invalid vertex format
                if (setMeshData.Mesh == null || setMeshData.Mesh.VertexFormat == VertexFormats.None)
                    return;
                try
                {
                    this.Name = setName;
                    this.SetMaterials(null);
                    this.SetEffectInstances(null);
                    this.SetAdjacency((GraphicsStream)null);
                    this.SkinInformation = null;                    
                    this.MeshData = setMeshData;
                } // try
                catch (Exception ex)
                {
                    throw new Exception("Failed to load the animated model meshes: " + ex.ToString());
                } // catch
            } // CustomMeshContainer(setName, setMeshData, setMaterials)
            
            /// <summary>
            /// Retrieve the animation frames used for this container
            /// </summary>
            public Frame[] FrameMatrices
            {
                get { return frameMatrices; }
                set { frameMatrices = value; }
            }

            /// <summary>
            /// Total number of attributes this mesh container contains
            /// </summary>
            public int NumberOfAttributes { get { return numberOfAttributes; } }
        } // class CustomMeshContainer
        #endregion
        
        #endregion

        #region Update Animation

        /// <summary>
        /// Calculate the Animation Matrix recursively through the structure.
        /// </summary>        
        private Matrix CalculateAnimationRecursively(Frame frame, Matrix parentMatrix)
        {            
            CustomMeshContainer mesh = frame.MeshContainer as CustomMeshContainer;            
            if (mesh != null)
            {
                return frame.TransformationMatrix * parentMatrix;
                //mesh = mesh.NextContainer as CustomMeshContainer;
            }
            Matrix worldAnimation;
            if (frame.FrameSibling != null)
            {
                worldAnimation = CalculateAnimationRecursively(frame.FrameSibling, parentMatrix);
                if (worldAnimation != Matrix.Zero) return worldAnimation;
            }
            if (frame.FrameFirstChild != null)
            {
                worldAnimation = CalculateAnimationRecursively(frame.FrameFirstChild, frame.TransformationMatrix * parentMatrix);
                if (worldAnimation != Matrix.Zero) return worldAnimation;
            }
            return Matrix.Zero;
        } // CalculateAnimationRecursively

        
        /// <summary>
        /// Render model with specified matrix.
        /// </summary>
        /// <param name="renderMatrix">Render matrix</param>
        public override Vector3 Update()
        {
            Animate();
            //obj.WorldObject = CalculateAnimationRecursively(rootFrame.FrameHierarchy, Matrix.Identity);
            return Vector3.Empty;
        } // Render(renderMatrix)

        float currentAnimationTime = 0.0f;
        /// <summary>
        /// Animate
        /// </summary>
        public void Animate()
        {
            if (rootFrame.AnimationController != null)
			{
				currentAnimationTime += ElapsedTime.MoveFactorPerSecond/60;
				// Reached to big number? Then reset.
                if (currentAnimationTime < 1.0e15f)
                {
                    double advanceTime = currentAnimationTime - rootFrame.AnimationController.Time;
                    // The advanceTime parameter should not be negative, DirectX expects 0 or a positive value.
                    if (advanceTime < 0)
                        advanceTime = 0.0f;
                    rootFrame.AnimationController.AdvanceTime(advanceTime);
                }
			} // if (rootFrame.AnimationController)
        } // Animate()


        /*public override void Update(Object obj)
        {
            int newElapsed = elapsed + ElapsedTime.MsLastFrame;
            float percentage;
            if (newElapsed < duration)
            {
                // The percentage of the duration of the animation that we need to realice in this frame.
                percentage = (float)(ElapsedTime.MsLastFrame) / duration;
                elapsed = newElapsed;
            }
            else
            {
                // We do the rest of the animation and no more.
                percentage = (float)(duration - elapsed) / duration;
                elapsed = duration;
            }
            // Calculate the amount of animation in this frame            
            //Vector3 thisFrameAnimation = animation * percentage;
            // Apply the animation to the object
            //obj.MoveRel(thisFrameAnimation.X, thisFrameAnimation.Y, thisFrameAnimation.Z);
        } // Update*/
/*

#endregion

#region Disposing
/// <summary>
/// Dispose
/// </summary>
public override void Dispose()
{
if (rootFrame.FrameHierarchy != null)
Frame.Destroy(rootFrame.FrameHierarchy, new CustomAllocateHierarchy());
if (rootFrame.AnimationController != null)
rootFrame.AnimationController.Dispose();
} // Dispose()
#endregion

} // StoredAnimation
} // FinalEngine.GraphicObjects
*/