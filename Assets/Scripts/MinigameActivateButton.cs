using UnityEngine;
using System.Collections;

public class MinigameActivateButton : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject Minigame;
    [SerializeField] private GameObject[] Doors;
    [SerializeField] private float doorOpenSpeed = 2f;
    public void Interact()
    {
        Debug.Log("Minigame started");
        Minigame.SetActive(true);

        // Pass this specific world button reference over to the UI manager
        ButtonsMiniGameManager manager = Minigame.GetComponent<ButtonsMiniGameManager>();
        if (manager != null)
        {
            manager.SetActivatingButton(this);
        }
    }

    // This is the function the UI Manager will trigger remotely on victory
    public void OpenDoors()
    {
        foreach (GameObject door in Doors)
        {
            if (door == null) continue;

            // 1. Turn off the Mesh Collider immediately so the player can pass
            if (door.TryGetComponent<MeshCollider>(out MeshCollider collider))
            {
                collider.enabled = false;
            }

            // 2. Start the blendshape animation routine
            if (door.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer smr))
            {
                StartCoroutine(AnimateDoorBlendshape(smr));
            }
        }
    }

    private IEnumerator AnimateDoorBlendshape(SkinnedMeshRenderer smr)
    {
        float progress = 0f;
        // Adjust the index (0, 1, etc.) to match your exact blendshape key layout
        int blendShapeIndex = 0; 

        while (progress < 100f)
        {
            progress += Time.deltaTime * doorOpenSpeed * 50f; 
            progress = Mathf.Clamp(progress, 0f, 100f);
            
            smr.SetBlendShapeWeight(blendShapeIndex, progress);
            yield return null;
        }
    }
}