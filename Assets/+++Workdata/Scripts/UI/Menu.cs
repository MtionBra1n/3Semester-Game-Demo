using System.Collections;

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents a menu that can be opened & closed.
/// </summary>
public class Menu : MonoBehaviour
{
    #region Inspector

    [Tooltip("Selectable to be selected when the menu is opened.")]
    [SerializeField] private Selectable selectOnOpen;

    [Tooltip("Remember the Selectable that was selected when the menu was opened and reselect it once the menu is closed.")]
    [SerializeField] private bool selectPreviousOnClose = true;

    [Tooltip("Hide the menu when the game starts.")]
    [SerializeField] private bool disableOnAwake = true;

    #endregion

    /// <summary>Cached <see cref="RectTransform"/> of the object.</summary>
    private RectTransform rectTransform;

    /// <summary>Cached <see cref="CanvasGroup"/> of the object.</summary>
    private CanvasGroup canvasGroup;

    /// <summary><see cref="Selectable"/> that was previously selected and will be selected again when the <see cref="Menu"/> is closed if <see cref="selectPreviousOnClose"/> is <c>true</c>.</summary>
    private Selectable selectOnClose;

    #region Unity Event Functions

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (disableOnAwake)
        {
            gameObject.SetActive(false);
        }
        else
        {
            // Instantly open menu.
            Open().Complete(true);
        }
    }

    #endregion

    /// <summary>
    /// Open the <see cref="Menu"/>.
    /// </summary>
    public Tween Open()
    {
        gameObject.SetActive(true);

        // Save the previous selection.
        if (selectPreviousOnClose)
        {
            // Get the currently selected GameObject.
            GameObject previousSelection = EventSystem.current.currentSelectedGameObject;
            if (previousSelection != null)
            {
                // Search for the actual Selectable component on the GameObject.
                selectOnClose = previousSelection.GetComponent<Selectable>();
            }
        }

        // Coroutine only necessary for select animation.
        // Select UI event is not called if it was enabled in the same frame.
        StartCoroutine(DelayedSelect(selectOnOpen));

        // Read out the height of the dialogue box.
        float height = rectTransform.rect.height;

        // Kill all tweens on the dialogue box that may be still running.
        this.DOKill();
        return DOTween.Sequence(this) // Pass this to set the dialogue box as the target of this sequence.
                      .Append(DOMove(Vector2.zero).From(new Vector2(0, -height)))
                      .Join(DOFade(1).From(0))
                      .SetUpdate(true) // Needs to be independent of timescale for the pause menu.
                      .Play();
    }

    /// <summary>
    /// Clos the <see cref="Menu"/>.
    /// </summary>
    public Tween Close()
    {
        // Select the saved Selectable again.
        if (selectPreviousOnClose && selectOnClose != null)
        {
            selectOnClose.StartCoroutine(DelayedSelect(selectOnClose));
        }

        // Read out the height of the dialogue box.
        float height = rectTransform.rect.height;

        // Kill all tweens on the dialogue box that may be still running.
        this.DOKill();
        return DOTween.Sequence(this) // Pass this to set the dialogue box as the target of this sequence.
                      .Append(DOMove(new Vector2(0, -height)).From(Vector2.zero))
                      .Join(DOFade(0).From(1))
                      .AppendCallback(() =>
                      {
                          gameObject.SetActive(false);
                      })
                      .SetUpdate(true) // Needs to be independent of timescale for the pause menu.
                      .Play();
    }

    /// <summary>
    /// Show the previously hidden <see cref="Menu"/>.
    /// </summary>
    public Tween Show()
    {
        return DOFade(1).SetUpdate(true);
    }

    /// <summary>
    /// Hide the <see cref="Menu"/>.
    /// </summary>
    public Tween Hide()
    {
        return DOFade(0).SetUpdate(true);
    }

    /// <summary>
    /// Move the <see cref="Menu"/> to a <paramref name="targetPosition"/>.
    /// </summary>
    /// <param name="targetPosition">Anchored target position to move the <see cref="Menu"/> to.</param>
    /// <returns><see cref="Tween"/> moving the <see cref="Menu"/>.</returns>
    private TweenerCore<Vector2, Vector2, VectorOptions> DOMove(Vector2 targetPosition)
    {
        return rectTransform.DOAnchorPos(targetPosition, 0.75f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Fade the <see cref="Menu"/> to a <see cref="targetAlpha"/>.
    /// </summary>
    /// <param name="targetAlpha">Target alpha value.</param>
    /// <returns><see cref="Tween"/> fading the <see cref="Menu"/>.</returns>
    private TweenerCore<float, float, FloatOptions> DOFade(float targetAlpha)
    {
        return canvasGroup.DOFade(targetAlpha, 0.75f).SetEase(Ease.InOutSine);
    }

    /// <summary>
    /// Coroutine that waits one frame before selecting.
    /// </summary>
    /// <param name="newSelection">The <see cref="Selectable"/> to select.</param>
    /// <returns>The <see cref="Coroutine"/> <see cref="IEnumerator"/>.</returns>
    private IEnumerator DelayedSelect(Selectable newSelection)
    {
        // Wait a frame.
        yield return null;
        Select(newSelection);
    }

    /// <summary>
    /// Safely select a <see cref="Selectable"/>.
    /// </summary>
    /// <param name="newSelection">The <see cref="Selectable"/> to select.</param>
    private void Select(Selectable newSelection)
    {
        if (newSelection == null) { return; }
        newSelection.Select();
    }
}
