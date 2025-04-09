using Godot;
using System;
using System.Threading.Tasks;

namespace Crosswalk
{
    /// <summary>
    /// GUI handles the in-game user interface elements like the pause menu, mute button,
    /// status labels (score, lives, etc.), and animated message sign.
    /// It also manages related sound effects and reacts to user inputs like pause/mute.
    /// </summary>
    public partial class GUI : Control
    {
        // Reference to the main level scene
        private Node2D _level;
        // Audio player for sound effects like button clicks or message slide sounds
        [Export] private AudioStreamPlayer2D _sfxPlayer;
        // The pause menu UI that becomes visible when the game is paused
        [Export] private TextureRect _pauseMenu;
        // Toggleable mute button that controls the master audio bus volume
        [Export] private TextureButton _muteButton;
        // Delay (in seconds) before actions like scene changes to allow SFX to play
        [Export] private float _sfxDelayTimer = 0.1f;
        // The animated sign that slides up to display temporary messages to the player
        [Export] private TextureRect _messageSign;
        // The label inside the message sign that displays the actual text
        [Export] private Label _messageLabel;
        // The signs that shows the amount of incoming pedestrians per difficulty level
        [Export] private TextureRect _incomingSign;
        // The label inside the incoming sign that displays the incoming pedestrian count
        [Export] private Label _incomingCountLabel;
        // The label inside the incoming sign that displays the text
        [Export] private Label _incomingLabel;
        // Signs that allow player to reset or exit the game
        [Export] private TextureRect _actionSign;
        // Reference to the pause button
        [Export] private Button _pauseButton;

        // Internal flag to determine if the message sign should currently be moving up
        private bool _messageUp = false;
        // Y position where the message sign should stop when sliding up (visible position)
        private int _messageTargetYPos = 352;
        // Y position where the message sign starts and returns to when sliding down (off-screen)
        private int _messageStartYPos = 640;
        // Speed (pixels per second) at which the message sign moves vertically
        private float _messageMoveSpeed = 750f;
        // Flag to wait for messages
        public bool _waitForMessage { get; private set; } = false;

        /// <summary>
        /// Called when the pause button is pressed. Pauses the game and shows the pause menu.
        /// </summary>
        public void _on_pause_button_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            _pauseMenu.Visible = true;
            GetTree().Paused = true;
        }

        /// <summary>
        /// Called when the play (unpause) button is pressed. Resumes the game and hides the pause menu.
        /// </summary>
        public void _on_play_button_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            _pauseMenu.Visible = false;
            GetTree().Paused = false;
        }

        /// <summary>
        /// Toggles the master volume when the mute button is pressed or released.
        /// </summary>
        private void _on_mute_button_toggled(bool toggled_on)
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");

            if (toggled_on)
            {
                GameManager.Instance._masterVolume = -80f; // Mute
            }
            else
            {
                // If previous saved volume was muted, set default -6 dB. Otherwise, restore it.
                if (GameManager.Instance._savedMasterVolume == -80f)
                    GameManager.Instance._masterVolume = -6.0f;
                else
                    GameManager.Instance._masterVolume = GameManager.Instance._savedMasterVolume;
            }

            GameManager.Instance.SetVolume("Master", GameManager.Instance._masterVolume);
            GameManager.Instance.SaveData(); // Persist volume setting
        }

        /// <summary>
        /// Called when the reset button is pressed. Placeholder for future reset functionality.
        /// </summary>
        public async void _on_reset_button_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            await ToSignal(GetTree().CreateTimer(_sfxDelayTimer), "timeout");

            GameManager.Instance.SaveData();
            GameManager.Instance._difficulty = 0;
            GameManager.Instance.ResetScore();
            GameManager.Instance.ResetLife();
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://scenes/levels/level.tscn");
        }

        /// <summary>
        /// Exits the level and returns to the main menu, with a small delay for SFX to play.
        /// </summary>
        public async void _on_exit_button_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            await ToSignal(GetTree().CreateTimer(_sfxDelayTimer), "timeout");

            GameManager.Instance.SaveData();
            GameManager.Instance._difficulty = 0;
            GameManager.Instance.ResetScore();
            GameManager.Instance.ResetLife();
            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://main-menu/scenes/main-menu.tscn");
        }

        /// <summary>
        /// Displays a message sign with text and slides it up, then down after a delay.
        /// </summary>
        public async Task ShowMessage(float messageTimer, string messageText,
            string pathToSound = "", bool showIncoming = false)
        {
            if (showIncoming)
            {
                _incomingSign.Visible = true;

                // Safely access /root/Level and its _pedestriansToSpawn
                if (HasNode("/root/Level"))
                {
                    _incomingCountLabel.Text = $"{GetNode<Level>("/root/Level")._pedestriansToSpawn}";
                }
                else
                {
                    GD.PushWarning("ShowMessage: Could not find /root/Level to display incoming count.");
                    _incomingCountLabel.Text = "?";
                }
            }

            _messageLabel.Text = messageText;
            _messageUp = true;
            PlaySfx("res://assets/audio/sfx/menu/slide-up-long.wav");

            if (pathToSound != "")
            {
                PlaySfx(pathToSound);
            }

            await ToSignal(GetTree().CreateTimer(messageTimer), "timeout");

            // Start sliding down
            _messageUp = false;

            // Wait until it's fully down
            while (_messageSign.Position.Y < _messageStartYPos)
            {
                await ToSignal(GetTree(), "process_frame");
            }

            // Snap to exact position
            _messageSign.Position = new Vector2(_messageSign.Position.X, _messageStartYPos);
            _incomingSign.Visible = false;
        }

        /// <summary>
        /// Displays a message sign with text and slides it up, then down after a delay.
        /// </summary>
        public async Task ShowGameOver(string pathToSound = "")
        {
            _pauseButton.Visible = false;
            _actionSign.Visible = true;

            // Safe access to Level and GUI inside it
            if (GetTree().Root.HasNode("Level") && GetTree().Root.GetNode("Level").HasNode("GUI"))
            {
                var level = GetTree().Root.GetNode("Level");
                level.MoveChild(level.GetNode("GUI"), level.GetChildCount() - 1);
            }
            else
            {
                GD.PushWarning("ShowGameOver: Could not find Level or GUI node for z-order change.");
            }

            _messageLabel.Text = "GAME_OVER";
            _messageUp = true;
            PlaySfx("res://assets/audio/sfx/menu/slide-up-long.wav");

            if (pathToSound != "")
            {
                PlaySfx(pathToSound);
            }

            while (_messageSign.Position.Y < _messageStartYPos)
            {
                await ToSignal(GetTree(), "process_frame");
            }
        }

        /// <summary>
        /// Moves the message sign up toward a target Y position at a fixed speed.
        /// </summary>
        private void MessageUp(double delta)
        {
            if (_messageSign.Position.Y > _messageTargetYPos)
            {
                _messageSign.Position += Vector2.Up * (float)(_messageMoveSpeed * delta);

                if (_messageSign.Position.Y <= _messageTargetYPos)
                {
                    _messageSign.Position = new Vector2(_messageSign.Position.X, _messageTargetYPos);
                }
            }
        }

        /// <summary>
        /// Moves the message sign down toward a start Y position at a fixed speed.
        /// </summary>
        private void MessageDown(double delta)
        {
            if (_messageSign.Position.Y < _messageStartYPos)
            {
                _messageSign.Position += Vector2.Down * (float)(_messageMoveSpeed * delta);

                if (_messageSign.Position.Y >= _messageStartYPos)
                {
                    _messageSign.Position = new Vector2(_messageSign.Position.X, _messageStartYPos);
                }
            }
        }

        /// <summary>
        /// Loads and plays a sound effect from a given file path.
        /// </summary>
        public void PlaySfx(string pathToSfx)
        {
            if (_sfxPlayer == null || string.IsNullOrEmpty(pathToSfx))
            {
                GD.PushWarning("PlaySfx: No valid SFX path provided.");
                return;
            }

            var stream = GD.Load<AudioStream>(pathToSfx);
            if (stream != null)
            {
                _sfxPlayer.Stream = stream;
                _sfxPlayer.Play();
            }
            else
            {
                GD.PushError($"PlaySfx: Failed to load AudioStream from path: {pathToSfx}");
            }
        }

        /// <summary>
        /// Called when the node enters the scene tree. Initializes UI labels and sets up mute state.
        /// </summary>
        public override async void _Ready()
        {
            SetProcessMode(ProcessModeEnum.Always); // GUI should always process even when game is paused

            // Wait a frame to make sure node is fully in tree
            await ToSignal(GetTree(), "process_frame");

            // Sync mute button state with actual audio bus volume
            if (AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master")) <= -80f)
            {
                _muteButton.ButtonPressed = true;
            }

            // Bind GUI labels to the GameManager
            GameManager.Instance.SetScoreLabel(
                GetNode<Label>("GuiRect/ScoreLabel"));
            GameManager.Instance.SetHighscoreLabel(
                GetNode<Label>("GuiRect/HighscoreLabel"));
            GameManager.Instance.SetLifeLabel(
                GetNode<Label>("GuiRect/LifeLabel"));
        }

        /// <summary>
        /// Runs every frame. Moves the message sign up or down depending on _messageUp state.
        /// </summary>
        public override void _Process(Double delta)
        {
            if (_messageSign != null && _messageUp)
            {
                MessageUp(delta);
            }
            else
            {
                MessageDown(delta);
            }
        }
    }
}
