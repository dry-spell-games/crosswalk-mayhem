using Godot;
using System;
using System.Collections.Generic;
// using System.Numerics;

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
        private AnimatedSprite2D animatedSprite; // Viittaa AnimatedSprite2D-komponenttiin
        private RayCast2D[] raycasts;
        public List<Vector2> StartPositions { get; set; } = new List<Vector2>

        {
            new Vector2(140, -400),
            new Vector2(220, -400)
        };

        public override void _Ready()
        {
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            AddToGroup("cars"); // Lisätään autot ryhmään, jotta ne voidaan poistaa
            InitialSpeed = Speed;

            // Luodaan taulukko raycasteille
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
                animatedSprite.Pause();
            }
            else
            {
                Move(delta);
                animatedSprite.Play();
            }

            // Käy läpi kaikki Car-luokan lapset ja poista ne, jos ne menevät alueen ulkopuolelle
            foreach (Area2D car in GetTree().GetNodesInGroup("cars"))
            {
                if (car.Position.Y > 900) // Tarkista sijainti
                {
                    GD.Print("Poistetaan auto: ", car.Name);
                    car.QueueFree(); // Poista auto
                }
            }
        }

        protected virtual void Move(double delta)
        {
            Position += new Vector2(0, Speed * (float)delta);
            PlayAnimation("drive");
        }

        private bool IsAnyRaycastColliding()
        {
            foreach (RayCast2D raycast in raycasts)
            {
                if (raycast != null)
                {
                    raycast.ForceRaycastUpdate(); // Päivittää tiedot välittömästi
                    if (raycast.IsColliding())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Yleinen metodi animaation vaihtamiseen
        protected void PlayAnimation(string animationName)
        {
            if (animatedSprite != null && animatedSprite.Animation != animationName)
            {
                GD.Print($"Starting animation: {animationName}");
                animatedSprite.Play(animationName);
            }
            animatedSprite.SpeedScale = Mathf.Clamp(Speed / InitialSpeed, 1.0f, 50.0f);
        }
    }
}
