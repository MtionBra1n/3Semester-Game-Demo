using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Holds a list of conditions and reacts once these conditions are fulfilled by the <see cref="gameState"/>.
/// </summary>
public class Reactor : MonoBehaviour
{
    #region Inspector

    [Tooltip("AND connected conditions that all need to be fulfilled.")]
    [SerializeField] private List<State> conditions;

    [Tooltip("Invoked when all the conditions become fulfilled.")]
    [SerializeField] private UnityEvent onFulfilled;

    [Tooltip("Invoked when any of the conditions return to being unfulfilled.")]
    [SerializeField] private UnityEvent onUnfulfilled;

    [Tooltip("Optional field to reference a QuestEntry, if this reactor represents a quest.")]
    [SerializeField] private QuestEntry questEntry;

    #endregion

    /// <summary>State of the <see cref="Reactor"/>. Used to detect difference once the <see cref="gameState"/> changes.</summary>
    private bool fulfilled = false;

    /// <summary>Cached <see cref="GameState"/> for checking the <see cref="conditions"/> against.</summary>
    private GameState gameState;

    #region Unity Event Functions

    private void Awake()
    {
        // Cache the GameState as it will be accessed frequently.
        gameState = FindObjectOfType<GameState>();
    }

    private void OnEnable()
    {
        if (questEntry != null)
        {
            questEntry.gameObject.SetActive(true);
            // Make sure the quest is not fulfilled when the activated.
            questEntry.SetQuestStatus(false);
        }
        // Manually check conditions when the reactor is enabled, in case the conditions are already true on activation.
        CheckConditions();
        GameState.StateChanged += CheckConditions;
    }

    private void OnDisable()
    {
        if (questEntry != null)
        {
            questEntry.gameObject.SetActive(false);
        }
        GameState.StateChanged -= CheckConditions;
    }

    #endregion

    /// <summary>
    /// Check the <see cref="conditions"/> against the <see cref="GameState"/> and perform the appropriate <see cref="UnityEvent"/> when the result changes.
    /// </summary>
    private void CheckConditions()
    {
        // Check the conditions against the gameState and save the result locally if the conditions are fulfilled.
        bool newFulfilled = gameState.CheckConditions(conditions);

        // Compare the locally saved result against the previous to detect a change
        // From false -> true
        if (!fulfilled && newFulfilled)
        {
            if (questEntry != null)
            {
                questEntry.SetQuestStatus(true);
            }
            onFulfilled.Invoke();
        }
        // From true -> false
        else if (fulfilled && !newFulfilled)
        {
            if (questEntry != null)
            {
                questEntry.SetQuestStatus(false);
            }
            onUnfulfilled.Invoke();
        }

        // Overwrite the old result with the new for the next invocation of this function.
        fulfilled = newFulfilled;
    }
}
