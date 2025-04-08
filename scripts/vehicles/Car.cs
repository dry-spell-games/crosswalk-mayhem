using Godot;
using System;
using System.Collections.Generic;

namespace Crosswalk
{
    public abstract partial class Car : Area2D {
        public virtual float Speed { get; set; }
        public virtual Vector2 Velocity { get; set; }
        public virtual float BrakingForce { get; set; }
        public virtual float AccelerationForce { get; set; }
        private bool isStopped = false;
        private bool isBraking = false;
        private bool isAccelerating = false;
        protected float InitialSpeed;
        private AnimatedSprite2D animatedSprite;
        private AnimatedSprite2D windShield;
        private RayCast2D[] raycasts;
        public List<Vector2> StartPositions { get; set; } = new List<Vector2>

        {
            new Vector2(140, -400),
            new Vector2(220, -400)
        };

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

        public void Initialize(Vector2 position)
        {
            Position = position;
            GD.Print($"{this} spawned at {Position} with speed {Speed}");
            GD.Print("auto instantioitu");
        }

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

        protected virtual void Move(double delta)
        {
            Position += new Vector2(0, Speed * (float)delta);
        }

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
    }
}
