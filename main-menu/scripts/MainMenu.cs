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
        // --- UI Elements and Animation Control ---
        [Export] private TextureRect _signs = null; // Main menu signs (e.g. Play/Quit) that animate into view
        [Export] private int _signsTargetYPos = 352; // Target Y position for the signs when fully visible
        [Export] private int _signsStartYPos = 640; // Starting Y position for the signs (off-screen position)
        private float _signsMoveSpeed = 750f; // Speed at which the signs move up/down
        private bool _signsUp = true; // Whether the signs are currently in the "up" (visible) position

        [Export] private TextureRect _settings = null; // Settings panel that slides into view
        [Export] private int _settingsTargetYPos = 320; // Target Y position for the settings panel
        [Export] private int _settingsStartYPos = 640; // Starting Y position for the settings panel (off-screen)
        private bool _settingsUp = false; // Whether the settings panel is currently visible

        [Export] private AnimatedSprite2D _english = null; // English language flag with animation
        [Export] private AnimatedSprite2D _finnish = null; // Finnish language flag with animation

        // --- Audio Controls and Players ---
        [Export] private AudioControl _masterAudioControl = null; // Slider control for master volume
        [Export] private AudioControl _musicAudioControl = null; // Slider control for music volume
        [Export] private AudioControl _sfxAudioControl = null; // Slider control for sound effects volume

        [Export] private AudioStreamPlayer2D _musicPlayer; // Audio player node for background music
        [Export] private AudioStreamPlayer2D _sfxPlayer; // Audio player node for UI sound effects

        [Export] private float _musicDelayTimer = 1.0f; // Delay before background music starts playing
        [Export] private float _sfxDelayTimer = 0.1f; // Delay before sound effects play after input

        // --- Tutorial and Settings ---
        [Export] private TextureRect _tutorialRect; // Panel displaying tutorial instructions
        private bool _resetHighscore = false; // Flag indicating whether to reset highscore on OK

        /// <summary>
        /// Called when the English button is pressed.
        /// Sets the game language to English and plays the flag animation.
        /// </summary>
        public void _on_english_pressed()
        {
            GameManager.Instance.SetLanguage("en");
            _english.Play("wave");
            _finnish.Play("still");

            PlaySfx("res://assets/audio/sfx/menu/button.wav");
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

            PlaySfx("res://assets/audio/sfx/menu/button.wav");
        }

        /// <summary>
        /// Called when the Play button is pressed.
        /// Loads the main level scene.
        /// </summary>
        public async void _on_play_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            await ToSignal(GetTree().CreateTimer(_sfxDelayTimer), "timeout");

            GameManager.Instance.SaveData();
            GetTree().ChangeSceneToFile("res://scenes/levels/level.tscn");
        }

        /// <summary>
        /// Called when the Settings button is pressed.
        /// Hides the main menu and shows the settings panel.
        /// </summary>
        public void _on_settings_pressed()
        {
            _signsUp = false;
            _settingsUp = true;

            PlaySfx("res://assets/audio/sfx/menu/slide-up-long.wav");
        }

        /// <summary>
        /// Called when the Quit button is pressed.
        /// Exits the game.
        /// </summary>
        public async void _on_quit_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            await ToSignal(GetTree().CreateTimer(_sfxDelayTimer), "timeout");

            GameManager.Instance.SaveData();
            GetTree().Quit();
        }

        /// <summary>
        /// Called when the Tutorial button is pressed.
        /// Displays the tutorial instructions panel.
        /// </summary>
        public void _on_tutorial_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            _tutorialRect.Visible = true;
        }

        /// <summary>
        /// Called when the Exit Tutorial button is pressed.
        /// Hides the tutorial instructions panel.
        /// </summary>
        public void _on_tutorial_exit_pressed()
        {
            PlaySfx("res://assets/audio/sfx/menu/button.wav");
            _tutorialRect.Visible = false;
        }

        /// <summary>
        /// Called when the Reset Highscore toggle is changed.
        /// Updates the internal flag and plays a sound effect.
        /// </summary>
        public void _on_reset_highscore_toggled(bool toggledOn)
        {
            _resetHighscore = toggledOn;

            PlaySfx("res://assets/audio/sfx/menu/button.wav");
        }

        /// <summary>
        /// Called when the OK button is pressed in the settings menu.
        /// Applies changes (resets highscore if toggled) and hides the settings panel.
        /// </summary>
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

            PlaySfx("res://assets/audio/sfx/menu/slide-up-long.wav");
        }

        /// <summary>
        /// Called when the Cancel button is pressed in the settings menu.
        /// Reverts audio settings and resets the highscore toggle.
        /// </summary>
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

            PlaySfx("res://assets/audio/sfx/menu/slide-up-long.wav");
        }

        /// <summary>
        /// Moves a TextureRect up toward a target Y position at a fixed speed.
        /// </summary>
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

        /// <summary>
        /// Moves a TextureRect down toward a target Y position at a fixed speed.
        /// </summary>
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


        /// <summary>
        /// Plays the background music after a short delay.
        /// </summary>
        public async void PlayMusic()
        {
            await ToSignal(GetTree().CreateTimer(_musicDelayTimer), "timeout");

            if (_musicPlayer != null)
            {
                _musicPlayer.Play();
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

            PlaySfx("res://assets/audio/sfx/menu/slide-up.wav");
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
