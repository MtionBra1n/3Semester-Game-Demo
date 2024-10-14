using System;
using System.Collections.Generic;
using System.Linq;

using DG.Tweening;

using Ink;
using Ink.Runtime;
#if UNITY_EDITOR
using Ink.UnityIntegration;
#endif

using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages both the DialogueUI as well as flow of dialogue through an Ink <see cref="Ink.Runtime.Story"/> object.
/// </summary>
public class DialogueController : MonoBehaviour
{
    private const string SpeakerSeparator = ":";
    private const string EscapedColon = "::";
    private const string EscapedColonPlaceholder = "§";

    /// <summary>Invoked when the Dialogue UI opens.</summary>
    public static event Action DialogueOpened;

    /// <summary>Invoked when the Dialogue UI closes.</summary>
    public static event Action DialogueClosed;

    /// <summary>Generic Ink event supplying an identifier.</summary>
    public static event Action<string> InkEvent;

    #region Inspector

    [Header("Ink")]

    [Tooltip("Compiled ink text asset.")]
    [SerializeField] private TextAsset inkAsset;

    [Header("UI")]

    [Tooltip("DialogueBox to display the dialogue in.")]
    [SerializeField] private DialogueBox dialogueBox;

    #endregion

    /// <summary>Cached reference to the GameState.</summary>
    private GameState gameState;

    /// <summary>Ink story created out of the compiled inkAsset.</summary>
    private Story inkStory;

    #region Unity Event Functions

    private void Awake()
    {
        // Search for the GameState and cache it.
        gameState = FindObjectOfType<GameState>();

        // Initialize Ink.
        inkStory = new Story(inkAsset.text);
        // Add error handling.
        inkStory.onError += OnInkError;
        // Connect an ink function to a C# function.
        inkStory.BindExternalFunction<string>("Event", Event);
        inkStory.BindExternalFunction<string>("Get_State", Get_State);
        inkStory.BindExternalFunction<string, int>("Add_State", Add_State);
#if UNITY_EDITOR
        // Link the playing inkStory to the Ink Window in the Unity Editor for debugging.
        InkPlayerWindow.Attach(inkStory);
#endif
    }

    private void OnEnable()
    {
        DialogueBox.DialogueContinued += OnDialogueContinued;
        DialogueBox.ChoiceSelected += OnChoiceSelected;
    }

    private void Start()
    {
        dialogueBox.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        DialogueBox.DialogueContinued -= OnDialogueContinued;
        DialogueBox.ChoiceSelected -= OnChoiceSelected;
    }

    private void OnDestroy()
    {
        inkStory.onError -= OnInkError;
    }

    #endregion

    #region Dialogue Lifecycle

    /// <summary>
    /// Start a new Dialogue, jumping to the specified knot.stitch in the ink files.
    /// </summary>
    /// <param name="dialoguePath">Path to a specified knot.stitch in the ink files.</param>
    public void StartDialogue(string dialoguePath)
    {
        OpenDialogue();

        // Like '-> knot' in ink.
        inkStory.ChoosePathString(dialoguePath);
        ContinueDialogue();
    }

    /// <summary>
    /// Show the dialogue UI.
    /// </summary>
    private void OpenDialogue()
    {
        dialogueBox.gameObject.SetActive(true);
        dialogueBox.DOShow();

        DialogueOpened?.Invoke();
    }

    /// <summary>
    /// Hide the dialogue UI and clean up.
    /// </summary>
    private void CloseDialogue()
    {
        // Deselect everything in the UI.
        EventSystem.current.SetSelectedGameObject(null);
        dialogueBox.DOHide()
                   .OnComplete(() =>
                   {
                       dialogueBox.gameObject.SetActive(false);
                   });

        DialogueClosed?.Invoke();
    }

    /// <summary>
    /// Advance the <see cref="inkStory"/>, showing the next line of text and <see cref="Choice"/>s if available.
    /// Automatically closes the dialog once the end of the <see cref="inkStory"/> is reached.
    /// </summary>
    private void ContinueDialogue()
    {
        // First check if we even can continue the dialogue.
        if (IsAtEnd())
        {
            CloseDialogue();
            return;
        }

        // Then check if we can just advance the dialogue or if we hit choices that prevents us from doing so.
        DialogueLine line;
        if (CanContinue())
        {
            // Advance the dialogue and get the next line of text.
            string inkLine = inkStory.Continue();
            // Skip empty lines.
            if (string.IsNullOrWhiteSpace(inkLine))
            {
                ContinueDialogue();
                return;
            }
            // Pass the raw inkLine for parsing into a DialogueLine.
            line = ParseText(inkLine, inkStory.currentTags);
        }
        else
        {
            // Create empty DialogueLine with just choices added below if we can't continue.
            line = new DialogueLine();
        }

        // Save the current choices into the dialogue line.
        line.choices = inkStory.currentChoices;

        dialogueBox.DisplayText(line);
    }

    /// <summary>
    /// Select <see cref="Choice"/> in the <see cref="inkStory"/> and continue.
    /// </summary>
    /// <param name="choiceIndex">The index of the choice chosen.</param>
    private void SelectChoice(int choiceIndex)
    {
        inkStory.ChooseChoiceIndex(choiceIndex);
        ContinueDialogue();
    }

    /// <summary>
    /// Function to subscribe to the <see cref="DialogueBox.DialogueContinued"/> event.
    /// Is called when we want to show the next <see cref="DialogueLine"/>.
    /// </summary>
    private void OnDialogueContinued(DialogueBox _)
    {
        ContinueDialogue();
    }

    /// <summary>
    /// Function to subscribe to the <see cref="DialogueBox.ChoiceSelected"/> event.
    /// Is called when a choice was selected and then continue the dialogue.
    /// </summary>
    private void OnChoiceSelected(DialogueBox _, int choiceIndex)
    {
        SelectChoice(choiceIndex);
    }

    #endregion

    #region Ink

    /// <summary>
    /// Parse the raw ink <see cref="string"/> into a <see cref="DialogueLine"/> with information extracted.
    /// </summary>
    /// <param name="inkLine">The raw ink <see cref="string"/>.</param>
    /// <param name="tags">List of tags for the current <paramref name="inkLine"/>.</param>
    /// <returns>New <see cref="DialogueLine"/> containing the extracted information.</returns>
    private DialogueLine ParseText(string inkLine, List<string> tags)
    {
        // Replace :: with § as a placeholder to prevent splitting.
        inkLine = inkLine.Replace(EscapedColon, EscapedColonPlaceholder);

        // Split string into parts only at the unescaped : that remain.
        List<string> parts = inkLine.Split(SpeakerSeparator).ToList();

        string speaker;
        string text;

        // Separate the string parts into their respective functions.
        switch (parts.Count)
        {
            case 1:
                speaker = null;
                text = parts[0];
                break;
            case 2:
                speaker = parts[0];
                text = parts[1];
                break;
            default:
                Debug.LogWarning($@"Ink dialogue line was split at more {SpeakerSeparator} than expected. Please make sure to use {EscapedColon} for {SpeakerSeparator} inside text.");
                goto case 2;
        }

        DialogueLine line = new DialogueLine();
        // Trim whitespaces on both ends.
        line.speaker = speaker?.Trim();
        // Replace § back to : for display on the UI.
        line.text = text.Trim().Replace(EscapedColonPlaceholder, SpeakerSeparator);

        // Look at tags to add additional information to a DialogueLine.
        if (tags.Contains("thought"))
        {
            line.text = $"<i>{line.text}</i>";
        }

        return line;
    }

    /// <summary>
    /// Check if the <see cref="inkStory"/> can be executed further.
    /// </summary>
    /// <returns>Returns <c>true</c> if the <see cref="inkStory"/> can be executed further.</returns>
    private bool CanContinue()
    {
        return inkStory.canContinue;
    }

    /// <summary>
    /// Check if the <see cref="inkStory"/> execution reached <see cref="Choice"/>s that can be displayed.
    /// </summary>
    /// <returns>Returns <c>true</c> if the <see cref="inkStory"/> execution reached <see cref="Choice"/>s.</returns>
    private bool HasChoices()
    {
        return inkStory.currentChoices.Count > 0;
    }

    /// <summary>
    /// Check if the <see cref="inkStory"/> execution reached it's end and can proceed no further.
    /// </summary>
    /// <returns>Returns <c>true</c> if the <see cref="inkStory"/> execution reached it's end.</returns>
    /// <remarks>
    /// The <see cref="inkStory"/> execution reached it's end when it can not continue further and has no <see cref="Choice"/>s to display.
    /// </remarks>
    private bool IsAtEnd()
    {
        return !CanContinue() && !HasChoices();
    }

    /// <summary>
    /// Function for error handling any errors from the <see cref="inkStory"/>.
    /// </summary>
    /// <param name="message">Message of the error.</param>
    /// <param name="type">Type of the error.</param>
    private void OnInkError(string message, ErrorType type)
    {
        switch (type)
        {
            case ErrorType.Author:
                break;
            case ErrorType.Warning:
                Debug.LogWarning(message);
                break;
            case ErrorType.Error:
                Debug.LogError(message);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

    /// <summary>
    /// Invoke an "Ink Event"
    /// </summary>
    /// <remarks>
    /// This function is bound to the function in Ink of the same name.
    /// </remarks>
    /// <param name="eventName">Name of the event</param>
    private void Event(string eventName)
    {
        InkEvent?.Invoke(eventName);
    }

    /// <summary>
    /// Get the <see cref="State.amount"/> of the <see cref="State"/> with the given <paramref name="id"/> from the <see cref="gameState"/>.
    /// </summary>
    /// <remarks>
    /// This function is bound to the function in Ink of the same name.
    /// </remarks>
    /// <param name="id">The id of the <see cref="State"/>.</param>
    /// <returns>The <see cref="State.amount"/> of the <see cref="State"/> with the given <paramref name="id"/> if it exists; <c>0</c> otherwise.</returns>
    private object Get_State(string id)
    {
        State state = gameState.Get(id);
        return state != null ? state.amount : 0;
    }

    /// <summary>
    /// Add a new <see cref="State"/> to the <see cref="gameState"/> or add a value to an existing <see cref="State"/>.
    /// </summary>
    /// <remarks>
    /// This function is bound to the function in Ink of the same name.
    /// </remarks>
    /// <param name="id">Id of the <see cref="State"/> to create or modify.</param>
    /// <param name="amount">Amount to add to the <see cref="State"/>.</param>
    private void Add_State(string id, int amount)
    {
        gameState.Add(id, amount);
    }

    #endregion
}

/// <summary>
/// Container that holds all information about one line of dialogue.
/// </summary>
public struct DialogueLine
{
    /// <summary>The speaker of the dialogue.</summary>
    public string speaker;

    /// <summary>Text content of the dialogue.</summary>
    public string text;

    /// <summary>Available choices after the text.</summary>
    public List<Choice> choices;

    // Here we can also add other information like speaker images or sounds.
}
