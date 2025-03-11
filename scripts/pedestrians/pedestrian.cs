using Godot;
using System;
using System.Collections.Generic; // Tarvitaan Arraylistaan

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
        [Export] public virtual float SpeedTimer { get; set; } // Kiihdytyksen aika
        [Export] public float SpeedMultiplierFast { get; set; } = 3.0f; // Nopeampi arvo
        [Export] public float SpeedMultiplierNormal { get; set; } = 1.0f; // Normaali arvo
        [Export] public virtual float FlightDirection { get; set; } // Lentosuunta

        // ArrayListaa ei pysty exporttaamaan
        public List<Vector2> StartPositions { get; set; } = new List<Vector2>
        {
            new Vector2(-30, 480),
            new Vector2(-30, 520),
            new Vector2(-30, 560),
            new Vector2(400, 500),
            new Vector2(400, 540),
            new Vector2(400, 580)
        };

        protected bool isFlying = false;
        protected bool isStopped = false; // Pysäyttämisen tila
        protected bool canBeStopped = true; // False if pedestrian was stopped recently
        private AnimatedSprite2D animatedSprite; // Viittaa AnimatedSprite2D-komponenttiin
        protected float InitialSpeed; // Tallentaa alkuperäisen nopeuden
        protected bool IsSpeeding = false;

        public override void _Ready()
        {
            InitialSpeed = Speed; // Tallentaa alkuperäisen nopeuden
            AddToGroup("pedestrians"); // Lisätään jalankulkijat ryhmään, jotta ne voidaan poistaa

            // Signaalit area_entered ja input_event
            Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));

            // Haetaan InteractionArea solmu ja yhdistetään signaali
            var interactionArea = GetNode<Area2D>("InteractionArea");
            if (interactionArea != null)
            {
                interactionArea.Connect("input_event", Callable.From((Node viewport,
                InputEvent @event, int shapeIdx) => OnInputEvent(@event)));
            }
            // Haetaan AnimatedSpride2D
            animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        }

        public void Initialize(Vector2 position)
        {
            Position = position;

            // Asetetaan ZIndex, määrittää renderöinnin Y:n mukaan
            // Suurempi -> näkyy ylempänä
            ZIndex = Mathf.RoundToInt(GlobalPosition.Y);

            // Jos spawn-pisteen X-koordinaatti on yli 399, käännä nopeus negatiiviseksi
            if (Position.X > 399)
            {
                Speed = -MathF.Abs(Speed); // Varmistetaan, että Speed on negatiivinen
            }
            else
            {
                Speed = MathF.Abs(Speed); // Varmistetaan, että Speed on positiivinen
            }

            GD.Print($"{this} spawned at {Position} with speed {Speed}");
        }

        public override void _Process(double delta)
        {

            if (isStopped)
            {
                PlayAnimation("idle");
            }
            else if (!isFlying) // Jos ei ole lentämässä
            {
                Move(delta, IsSpeeding);
            }

            // Käy läpi kaikki Pedestrian-luokan lapset ja poista ne, jos ne menevät alueen ulkopuolelle
            foreach (Area2D pedestrian in GetTree().GetNodesInGroup("pedestrians"))
            {
                if (pedestrian.Position.X > 410 || pedestrian.Position.X < -50) // Tarkista sijainti
                {
                    GD.Print("Poistetaan jalankulkija: ", pedestrian.Name);
                    pedestrian.QueueFree(); // Poista jalankulkija
                }
            }
        }

        protected virtual void Move(double delta, bool IsSpeeding)
        {
            // Käännetään animaatio suunnan mukaan
            animatedSprite.FlipH = Speed < 0; // Kääntää hahmon, jos nopeus on negatiivinen

            // Nopeampi liike, jos IsSpeeding on päällä
            float SpeedMultiplier = IsSpeeding ? SpeedMultiplierFast : SpeedMultiplierNormal;
            Position += new Vector2(Speed * SpeedMultiplier * (float)delta, 0);

            if (!IsSpeeding)
            {
                PlayAnimation("walk");
            }
            else {
                PlayAnimation("run");
            }
        }

        private void OnAreaEntered(Area2D area)
        {
            if (area is Car)
            {
                GD.Print("Pedestrian detected a Car!");
                HandleCarCollision(area as Car);
            }
        }

        protected void Fly(double delta)
        {
            FlyTime += (float)delta;
            Position += new Vector2(FlightDirection, -250) * (float)delta;
            RotationDegrees += RotationSpeed * (float)delta;
        }

        protected virtual void HandleCarCollision(Car car)
        {
            GD.Print("Handling collision with a Car...");
        }

        /// <summary>
        /// Käsittelee jalankulkijan klikkaamisen, jossa on mukana cooldown.
        /// Tämä funktio suoritetaan, kun käyttäjä klikkaa jalankulkijaa hiirellä.
        /// Se pysäyttää hahmon määritetyksi ajaksi ja estää uudelleenkäytön cooldown-ajalla.
        /// Käyttää async-metodia ajastuksen suorittamiseen asynkronisesti taustalla.
        /// </summary>
        /// <param name="viewport">Viewport, jossa tapahtuma tapahtui (ei käytetä tässä,
        /// mutta Godot vaatii sen metodin signatuurissa).</param>
        /// <param name="event">Tapahtuma, joka sisältää tiedot käyttäjän syötteestä.
        /// @event käytetään, koska "event" on C#:n varattu avainsana.</param>
        /// <param name="shapeIdx">Klikatun CollisionShape2D-elementin indeksi
        /// (jos jalankulkijalla on useita hitboxeja).</param>
        private async void OnInputEvent(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
            {
                if (mouseEvent.ButtonIndex == MouseButton.Left)
                {
                    if (!canBeStopped)
                    {
                        GD.Print("Pedestrian cannot be stopped yet! Cooldown active.");
                        return;
                    }

                    isStopped = true;
                    canBeStopped = false;
                    PlayAnimation("idle");
                    GD.Print("Pedestrian stopped for " + StopDuration + " seconds.");

                    await ToSignal(GetTree().CreateTimer(StopDuration), "timeout");

                    isStopped = false;
                    PlayAnimation("walk");
                    GD.Print("Pedestrian resumed!");

                    GD.Print($"Cooldown started for {StopCooldown} seconds.");
                    await ToSignal(GetTree().CreateTimer(StopCooldown), "timeout");

                    canBeStopped = true;
                    GD.Print("Cooldown finished, pedestrian can be stopped again.");
                }

                else if (mouseEvent.ButtonIndex == MouseButton.Right)
                {
                    GD.Print("Pedestrian speeding for " + SpeedTimer + " seconds.");
                    IsSpeeding = true;
                    PlayAnimation("run");

                    await ToSignal(GetTree().CreateTimer(SpeedTimer), "timeout");

                    IsSpeeding = false;
                    GD.Print("Pedestrian back to normal speed.");
                    PlayAnimation("walk");
                }
            }
        }

        // Yleinen metodi animaation vaihtamiseen
        protected void PlayAnimation(string animationName)
        {
            if (animatedSprite != null && animatedSprite.Animation != animationName)
            {
                GD.Print($"Starting animation: {animationName}");
                animatedSprite.Play(animationName);
            }
        }
    }
}
