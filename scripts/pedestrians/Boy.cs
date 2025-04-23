using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// Boy is subclass of abstract class Pedestrian. It uses mostly base methods,
    /// but it also randomly generates flight direction.
    /// </summary>
    public partial class Boy : Pedestrian
    {
        #region Public Properties

        [Export] public override float Speed { get; set; } = 40.0f;
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
        /// Called when the node is added to the scene for the first time.
        /// Initializes flight direction and sprite node.
        /// </summary>
        public override void _Ready()
        {
            base._Ready();
            FlightDirection = GD.RandRange(-300, 300);
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        /// <summary>
        /// Called every frame. Manages flying behavior or default pedestrian behavior.
        /// </summary>
        public override void _Process(double delta)
        {
            if (isFlying)
            {
                base.Fly(delta);
                animatedSprite.Offset = new Vector2(1, 14);
            }
            else
            {
                base._Process(delta);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles logic when the pedestrian is hit by a car.
        /// Starts flying animation and resets fly timer.
        /// </summary>
        /// <param name="car">The car that collided with the pedestrian.</param>
        protected override void HandleCarCollision(Car car)
        {
            isFlying = true;
            GD.Print("Boy was hit by a car :[");
            FlyTime = 0;
        }

        #endregion
    }
}
