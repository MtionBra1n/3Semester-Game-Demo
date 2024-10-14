using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Simple component to modify the <see cref="GameState"/>.
/// </summary>
public class StateUpdater : MonoBehaviour
{
    #region Inspector

    [Tooltip("List of states to be added to the GameState when calling UpdateStates().")]
    [SerializeField] private List<State> stateUpdates;

    #endregion

    /// <summary>
    /// Add the <see cref="stateUpdates"/> to the <see cref="GameState"/>.
    /// </summary>
    public void UpdateStates()
    {
        // Find the GameState and add all stateUpdates to it.
        FindObjectOfType<GameState>().Add(stateUpdates);
    }
}
