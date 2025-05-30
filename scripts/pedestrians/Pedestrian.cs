using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Crosswalk
{
    /// <summary>
    /// Base class for all pedestrian characters.
    /// Handles movement, touch input, collision, and scoring behavior.
    /// </summary>
    public abstract partial class Pedestrian : Area2D
    {
        #region Public Fields

        // Base walking speed of the pedestrian
        [Export] public virtual float Speed { get; set; }
        // Rotation speed when flying after a collision
        [Export] public virtual float RotationSpeed { get; set; }
        // Current flight time after getting hit
        [Export] public virtual float FlyTime { get; set; }
        // Duration the pedestrian stays stopped after tapping
        [Export] public virtual float StopDuration { get; set; } = 2.0f;
        // Cooldown before the pedestrian can be stopped again
        [Export] public virtual float StopCooldown { get; set; } = 3.0f;
        // Duration the pedestrian runs after a double tap
        [Export] public virtual float SpeedTimer { get; set; }
        // Speed multiplier when sprinting
        [Export] public float SpeedMultiplierFast { get; set; } = 3.0f;
        // Default speed multiplier when walking
        [Export] public float SpeedMultiplierNormal { get; set; } = 1.0f;
        // Horizontal flight direction after a collision
        [Export] public virtual float FlightDirection { get; set; }
        // Maximum allowed flying time before auto-clean

        // Predefined starting spawn positions for pedestrians
        public List<Vector2> StartPositions { get; set; } = new()
        {
            new Vector2(-30, 497),
            new Vector2(-30, 528),
            new Vector2(-30, 560),
            new Vector2(-30, 592),
            new Vector2(400, 512),
            new Vector2(400, 544),
            new Vector2(400, 576)
        };

        #endregion

        #region Private Fields

        [Export] private float _maxFlyTime = 5f;
        // Audio player for pedestrian sound effects
        [Export] private AudioStreamPlayer2D _sfxPlayer;
        // Path to hit sound effect
        [Export] private string _hitSound = "";
        // Path to scream sound effect
        [Export] private string _screamSound = "";
        // Path to tap sound effect
        [Export] private string _tapSound = "";
        // Path to score sound effect
        [Export] private string _scoreSound = "";

        // Speed of scaling effect while flying
        [Export] private float _scaleSpeed = 0.5f;
        // Maximum scale during flying animation
        [Export] private float _maxScale = 2f;

        // Flag indicating if pedestrians have red light
        private bool _redLightsForPedestrians = false;
        // Score multiplier based on current difficulty
        private int _scoreMultiplier = GameManager.Instance._difficulty;
        // Max time (seconds) between taps to trigger a double tap
        private const float _doubleTapThreshold = 0.3f;
        // Time of last tap input
        private float _lastTapTime = 0f;
        // Waiting state for a second tap
        private bool _isWaitingForDoubleTap = false;
        // True if currently scaling up during flying animation
        private bool _isScalingUp = true;
        // Reference to pedestrian's AnimatedSprite2D
        private AnimatedSprite2D _animatedSprite;
        // True if pedestrian is flying after collision
        protected bool _isFlying = false;
        // True if pedestrian is currently stopped
        protected bool _isStopped = false;
        // True if pedestrian can be stopped again (cooldown ready)
        protected bool _canBeStopped = true;
        // True if pedestrian is sprinting
        protected bool _isSpeeding = false;
        // True if playing random idle animation
        protected bool _randomStop = false;
        // True if pedestrian has been hit by a car
        protected bool _isHit = false;
        // Saved initial walking speed
        protected float _initialSpeed;

        #endregion

        #region Godot Built-in Methods

        /// <summary>
        /// Called when the node is added to the scene tree.
        /// Initializes signals and references.
        /// </summary>
        public override void _Ready()
        {
            _initialSpeed = Speed;
            AddToGroup("pedestrians");

            Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));

            var interactionArea = GetNode<Area2D>("InteractionArea");
            interactionArea?.Connect("input_event", Callable.From((Node viewport, InputEvent @event, int shapeIdx) => OnInputEvent(@event)));

            _animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        /// <summary>
        /// Called every frame. Handles movement and boundary cleanup.
        /// </summary>
        public override void _Process(double delta)
        {
            if (_isStopped)
            {
                PlayAnimation(_randomStop ? "idle2" : "idle");
                return;
            }
            else if (!_isFlying)
            {
                Move(delta, _isSpeeding);
            }

            CleanupOutOfBoundsPedestrians();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes pedestrian starting position and speed direction.
        /// </summary>
        public void Initialize(Vector2 position)
        {
            Position = position;
            ZIndex = Mathf.RoundToInt(GlobalPosition.Y + 100);
            Speed = (Position.X > 399) ? -MathF.Abs(Speed) : MathF.Abs(Speed);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Handles normal movement logic including walking and running.
        /// </summary>
        /// <param name="delta">Time elapsed since the last frame (used for frame-independent movement).</param>
        /// <param name="IsSpeeding">If true, applies faster movement logic (running instead of walking).</param>
        protected virtual void Move(double delta, bool IsSpeeding)
        {
            _animatedSprite.FlipH = Speed < 0;
            float speedMultiplier = IsSpeeding ? SpeedMultiplierFast : SpeedMultiplierNormal;
            Position += new Vector2(Speed * speedMultiplier * (float)delta, 0);

            PlayAnimation(IsSpeeding ? "run" : "walk");
        }


        /// <summary>
        /// Handles flying behavior after collision.
        /// If a pedestrian flies too long, it's deleted.
        /// <param name="delta">Time elapsed since the last frame (used for frame-independent movement).</param>
        /// </summary>
        protected void Fly(double delta)
        {
            _scaleSpeed = 0.5f * (float)delta;
            Vector2 currentScale = _animatedSprite.Scale;

            if (_isScalingUp)
            {
                currentScale += new Vector2(_scaleSpeed, _scaleSpeed);
                if (currentScale.X >= _maxScale)
                {
                    currentScale = new Vector2(_maxScale, _maxScale);
                    _isScalingUp = false;
                }
            }
            else
            {
                currentScale -= new Vector2(_scaleSpeed, _scaleSpeed);
                if (currentScale.X <= 1f)
                {
                    currentScale = new Vector2(1f, 1f);
                }
            }

            _animatedSprite.Scale = currentScale;

            FlyTime += (float)delta;
            Position += new Vector2(FlightDirection, -250) * (float)delta;
            RotationDegrees += RotationSpeed * (float)delta;

            PlayAnimation("fly");

            if (FlyTime > _maxFlyTime && !IsQueuedForDeletion())
            {
                QueueFree();
            }
        }

        /// <summary>
        /// Hook for handling car collision. Override in derived classes.
        /// </summary>
        protected virtual void HandleCarCollision(Car car)
        {

        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles entering collision with areas (cars).
        /// </summary>
        private void OnAreaEntered(Area2D area)
        {
            if (area is Car car && !_isHit)
            {
                PlayCollisionSounds();
                GameManager.Instance.UpdateLife(-1);
                _isHit = true;

                HandleCarCollision(car);
                RotationSpeed = car.Speed * 3f;

                if (!_isFlying)
                {
                    GD.PrintErr($"[ERROR] {Name} should be flying but isFlying = false!");
                }
            }
        }

        /// <summary>
        /// Handles player input events like tapping pedestrians.
        /// </summary>
        private void OnInputEvent(InputEvent @event)
        {
            if (@event is InputEventScreenTouch touch && touch.Pressed)
            {
                if (!_isFlying)
                {
                    GetNode<GUI>("/root/Level/GUI").PlaySfx(_tapSound);
                }
                HandleTouchInput();
            }
        }

        /// <summary>
        /// Handles touch input for single or double taps asynchronously.
        /// </summary>
        private async void HandleTouchInput()
        {
            float currentTime = Time.GetTicksMsec() / 1000f;

            if ((currentTime - _lastTapTime) < _doubleTapThreshold)
            {
                _isStopped = false;
                StartSpeedBoost();
                _isWaitingForDoubleTap = false;
            }
            else
            {
                _lastTapTime = currentTime;
                _isWaitingForDoubleTap = true;

                if (_canBeStopped && !_isSpeeding)
                {
                    HandleTapOrStop();
                }

                await ToSignal(GetTree().CreateTimer(_doubleTapThreshold), "timeout");
                _isWaitingForDoubleTap = false;
            }
        }

        /// <summary>
        /// Stops pedestrian on tap, then resumes after a cooldown.
        /// </summary>
        private async void HandleTapOrStop()
        {
            if (!_canBeStopped) return;

            _isStopped = true;
            _canBeStopped = false;
            PlayAnimation("idle");

            await ToSignal(GetTree().CreateTimer(StopDuration), "timeout");

            _isStopped = false;
            PlayAnimation("walk");

            await ToSignal(GetTree().CreateTimer(StopCooldown), "timeout");
            _canBeStopped = true;
        }

        /// <summary>
        /// Temporarily boosts pedestrian speed.
        /// </summary>
        private async void StartSpeedBoost()
        {
            _isSpeeding = true;
            _canBeStopped = false;

            await ToSignal(GetTree().CreateTimer(SpeedTimer), "timeout");

            _isSpeeding = false;
            _canBeStopped = true;
        }

        /// <summary>
        /// Plays animation by name if not already playing.
        /// </summary>
        protected void PlayAnimation(string animationName)
        {
            if (_animatedSprite != null && _animatedSprite.Animation != animationName)
            {
                _animatedSprite.Play(animationName);
            }
        }

        /// <summary>
        /// Plays a sound effect given a resource path.
        /// </summary>
        private void PlaySfx(string pathToSfx)
        {
            if (_sfxPlayer != null && !string.IsNullOrEmpty(pathToSfx))
            {
                var stream = GD.Load<AudioStream>(pathToSfx);
                if (stream != null)
                {
                    _sfxPlayer.Stream = stream;
                    _sfxPlayer.Play();
                }
            }
        }

        /// <summary>
        /// Plays collision sounds sequentially.
        /// </summary>
        private async void PlayCollisionSounds()
        {
            if (_sfxPlayer != null && _screamSound != null)
            {
                _sfxPlayer.Play();
                await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
                _sfxPlayer.Stream = GD.Load<AudioStream>(_screamSound);
                _sfxPlayer.Play();
            }
        }

        /// <summary>
        /// Deletes pedestrians that move outside the screen boundaries.
        /// </summary>
        private void CleanupOutOfBoundsPedestrians()
        {
            foreach (Area2D pedestrian in GetTree().GetNodesInGroup("pedestrians").Cast<Area2D>())
            {
                Vector2 pos = pedestrian.Position;
                if ((pos.X > 410 || pos.X < -40 || pos.Y > 700 || pos.Y < -100) && !pedestrian.IsQueuedForDeletion())
                {
                    if (pedestrian is Pedestrian p && !p._isFlying)
                    {
                        if (!GameManager.Instance._gameOver)
                        {
                            if (p is Grandma || p is Grandpa)
                                GameManager.Instance.AddScore(50 * (_scoreMultiplier + 1));
                            else if (p is Girl || p is Boy)
                                GameManager.Instance.AddScore(30 * (_scoreMultiplier + 1));
                            else if (p is Woman || p is Man)
                                GameManager.Instance.AddScore(20 * (_scoreMultiplier + 1));

                            GetNode<GUI>("/root/Level/GUI").PlaySfx(_scoreSound);
                        }
                        pedestrian.QueueFree();
                    }
                }
            }
        }

        #endregion
    }
}
