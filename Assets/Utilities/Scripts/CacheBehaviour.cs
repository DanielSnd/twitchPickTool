/*
The MIT License (MIT)

Copyright (c) 2015, Nick Gravelyn https://github.com/nickgravelyn/UnityToolbag/tree/master/CacheBehaviour

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using UnityEngine;
using System;

    /// <summary>
    /// A safe, drop-in replacement for MonoBehaviour as your base class. Each property value is cached
    /// and GetComponent will be called if the instance is null or is destroyed.
    /// </summary>
    public abstract class CacheBehaviour : MonoBehaviour
    {
        [HideInInspector, NonSerialized]
        private Animation _animation;

        /// <summary>
        /// Gets the Animation attached to the object.
        /// </summary>
        public new Animation animation { get { return _animation ? _animation : (_animation = GetComponent<Animation>()); } }

        [HideInInspector, NonSerialized]
        private AudioSource _audio;

        /// <summary>
        /// Gets the AudioSource attached to the object.
        /// </summary>
        public new AudioSource audio { get { return _audio ? _audio : (_audio = GetComponent<AudioSource>()); } }

        [HideInInspector, NonSerialized]
        private Camera _camera;

        /// <summary>
        /// Gets the Camera attached to the object.
        /// </summary>
        public new Camera camera { get { return _camera ? _camera : (_camera = GetComponent<Camera>()); } }

        [HideInInspector, NonSerialized]
        private Collider _collider;

        /// <summary>
        /// Gets the Collider attached to the object.
        /// </summary>
        public new Collider collider { get { return _collider ? _collider : (_collider = GetComponent<Collider>()); } }

        [HideInInspector, NonSerialized]
        private Collider2D _collider2D;

        /// <summary>
        /// Gets the Collider2D attached to the object.
        /// </summary>
        public new Collider2D collider2D { get { return _collider2D ? _collider2D : (_collider2D = GetComponent<Collider2D>()); } }

        [HideInInspector, NonSerialized]
        private ConstantForce _constantForce;

        /// <summary>
        /// Gets the ConstantForce attached to the object.
        /// </summary>
        public new ConstantForce constantForce { get { return _constantForce ? _constantForce : (_constantForce = GetComponent<ConstantForce>()); } }

        [HideInInspector, NonSerialized]
        private GUIText _guiText;

        /// <summary>
        /// Gets the GUIText attached to the object.
        /// </summary>
        public new GUIText guiText { get { return _guiText ? _guiText : (_guiText = GetComponent<GUIText>()); } }

        [HideInInspector, NonSerialized]
        private GUITexture _guiTexture;

        /// <summary>
        /// Gets the GUITexture attached to the object.
        /// </summary>
        public new GUITexture guiTexture { get { return _guiTexture ? _guiTexture : (_guiTexture = GetComponent<GUITexture>()); } }

        [HideInInspector, NonSerialized]
        private HingeJoint _hingeJoint;

        /// <summary>
        /// Gets the HingeJoint attached to the object.
        /// </summary>
        public new HingeJoint hingeJoint { get { return _hingeJoint ? _hingeJoint : (_hingeJoint = GetComponent<HingeJoint>()); } }

        [HideInInspector, NonSerialized]
        private Light _light;

        /// <summary>
        /// Gets the Light attached to the object.
        /// </summary>
        public new Light light { get { return _light ? _light : (_light = GetComponent<Light>()); } }

        [HideInInspector, NonSerialized]
        private NetworkView _networkView;

        /// <summary>
        /// Gets the NetworkView attached to the object.
        /// </summary>
        public new NetworkView networkView { get { return _networkView ? _networkView : (_networkView = GetComponent<NetworkView>()); } }

        [HideInInspector, NonSerialized]
        private ParticleEmitter _particleEmitter;

        /// <summary>
        /// Gets the ParticleEmitter attached to the object.
        /// </summary>
        public new ParticleEmitter particleEmitter { get { return _particleEmitter ? _particleEmitter : (_particleEmitter = GetComponent<ParticleEmitter>()); } }

        [HideInInspector, NonSerialized]
        private ParticleSystem _particleSystem;

        /// <summary>
        /// Gets the ParticleSystem attached to the object.
        /// </summary>
        public new ParticleSystem particleSystem { get { return _particleSystem ? _particleSystem : (_particleSystem = GetComponent<ParticleSystem>()); } }

        [HideInInspector, NonSerialized]
        private Renderer _renderer;

        /// <summary>
        /// Gets the Renderer attached to the object.
        /// </summary>
        public new Renderer renderer { get { return _renderer ? _renderer : (_renderer = GetComponent<Renderer>()); } }

        [HideInInspector, NonSerialized]
        private Rigidbody _rigidbody;

        /// <summary>
        /// Gets the Rigidbody attached to the object.
        /// </summary>
        public new Rigidbody rigidbody { get { return _rigidbody ? _rigidbody : (_rigidbody = GetComponent<Rigidbody>()); } }

        [HideInInspector, NonSerialized]
        private Rigidbody2D _rigidbody2D;

    /// <summary>
    /// Gets the Spriterenderer attached to the object.
    /// </summary>
    public new SpriteRenderer spriteRenderer {
        get
        {
            return _spriteRenderer ? _spriteRenderer :
                (_spriteRenderer = GetComponentInChildren<SpriteRenderer>());
        } }

    [HideInInspector, NonSerialized]
    private SpriteRenderer _spriteRenderer;

    /// <summary>
    /// Gets the Rigidbody2D attached to the object.
    /// </summary>
    public new Rigidbody2D rigidbody2D { get { return _rigidbody2D ? _rigidbody2D : (_rigidbody2D = GetComponent<Rigidbody2D>()); } }
    }