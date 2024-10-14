using System;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

using Ink.Runtime;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Responsible for the actual display of the dialogue & choices on the UI. Fills the correct text components and displays the correct dialogue buttons.
/// </summary>
public class DialogueBox : MonoBehaviour
{
    /// <summary>Invoked when the player wants to continue the dialogue.</summary>
    public static event Action<DialogueBox> DialogueContinued;

    /// <summary>Invoked when the player selected a choice.</summary>
    public static event Action<DialogueBox, int> ChoiceSelected;

    #region Inspector

    [Tooltip("Text component that displays the currently speaking actor.")]
    [SerializeField] private TextMeshProUGUI dialogueSpeaker;

    [Tooltip("Text component that contains the displayed dialogue lines.")]
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Tooltip("Button to continue the dialogue.")]
    [SerializeField] private Button continueButton;

    [Header("Choices")]

    [Tooltip("Container that holds buttons for each available choice.")]
    [SerializeField] private Transform choiceContainer;

    [Tooltip("Prefab for the choice buttons.")]
    [SerializeField] private Button choiceButtonPrefab;

    // TODO Add Tween settings for animation.

    #endregion

    /// <summary>Cached <see cref="RectTransform"/> of the object.</summary>
    private RectTransform rectTransform;

    /// <summary>Cached <see cref="CanvasGroup"/> of the object.</summary>
    private CanvasGroup canvasGroup;

    #region Unity Event Functions

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        // Invoke DialogueContinued event when continueButton on the UI is clicked.
        continueButton.onClick.AddListener(() =>
        {
            DialogueContinued?.Invoke(this);
        });
    }

    private void OnEnable()
    {
        // Make sure the text boxes are cleared.
        dialogueSpeaker.SetText(string.Empty);
        dialogueText.SetText(string.Empty);
        ClearChoices();
    }

    #endregion

    /// <summary>
    /// Shows the content of one <see cref="DialogueLine"/> in the appropriate areas on the UI.
    /// </summary>
    /// <param name="dialogueLine">Content to show in the UI.</param>
    public void DisplayText(DialogueLine dialogueLine)
    {
        // Do not update the speaker if new speaker is null.
        // Use null to retain the old speaker. Use "" to remove the speaker.
        if (dialogueLine.speaker != null)
        {
            // Get the speaker name from the line and set it on the UI.
            dialogueSpeaker.SetText(dialogueLine.speaker);
        }
        // Get the dialogue from the line and set it on the UI.
        dialogueText.SetText(dialogueLine.text);

        // Read out other information such as speaker images.

        DisplayButtons(dialogueLine.choices);
    }

    /// <summary>
    /// Display the correct buttons depending on the availability of <paramref name="choices"/>.
    /// </summary>
    /// <param name="choices"></param>
    private void DisplayButtons(List<Choice> choices)
    {
        Selectable newSelection;

        // If DialogueLine has no Choices show continueButton.
        if (choices == null || choices.Count == 0)
        {
            ShowContinueButton(true);
            ShowChoices(false);
            newSelection = continueButton;
        }
        else // Show the Choices
        {
            ClearChoices();
            List<Button> choiceButtons = GenerateChoices(choices);

            ShowContinueButton(false);
            ShowChoices(true);
            newSelection = choiceButtons[0];
        }

        // At the very end, tell the EventSystem to select newSelection in the UI.
        // Do this with a Coroutine for a slight delay. (needed)
        StartCoroutine(DelayedSelect(newSelection));
    }

    /// <summary>
    /// Clears all choices inside the <see cref="choiceContainer"/>.
    /// </summary>
    private void ClearChoices()
    {
        // Iterate over all child Transforms by putting the parent Transform selectionContainer in a foreach-loop.
        // This automatically iterates over all children.
        foreach (Transform child in choiceContainer)
        {
            // Destroy each child GameObject.
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Generate choice buttons based on the passed ink <paramref name="choices"/>.
    /// </summary>
    /// <param name="choices">List of ink <see cref="Choice"/>s.</param>
    /// <returns>Return a list of choice buttons.</returns>
    private List<Button> GenerateChoices(List<Choice> choices)
    {
        // Create a new list with pre-allocated space for each choice.
        List<Button> choiceButtons = new List<Button>(choices.Count);

        // Iterate over each Choice and generate a Button for each.
        for (int i = 0; i < choices.Count; i++)
        {
            Choice choice = choices[i];
            // Instantiate a new Button from the prefab and directly parent it to the choiceContainer.
            Button button = Instantiate(choiceButtonPrefab, choiceContainer);

            // Bind the button click to the invocation of the ChoiceSelected event.
            // Important to create copy of i for closure in the lambda below.
            // https://www.jetbrains.com/help/rider/AccessToModifiedClosure.html
            int index = i;
            button.onClick.AddListener(() => ChoiceSelected?.Invoke(this, index));

            // Get the child TextMeshPro component from the button and set the text from the selection.
            TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.SetText(choice.text);
            button.name = choice.text;

            // Add button to the list.
            choiceButtons.Add(button);
        }

        return choiceButtons;
    }

    /// <summary>
    /// Controls the display of the <see cref="continueButton"/>.
    /// </summary>
    /// <param name="show">Boolean to show or hide.</param>
    private void ShowContinueButton(bool show)
    {
        continueButton.gameObject.SetActive(show);
    }

    /// <summary>
    /// Controls the display of the <see cref="choiceContainer"/> that holds the choices.
    /// </summary>
    /// <param name="show">Boolean to show or hide.</param>
    private void ShowChoices(bool show)
    {
        choiceContainer.gameObject.SetActive(show);
    }

    /// <summary>
    /// Coroutine to select a given <see cref="GameObject"/> after one frame delay
    /// </summary>
    /// <param name="selectable">The <see cref="GameObject"/> to select.</param>
    /// <returns>The <see cref="Coroutine"/> <see cref="IEnumerator"/>.</returns>
    private IEnumerator DelayedSelect(Selectable selectable)
    {
        //yield return new WaitForFixedUpdate(); // Waits for next FixedUpdate() (1/50 sec)
        yield return null; // Waits for next Update() (next frame - 1/fps sec)
        // Select the GameObject on the UI through the EventSystem.
        selectable.Select();
    }

    #region Animations

    /// <summary>
    /// Show the <see cref="DialogueBox"/>, using <see cref="DOTween"/> moving and fading the <see cref="DialogueBox"/>.
    /// </summary>
    /// <returns><see cref="Sequence"/> <see cref="Tween"/> showing the <see cref="DialogueBox"/>.</returns>
    public Tween DOShow()
    {
        // Read out the height of the dialogue box.
        float height = rectTransform.rect.height;

        // Kill all tweens on the dialogue box that may be still running.
        this.DOKill();
        return DOTween.Sequence(this) // Pass this to set the dialogue box as the target of this sequence.
                      .Append(DOMove(Vector2.zero).From(new Vector2(0, -height)))
                      .Join(DOFade(1).From(0));
    }

    /// <summary>
    /// Hide the <see cref="DialogueBox"/>, using <see cref="DOTween"/> moving and fading the <see cref="DialogueBox"/>.
    /// </summary>
    /// <returns><see cref="Sequence"/> <see cref="Tween"/> hiding the <see cref="DialogueBox"/>.</returns>
    public Tween DOHide()
    {
        // Read out the height of the dialogue box.
        float height = rectTransform.rect.height;

        // Kill all tweens on the dialogue box that may be still running.
        this.DOKill();
        return DOTween.Sequence(this) // Pass this to set the dialogue box as the target of this sequence.
                      .Append(DOMove(new Vector2(0, -height)).From(Vector2.zero))
                      .Join(DOFade(0).From(1));
    }

    /// <summary>
    /// Move the <see cref="DialogueBox"/> to a <paramref name="targetPosition"/>.
    /// </summary>
    /// <param name="targetPosition">Anchored target position to move the <see cref="DialogueBox"/> to.</param>
    /// <returns><see cref="Tween"/> moving the <see cref="DialogueBox"/>.</returns>
    private TweenerCore<Vector2, Vector2, VectorOptions> DOMove(Vector2 targetPosition)
    {
        return rectTransform.DOAnchorPos(targetPosition, 0.75f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Fade the <see cref="DialogueBox"/> to a <see cref="targetAlpha"/>.
    /// </summary>
    /// <param name="targetAlpha">Target alpha value.</param>
    /// <returns><see cref="Tween"/> fading the <see cref="DialogueBox"/>.</returns>
    private TweenerCore<float, float, FloatOptions> DOFade(float targetAlpha)
    {
        return canvasGroup.DOFade(targetAlpha, 0.75f).SetEase(Ease.InOutSine);
    }

    #endregion
}
