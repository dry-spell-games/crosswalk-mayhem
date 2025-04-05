using Godot;
using System;

namespace Crosswalk
{
    public partial class GUI : Control
    {
        [Export] private Node _gui;
        [Export] private AudioStreamPlayer2D _sfxPlayer;
        [Export] private TextureRect _pauseMenu;
        [Export] private TextureButton _muteButton;
        [Export] private float _sfxDelayTimer = 0.1f;
        [Export] private TextureRect _messageSign;
        [Export] private Label _messageLabel;
        private bool _messageUp = false;
        private int _messageTargetYPos = 352;
        private int _messageStartYPos = 640;
        private float _messageMoveSpeed = 750f;

        public void _on_pause_button_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            _pauseMenu.Visible = true;

            GetTree().Paused = true;
        }

        public void _on_play_button_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            _pauseMenu.Visible = false;

            GetTree().Paused = false;
        }

        private void _on_mute_button_toggled(bool toggled_on)
        {
            if (toggled_on)
            {
                GameManager.Instance._masterVolume = -80f;
                GameManager.Instance.SetVolume("Master", GameManager.Instance._masterVolume);
            }
            else
            {
                if (GameManager.Instance._savedMasterVolume == -80f)
                {
                    GameManager.Instance._masterVolume = -6.0f;
                    GameManager.Instance.SetVolume("Master", GameManager.Instance._masterVolume);
                }
                else
                {
                    GameManager.Instance._masterVolume = GameManager.Instance._savedMasterVolume;
                    GameManager.Instance.SetVolume("Master", GameManager.Instance._masterVolume);
                }
            }

            GameManager.Instance.SaveData();
        }

        public void _on_reset_button_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");

            // Implement reset button functionality here!
            GD.Print("Implement reset button functionality!");
        }

        public async void _on_exit_button_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            await ToSignal(GetTree().CreateTimer(_sfxDelayTimer), "timeout");

            GetTree().Paused = false;
            GetTree().ChangeSceneToFile("res://main-menu/scenes/main-menu.tscn");
        }

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
            if (_messageSign.Position.Y > _messageTargetYPos)  // Move up if current Y position is above the target
            {
                _messageSign.Position += Vector2.Up * (float)(_messageMoveSpeed * delta);

                // If it reaches or passes the target Y position, snap it to the target
                if (_messageSign.Position.Y <= _messageTargetYPos)
                {
                    _messageSign.Position = new Vector2(_messageSign.Position.X, _messageTargetYPos);
                }
            }
        }

        /// <summary>
        /// Moves the message sign down toward a target Y position at a fixed speed.
        /// </summary>
        private void MessageDown(double delta)
        {
            if (_messageSign.Position.Y < _messageStartYPos)  // Move down if current Y position is below the target
            {
                _messageSign.Position += Vector2.Down * (float)(_messageMoveSpeed * delta);

                // If it reaches or passes the target Y position, snap it to the target
                if (_messageSign.Position.Y >= _messageStartYPos)
                {
                    _messageSign.Position = new Vector2(_messageSign.Position.X, _messageStartYPos);
                }
            }
        }

        /// <summary>
        /// Plays a sound effect from a given file path.
        /// </summary>
        public void PlaySfx(string pathToSfx)
        {
            _sfxPlayer.Stream = GD.Load<AudioStream>(pathToSfx);
            _sfxPlayer.Play();
        }

        public override void _Ready()
        {
            SetProcessMode(ProcessModeEnum.Always); // Keeps this node running even when paused

            if (AudioServer.GetBusVolumeDb(AudioServer.GetBusIndex("Master")) <= -80f)
            {
                _muteButton.ButtonPressed = true;
            }

            GameManager.Instance.SetScoreLabel(
                GetNode<Label>("GuiRect/ScoreLabel"));
            GameManager.Instance.SetHighscoreLabel(
                GetNode<Label>("GuiRect/HighscoreLabel"));
            GameManager.Instance.SetLifeLabel(
                GetNode<Label>("GuiRect/LifeLabel"));
        }

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
