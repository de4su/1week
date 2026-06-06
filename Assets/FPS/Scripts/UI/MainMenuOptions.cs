using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Unity.FPS.UI
{
    public class MainMenuOptions : MonoBehaviour
    {
        public GameObject OptionsPanel;
        public GameObject MainMenuRoot;
        public GameObject DefaultSelection;

        public void OpenOptions()
        {
            OptionsPanel.SetActive(true);
            MainMenuRoot.SetActive(false);
            
            if (DefaultSelection != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(DefaultSelection);
            }
        }

        public void CloseOptions()
        {
            OptionsPanel.SetActive(false);
            MainMenuRoot.SetActive(true);
        }

        void Update()
        {
            if (OptionsPanel.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseOptions();
            }
        }
    }
}