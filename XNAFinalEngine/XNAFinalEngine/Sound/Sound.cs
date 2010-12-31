
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
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using XNAFinalEngine.EngineCore;
using XNAFinalEngine.Graphics;
using XNAFinalEngine.Helpers;
#endregion

namespace XNAFinalEngine.Sounds
{

    /// <summary>
    /// Play sound files. Supports positional sound and some other effects.
    /// </summary>
    public class Sound
    {
        
        #region Variables

        /// <summary>
        /// XNA Sound Effect.
        /// </summary>
        private readonly SoundEffect soundEffect;

        #endregion

        #region Constructor

        /// <summary>
        /// Load the sound file.
        /// </summary>
        public Sound(string _soundFilename)
        {
            string fullFilename = Directories.SoundsDirectory + "\\" + _soundFilename;
            if (File.Exists(fullFilename + ".xnb") == false)
            {
                throw new Exception("Failed to load sound. File " + fullFilename + " does not exists!");
            } // if (File.Exists)
            try
            {
                if (EngineManager.UsesSystemContent)
                    soundEffect = EngineManager.SystemContent.Load<SoundEffect>(fullFilename);
                else
                    soundEffect = EngineManager.CurrentContent.Load<SoundEffect>(fullFilename);
            } // try
            catch (Exception exception)
            {
                throw new Exception("Failed to load sound file: " + fullFilename + "\n" + exception.Message);
            }
        } // Sound

        #endregion

        #region Play

        /// <summary>
        /// Play a new instance of this sound.
        /// </summary>
        public SoundInstance Play(float _volume = 1.0f)
        {
            SoundInstance soundInstance = new SoundInstance(soundEffect, _volume);
            SoundManager.AddSound(soundInstance);
            return soundInstance;
        } // Play

        /// <summary>
        /// Play a new instance of this sound.
        /// </summary>
        public SoundInstance Play(Graphics.Object _listenerObject, Graphics.Object _emitterObject,
                                  float _volume = 1.0f, float _dopplerScale = 1.0f)
        {
            SoundInstance soundInstance = new SoundInstance(soundEffect, _listenerObject, _emitterObject, _volume, _dopplerScale);
            SoundManager.AddSound(soundInstance);
            return soundInstance;
        } // Play

        /// <summary>
        /// Play a new instance of this sound.
        /// </summary>
        public SoundInstance Play(Camera _listenerObject, Graphics.Object _emitterObject,
                                  float _volume = 1.0f, float _dopplerScale = 1.0f)
        {
            SoundInstance soundInstance = new SoundInstance(soundEffect, _listenerObject, _emitterObject, _volume, _dopplerScale);
            SoundManager.AddSound(soundInstance);
            return soundInstance;
        } // Play

        #endregion

    } // Sound

    /// <summary>
    /// Sound Instance.
    /// </summary>
    public class SoundInstance
    {

        #region Variables

        /// <summary>
        /// Sound Effect Instance.
        /// </summary>
        private readonly SoundEffectInstance soundEffectInstance;

        /// <summary>
        /// Audio emitter, it’s needed by the positional sound.
        /// </summary>
        private readonly AudioEmitter emitter = new AudioEmitter();

        /// <summary>
        /// Emitter last position.
        /// </summary>
        private readonly Vector3 emitterLastPosition;

        /// <summary>
        /// Audio Listener, it’s needed by the positional sound.
        /// </summary>
        private readonly AudioListener listener = new AudioListener();

        /// <summary>
        /// Listener last position.
        /// </summary>
        private readonly Vector3 listenerLastPosition;

        /// <summary>
        /// The graphic object that will be the sound listener.
        /// </summary>
        private readonly Graphics.Object listenerObject;

        /// <summary>
        /// The camera that will be the sound listener.
        /// </summary>
        private readonly Camera listenerCamera;

        /// <summary>
        /// The graphic object that will be the sound emitter.
        /// </summary>
        private readonly Graphics.Object emitterObject;

        #endregion

        #region Properties

        /// <summary>
        /// Sounds volume for this sound. Range between 0.0f to 1.0f. It will be multiplied by the master sound volume.
        /// </summary>
        public float Volume { get; set; }

        /// <summary>
        /// Doppler scale
        /// This value determines how much to modify the calculated Doppler effect between this object and a AudioListener.
        /// Values below 1.0 scale down the Doppler effect to make it less apparent. Values above 1.0 exaggerate the Doppler effect.
        /// A value of 1.0 leaves the effect unmodified.
        /// Note that this value modifies only the calculated Doppler between this object and a AudioListener.
        /// The calculated Doppler is a product of the relationship between AudioEmitter.Velocity and AudioListener.Velocity.
        /// If the calculation yields a result of no Doppler effect, this value has no effect.
        /// </summary>
        public float DopplerScale { get; set; }
                
        /// <summary>
        /// Is over?
        /// </summary>
        public bool IsOver { get { return soundEffectInstance.State == SoundState.Stopped; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a 2D sound.
        /// </summary>
        public SoundInstance(SoundEffect soundEffect, float _volume = 1.0f)
        {
            soundEffectInstance = soundEffect.CreateInstance();
            Volume = _volume;
            soundEffectInstance.Play();
        } // SoundInstance

        /// <summary>
        /// Creates a 3D sound.
        /// The listener is a graphic object.
        /// </summary>
        public SoundInstance(SoundEffect soundEffect,
                             Graphics.Object _listenerObject, Graphics.Object _emitterObject,
                             float _volume = 1.0f, float _dopplerScale = 1.0f)
        {
            soundEffectInstance = soundEffect.CreateInstance();
            Volume = _volume;
            DopplerScale = _dopplerScale;
            listenerObject = _listenerObject;
            emitterObject = _emitterObject;
            listenerLastPosition = _listenerObject.WorldPosition;
            emitterLastPosition = _emitterObject.WorldPosition;
            soundEffectInstance.Apply3D(listener, emitter);
            soundEffectInstance.Play();
        } // SoundInstance

        /// <summary>
        /// Creates a 3D sound.
        /// The listener is a camera.
        /// </summary>
        public SoundInstance(SoundEffect soundEffect,
                             Camera _listenerObject, Graphics.Object _emitterObject,
                             float _volume = 1.0f, float _dopplerScale = 1.0f)
        {
            soundEffectInstance = soundEffect.CreateInstance();
            Volume = _volume;
            DopplerScale = _dopplerScale;
            listenerCamera = _listenerObject;
            emitterObject = _emitterObject;
            listenerLastPosition = _listenerObject.Position;
            emitterLastPosition = _emitterObject.WorldPosition;
            soundEffectInstance.Apply3D(listener, emitter);
            soundEffectInstance.Play();
        } // SoundInstance

        #endregion

        #region Stop Pause Resume

        /// <summary>
        /// Stop the sound.
        /// </summary>
        public void Stop()
        {
            soundEffectInstance.Stop();
        } // Stop

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
        /// Update sound instance parameters.
        /// </summary>
        internal virtual void Update()
        {
            if (soundEffectInstance.State == SoundState.Playing)
            {
                soundEffectInstance.Volume = Volume;
                if (emitterObject != null) // If the sound is 3D
                {
                    if (listenerObject != null) // If the listener is a graphic object
                    {
                        listener.Position = listenerObject.WorldPosition;
                        listener.Forward = listenerObject.WorldMatrix.Forward;
                        listener.Up = listenerObject.WorldMatrix.Up;
                        listener.Velocity = (listenerObject.WorldPosition - listenerLastPosition) / (float)EngineManager.FrameTime;
                    }
                    else // If the listener is a camera
                    {
                        listener.Position = ApplicationLogic.Camera.Position;
                        listener.Forward = -ApplicationLogic.Camera.ZAxis; // Z or -Z?
                        listener.Up = ApplicationLogic.Camera.YAxis;
                        listener.Velocity = (listenerCamera.Position - listenerLastPosition) / (float)EngineManager.FrameTime;
                    }
                    emitter.Position = emitterObject.WorldPosition;
                    emitter.Forward = emitterObject.WorldMatrix.Forward;
                    emitter.Up = emitterObject.WorldMatrix.Up;
                    emitter.Velocity = (emitterObject.WorldPosition - emitterLastPosition) / (float)EngineManager.FrameTime;
                    emitter.DopplerScale = DopplerScale;
                    soundEffectInstance.Apply3D(listener, emitter);
                }
            }
        } // Update

        #endregion

    } // SoundInstance

} // XNAFinalEngine.Sounds



