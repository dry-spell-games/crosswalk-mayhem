using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// Woman is a subclass of abstract class Pedestrian. It uses mostly base methods,
    /// but it also randomly generates flight direction.
    /// </summary>
    public partial class Woman : Pedestrian
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

        private AnimatedSprite2D _animatedSprite;

        #endregion

        #region Godot Built-In Methods

        /// <summary>
        /// Called when the node enters the scene tree for the first time.
        /// Initializes flight direction and retrieves the animated sprite node.
        /// </summary>
        public override void _Ready()
        {
            base._Ready();
            FlightDirection = GD.RandRange(-300, 300);
            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        /// <summary>
        /// Called every frame. Handles flying or walking logic based on character state.
        /// </summary>
        /// <param name="delta">Time elapsed since the last frame.</param>
        public override void _Process(double delta)
        {
            if (_isFlying)
            {
                base.Fly(delta);
                _animatedSprite.Offset = new Vector2(-1, 18);
            }
            else
            {
                base._Process(delta);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles logic when the woman is hit by a car.
        /// Enables flying behavior and resets flying timer.
        /// </summary>
        /// <param name="car">The car that collided with the pedestrian.</param>
        protected override void HandleCarCollision(Car car)
        {
            _isFlying = true;
            FlyTime = 0;
        }

        #endregion
    }
}
