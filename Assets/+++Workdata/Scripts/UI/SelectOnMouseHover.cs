using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Component to automatically select a selectable if the mouse hovers over it.
/// This will bypass the hover state of the Unity UI.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Selectable))]
public class SelectOnMouseHover : MonoBehaviour, IPointerEnterHandler, IDeselectHandler
{
    /// <summary>Cached <see cref="Selectable"/> on the <see cref="GameObject"/>.</summary>
    private Selectable selectable;

    #region Unity Event Functions

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Do nothing if the Selectable is not interactable (greyed out).
        if (!selectable.interactable) { return; }

        selectable.Select();
    }

    // Handling for deselect is necessary when moving the selection with the keyboard/controller while the mouse is still over the button.
    public void OnDeselect(BaseEventData eventData)
    {
        // Do nothing if the Selectable is not interactable (greyed out).
        if (!selectable.interactable) { return; }

        // Communicate to the selectable that the pointer has left the selectable
        // so it is ignored until the next OnPointerEnter().
        selectable.OnPointerExit(null);
    }

    #endregion
}
