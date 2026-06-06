using UnityEngine;

public class MinigameActivateButton : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject Minigame;
    public void Interact()
    {
        Debug.Log("Minigame started");
        Minigame.SetActive(true);
    }
}