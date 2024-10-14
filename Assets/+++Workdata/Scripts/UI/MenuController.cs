using System;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the opening/closing of nested menus, such as the main and pause menu.
/// </summary>
public class MenuController : MonoBehaviour
{
    /// <summary>Invoked when the base menu opens.</summary>
    public static event Action BaseMenuOpening;

    /// <summary>Invoked when the base menu closed.</summary>
    public static event Action BaseMenuClosed;

    #region Inspector

    [Tooltip("Path of the scene that starts the game.")]
    [SerializeField] private string startScene = "Scenes/Sandbox";

    [Tooltip("Path of the scene of the main menu.")]
    [SerializeField] private string menuScene = "Scenes/MainMenu";

    [Tooltip("Base to be opened/closed with the ToggleMenu Action.\nThis is usually the main or pause menu.")]
    [SerializeField] private Menu baseMenu;

    [Tooltip("Prevent the base menu from being closed. E.g. in the main menu.")]
    [SerializeField] private bool preventBaseClosing;

    [Tooltip("Hides the previous open menus when opening a new menu on-top.")]
    [SerializeField] private bool hidePreviousMenu;

    #endregion

    /// <summary>The instantiated <see cref="GameInput"/> to query the menu inputs from.</summary>
    private GameInput input;

    /// <summary><see cref="Stack{T}"/> of the currently opened <see cref="Menu"/>s.</summary>
    private Stack<Menu> openMenus;

    #region Unity Event Functions

    private void Awake()
    {
        // Create new input.
        input = new GameInput();

        // Subscribe to input events.
        input.UI.ToggleMenu.performed += ToggleMenu;
        input.UI.GoBackMenu.performed += GoBackMenu;

        // Create a new empty stack.
        openMenus = new Stack<Menu>();

        // Reset timescale on scene start.
        Time.timeScale = 1;
    }

    private void Start()
    {
        // Add base menu to stack in case it was open on start. E.g. the main menu.
        if (baseMenu.gameObject.activeSelf)
        {
            openMenus.Push(baseMenu);
        }
    }

    private void OnEnable()
    {
        // Enable the input together with the component.
        input.Enable();
    }

    private void OnDisable()
    {
        // Disable the input together with the component.
        input.Disable();
    }

    private void OnDestroy()
    {
        // Unsubscribe from input events.
        input.UI.ToggleMenu.performed -= ToggleMenu;
        input.UI.GoBackMenu.performed -= GoBackMenu;
    }

    #endregion

    #region Menu Functions

    /// <summary>
    /// Load the <see cref="Scene"/> that starts the game.
    /// </summary>
    public void StartGame()
    {
        SceneManager.LoadScene(startScene);
    }

    /// <summary>
    /// Load the main menu <see cref="Scene"/>.
    /// </summary>
    public void ToMainMenu()
    {
        SceneManager.LoadScene(menuScene);
    }

    /// <summary>
    /// Quit the game.
    /// </summary>
    /// <remarks>
    /// Stops Play-Mode in the editor.
    /// </remarks>
    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #region Menu Controls

    /// <summary>
    /// Open a <see cref="Menu"/>.
    /// </summary>
    /// <param name="menu">The <see cref="Menu"/> to open.</param>
    public void OpenMenu(Menu menu)
    {
        if (menu == baseMenu)
        {
            BaseMenuOpening?.Invoke();
        }

        // Hide the menu currently on top of the stack before opening another menu on top.
        if (hidePreviousMenu && openMenus.Count > 0)
        {
            openMenus.Peek().Hide();
        }

        menu.Open();
        // Add menu to the stack.
        openMenus.Push(menu);
    }

    /// <summary>
    /// Close the top most open <see cref="Menu"/>.
    /// </summary>
    public void CloseMenu()
    {
        if (openMenus.Count == 0) { return; }

        // Prevent base menu from closing.
        if (preventBaseClosing &&
            openMenus.Count == 1 &&
            openMenus.Peek() == baseMenu) // Look at the top most menu on the stack without removing it.
        {
            return;
        }

        // Remove top most menu from the stack.
        Menu closingMenu = openMenus.Pop();
        closingMenu.Close();

        // Unhide the menu on top of the stack, after closing the previously open menu.
        if (hidePreviousMenu && openMenus.Count > 0)
        {
            openMenus.Peek().Show();
        }

        if (closingMenu == baseMenu)
        {
            BaseMenuClosed?.Invoke();
        }
    }

    /// <summary>
    /// Toggle (open/close) the menu.
    /// </summary>
    /// <param name="_">Callback context of the <see cref="InputAction"/>.</param>
    private void ToggleMenu(InputAction.CallbackContext _)
    {
        // Open the base menu when not open.
        if (!baseMenu.gameObject.activeSelf)
        {
            OpenMenu(baseMenu);
        }
        else // Otherwise do the same as GoBackMenu()
        {
            GoBackMenu(_);
        }
    }

    /// <summary>
    /// Go back one level of nested menus, closing the currently open menu.
    /// </summary>
    /// <param name="_">Callback context of the <see cref="InputAction"/>.</param>
    private void GoBackMenu(InputAction.CallbackContext _)
    {
        CloseMenu();
    }

    #endregion

    #endregion
}
