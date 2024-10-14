using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Component to hold a list of <see cref="InkEvent"/>s.
/// </summary>
public class InkEvents : MonoBehaviour
{
    #region Inspector

    [Tooltip("List of ink events that can be invoked from inside an ink file.")]
    [SerializeField] private List<InkEvent> inkEvents;

    #endregion

    #region Unity Event Functions

    private void OnEnable()
    {
        DialogueController.InkEvent += TryInvokeEvent;
    }

    private void OnDisable()
    {
        DialogueController.InkEvent -= TryInvokeEvent;
    }

    #endregion

    /// <summary>
    /// Try to find a <see cref="InkEvent"/> in <see cref="inkEvents"/> with the same name as the passed <paramref name="eventName"/> and invoke it if found.
    /// </summary>
    /// <param name="eventName">Name of the <see cref="InkEvent"/> to invoke.</param>
    private void TryInvokeEvent(string eventName)
    {
        foreach (InkEvent inkEvent in inkEvents)
        {
            if (inkEvent.name == eventName)
            {
                inkEvent.onEvent.Invoke();
                // Stop after finding the event. (Ignores duplicates)
                return;
            }
        }
    }
}

/// <summary>
/// A named <see cref="UnityEvent"/> invoked from inside ink dialogue.
/// </summary>
[Serializable]
public struct InkEvent
{
    [Tooltip("Name of the ink event.")]
    public string name;

    [Tooltip("Invoked when the ink event is invoked.")]
    public UnityEvent onEvent;
}
