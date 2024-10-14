using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Collectable that can be picked up by the player. It registers itself into the <see cref="GameState"/> when picked up.
/// </summary>
public class Collectable : MonoBehaviour
{
    #region Inspector

    [Tooltip("Id of the collectable and the amount to be picked up.")]
    [SerializeField] private State state;

    [Tooltip("Invoked when the collectable is collected.")]
    [SerializeField] private UnityEvent onCollected;

    #endregion

    /// <summary>
    /// Collect this collectable which add its <see cref="state"/> to the <see cref="GameState"/> and destroys itself.
    /// </summary>
    public void Collect()
    {
        onCollected.Invoke();
        // Search for the GameState and add itself to it.
        FindObjectOfType<GameState>().Add(state);
        // Destroy ("despawn") self.
        Destroy(gameObject);
    }
}
