using Godot;
using System;
using System.Runtime;

namespace Crosswalk
{
    /// <summary>
    /// MainMenu handles the game's main menu UI, including language selection,
    /// scene transition, and animated intro sign movement.
    /// </summary>
    public partial class MainMenu : Control
    {
        // UI Elements and animation control
        [Export] private TextureRect _signs = null; // Sign graphic to be animated into view
        [Export] private int _signsTargetYPos = 352; // Final Y position of the button signs
        [Export] private int _signsStartYPos = 640; // Starting Y position of the button signs
        private float _signsMoveSpeed = 750f; // Speed at which the signs move
        private bool _signsUp = true;
        [Export] private TextureRect _settings = null;
        [Export] private int _settingsTargetYPos = 320; // Final Y position of the settings signs
        [Export] private int _settingsStartYPos = 640; // Starting Y position of the settings signs
        private bool _settingsUp = false;
        [Export] private AnimatedSprite2D _english = null; // English language flag animation
        [Export] private AnimatedSprite2D _finnish = null; // Finnish language flag animation

        [Export] private AudioControl _masterAudioControl = null;
        [Export] private AudioControl _musicAudioControl = null;
        [Export] private AudioControl _sfxAudioControl = null;
        [Export] private AudioStreamPlayer2D _musicPlayer;
        [Export] private AudioStreamPlayer2D _sfxPlayer;
        [Export] private float _musicDelayTimer = 1.0f;
        [Export] private float _sfxDelayTimer = 0.1f;
        [Export] private TextureRect _tutorialRect;
        private bool _resetHighscore = false;

        /// <summary>
        /// Called when the English button is pressed.
        /// Sets the game language to English and plays the flag animation.
        /// </summary>
        public void _on_english_pressed()
        {
            GameManager.Instance.SetLanguage("en");
            _english.Play("wave");
            _finnish.Play("still");

            PlaySfx("res://assets/audio/sfx/button.wav");
        }

        /// <summary>
        /// Called when the Finnish button is pressed.
        /// Sets the game language to Finnish and plays the flag animation.
        /// </summary>
        public void _on_finnish_pressed()
        {
            GameManager.Instance.SetLanguage("fi");
            _english.Play("still");
            _finnish.Play("wave");

            PlaySfx("res://assets/audio/sfx/button.wav");
        }

        /// <summary>
        /// Called when the Play button is pressed.
        /// Loads the main level scene.
        /// </summary>
        public async void _on_play_pressed()
        {
            PlaySfx("res://assets/audio/sfx/button.wav");
            await ToSignal(GetTree().CreateTimer(_sfxDelayTimer), "timeout");

            GameManager.Instance.SaveData();
            GetTree().ChangeSceneToFile("res://scenes/levels/level.tscn");
        }

        public void _on_settings_pressed()
        {
            _signsUp = false;
            _settingsUp = true;

            PlaySfx("res://assets/audio/sfx/slide-up-long.wav");
        }

        /// <summary>
        /// Called when the Quit button is pressed.
        /// Exits the game.
        /// </summary>
        public async void _on_quit_pressed()
        {
            PlaySfx("res://assets/audio/sfx/button.wav");
            await ToSignal(GetTree().CreateTimer(_sfxDelayTimer), "timeout");

            GameManager.Instance.SaveData();
            GetTree().Quit();
        }

        public void _on_tutorial_pressed()
        {
            PlaySfx("res://assets/audio/sfx/button.wav");
            _tutorialRect.Visible = true;
        }

        public void _on_tutorial_exit_pressed()
        {
            PlaySfx("res://assets/audio/sfx/button.wav");
            _tutorialRect.Visible = false;
        }

        public void _on_reset_highscore_toggled(bool toggledOn)
        {
            _resetHighscore = toggledOn;

            PlaySfx("res://assets/audio/sfx/button.wav");
        }

        public void _on_ok_pressed()
        {
            if (_resetHighscore)
            {
                GameManager.Instance.SetHighscore(0);
            }
            _resetHighscore = false;

            GameManager.Instance.SaveData();

            GetNode<Button>("Settings/ResetHighscore").ButtonPressed = false;
            _signsUp = true;
            _settingsUp = false;

            PlaySfx("res://assets/audio/sfx/slide-up-long.wav");
        }

        public void _on_calcel_pressed()
        {
            _masterAudioControl.SetVolumeSlider(GameManager.Instance._savedMasterVolume);
            _musicAudioControl.SetVolumeSlider(GameManager.Instance._savedMusicVolume);
            _sfxAudioControl.SetVolumeSlider(GameManager.Instance._savedSfxVolume);

            _masterAudioControl.ApplyCurrentSliderValue();
            _musicAudioControl.ApplyCurrentSliderValue();
            _sfxAudioControl.ApplyCurrentSliderValue();

            _resetHighscore = false;

            GetNode<Button>("Settings/ResetHighscore").ButtonPressed = false;
            _signsUp = true;
            _settingsUp = false;

            PlaySfx("res://assets/audio/sfx/slide-up-long.wav");
        }

        private void MoveUp(double delta, int target, TextureRect toBeMoved)
        {
            if (toBeMoved.Position.Y > target)  // Move up if current Y position is above the target
            {
                toBeMoved.Position += Vector2.Up * (float)(_signsMoveSpeed * delta);

                // If it reaches or passes the target Y position, snap it to the target
                if (toBeMoved.Position.Y <= target)
                {
                    toBeMoved.Position = new Vector2(toBeMoved.Position.X, target);
                }
            }
        }

        private void MoveDown(double delta, int target, TextureRect toBeMoved)
        {
            if (toBeMoved.Position.Y < target)  // Move down if current Y position is below the target
            {
                toBeMoved.Position += Vector2.Down * (float)(_signsMoveSpeed * delta);

                // If it reaches or passes the target Y position, snap it to the target
                if (toBeMoved.Position.Y >= target)
                {
                    toBeMoved.Position = new Vector2(toBeMoved.Position.X, target);
                }
            }
        }

        public async void PlayMusic()
        {
            await ToSignal(GetTree().CreateTimer(_musicDelayTimer), "timeout");

            if (_musicPlayer != null)
            {
                _musicPlayer.Play();
            }
        }

        public void PlaySfx(string pathToSfx)
        {
            _sfxPlayer.Stream = GD.Load<AudioStream>(pathToSfx);
            _sfxPlayer.Play();
        }

        /// <summary>
        /// Called when the scene is ready.
        /// Initializes the flag animation based on the saved language setting.
        /// </summary>
        public override void _Ready()
        {
            GameManager.Instance.LoadData();
            TranslationServer.SetLocale(GameManager.Instance._langCode);

            if (GameManager.Instance._langCode == "en")
            {
                _english.Play("wave");
            }
            else
            {
                _finnish.Play("wave");
            }

            if (_masterAudioControl != null)
            {
                //_masterAudioControl._originalVolume = GameManager.Instance._masterVolume;
                _masterAudioControl.Initialize(GameManager.Instance._masterVolume);
            }
            if (_musicAudioControl != null)
            {
                //_musicAudioControl._originalVolume = GameManager.Instance._musicVolume;
                _musicAudioControl.Initialize(GameManager.Instance._musicVolume);
            }
            if (_sfxAudioControl != null)
            {
                //_sfxAudioControl._originalVolume = GameManager.Instance._sfxVolume;
                _sfxAudioControl.Initialize(GameManager.Instance._sfxVolume);
            }

            PlaySfx("res://assets/audio/sfx/slide-up.wav");
            PlayMusic();
        }

        /// <summary>
        /// Called every frame.
        /// Handles sign movement if it hasn’t yet reached its target position.
        /// </summary>
        public override void _Process(Double delta)
        {
            if (_signs != null && _settings != null && _signsUp && !_settingsUp)
            {
                MoveDown(delta, _settingsStartYPos, _settings);
                MoveUp(delta, _signsTargetYPos, _signs);
            }
            else if (_signs != null && _settings != null && !_signsUp && _settingsUp)
            {
                MoveDown(delta, _signsStartYPos, _signs);
                MoveUp(delta, _settingsTargetYPos, _settings);
            }
        }
    }
}
