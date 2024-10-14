using System;

using UnityEngine;

/// <summary>
/// Represents a single state that has a defined name (<see cref="id"/>) and value (<see cref="amount"/>).
/// </summary>
[Serializable]
public class State
{
    [Tooltip("The id of the state. Used to identify the state.")]
    public string id;

    [Tooltip("The value of the state.")]
    public int amount;

    /// <summary>
    /// Constructor that creates a new <see cref="State"/> object with the passed <paramref name="id"/> and <paramref name="amount"/> set directly.
    /// </summary>
    /// <param name="id">The id of the state.</param>
    /// <param name="amount">The value of the state.</param>
    public State(string id, int amount)
    {
        this.id = id;
        this.amount = amount;
    }
}
