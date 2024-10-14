using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Models a specific interaction put on <see cref="Interactable"/>s that will be executed if active and the <see cref="Interactable"/> is interacted with.
/// </summary>
public class Interaction : MonoBehaviour
{
    #region Inspector

    [Tooltip("Invoked when the Interactable executes this interaction.")]
    [SerializeField] private UnityEvent onInteracted;

    [Tooltip("Next Interaction to be activated once this Interaction was executed. It will be executed the next time the player interacts with this Interactable.")]
    [SerializeField] private Interaction nextInteraction;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        // Deactivate itself to be activated later.
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // Get all sibling Interactions by finding all child Interactions on the parent.
        List<Interaction> interactions = transform.parent.GetComponentsInChildren<Interaction>().ToList();

        // Deactivate all siblings.
        foreach (Interaction interaction in interactions)
        {
            // Skip self.
            if (interaction == this) { continue; }

            interaction.gameObject.SetActive(false);
        }
    }

    #endregion

    /// <summary>
    /// Execute the <see cref="Interaction"/>.
    /// </summary>
    public void Execute()
    {
        // If nextInteraction is set, activate it for the next Interact action by the player.
        if (nextInteraction != null)
        {
            nextInteraction.gameObject.SetActive(true);
        }

        // Has to be invoked after activating nextInteraction because we are potentially
        // starting a quest which could activate "Quest-Delivering"-Interactions,
        // that need to override the activation of any other Interaction.
        onInteracted.Invoke();
    }
}
