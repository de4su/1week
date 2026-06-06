using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class VolumeSlider : MonoBehaviour
    {
        public enum VolumeType
        {
            Master,
            Music,
            SFX
        }

        public VolumeType Type;
        public Slider Slider;

        void Start()
        {
            if (Slider == null)
                Slider = GetComponent<Slider>();

            // Initialize slider value
            switch (Type)
            {
                case VolumeType.Master:
                    Slider.value = AudioUtility.GetMasterVolume();
                    break;
                case VolumeType.Music:
                    Slider.value = AudioUtility.GetMusicVolume();
                    break;
                case VolumeType.SFX:
                    Slider.value = AudioUtility.GetSFXVolume();
                    break;
            }

            Slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        void OnSliderValueChanged(float value)
        {
            switch (Type)
            {
                case VolumeType.Master:
                    AudioUtility.SetMasterVolume(value);
                    PlayerPrefs.SetFloat("MasterVolume", value);
                    break;
                case VolumeType.Music:
                    AudioUtility.SetMusicVolume(value);
                    PlayerPrefs.SetFloat("MusicVolume", value);
                    break;
                case VolumeType.SFX:
                    AudioUtility.SetSFXVolume(value);
                    PlayerPrefs.SetFloat("SFXVolume", value);
                    break;
            }
        }
    }
}