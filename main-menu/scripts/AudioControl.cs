using Godot;
using System;
using System.Threading;

namespace Crosswalk
{
    /// <summary>
    /// AudioControl manages the UI slider for a specific audio bus (e.g. Master, Music, SFX).
    /// It converts slider values to decibels and updates the corresponding volume in the AudioServer.
    /// </summary>
    public partial class AudioControl : Control
    {
        [Export] private HSlider _volumeSlider = null; // UI slider controlling the volume
        [Export] public string _busName; // The name of the audio bus this slider controls (e.g. "Master", "Music", "Sfx")

        /// <summary>
        /// Called when the slider value is changed by the user.
        /// Triggers volume update for the associated audio bus.
        /// </summary>
        /// <param name="value">The new linear value from the slider (0.0 to 1.0)</param>
        private void OnVolumeSliderValueChanged(float value)
        {
            UpdateVolume();
        }

        /// <summary>
        /// Converts the current slider value to decibels and updates the audio bus.
        /// Also stores the value in GameManager for saving.
        /// </summary>
        private void UpdateVolume()
        {
            float linearVolume = (float)_volumeSlider.Value;
            float decibelVolume = Mathf.LinearToDb(linearVolume);
            GameManager.Instance.SetVolume(_busName, decibelVolume);

            // Store the current decibel volume in GameManager based on bus name
            switch (_busName)
            {
                case "Master":
                    GameManager.Instance._masterVolume = Mathf.Max(decibelVolume, -80.0f);
                    break;
                case "Music":
                    GameManager.Instance._musicVolume = Mathf.Max(decibelVolume, -80.0f);
                    break;
                case "Sfx":
                    GameManager.Instance._sfxVolume = Mathf.Max(decibelVolume, -80.0f);
                    break;
            }
        }

        /// <summary>
        /// Initializes the slider's value based on a decibel input.
        /// Used when loading saved settings.
        /// </summary>
        /// <param name="dBValue">Volume in decibels</param>
        public void Initialize(float dBValue)
        {
            float linearValue = Mathf.DbToLinear(dBValue);
            _volumeSlider.Value = linearValue;
        }

        /// <summary>
        /// Programmatically sets the slider value without triggering the ValueChanged signal,
        /// then reconnects the signal afterward.
        /// </summary>
        /// <param name="dBValue">Volume in decibels</param>
        public void SetVolumeSlider(float dBValue)
        {
            float linearValue = Mathf.DbToLinear(dBValue);
            var callable = new Callable(this, nameof(OnVolumeSliderValueChanged));

            if (_volumeSlider.IsConnected(Slider.SignalName.ValueChanged, callable))
                _volumeSlider.Disconnect(Slider.SignalName.ValueChanged, callable);

            _volumeSlider.Value = linearValue;

            _volumeSlider.Connect(Slider.SignalName.ValueChanged, callable);
        }

        /// <summary>
        /// Applies the current slider value to the audio bus.
        /// Used when restoring or applying changes manually.
        /// </summary>
        public void ApplyCurrentSliderValue()
        {
            UpdateVolume();
        }

        /// <summary>
        /// Called when the node is added to the scene.
        /// Connects the slider's ValueChanged signal to handle live volume updates.
        /// </summary>
        public override void _Ready()
        {
            base._Ready();

            _volumeSlider.Connect(Slider.SignalName.ValueChanged,
                new Callable(this, nameof(OnVolumeSliderValueChanged)));
        }
    }
}
