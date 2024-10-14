using System;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Holds a list of <see cref="State"/> objects that represent the trackable state of the game.
/// </summary>
public class GameState : MonoBehaviour
{
    /// <summary>Invoked when any <see cref="State"/> in <see cref="states"/> was changed.</summary>
    public static event Action StateChanged;

    #region Inspector

    [Tooltip("A list of states representing the current trackable state of the game.")]
    [SerializeField] private List<State> states;

    #endregion

    /// <summary>
    /// Get the <see cref="State"/> with the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Id of the <see cref="State"/>.</param>
    /// <returns>The <see cref="State"/> with the given  <paramref name="id"/> if it exists; null otherwise.</returns>
    public State Get(string id)
    {
        foreach (State state in states)
        {
            if (state.id == id)
            {
                return state;
            }
        }

        return null;
    }

    /// <summary>
    /// Add a new <see cref="State"/> to the <see cref="GameState"/> or add a value to an existing <see cref="State"/>.
    /// </summary>
    /// <param name="id">Id of the <see cref="State"/> to create or modify.</param>
    /// <param name="amount">Amount to add to the <see cref="State"/>.</param>
    /// <param name="invokeEvent">If the <see cref="StateChanged"/> event should be invoked after changing adding the <see cref="State"/>.</param>
    public void Add(string id, int amount, bool invokeEvent = true)
    {
        // Safety checks to prevent wrong usage.
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogError("Id of state is empty. Make sure to give each state an id.", this);
            return;
        }

        if (amount == 0)
        {
            Debug.LogWarning($"Trying to add 0 to id '{id}'. This will result in no change to the state.", this);
            return;
        }

        // Search for the state in case it already exists.
        State state = Get(id);

        // If it does not exist, create a new state and add it to the GameState.
        if (state == null)
        {
            State newState = new State(id, amount);
            states.Add(newState);
        }
        else // Otherwise add the amount to the amount in the state which was found.
        {
            state.amount += amount;
        }

        if (invokeEvent)
        {
            StateChanged?.Invoke();
        }
    }

    /// <summary>
    /// Add a new <see cref="State"/> to the <see cref="GameState"/> or add a value to an existing <see cref="State"/>.
    /// </summary>
    /// <param name="state">The <see cref="State"/> to add.</param>
    /// <param name="invokeEvent">If the <see cref="StateChanged"/> event should be invoked after changing adding the <see cref="State"/>.</param>
    public void Add(State state, bool invokeEvent = true)
    {
        // Redirect to existing function.
        Add(state.id, state.amount, invokeEvent);
    }

    /// <summary>
    /// Add a a list of new <see cref="State"/> to the <see cref="GameState"/> or add values to existing <see cref="State"/>s.
    /// </summary>
    /// <param name="states">List of <see cref="State"/>s to add.</param>
    public void Add(List<State> states)
    {
        // Loop over list and redirect each to existing function.
        foreach (State state in states)
        {
            Add(state, false);
        }

        StateChanged?.Invoke();
    }

    /// <summary>
    /// Check <paramref name="conditions"/> against the game <see cref="states"/>. All conditions are implicitly AND connected.
    /// A condition passes if the value of the <see cref="State"/> in the game <see cref="states"/> is equal or higher than the value of the condition.
    /// </summary>
    /// <param name="conditions">List of conditions to check.</param>
    /// <returns>If all <paramref name="conditions"/> passed.</returns>
    public bool CheckConditions(List<State> conditions)
    {
        // Check each condition in the list of conditions.
        foreach (State condition in conditions)
        {
            // Get the state with the same id as the condition.
            State state = Get(condition.id);
            // Extract the value if the state exists; otherwise 0.
            int stateAmount = state != null ? state.amount : 0;
            // Compare the value of the state against the value of the condition.
            if (stateAmount < condition.amount)
            {
                // Return immediately false if any condition fails.
                // We do not need to check any conditions after that (Short-circuit).
                return false;
            }
        }

        // If we passed through the loop without ever returning false,
        // we reach the end of the function and can confidently return true.
        return true;
    }
}
