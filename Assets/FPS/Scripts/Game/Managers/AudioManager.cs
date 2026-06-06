using UnityEngine;
using UnityEngine.Audio;

namespace Unity.FPS.Game
{
    public class AudioManager : MonoBehaviour
    {
        public AudioMixer[] AudioMixers;

        public AudioMixerGroup[] FindMatchingGroups(string subPath)
        {
            for (int i = 0; i < AudioMixers.Length; i++)
            {
                AudioMixerGroup[] results = AudioMixers[i].FindMatchingGroups(subPath);
                if (results != null && results.Length != 0)
                {
                    return results;
                }
            }

            return null;
        }

        public void SetFloat(string name, float value)
        {
            for (int i = 0; i < AudioMixers.Length; i++)
            {
                if (AudioMixers[i] != null)
                {
                    AudioMixers[i].SetFloat(name, value);
                }
            }
        }

        public bool GetFloat(string name, out float value)
        {
            value = 0f;
            for (int i = 0; i < AudioMixers.Length; i++)
            {
                if (AudioMixers[i] != null)
                {
                    if (AudioMixers[i].GetFloat(name, out value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void Start()
        {
            if (PlayerPrefs.HasKey("MasterVolume"))
            {
                AudioUtility.SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume"));
            }
            if (PlayerPrefs.HasKey("MusicVolume"))
            {
                AudioUtility.SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));
            }
            if (PlayerPrefs.HasKey("SFXVolume"))
            {
                AudioUtility.SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume"));
            }
        }
    }
}