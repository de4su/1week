using UnityEngine;

public class MiniGameButtonId : MonoBehaviour
{
    [SerializeField] private ButtonsMiniGameManager manager;
    [SerializeField] private int myIndex; // Set this to 0 for button 1, 1 for button 2, etc.

    // Call this function via your UI Button component's "OnClick()" event panel in the inspector
    public void HandleClick()
    {
        if (manager != null)
        {
            manager.MinigameButtonClicked(myIndex);
        }
    }
}