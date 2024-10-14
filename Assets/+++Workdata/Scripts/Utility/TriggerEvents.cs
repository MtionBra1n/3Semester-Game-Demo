using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// General purpose component to offer UnityEvents in the inspector for OnTriggerEnter() and OnTriggerExit().
/// </summary>
public class TriggerEvents : MonoBehaviour
{
    private const string NoTag = "Untagged";
    private const string PlayerTag = "Player";

    #region Inspector

    [Tooltip("Invoked when OnTriggerEnter() is called.")]
    [SerializeField] private UnityEvent<Collider> onTriggerEnter;

    [Tooltip("Invoked when OnTriggerExit() is called.")]
    [SerializeField] private UnityEvent<Collider> onTriggerExit;

    [Tooltip("Enable to filter the interacting collider by a specified tag.")]
    [SerializeField] private bool filterOnTag = true;

    [Tooltip("Tag of the interacting Collider to filter on.")]
    [SerializeField] private string reactOn = PlayerTag;

    [Header("Advanced")]

    [Tooltip("Treat overlapping triggers as one, by only executing the UnityEvents on first enter/last exit.")]
    [SerializeField] private bool combineTriggers = true;

    #endregion

    private int triggerCount = 0;

    #region Unity Event Functions

    /// <summary>
    /// Called when a value in the inspector is changed.
    /// </summary>
    private void OnValidate()
    {
        // Replaces an 'empty' reactOn field with "Untagged".
        if (string.IsNullOrWhiteSpace(reactOn))
        {
            reactOn = NoTag;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Return if we are filtering by tag and the tag doesn't match reactOn.
        if (filterOnTag && !other.CompareTag(reactOn)) { return; }

        // Count up the times we entered a trigger.
        triggerCount++;

        // Try to fix wacky triggers if the counter got out of sync.
        if (triggerCount < 1)
        {
            triggerCount = 1;
        }

        // Stop early if we are not entering the first trigger.
        if (combineTriggers && triggerCount != 1) { return; }

        onTriggerEnter.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        // Return if we are filtering by tag and the tag doesn't match reactOn.
        if (filterOnTag && !other.CompareTag(reactOn)) { return; }

        // Count down once we exit a trigger.
        triggerCount--;

        // Try to fix wacky triggers if the counter got out of sync.
        if (triggerCount < 0)
        {
            triggerCount = 0;
        }

        // Stop early if we are not exiting the last trigger.
        if (combineTriggers && triggerCount != 0) { return; }

        onTriggerExit.Invoke(other);
    }

    #endregion
}
