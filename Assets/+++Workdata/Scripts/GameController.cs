using UnityEngine;

/// <summary>
/// Controls the overall state of the game, handles the activation/deactivation of the Player and UI.
/// </summary>
public class GameController : MonoBehaviour
{
    /// <summary>Reference to the player.</summary>
    private PlayerController player;

    /// <summary>Reference to the <see cref="DialogueController"/>.</summary>
    private DialogueController dialogueController;

    /// <summary>Reference to the <see cref="menuController"/>.</summary>
    private MenuController menuController;

    #region Unity Event Functions

    private void Awake()
    {
        // Find the first Component of type PlayerController in the scene. Null if none is found.
        player = FindCriticalComponent<PlayerController>();

        // Find the first Component of type DialogueController in the scene. Null if none is found.
        dialogueController = FindCriticalComponent<DialogueController>();

        // Find the first Component of type MenuController in the scene. Null if none is found.
        menuController = FindCriticalComponent<MenuController>();
    }

    private void OnEnable()
    {
        DialogueController.DialogueClosed += EndDialogue;

        MenuController.BaseMenuOpening += EnterPauseMode;
        MenuController.BaseMenuClosed += EnterPlayMode;
    }

    private void Start()
    {
        EnterPlayMode();
    }

    private void OnDisable()
    {
        DialogueController.DialogueClosed -= EndDialogue;

        MenuController.BaseMenuOpening -= EnterPauseMode;
        MenuController.BaseMenuClosed -= EnterPlayMode;
    }

    #endregion

    #region Modes

    public void EnterPlayMode()
    {
        Time.timeScale = 1;
        // Lock the cursor to the center of the screen & hide it.
        // In the editor: Unlock with ESC.
        Cursor.lockState = CursorLockMode.Locked;
        player.EnableInput();
        menuController.enabled = true;
    }

    public void EnterDialogueMode()
    {
        Time.timeScale = 1;
        // Hide cursor in dialogue.
        Cursor.lockState = CursorLockMode.Locked;
        player.DisableInput();
        menuController.enabled = false;
    }

    public void EnterCutsceneMode()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        player.DisableInput();
        menuController.enabled = false;
    }

    public void EnterPauseMode()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        player.DisableInput();
        menuController.enabled = true;
    }

    #endregion

    /// <summary>
    /// Starts a new dialogue and displays it on the UI.
    /// </summary>
    /// <param name="dialoguePath">Path to a specified knot.stitch in the ink files.</param>
    public void StartDialogue(string dialoguePath)
    {
        EnterDialogueMode();
        // Pass the dialoguePath.
        dialogueController.StartDialogue(dialoguePath);
    }

    /// <summary>
    /// Function to subscribe to the end of a dialogue. Starts the player mode again.
    /// </summary>
    private void EndDialogue()
    {
        EnterPlayMode();
    }

    private T FindCriticalComponent<T>() where T : Component
    {
        // Find the first Component of type T in the scene. Null if none is found.
        T component = FindObjectOfType<T>();

        // Log an error if no component is found.
        if (component == null)
        {
            Debug.LogError($"No {typeof(T).Name} found in scene.", this);
        }

        return component;
    }
}
