using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the player. Manages movement, camera rotation and other player actions.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class Player_Controller : MonoBehaviour
{
    #region Inspector

    [Min(0)]
    [Tooltip("The maximum speed of the player in uu/s.")]
    [SerializeField] private float movementSpeed = 5f;

    [Min(0)]
    [Tooltip("How fast the movement speed is in-/decreasing.")]
    [SerializeField] private float speedChangeRate = 10f;

    #endregion

    /// <summary>Cached reference to the <see cref="CharacterController"/> component.</summary>
    private CharacterController characterController;

    /// <summary>The instantiated GameInput to query the player inputs from.</summary>
    private GameInput input;

    /// <summary>The cached <i>Move</i> <see cref="InputAction"/> from the <see cref="GameInput"/>.</summary>
    private InputAction moveAction;

    /// <summary>Cached input from the player for the movement.</summary>
    private Vector2 moveInput;

    /// <summary>Last movement passed into the <see cref="characterController"/>.</summary>
    private Vector3 lastMovement;

    #region Unity Event Functions

    /// <summary>
    /// Called once at the beginning of the game (if the <see cref="GameObject"/>/<see cref="Component"/> is active at that time).
    /// </summary>
    private void Awake()
    {
        // Search for the CharacterController on this GameObject.
        characterController = GetComponent<CharacterController>();

        // Create new input.
        input = new GameInput();
        // Cache the Move action specifically for easier usage.
        moveAction = input.Player.Move;

        // TODO Subscribe to input events.
    }

    /// <summary>
    /// Called always when the <see cref="GameObject"/>/<see cref="Component"/> becomes active/enabled.
    /// </summary>
    private void OnEnable()
    {
        // Enable the input together with the component.
        input.Enable();
    }

    /// <summary>
    /// Runs every frame - use for visuals &amp; input reading.
    /// </summary>
    private void Update()
    {
        // Read out the value from the input the Move action recceives.
        moveInput = moveAction.ReadValue<Vector2>();

        // Rotate and move the player with the CharacterController.
        Rotate(moveInput);
        Move(moveInput);
    }

    /// <summary>
    /// Called always when the <see cref="GameObject"/>/<see cref="Component"/> becomes inactive/disabled.
    /// </summary>
    private void OnDisable()
    {
        // Disable the input together with the component.
        input.Disable();
    }

    /// <summary>
    /// Called once when the <see cref="GameObject"/>/<see cref="Component"/> gets destroyed - Used for clean-up.
    /// </summary>
    private void OnDestroy()
    {
        // TODO Unsubscribe from input events.
    }

    #endregion

    #region Movement

    /// <summary>
    /// Rotate the player towards the <paramref name="moveInput"/>.
    /// </summary>
    /// <param name="moveInput">The move input from the player.</param>
    private void Rotate(Vector2 moveInput)
    {
        // Only rotate the player if have input.
        if (moveInput != Vector2.zero)
        {
            // Remap Vector2 to Vector3 in the plane (y = 0).
            // Normalize input (make the vector be of length 1).
            Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            // Set the rotation of our transform towards the inputDirection.
            transform.rotation = Quaternion.LookRotation(inputDirection);
        }
    }

    /// <summary>
    /// Move the player forward with the <see cref="CharacterController"/>.
    /// </summary>
    /// <param name="moveInput">The move input from the player.</param>
    private void Move(Vector2 moveInput)
    {
        // Calculate the desired speed we want to reach based on the current input.
        // Multiply our max speed (movementSpeed) with the length of the input vector (analogue stick can be tilted gradually).
        float targetSpeed = moveInput == Vector2.zero ? 0 : movementSpeed * moveInput.magnitude;

        // Read the velocity out from last frame. (0,0,0) at the beginning.
        Vector3 currentVelocity = lastMovement;
        // Set y to zero to only get the velocity on the plane (Ignore jumping/falling).
        currentVelocity.y = 0;
        // Get the length of the current velocity which is our current speed.
        float currentSpeed = currentVelocity.magnitude;

        // Check if we are not near the target speed.
        // If we are not, slowly approach the target speed using lerp.
        if (Mathf.Abs(currentSpeed - targetSpeed) > 0.01f)
        {
            // Lets the character slowly approach the targetSpeed based on their currentSpeed.
            // Note: As the currentSpeed changes this is not creating a linear movement
            // but is instead resulting in a curve making the movement "speed up"/"slow down".
            // This calculation will get infinitely near the targetSpeed but never reach it,
            // therefore we need the else-case once we are very close.
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speedChangeRate * Time.deltaTime);
        }
        else // Otherwise set the currentSpeed to targetSpeed directly.
        {
            currentSpeed = targetSpeed;
        }

        // Multiply forward vector of our transform with the currentSpeed to get the final velocity (direction + speed).
        // This sets the length of the vector to currentSpeed.
        Vector3 movement = transform.forward * currentSpeed;

        // Pass the movement for this frame into the CharacterController.
        // CharacterController does the rest for us, incl. gravity if we call this function every frame.
        characterController.SimpleMove(movement);

        // Save the vector passed to the CharacterController for next frame.
        lastMovement = movement;
    }

    #endregion
}