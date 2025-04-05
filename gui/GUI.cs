using Godot;
using System;

namespace Crosswalk
{
    /// <summary>
    /// GUI handles the in-game user interface elements like the pause menu, mute button,
    /// status labels (score, lives, etc.), and animated message sign.
    /// It also manages related sound effects and reacts to user inputs like pause/mute.
    /// </summary>
    public partial class GUI : Control
    {
        // Reference to the main GUI container node (can be used for grouping or visibility control)
        [Export] private Node _gui;
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

        // Internal flag to determine if the message sign should currently be moving up
        private bool _messageUp = false;
        // Y position where the message sign should stop when sliding up (visible position)
        private int _messageTargetYPos = 352;
        // Y position where the message sign starts and returns to when sliding down (off-screen)
        private int _messageStartYPos = 640;
        // Speed (pixels per second) at which the message sign moves vertically
        private float _messageMoveSpeed = 750f;

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
        public void _on_reset_button_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            GD.Print("Implement reset button functionality!");
        }

        /// <summary>
        /// Exits the level and returns to the main menu, with a small delay for SFX to play.
        /// </summary>
        public async void _on_exit_button_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            await ToSignal(GetTree().CreateTimer(_sfxDelayTimer), "timeout");

            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://main-menu/scenes/main-menu.tscn");
        }

        /// <summary>
        /// Displays a message sign with text and slides it up, then down after a delay.
        /// </summary>
        public async void ShowMessage(float messageTimer, string messageText)
        {
            _messageLabel.Text = messageText;
            _messageUp = true;
            PlaySfx("res://assets/audio/sfx/menu/slide-up-long.wav");

            await ToSignal(GetTree().CreateTimer(messageTimer), "timeout");

            _messageUp = false;
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
            _sfxPlayer.Stream = GD.Load<AudioStream>(pathToSfx);
            _sfxPlayer.Play();
        }

        /// <summary>
        /// Called when the node enters the scene tree. Initializes UI labels and sets up mute state.
        /// </summary>
        public override void _Ready()
        {
            SetProcessMode(ProcessModeEnum.Always); // GUI should always process even when game is paused

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
