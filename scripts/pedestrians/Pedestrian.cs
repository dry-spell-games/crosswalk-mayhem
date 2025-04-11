using Godot;
using System;
using System.Collections.Generic; // Required for arraylist
using System.Linq;

namespace Crosswalk
{
    public abstract partial class Pedestrian : Area2D
    {
        [Export] public virtual float Speed { get; set; }
        [Export] public virtual float RotationSpeed { get; set; }
        // Flight time, default value
        [Export] public virtual float FlyTime { get; set; }
        // How long pedestrian stays stopped, default value
        [Export] public virtual float StopDuration { get; set; } = 2.0f;
        // Cooldown for stop action, default value
        [Export] public virtual float StopCooldown { get; set; } = 3.0f;
        [Export] public virtual float SpeedTimer { get; set; } // Timer for faster speed
        [Export] public float SpeedMultiplierFast { get; set; } = 3.0f; // Multiplier for sprinting
        [Export] public float SpeedMultiplierNormal { get; set; } = 1.0f;
        [Export] public virtual float FlightDirection { get; set; } // Direction of flight
        [Export] private float _maxFlyTime = 5f;
        [Export] private AudioStreamPlayer2D _sfxPlayer;
        [Export] private string _hitSound = "";
        [Export] private string _screamSound = "";
        [Export] private string _tapSound = "";
        [Export] private string _scoreSound = "";
        private bool RedLightsForPedestrians = false;
        private int _scoreMultiplier = GameManager.Instance._difficulty;


        // Attributes for mobile control
        private const float DoubleTapThreshold = 0.3f; // Max time for double tap
        private float lastTapTime = 0f;
        private bool isWaitingForDoubleTap = false;

        // ArrayList can not be exported
        public List<Vector2> StartPositions { get; set; } = new List<Vector2>
        {
            new Vector2(-30, 497),
            new Vector2(-30, 528),
            new Vector2(-30, 560),
            new Vector2(-30, 592),
            new Vector2(400, 512),
            new Vector2(400, 544),
            new Vector2(400, 576)
        };

        protected bool isFlying = false;
        protected bool isStopped = false; // if true pedestrian is moving
        protected bool canBeStopped = true; // False if pedestrian was stopped recently
        private AnimatedSprite2D animatedSprite; // Refers to AnimatedSprite2D component
        protected float InitialSpeed; // Saves original speed
        protected bool IsSpeeding = false;
        protected bool randomStop = false; // Plays different idle animation when true
        protected bool _isHit = false;

        public override void _Ready()
        {
            InitialSpeed = Speed; // Saves original speed
            AddToGroup("pedestrians"); // Adds pedestrians to group so they can be removed later

            // Signal for area_entered
            Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));

            // Gets InteractionArea node and connects signal to it
            var interactionArea = GetNode<Area2D>("InteractionArea");
            if (interactionArea != null)
            {
                interactionArea.Connect("input_event", Callable.From((Node viewport,
                InputEvent @event, int shapeIdx) => OnInputEvent(@event)));
            }
            // Gets AnimatedSprite2d for pedestrians
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        public void Initialize(Vector2 position)
        {
            Position = position;

            // Sets ZIndex for rendering Pedestrians, higher value of Y is rendered on top
            ZIndex = Mathf.RoundToInt(GlobalPosition.Y + 100);

            // If SpawnPoints X coordinate is over 399, Speed is converted to negative
            if (Position.X > 399)
            {
                Speed = -MathF.Abs(Speed); // Converts to negative speed
            }
            else
            {
                Speed = MathF.Abs(Speed); // Else speed is positive
            }
            GD.Print($"{this} spawned at {Position} with speed {Speed}");
        }

        public override void _Process(double delta)
        {
            if (isStopped)
            {
                PlayAnimation(randomStop ? "idle2" : "idle");
                return;
            }
            else if (!isFlying)
            {
                Move(delta, IsSpeeding);
            }

            // Clean up pedestrians out of bounds
            foreach (Area2D pedestrian in GetTree().GetNodesInGroup("pedestrians").Cast<Area2D>())
            {
                Vector2 pos = pedestrian.Position;

                bool outOfBoundsX = pos.X > 410 || pos.X < -40;
                bool outOfBoundsY = pos.Y > 700 || pos.Y < -100;

                if ((outOfBoundsX || outOfBoundsY) && !pedestrian.IsQueuedForDeletion())
                {
                    // Cast to Pedestrian to access isFlying
                    if (pedestrian is Pedestrian p && !p.isFlying)
                    {
                        GD.Print("Poistetaan jalankulkija: ", pedestrian.Name);

                        if (!GameManager.Instance._gameOver)
                        {
                            switch (p.Name)
                            {
                                case "Grandma":
                                case "Grandpa":
                                    GameManager.Instance.AddScore(50 * (_scoreMultiplier + 1));
                                    break;
                                case "Girl":
                                case "Boy":
                                    GameManager.Instance.AddScore(30 * (_scoreMultiplier + 1));
                                    break;
                                case "Woman":
                                case "Man":
                                    GameManager.Instance.AddScore(20 * (_scoreMultiplier + 1));
                                    break;
                            }

                            GetNode<GUI>("/root/Level/GUI").PlaySfx(_scoreSound);
                        }
                        pedestrian.QueueFree();
                    }
                }
            }
        }

        protected virtual void Move(double delta, bool IsSpeeding)
        {
            // Flip the animation based on direction
            animatedSprite.FlipH = Speed < 0; // Flips the character if moving left (negative speed)

            // Adjust movement speed based on whether speeding is enabled
            float SpeedMultiplier = IsSpeeding ? SpeedMultiplierFast : SpeedMultiplierNormal;
            Position += new Vector2(Speed * SpeedMultiplier * (float)delta, 0); // Apply movement

            // Play the appropriate animation based on movement speed
            if (!IsSpeeding)
            {
                PlayAnimation("walk");
            }
            else
            {
                PlayAnimation("run");
            }
        }

        public void OnAreaEntered(Area2D area)
        {
            GD.Print($"[OnAreaEntered] {Name} detected area: {area.Name} (type: {area.GetType().Name})");

            if (area is Car car)
            {
                if (!_isHit)
                {
                    PlayCollisionSounds();
                    GameManager.Instance.UpdateLife(-1);
                    _isHit = true;

                    GD.Print($"[HIT] {Name} collided with car: {car.Name}");

                    HandleCarCollision(car);
                    RotationSpeed = car.Speed * 3f;
                    GD.Print($"Collision car speed: {car.Speed}");

                    GD.Print($"[DEBUG] After collision: {Name} isFlying = {isFlying}");

                    if (!isFlying)
                    {
                        GD.PrintErr($"[ERROR] {Name} should be flying but isFlying = false!");
                    }
                }
            }
        }

        protected void Fly(double delta)
        {
            FlyTime += (float)delta;
            animatedSprite.Offset = new Vector2(0, 15);
            Position += new Vector2(FlightDirection, -250) * (float)delta;
            RotationDegrees += RotationSpeed * (float)delta;
            PlayAnimation("fly");

            if (FlyTime > 5f && !IsQueuedForDeletion())
            {
                GD.Print($"[AUTO-FREE] {Name} flew too long, auto-cleaning...");

                QueueFree();
            }
        }

        protected virtual void HandleCarCollision(Car car)
        {
            GD.Print("Handling collision with a Car...");
        }

        /// <summary>
        /// Handles pedestrian click events, including a cooldown mechanism.
        /// This function is triggered when the user clicks on the pedestrian with the mouse.
        /// It stops the character for a specified duration and prevents reactivation during cooldown.
        /// Uses an async method to handle timing asynchronously in the background.
        /// </summary>
        /// <param name="viewport">The viewport where the event occurred (not used in this method,
        /// but required by Godot for the method signature).</param>
        /// <param name="event">The event containing information about the user's input.
        /// @event is used because "event" is a reserved keyword in C#.</param>
        /// <param name="shapeIdx">The index of the clicked CollisionShape2D element
        /// (useful if the pedestrian has multiple hitboxes).</param>
        private void OnInputEvent(InputEvent @event)
        {
            if (@event is InputEventScreenTouch touch && touch.Pressed)
            {
                if (!isFlying)
                {
                    PlaySfx(_tapSound);
                }
                HandleTouchInput();
            }
        }

        // Handles mobile input asynchronously
        private async void HandleTouchInput()
        {
            float currentTime = Time.GetTicksMsec() / 1000f;

            if ((currentTime - lastTapTime) < DoubleTapThreshold)
            {
                // Double tap → run immediately, even if already stopped
                isStopped = false;
                StartSpeedBoost();
                isWaitingForDoubleTap = false;
            }
            else
            {
                lastTapTime = currentTime;
                isWaitingForDoubleTap = true;

                // Single tap → stop immediately (if allowed)
                if (canBeStopped && !IsSpeeding)
                {
                    HandleTapOrStop();
                }

                // Give player time to double tap (to override stop with run)
                await ToSignal(GetTree().CreateTimer(DoubleTapThreshold), "timeout");
                isWaitingForDoubleTap = false;
            }
        }

        private async void HandleTapOrStop()
        {
            if (!canBeStopped)
            {
                GD.Print("Pedestrian cannot be stopped yet! Cooldown active.");
                return;
            }

            isStopped = true;
            canBeStopped = false;
            PlayAnimation("idle");
            GD.Print("Pedestrian stopped.");

            await ToSignal(GetTree().CreateTimer(StopDuration), "timeout");

            isStopped = false;
            PlayAnimation("walk");
            GD.Print("Pedestrian resumed!");

            await ToSignal(GetTree().CreateTimer(StopCooldown), "timeout");
            canBeStopped = true;
            GD.Print("Cooldown finished, pedestrian can be stopped again.");
        }

        // Sets IsSpeeding to true while timer is running.
        // IsSpeeding is checked at Move method
        private async void StartSpeedBoost()
        {
            GD.Print("Pedestrian speeding for " + SpeedTimer + " seconds.");
            IsSpeeding = true;
            canBeStopped = false; // Prevent stopping while speeding

            await ToSignal(GetTree().CreateTimer(SpeedTimer), "timeout");

            IsSpeeding = false;
            canBeStopped = true; // <<< Add this line
            GD.Print("Pedestrian back to normal speed, can be stopped again.");
        }

        // Method gets animation name string as parameter and plays given animation
        protected void PlayAnimation(string animationName)
        {
            if (animatedSprite != null && animatedSprite.Animation != animationName)
            {
                GD.Print($"Starting animation: {animationName}");
                animatedSprite.Play(animationName);
            }
        }

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

        private async void PlayCollisionSounds()
        {
            if (_sfxPlayer != null && _hitSound != null && _screamSound != null)
            {
                _sfxPlayer.Stream = GD.Load<AudioStream>(_hitSound);
                _sfxPlayer.Play();
                await ToSignal(GetTree().CreateTimer(0.25f), "timeout");
                _sfxPlayer.Stream = GD.Load<AudioStream>(_screamSound);
                _sfxPlayer.Play();
            }
        }
    }
}
