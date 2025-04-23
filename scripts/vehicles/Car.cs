using Godot;
using System;
using System.Collections.Generic;

namespace Crosswalk
{
    /// <summary>
    /// Abstract class Car for base methods and attributes for cars. Does following actions
    /// for cars: move, initialize, remove, plays animations, and checks raycast
    /// </summary>
    public abstract partial class Car : Area2D
    {
        #region Public Properties

        public virtual float _speed { get; set; }
        public virtual Vector2 _velocity { get; set; }
        public virtual float _brakingForce { get; set; }
        public virtual float _accelerationForce { get; set; }

        // Two different spawnpositions for cars
        public List<Vector2> _startPositions { get; set; } = new List<Vector2>
        {
            new Vector2(140, -400),
            new Vector2(220, -400)
        };

        #endregion

        #region Private Properties

        private bool _isStopped = false;
        private bool _isBraking = false;
        private bool _isAccelerating = false;
        private float _initialSpeed;
        private AnimatedSprite2D _animatedSprite; // Sprite for the car itself
        private AnimatedSprite2D _windshield; // Sprite for separated windshield
        private RayCast2D[] _raycasts; // Array for multiple raycasts

        #endregion

        #region Godot Built-In Methods

        /// <summary>
        /// Called when the node is added to the scene tree. Initializes sprites, raycasts, and speed.
        /// </summary>
        public override void _Ready()
        {
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            // Animation for windshield on a different sprite
            _windshield = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D/Windshield");
            if (_windshield != null)
            {
                _windshield = GetNode<AnimatedSprite2D>("AnimatedSprite2D/Windshield");
                _windshield.Play("drive");
            }
            AddToGroup("cars"); // Adds cars to a group so they can be removed later
            _initialSpeed = _speed;

            // Array for cars' raycasts, only active one used in current version 1.0 cars
            // is RCMiddle, but have left others for possible future uses.
            _raycasts = new RayCast2D[]
            {
                GetNodeOrNull<RayCast2D>("RaycastHolder/RCLeft"), // Left
                GetNodeOrNull<RayCast2D>("RaycastHolder/RCMLeft"), // Middle Left
                GetNodeOrNull<RayCast2D>("RaycastHolder/RCMiddle"), // Middle
                GetNodeOrNull<RayCast2D>("RaycastHolder/RCMRight"), // Middle Right
                GetNodeOrNull<RayCast2D>("RaycastHolder/RCRight") // Right
            };
        }

        /// <summary>
        /// Called every frame. Handles movement, animation, raycast checking, and car removal.
        /// </summary>
        public override void _Process(double delta)
        {
            if (IsAnyRaycastColliding())
            {
                // Slows vehicles using braking force
                _speed = Mathf.Max(_speed - _brakingForce * (float)delta, 0);
            }
            else if (!IsAnyRaycastColliding() && _speed < _initialSpeed)
            {
                // Accelerates car using acceleration force
                _speed = Mathf.Max(_speed + _accelerationForce * (float)delta, 0);
            }

            if (_speed < 1)
            {
                _animatedSprite.Play("idle");
                if (_windshield != null)
                {
                    _windshield.Pause();
                }
            }
            else
            {
                Move(delta);
                _animatedSprite.Play();
                if (_windshield != null)
                {
                    float custom_scale = _speed / 200; // Scaling speed for windshield animation
                    _windshield.Play("drive", custom_scale);
                }
            }

            // Goes through every child of Car class and removes those which are out of bounds
            foreach (Area2D car in GetTree().GetNodesInGroup("cars"))
            {
                if (car.Position.Y > 1500) // If true car is out of bounds
                {
                    car.QueueFree(); // Removes car instance
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the car's position and logs debug info.
        /// </summary>
        public void Initialize(Vector2 position)
        {
            Position = position;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if any of the raycasts is currently colliding with an object.
        /// </summary>
        private bool IsAnyRaycastColliding()
        {
            foreach (RayCast2D raycast in _raycasts)
            {
                if (raycast != null)
                {
                    raycast.ForceRaycastUpdate(); // Forces raycast check
                    if (raycast.IsColliding())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Moves the car forward based on current speed and frame delta.
        /// </summary>
        protected virtual void Move(double delta)
        {
            Position += new Vector2(0, _speed * (float)delta);
        }

        #endregion
    }
}
