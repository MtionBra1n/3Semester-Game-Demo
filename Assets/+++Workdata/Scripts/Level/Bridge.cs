using DG.Tweening;

using UnityEngine;

/// <summary>
/// Bridge that can extend and retract its platform.
/// </summary>
public class Bridge : MonoBehaviour
{
    #region Inspector

    [Tooltip("The platform that will move when extending/retracting.")]
    [SerializeField] private Transform platform;

    [Tooltip("Local position of the platform in the retracted state.")]
    [SerializeField] private Vector3 retractedPosition;

    [Tooltip("Local position of the platform in the extended state.")]
    [SerializeField] private Vector3 extendedPosition;

    [Tooltip("If starting in the extended state.")]
    [SerializeField] private bool startExtended;

    [Header("Animation")]

    [Tooltip("Extension/Retraction time in seconds.")]
    [Min(0)]
    [SerializeField] private float moveDuration = 1f;

    [Tooltip("Ease of the platform movement.")]
    [SerializeField] private Ease ease = DOTween.defaultEaseType;

    #endregion

    /// <summary>State of the bridge.</summary>
    private bool isExtended;

    #region Unity Event Functions

    private void Awake()
    {
        // Set bridge to correct state on awake.
        isExtended = startExtended;
        platform.localPosition = startExtended ? extendedPosition : retractedPosition;
    }

    #endregion

    /// <summary>
    /// Toggle the state of the bridge, between extended and retracted.
    /// </summary>
    public void Toggle()
    {
        if (isExtended)
        {
            Retract();
        }
        else
        {
            Extend();
        }
    }

    /// <summary>
    /// Extend the bridge.
    /// </summary>
    public void Extend()
    {
        isExtended = true;
        MovePlatform(extendedPosition);
    }

    /// <summary>
    /// Retract the bridge.
    /// </summary>
    public void Retract()
    {
        isExtended = false;
        MovePlatform(retractedPosition);
    }

    /// <summary>
    /// Move the <see cref="platform"/> of the bridge to the <paramref name="targetPosition"/>.
    /// </summary>
    /// <param name="targetPosition">Position to move the <see cref="platform"/> to.</param>
    private void MovePlatform(Vector3 targetPosition)
    {
        float speed = (retractedPosition - extendedPosition).magnitude / moveDuration;

        platform.DOKill();
        platform.DOLocalMove(targetPosition, speed)
                .SetSpeedBased()
                .SetEase(ease);
    }
}
