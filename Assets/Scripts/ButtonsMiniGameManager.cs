using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.FPS.Gameplay;
using UnityEngine.InputSystem;
using System.Linq;


public class ButtonsMiniGameManager : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private int sequenceSize = 5;
    [SerializeField] private float flashDuration = 1.0f;
    [SerializeField] private float delayBetweenButtons = 0.2f;
    [SerializeField] private float minigameStartDelay = 1.0f;

    private List<int> buttonSequence;
    private List<int> playerSequence = new List<int>();
    private bool isPlayerTurn = false;

    void OnEnable()
    {
        StartMinigameRun();
    }

    List<int> createButtonSequence(int size)
    {
        List<int> _sequence = new List<int>();
        List<int> _numberPool = new List<int>();

        // Dynamically populate the pool based on assigned buttons (e.g., 0 to buttons.Length - 1)
        for (int i = 0; i < buttons.Length; i++)
        {
            _numberPool.Add(i);
        }

        if (size > _numberPool.Count)
        {
            Debug.LogWarning("Requested size is larger than the available unique numbers. Clamping to 9.");
            size = _numberPool.Count;
        }

        // 2. Randomly pull numbers out of the pool one by one
        for (int i = 0; i < size; i++)
        {
            // Pick a random remaining index in the pool
            int randomIndex = Random.Range(0, _numberPool.Count);
            
            // Add that number to our sequence
            _sequence.Add(_numberPool[randomIndex]);
            
            // Remove it from the pool so it can never be picked again
            _numberPool.RemoveAt(randomIndex);
        }

        return _sequence;
    }

    void StartMinigameRun()
    {
        // Find the player input handler (adjust how you reference it if needed)
        PlayerInputHandler playerInput = FindAnyObjectByType<PlayerInputHandler>();

        if (playerInput != null)
        {
            Debug.Log("fuck");
            playerInput.SetMinigameMode(true); // Frees mouse, disables guns
        }

        playerSequence.Clear();
        isPlayerTurn = false;
        buttonSequence = createButtonSequence(sequenceSize);
        StartCoroutine(ShowSequence());

        // --- ADD THIS TO UNLOCK THE MOUSE ---
        Cursor.lockState = CursorLockMode.None; // Stops the cursor from being trapped in the center
        Cursor.visible = true;                  // Makes the cursor visible
    }

    IEnumerator ShowSequence()
    {
        isPlayerTurn = false;
        // Brief pause before the sequence starts showing to let the player focus
        yield return new WaitForSeconds(minigameStartDelay);

        foreach (int button in buttonSequence)
        {
            yield return FlashButton(button, Color.green, flashDuration);

            // Tiny buffer window so back-to-back flashes don't bleed together visually
            yield return new WaitForSeconds(delayBetweenButtons);
        }

        // Sequence is finished showing, player is allowed to input now
        isPlayerTurn = true;
    }

    // Public method that individual button scripts will call when clicked
    public void MinigameButtonClicked(int buttonIndex)
    {
        // Ignore input if the sequence is still flashing on screen
        if (!isPlayerTurn) return;

        playerSequence.Add(buttonIndex);

        // Check if the player messed up immediately on this specific step
        if (playerSequence[playerSequence.Count - 1] != buttonSequence[playerSequence.Count - 1])
        {
            StartCoroutine(FlashButton(buttonIndex, Color.red, 0.25f));
            MiniGameFailed();
            return;
        }
        else StartCoroutine(FlashButton(buttonIndex, Color.blue, 0.25f));

        // If player successfully tracked the sequence up to the current total length
        if (playerSequence.Count == buttonSequence.Count)
        {
            StartCoroutine(WinSequenceRoutine());
        }
    }


    private IEnumerator WinSequenceRoutine()
    {
        // Block further inputs immediately
        isPlayerTurn = false; 

        // 2. FORCE the script to wait 0.25 seconds for the green flashes to finish!
        yield return new WaitForSeconds(1f);

        // 3. NOW it is safe to close everything down
        MiniGameWon();
    }

    void MiniGameFailed()
    {
        Debug.Log("Mismatched sequence! Game Over.");
        isPlayerTurn = false;
        // Handle failure state (e.g., reset minigame, play buzzer sfx, etc.)

        StartMinigameRun();
        // Lock the cursor back to the center and hide it for FPS gameplay
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        // (Optional) Code to close your HUD panel goes here
    }

    void MiniGameWon()
    {
        Debug.Log("Sequence matched! Mini-game completed.");
        isPlayerTurn = false;
        // Handle success state (e.g., unlock door, give ammo, close HUD panel)

        // Lock the cursor back to the center and hide it for FPS gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayerInputHandler playerInput = FindAnyObjectByType<PlayerInputHandler>();
    
        if (playerInput != null)
        {
            playerInput.SetMinigameMode(false); // Locks mouse, re-enables guns
        }
        
        // (Optional) Code to unlock door / reward player / close HUD goes here
        gameObject.SetActive(false);
    }

    IEnumerator FlashButton(int buttonIndex, Color flashColor, float duration)
    {
        Button _currentButton = buttons[buttonIndex];
        Image _buttonImage = _currentButton.GetComponent<Image>();
        Color _originalColor = _buttonImage.color;

        // Change to a feedback color (e.g., Cyan, Yellow, or Blue) 
        // so it looks distinct from the game's green demonstration
        _buttonImage.color = flashColor; 

        // Quick flash duration
        yield return new WaitForSeconds(duration);

        // Revert back to original color
        _buttonImage.color = _originalColor;
    }
}