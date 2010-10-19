
#region License
/*
Copyright (c) 2008-2010, Laboratorio de Investigación y Desarrollo en Visualización y Computación Gráfica - 
                         Departamento de Ciencias e Ingeniería de la Computación - Universidad Nacional del Sur.
All rights reserved.
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

•	Redistributions of source code must retain the above copyright, this list of conditions and the following disclaimer.

•	Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer
    in the documentation and/or other materials provided with the distribution.

•	Neither the name of the Universidad Nacional del Sur nor the names of its contributors may be used to endorse or promote products derived
    from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

-----------------------------------------------------------------------------------------------------------------------------------------------
Author: Schneider, José Ignacio (jis@cs.uns.edu.ar)
-----------------------------------------------------------------------------------------------------------------------------------------------

*/
#endregion

#region Using directives
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Helpers;
using XNAFinalEngine.GraphicElements;
#endregion

namespace XNAFinalEngine.Sounds
{

    /// <summary>
    /// Play sound files. Supports positional sound and some other effects.
    /// We can’t dispose the sound from the system resources if we want to use it again in the same content manager.
    /// Because of that is difficult to use a good resource scheme.
    /// The XNA audio capabilities are also limited in other ways, for instance we can’t use more than 2 sound's channels.
    /// However, the XNA audio library is good enough for small to medium projects, and its code is clean.
    /// </summary>
    public class Sound
    {
        
        #region Variables

        /// <summary>
        /// XNA Sound Effect.
        /// </summary>
        private SoundEffect soundEffect;

        /// <summary>
        /// Sound Effect Instance.
        /// </summary>
        private SoundEffectInstance soundEffectInstance;

        /// <summary>
        /// The name of the sound file.
        /// </summary>
        private String soundFilename = "";

        /// <summary>
        /// Audio emitter, it’s needed by the positional sound.
        /// </summary>
        private AudioEmitter emitter = new AudioEmitter();

        /// <summary>
        /// Audio Listener, it’s needed by the positional sound.
        /// </summary>
        private AudioListener listener = new AudioListener();

        /// <summary>
        /// The graphic object that will be the sound listener.
        /// </summary>
        private XNAFinalEngine.GraphicElements.Object listenerObject = null;

        /// <summary>
        /// The camera that will be the sound listener.
        /// </summary>
        private Camera listenerCamera = null;

        /// <summary>
        /// The graphic object that will be the sound emitter.
        /// </summary>
        private XNAFinalEngine.GraphicElements.Object emitterObject = null;

        /// <summary>
        /// Sounds volume for this sound. Range between 0.0f to 1.0f. It will be multiplied by the master sound volume.
        /// </summary>
        private float volume = 0.8f;

        /// <summary>
        /// Doppler scale.
        /// This value determines how much to modify the calculated Doppler effect between this object and a AudioListener.
        /// Values below 1.0 scale down the Doppler effect to make it less apparent. Values above 1.0 exaggerate the Doppler effect.
        /// A value of 1.0 leaves the effect unmodified.
        /// Note that this value modifies only the calculated Doppler between this object and a AudioListener.
        /// The calculated Doppler is a product of the relationship between AudioEmitter.Velocity and AudioListener.Velocity.
        /// If the calculation yields a result of no Doppler effect, this value has no effect.
        /// </summary>
        private float dopplerScale = 1;

        #endregion

        #region Properties

        /// <summary>
        /// Sounds volume for this sound. Range between 0.0f to 1.0f. It will be multiplied by the master sound volume.
        /// </summary>
        public float Volume
        {
            get { return volume; }
            set { volume = value; }
        }

        /// <summary>
        /// Doppler scale
        /// This value determines how much to modify the calculated Doppler effect between this object and a AudioListener.
        /// Values below 1.0 scale down the Doppler effect to make it less apparent. Values above 1.0 exaggerate the Doppler effect.
        /// A value of 1.0 leaves the effect unmodified.
        /// Note that this value modifies only the calculated Doppler between this object and a AudioListener.
        /// The calculated Doppler is a product of the relationship between AudioEmitter.Velocity and AudioListener.Velocity.
        /// If the calculation yields a result of no Doppler effect, this value has no effect.
        /// </summary>
        /// </summary>
        public float DopplerScale
        {
            get { return dopplerScale; }
            set { dopplerScale = value; }
        }

        /// <summary>
        /// XNA sound state
        /// </summary>
        public SoundState State { get { return soundEffectInstance.State; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Hidden method that avoids hierarchy's problems.
        /// </summary>
        protected Sound() { }

        /// <summary>
        /// Creates a 2D sound. Initial state = stop.
        /// </summary>
        public Sound(string _soundFilename)
        {
            soundFilename = _soundFilename;
            string fullFilename = Directories.SoundsDirectory + "\\" + _soundFilename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load sound. File " + fullFilename + " does not exists!");
            } // if (File.Exists)
            try
            {
                soundEffect = EngineManager.Content.Load<SoundEffect>(fullFilename);
                soundEffectInstance = soundEffect.CreateInstance();
            } // try
            catch (Exception exception)
            {
                throw new Exception("Failed to load sound file: " + fullFilename + "\n" + exception.Message);
            }
            SoundManager.AddSound(this);
        } // Sound

        /// <summary>
        /// Creates a 3D sound. Initial state = stop.
        /// The listener is a graphic object.
        /// </summary>
        public Sound(string _soundFilename, XNAFinalEngine.GraphicElements.Object _listenerObject, XNAFinalEngine.GraphicElements.Object _emitterObject) : this(_soundFilename)
        {
            listenerObject = _listenerObject;
            emitterObject = _emitterObject;
            soundEffectInstance.Apply3D(listener, emitter);
        } // Sound

        /// <summary>
        /// Creates a 3D sound. Initial state = stop.
        /// The listener is a camera.
        /// </summary>
        public Sound(string _soundFilename, Camera _listenerObject, XNAFinalEngine.GraphicElements.Object _emitterObject) : this(_soundFilename)
        {
            listenerCamera = _listenerObject;
            emitterObject = _emitterObject;
            soundEffectInstance.Apply3D(listener, emitter);
        } // Sound

        #endregion

        #region Stop Play Pause

        /// <summary>
        /// Stop the sound.
        /// </summary>
        public void Stop()
        {            
            soundEffectInstance.Stop();
        } // Stop

        /// <summary>
        /// Play the sound from the start.
        /// </summary>
        public void Play()
        {
            soundEffectInstance.Play();
        } // Play

        /// <summary>
        /// Pause the sound.
        /// </summary>
        public void Pause()
        {
            soundEffectInstance.Pause();
        } // Pause

        /// <summary>
        /// Resume the sound.
        /// </summary>
        public void Resume()
        {
            soundEffectInstance.Resume();
        } // Resume

        #endregion

        #region Update

        /// <summary>
        /// Update sound parameters.
        /// </summary>
        public virtual void Update()
        {
            if (soundEffectInstance.State == SoundState.Playing)
            {
                soundEffectInstance.Volume = volume * SoundManager.MasterSoundVolume;
                if (emitterObject != null) // Si el sonido es 3D
                {
                    if (listenerObject != null) // Si el objeto receptor es un objeto grafico
                    {
                        listener.Position = listenerObject.WorldPosition;
                        listener.Forward = listenerObject.WorldMatrix.Forward;
                        listener.Up = listenerObject.WorldMatrix.Up;
                        listener.Velocity = listenerObject.Velocity;
                    }
                    else // Si el objeto receptor es una camara
                    {
                        listener.Position = ApplicationLogic.Camera.Position;
                        listener.Forward = ApplicationLogic.Camera.ZAxis; // Z or -Z?
                        listener.Up = ApplicationLogic.Camera.YAxis;
                        //listener.Velocity = ApplicationLogic.Camera.Velocity; // TODO
                    }
                    emitter.Position = emitterObject.WorldPosition;
                    emitter.Forward = emitterObject.WorldMatrix.Forward;
                    emitter.Up = emitterObject.WorldMatrix.Up;
                    emitter.Velocity = emitterObject.Velocity;
                    emitter.DopplerScale = dopplerScale;
                    soundEffectInstance.Apply3D(listener, emitter);
                }
            }
        } // Update

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose the sound.
        /// </summary>
        public void Dispose()
        {
            SoundManager.RemoveSound(this);
            // We can’t dispose the sound from the system resources if we want to use it again in the same content mananger.
            //soundEffectInstance.Dispose();
            //soundEffect.Dispose();
        } // Dispose

        #endregion

    } // Sound
} // XNAFinalEngine.Sounds

