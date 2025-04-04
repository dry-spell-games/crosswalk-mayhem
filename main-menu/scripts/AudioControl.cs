using Godot;
using System;
using System.Threading;

namespace Crosswalk
{
    public partial class AudioControl : Control
    {
        [Export] private HSlider _volumeSlider = null;
        [Export] public string _busName;
        public float _originalVolume = 0.0f;

        private void OnVolumeSliderValueChanged(float value)
        {
            UpdateVolume();
        }

        private void UpdateVolume()
        {
            float linearVolume = (float)_volumeSlider.Value;
            float decibelVolume = Mathf.LinearToDb(linearVolume);
            GameManager.Instance.SetVolume(_busName, decibelVolume);

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

        public void Initialize(float dBValue)
        {
            float linearValue = Mathf.DbToLinear(dBValue);
            _volumeSlider.Value = linearValue;
        }

        public void SetVolumeSlider(float dBValue)
        {
            float linearValue = Mathf.DbToLinear(dBValue);
            var callable = new Callable(this, nameof(OnVolumeSliderValueChanged));

            if (_volumeSlider.IsConnected(Slider.SignalName.ValueChanged, callable))
                _volumeSlider.Disconnect(Slider.SignalName.ValueChanged, callable);

            _volumeSlider.Value = linearValue;
            _volumeSlider.Connect(Slider.SignalName.ValueChanged, callable);
        }

        public void ApplyCurrentSliderValue()
        {
            UpdateVolume();
        }

        public override void _Ready()
        {
            base._Ready();

            _volumeSlider.Connect(Slider.SignalName.ValueChanged,
                new Callable(this, nameof(OnVolumeSliderValueChanged)));
        }
    }
}
