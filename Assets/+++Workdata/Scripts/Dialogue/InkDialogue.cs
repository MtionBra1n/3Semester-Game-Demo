using UnityEngine;

/// <summary>
/// Component to initiate a dialogue, displayed on the UI.
/// </summary>
public class InkDialogue : MonoBehaviour
{
    #region Inspector

    [Tooltip("Path to a specified knot.stitch in the ink file.")]
    [SerializeField] private string dialoguePath;

    #endregion

    /// <summary>
    /// Start a dialogue on the dialogue UI at the knot.stitch specified in <see cref="dialoguePath"/>.
    /// </summary>
    public void StartDialogue()
    {
        // Abort if dialoguePath is not filled.
        if (string.IsNullOrWhiteSpace(dialoguePath))
        {
            Debug.LogWarning("No dialogue path defined.", this);
            return;
        }

        // Search for the GameController and start the dialogue.
        FindObjectOfType<GameController>().StartDialogue(dialoguePath);
    }
}
