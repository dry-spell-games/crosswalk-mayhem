using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// Girl is subclass of abstract class Pedestrian. It uses mostly base methods,
    /// but it also randomly generates flight direction.
    /// </summary>
    public partial class Girl : Pedestrian
    {
        #region Public Properties

        [Export] public override float Speed { get; set; } = 50.0f;
        [Export] public override float RotationSpeed { get; set; } = 30000;
        [Export] public override float StopDuration { get; set; } = 3.0f;
        [Export] public override float StopCooldown { get; set; } = 10.0f;
        [Export] public override float SpeedTimer { get; set; } = 2.0f;
        [Export] public override float FlightDirection { get; set; }

        #endregion

        #region Private Properties

        private AnimatedSprite2D animatedSprite;

        #endregion

        #region Godot Built-In Methods

        /// <summary>
        /// Called when the node enters the scene tree.
        /// Initializes the girl's flight direction and sprite reference.
        /// </summary>
        public override void _Ready()
        {
            base._Ready();
            FlightDirection = GD.RandRange(-300, 300);
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        /// <summary>
        /// Called every frame. Updates girl’s movement or flying behavior.
        /// </summary>
        public override void _Process(double delta)
        {
            if (isFlying)
            {
                base.Fly(delta);
            }
            else
            {
                base._Process(delta);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Executes behavior when the girl is hit by a car.
        /// Enables flying mode and resets flying timer.
        /// </summary>
        /// <param name="car">The car object that collided with the girl.</param>
        protected override void HandleCarCollision(Car car)
        {
            isFlying = true;
            GD.Print("Girl was hit by a car :[");
            FlyTime = 0;
        }

        #endregion
    }
}
