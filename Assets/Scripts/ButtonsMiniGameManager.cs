using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.FPS.Gameplay;
using UnityEngine.InputSystem;
using System.Linq;
using DG.Tweening;

public class ButtonsMiniGameManager : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private int sequenceSize = 5;
    [SerializeField] private float flashDuration = 1.0f;
    [SerializeField] private float delayBetweenButtons = 0.2f;
    [SerializeField] private float minigameStartDelay = 1.0f;

    [SerializeField] private Sprite greenSprite;
    [SerializeField] private Sprite blueSprite;
    [SerializeField] private Sprite redSprite;
    [SerializeField] private Sprite defaultSprite;

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
            StartCoroutine(FlashButton(buttonIndex, Color.red, flashDuration));
            MiniGameFailed();
            return;
        }
        else StartCoroutine(FlashButton(buttonIndex, Color.blue, flashDuration));

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
        Transform _buttonTransform = _currentButton.transform;
        
        Vector3 _originalScale = _buttonTransform.localScale;

        // 1. Figure out which sprite to use based on the color passed in
        Sprite targetSprite = defaultSprite; // Fallback default

        switch (flashColor)
        {
            case Color c when c == Color.green:
                targetSprite = greenSprite;
                break;
            case Color c when c == Color.red:
                targetSprite = redSprite;
                break;
            case Color c when c == Color.blue:
                targetSprite = blueSprite;
                break;
            default:
                // If it's a weird custom color, just default to green or keep the current one
                targetSprite = greenSprite; 
                break;
        }

        // Safety reset for the scale tween
        _buttonTransform.DOKill();

        float pushTime = duration * 0.25f;
        float releaseTime = duration * 0.75f;

        // 2. THE CLICK DOWN & SPRITE SWAP
        _buttonImage.sprite = targetSprite; 
        _buttonTransform.DOScale(_originalScale * 0.85f, pushTime).SetEase(Ease.OutQuad);

        yield return new WaitForSeconds(pushTime);

        // 3. THE RECOVERY BOUNCE (Starts expanding back up, but KEEPS the colored sprite)
        _buttonTransform.DOScale(_originalScale, releaseTime).SetEase(Ease.OutBack);

        // Wait for the expansion animation to completely finish playing...
        yield return new WaitForSeconds(releaseTime);

        // 4. RESET TO DEFAULT (Now that the movement is done, swap back to normal)
        _buttonImage.sprite = defaultSprite;
    }
}