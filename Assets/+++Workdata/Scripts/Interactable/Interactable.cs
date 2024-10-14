using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Marks a <see cref="GameObject"/> as being interactable and offers events when being interacted on by the player.
/// </summary>
public class Interactable : MonoBehaviour
{
    #region Inspector

    [Tooltip("Invoked when the player interacts with the Interactable.")]
    [SerializeField] private UnityEvent onInteracted;

    [Tooltip("Invoked when the player selects this Interactable, and they are able to interact with it.")]
    [SerializeField] private UnityEvent onSelected;

    [Tooltip("Invoked when the player deselects this Interactable, and they stop being able to interact with it.")]
    [SerializeField] private UnityEvent onDeselected;

    #endregion

    #region Unity Event Functions

    private void Start()
    {
        // Get all Interactions on the child GameObjects. This also includes all inactive ones.
        List<Interaction> interactions = GetComponentsInChildren<Interaction>(true).ToList(); // Convert array to list.

        // If at least one Interaction exists, activate the first one.
        if (interactions.Count > 0)
        {
            // Index 0 is the first one!
            // Activate the Interaction to be executed once this Interactable is interacted upon.
            interactions[0].gameObject.SetActive(true);
        }
    }

    #endregion

    /// <summary>
    /// Interact with the <see cref="Interactable"/>. Also execute the first active <see cref="Interaction"/> on this <see cref="Interactable"/>.
    /// </summary>
    public void Interact()
    {
        Interaction interaction = FindActiveInteraction();

        // If an active Interaction is found, execute it.
        if (interaction != null)
        {
            interaction.Execute();
        }

        onInteracted.Invoke();
    }

    /// <summary>
    /// Notify that the <see cref="Interactable"/> has been selected.
    /// </summary>
    public void Select()
    {
        onSelected.Invoke();
    }

    /// <summary>
    /// Notify that the <see cref="Interactable"/> has been deselected.
    /// </summary>
    public void Deselect()
    {
        onDeselected.Invoke();
    }

    /// <summary>
    /// Look for the first active <see cref="Interaction"/> inside the <see cref="Interactable"/>'s <see cref="GameObject"/>.
    /// </summary>
    /// <returns>The first <see cref="Interaction"/>. <c>null</c> if none found.</returns>
    private Interaction FindActiveInteraction()
    {
        return GetComponentInChildren<Interaction>(false);
    }
}
