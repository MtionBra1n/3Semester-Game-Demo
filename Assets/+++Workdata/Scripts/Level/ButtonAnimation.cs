using DG.Tweening;

using UnityEngine;

/// <summary>
/// Manages a tween animation for a button press.
/// </summary>
public class ButtonAnimation : MonoBehaviour
{
    #region Inspector

    [Tooltip("Distance to locally move the button during the press animation, in uu.")]
    [SerializeField] private float yMovement = -0.049f;

    [Tooltip("Color to change the button to while pressing down.")]
    [SerializeField] private Color pressColor = Color.yellow;

    [Tooltip("Time in sec to hold down the button before releasing.")]
    [Min(0)]
    [SerializeField] private float downDuration = 0.3f;

    [Header("In")]

    [Tooltip("Ease of the press animation.")]
    [SerializeField] private Ease easeIn = Ease.InSine;

    [Tooltip("Duration in sec of the press animation.")]
    [Min(0)]
    [SerializeField] private float durationIn = 0.3f;

    [Header("Out")]

    [Tooltip("Ease of the release animation.")]
    [SerializeField] private Ease easeOut = Ease.OutElastic;

    [Tooltip("Duration in sec of the release animation.")]
    [Min(0)]
    [SerializeField] private float durationOut = 0.5f;

    #endregion

    /// <summary>Cached <see cref="MeshRenderer"/> of the button.</summary>
    private MeshRenderer meshRenderer;

    /// <summary>Original <see cref="Color"/> of the button <see cref="Material"/>.</summary>
    private Color originalColor;

    /// <summary><see cref="Sequence"/> of the animation.</summary>
    private Sequence sequence;

    #region Unity Event Functions

    private void Awake()
    {
        // Cache MeshRenderer.
        meshRenderer = GetComponent<MeshRenderer>();
        // Save original color to restore it later.
        originalColor = meshRenderer.material.color;
    }

    #endregion

    /// <summary>
    /// Play the button animation.
    /// </summary>
    public void PlayAnimation()
    {
        // Complete previous sequence that is potentially still running.
        sequence.Complete(true);

        // Create an empty sequence.
        sequence = DOTween.Sequence();

                // Press down
        sequence.Append(transform.DOLocalMoveY(yMovement, durationIn).SetRelative().SetEase(easeIn))
                .Join(meshRenderer.material.DOColor(pressColor, durationIn).SetEase(Ease.Linear))
                // Wait
                .AppendInterval(downDuration)
                // Release
                .Append(transform.DOLocalMoveY(-yMovement, durationOut).SetRelative().SetEase(easeOut))
                .Join(meshRenderer.material.DOColor(originalColor, durationOut).SetEase(Ease.Linear));

        // Not needed because autoplay is on by default.
        sequence.Play();
    }
}
