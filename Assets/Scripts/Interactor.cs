using UnityEngine;
using UnityEngine.InputSystem;

interface IInteractable
{
    public void Interact();
}

public class Interactor : MonoBehaviour
{
    [SerializeField] private Transform InteractorSource;
    [SerializeField] private float InteractRange;

    [SerializeField] private InputAction interactAction;


    private void OnEnable()
    {
        interactAction.Enable(); 
    }

    private void OnDisable()
    {
        interactAction.Disable();
    }

    void Update()
    {
        if(interactAction.WasPressedThisFrame())
        {
            Ray ray = new Ray(InteractorSource.position, InteractorSource.forward);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, InteractRange))
            {
                if(hitInfo.collider.gameObject.TryGetComponent(out IInteractable interactObj))
                {
                    interactObj.Interact();
                }
            }
        }
    }
}