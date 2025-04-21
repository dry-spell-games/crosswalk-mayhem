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

        public virtual float Speed { get; set; }
        public virtual Vector2 Velocity { get; set; }
        public virtual float BrakingForce { get; set; }
        public virtual float AccelerationForce { get; set; }

        // Two different spawnpositions for cars
        public List<Vector2> StartPositions { get; set; } = new List<Vector2>
        {
            new Vector2(140, -400),
            new Vector2(220, -400)
        };

        #endregion

        #region Private Properties

        private bool isStopped = false;
        private bool isBraking = false;
        private bool isAccelerating = false;
        protected float InitialSpeed;
        private AnimatedSprite2D animatedSprite; // Sprite for the car itself
        private AnimatedSprite2D windShield; // Sprite for separated windshield
        private RayCast2D[] raycasts; // Array for multiple raycasts

        #endregion

        #region Godot Built-In Methods

        /// <summary>
        /// Called when the node is added to the scene tree. Initializes sprites, raycasts, and speed.
        /// </summary>
        public override void _Ready()
        {
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            // Animation for windshield on a different sprite
            windShield = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D/Windshield");
            if (windShield != null)
            {
                windShield = GetNode<AnimatedSprite2D>("AnimatedSprite2D/Windshield");
                windShield.Play("drive");
            }
            AddToGroup("cars"); // Adds cars to a group so they can be removed later
            InitialSpeed = Speed;

            // Array for cars' raycasts
            raycasts = new RayCast2D[]
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
                Speed = Mathf.Max(Speed - BrakingForce * (float)delta, 0);
            }
            else if (!IsAnyRaycastColliding() && Speed < InitialSpeed)
            {
                Speed = Mathf.Max(Speed + AccelerationForce * (float)delta, 0);
            }

            if (Speed < 1)
            {
                animatedSprite.Play("idle");
                if (windShield != null)
                {
                    windShield.Pause();
                }
            }
            else
            {
                Move(delta);
                animatedSprite.Play();
                if (windShield != null)
                {
                    float custom_scale = Speed / 200; // Scaling speed for windshield animation
                    windShield.Play("drive", custom_scale);
                }
            }

            // Goes through every child of Car class and removes those which are out of bounds
            foreach (Area2D car in GetTree().GetNodesInGroup("cars"))
            {
                if (car.Position.Y > 1500) // If true car is out of bounds
                {
                    GD.Print("Poistetaan auto: ", car.Name);
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
            GD.Print($"{this} spawned at {Position} with speed {Speed}");
            GD.Print("auto instantioitu");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if any of the raycasts is currently colliding with an object.
        /// </summary>
        private bool IsAnyRaycastColliding()
        {
            foreach (RayCast2D raycast in raycasts)
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
            Position += new Vector2(0, Speed * (float)delta);
        }

        #endregion
    }
}
