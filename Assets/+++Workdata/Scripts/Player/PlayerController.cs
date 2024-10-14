using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the player. Manages movement, camera rotation and other player actions.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    /// <summary>Hashed "MovementSpeed" animator parameter for faster access.</summary>
    private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");

    /// <summary>Hashed "Grounded" animator parameter for faster access.</summary>
    private static readonly int Grounded = Animator.StringToHash("Grounded");
    
    /// <summary>Hashed "Grounded" animator parameter for faster access.</summary>
    private static readonly int Crouching = Animator.StringToHash("Crouching");

    #region Inspector

    [Header("Movement")]

    [Min(0)]
    [Tooltip("The maximum speed of the player in uu/s.")]
    [SerializeField] private float movementSpeed = 5f;

    [Min(0)]
    [Tooltip("How fast the movement speed is in-/decreasing.")]
    [SerializeField] private float speedChangeRate = 10f;

    [Min(0)]
    [Tooltip("How fast the character rotates around it's y-axis.")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Slope Movement")]

    [Min(0)]
    [Tooltip("How much additional gravity force to apply while walking down a slope. In uu/s.")]
    [SerializeField] private float pullDownForce = 5f;

    [Tooltip("Layer mask used for the raycast while walking on a slope.")]
    [SerializeField] private LayerMask raycastMask = 1; // 1 is Default Physics Layer.

    [Min(0)]
    [Tooltip("Length of the raycast for checking for slopes in uu.")]
    [SerializeField] private float raycastLength = 0.5f;

    [Header("Camera")]

    [Tooltip("The focus and rotation point of the camera.")]
    [SerializeField] private Transform cameraTarget;

    [Range(-89f, 0f)]
    [Tooltip("The minimum vertical camera angle. Lower half of the horizon.")]
    [SerializeField] private float verticalCameraRotationMin = -30f;

    [Range(0f, 89f)]
    [Tooltip("The maximum vertical camera angle. Upper half of the horizon.")]
    [SerializeField] private float verticalCameraRotationMax = 70f;

    [Min(0)]
    [Tooltip("Sensitivity of the horizontal camera rotation. deg/s for controller.")]
    [SerializeField] private float cameraHorizontalSpeed = 200f;

    [Min(0)]
    [Tooltip("Sensitivity of the vertical camera rotation. deg/s for controller.")]
    [SerializeField] private float cameraVerticalSpeed = 130f;

    [Header("Animations")]

    [Tooltip("Animator of the character mesh.")]
    [SerializeField] private Animator animator;

    [Min(0)]
    [Tooltip("Time in sec the character has to be in the air before the animator reacts.")]
    [SerializeField] private float coyoteTime = 0.2f;

    #endregion

    /// <summary>Cached reference to the <see cref="CharacterController"/> component.</summary>
    private CharacterController characterController;

    /// <summary>The instantiated GameInput to query the player inputs from.</summary>
    private GameInput input;

    /// <summary>The cached <i>Look</i> <see cref="InputAction"/> from the <see cref="GameInput"/>.</summary>
    private InputAction lookAction;

    /// <summary>The cached <i>Move</i> <see cref="InputAction"/> from the <see cref="GameInput"/>.</summary>
    private InputAction moveAction;
    
    /// <summary>The cached <i>Crouch</i> <see cref="InputAction"/> from the <see cref="GameInput"/>.</summary>
    private InputAction crouchAction;

    /// <summary>Cached input from the player for the camera movement</summary>
    private Vector2 lookInput;

    /// <summary>Cached input from the player for the movement.</summary>
    private Vector2 moveInput;

    /// <summary>The target rotation the character tries to rotate towards, over time.</summary>
    private Quaternion characterTargetRotation = Quaternion.identity;

    /// <summary>The rotation of the player camera, dictated by the player's <see cref="lookInput"/>.</summary>
    private Vector2 cameraRotation;

    /// <summary>Last movement passed into the <see cref="characterController"/>.</summary>
    private Vector3 lastMovement;

    /// <summary>If the character is considered to be on the ground. Delayed by coyoteTime.</summary>
    private bool isGrounded = true;

    /// <summary>Time in sec the character is in the air.</summary>
    private float airTime;

    /// <summary>The Interactable that the player has currently selected and will interact on.</summary>
    private Interactable selectedInteractable;

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
        // Cache the Look and Move input actions specifically for easier usage.
        lookAction = input.Player.Look;
        moveAction = input.Player.Move;
        crouchAction = input.Player.Crouch;

        // Subscribe to input events.
        input.Player.Interact.performed += Interact;

        // Set initial player rotation to the rotation set in the scene.
        characterTargetRotation = transform.rotation;
        // Set initial camera rotation behind the player.
        cameraRotation = cameraTarget.rotation.eulerAngles;
    }

    /// <summary>
    /// Called always when the <see cref="GameObject"/>/<see cref="Component"/> becomes active/enabled.
    /// </summary>
    private void OnEnable()
    {
        // Enable the input together with the component.
        EnableInput();
    }

    /// <summary>
    /// Runs every frame - use for visuals &amp; input reading.
    /// </summary>
    private void Update()
    {
        // Query and save the newest input values from the hardware.
        ReadInput();

        // Rotate and move the player with the CharacterController.
        Rotate(moveInput);
        Move(moveInput);

        // Update isGrounded.
        CheckGround();

        // Update the parameters of the animator.
        UpdateAnimator();
    }

    /// <summary>
    /// Runs every frame - use for visuals &amp; stuff that needs to happen AFTER Update().
    /// </summary>
    private void LateUpdate()
    {
        RotateCamera(lookInput);
    }

    /// <summary>
    /// Called always when the <see cref="GameObject"/>/<see cref="Component"/> becomes inactive/disabled.
    /// </summary>
    private void OnDisable()
    {
        // Disable the input together with the component.
        DisableInput();
    }

    /// <summary>
    /// Called once when the <see cref="GameObject"/>/<see cref="Component"/> gets destroyed - Used for clean-up.
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from input events.
        input.Player.Interact.performed -= Interact;
    }

    #region Physics

    /// <summary>
    /// Called when a GameObject enters a Trigger, on both the entering GameObject as well as the Trigger.
    /// </summary>
    /// <param name="other">The trigger collider or the collider of the entering GameObject, depending on the side.</param>
    private void OnTriggerEnter(Collider other)
    {
        TrySelectInteractable(other);
    }

    /// <summary>
    /// Called when a GameObject exits a Trigger, on both the exiting GameObject as well as the Trigger.
    /// </summary>
    /// <param name="other">The trigger collider or the collider of the exiting GameObject, depending on the side.</param>
    private void OnTriggerExit(Collider other)
    {
        TryDeselectInteractable(other);
    }

    #endregion

    #endregion

    #region Input

    /// <summary>
    /// Enables the players <see cref="GameInput"/>.
    /// </summary>
    public void EnableInput()
    {
        input.Enable();
    }

    /// <summary>
    /// Disables the player <see cref="GameInput"/>.
    /// </summary>
    public void DisableInput()
    {
        input.Disable();
    }

    /// <summary>
    /// Query and save the newest input values from the hardware.
    /// </summary>
    private void ReadInput()
    {
        // Read out the value (Vector2) of the right analogue stick/Mouse delta-movement.
        lookInput = lookAction.ReadValue<Vector2>();
        // Read out the value (Vector2) of the left analogue stick/WASD.
        // Controller: Values between [-1, 1].
        // Mouse: Absolute movement in pixel since the last frame.
        //   E.g. (50px, -3px) (right, down)
        moveInput = moveAction.ReadValue<Vector2>();
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
            // Take the local movement inputDirection from the controller/keyboard
            // and transform them into world direction based on the cameraTarget orientation (same as camera rotation).
            Vector3 worldInputDirection = cameraTarget.TransformDirection(inputDirection);
            // Set y to 0 to have a flat vector on the plane.
            worldInputDirection.y = 0;

            // Calculate the target rotation based on our input.
            characterTargetRotation = Quaternion.LookRotation(worldInputDirection);
        }

        // Rotate the player towards the target rotation.
        // Check the difference between the player rotation and the target rotation.
        // Slowly rotate towards the target rotation if the difference is large.
        if (Quaternion.Angle(transform.rotation, characterTargetRotation) > 0.1f)
        {
            // Use Slerp (Spherical linear interpolation) instead of Lerp for rotation (Quaternions) or directions vectors.
            transform.rotation = Quaternion.Slerp(transform.rotation, characterTargetRotation, rotationSpeed * Time.deltaTime);
        }
        else // Otherwise, directly set the rotation to the target rotation directly.
        {
            transform.rotation = characterTargetRotation;
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

        // Multiply the targetRotation Quaternion with Vector3.forward (not commutative!).
        // to get a direction vector in the direction of the targetRotation.
        // In a sense "vectorize the quaternion" (loosing one axis of data: the roll).
        Vector3 targetDirection = characterTargetRotation * Vector3.forward;

        // Multiply targetDirection (unit-vector) with the currentSpeed to get the final velocity (direction + speed).
        // This sets the length of the vector to currentSpeed.
        Vector3 movement = targetDirection * currentSpeed;

        // Pass the movement for this frame into the CharacterController.
        // CharacterController does the rest for us, incl. gravity if we call this function every frame.
        characterController.SimpleMove(movement);

        // Use a raycast to check the surface below the player. 
        if (Physics.Raycast(transform.position + Vector3.up * 0.01f, Vector3.down, out RaycastHit hit, raycastLength, raycastMask, QueryTriggerInteraction.Ignore))
        {
            // Calculate the vector along the slope in the direction of our movement and check if it's going downwards.
            if (Vector3.ProjectOnPlane(movement, hit.normal).y < 0)
            {
                // Pull down the player with "additional gravity".
                characterController.Move(Vector3.down * (pullDownForce * Time.deltaTime));
            }
        }

        // Save the vector passed to the CharacterController for next frame.
        lastMovement = movement;
    }

    #endregion

    #region Ground Check

    /// <summary>
    /// Measures for how long the character is not on the ground and updates <see cref="isGrounded"/> delayed by <see cref="coyoteTime"/> when becoming airborne.
    /// </summary>
    private void CheckGround()
    {
        if (characterController.isGrounded)
        {
            // Reset the "stopwatch".
            airTime = 0;
        }
        else
        {
            // Count up the "stopwatch".
            airTime += Time.deltaTime;
        }

        // Set grounded to true if on the ground (0) or the airTime is still less than the coyoteTime. 
        isGrounded = airTime < coyoteTime;
    }

    #endregion

    #region Camera

    /// <summary>
    /// Rotate the camera around the player.
    /// </summary>
    /// <param name="lookInput">The look input from the player.</param>
    private void RotateCamera(Vector2 lookInput)
    {
        // Only rotate the camera when we have input.
        if (lookInput != Vector2.zero)
        {
            // Check if the mouse moved this frame.
            bool isMouseLook = IsMouseLook();

            // Time.deltaTime if controller, otherwise 1.
            // We multiply this with the input to get it independent of the framerate.
            // Only multiply with the controller input because the mouse input is already framerate independent (mouse delta).
            float deltaTimeMultiplier = isMouseLook ? 1 : Time.deltaTime;
            // Get the correct sensitivity value for the controller or mouse.
            float sensitivity = isMouseLook
                                    ? PlayerPrefs.GetFloat(SettingsMenu.MouseSensitivityKey, SettingsMenu.DefaultMouseSensitivity)
                                    : PlayerPrefs.GetFloat(SettingsMenu.ControllerSensitivityKey, SettingsMenu.DefaultControllerSensitivity);

            // Multiply lookInput with deltaTimeMultiplier and the correct sensitivity to get the final input value.
            lookInput *= deltaTimeMultiplier * sensitivity;

            // Multiply the input with the vertical camera speed in deg/s.
            // Vertical camera rotation around the X-axis of the player!
            // Additionally multiply with -1 if we are using the controller AND we want to invert the Y input.
            bool invertY = !isMouseLook && SettingsMenu.GetBool(SettingsMenu.InvertYKey, SettingsMenu.DefaultInvertY);
            cameraRotation.x += lookInput.y * cameraVerticalSpeed * (invertY ? -1 : 1);
            // Multiply the input with the horizontal camera speed in deg/s.
            // Horizontal camera rotation around the Y-axis of the player!
            cameraRotation.y += lookInput.x * cameraHorizontalSpeed;

            // Normalize the angles in cameraRotation so that they are always in the range of [-180, 180).
            cameraRotation.x = NormalizeAngle(cameraRotation.x);
            cameraRotation.y = NormalizeAngle(cameraRotation.y);

            // Clamp the vertical rotation to min/max set in the inspector.
            cameraRotation.x = Mathf.Clamp(cameraRotation.x, verticalCameraRotationMin, verticalCameraRotationMax);
        }

        // Important to always do even without input, so it is always steady and only moves if we give input.
        // This prevents it from rotating with it's parent Player object.
        // Create a new Quaternion with (x,y,z) rotation (like in the inspector).
        cameraTarget.rotation = Quaternion.Euler(cameraRotation.x, cameraRotation.y, 0);
    }

    /// <summary>
    /// Normalize an angle (in deg) to be in the range of [-180, 180).
    /// </summary>
    /// <param name="angle">Angle in deg.</param>
    /// <returns>Normalized angle in deg.</returns>
    private float NormalizeAngle(float angle)
    {
        // Limits the angle to (-360, 360).
        angle %= 360;

        // Limits the angle to [0, 360).
        if (angle < 0)
        {
            angle += 360;
        }

        // Remaps the angle from [0, 360) to [-180, 180).
        if (angle > 180)
        {
            angle -= 360;
        }

        return angle;
    }

    /// <summary>
    /// Determines if the mouse of was used for the look action.
    /// </summary>
    /// <returns>True, if the mouse was used.</returns>
    private bool IsMouseLook()
    {
        // If we give currently no input activeControl is null.
        if (lookAction.activeControl == null)
        {
            return true;
        }

        // Check the name of the hardware that gives input to lookAction.
        // PC Mouse: Mouse
        // Xbox controller on Windows: XInputControllerWindows
        // PS4 controller on Windows: DualShock4GamepadHID
        return lookAction.activeControl.device.name == "Mouse";
    }

    #endregion

    #region Animator

    /// <summary>
    /// Update the parameters of the animator.
    /// </summary>
    private void UpdateAnimator()
    {
        // Get current movement speed on the XZ plane.
        Vector3 velocity = lastMovement;
        velocity.y = 0;
        float speed = velocity.magnitude;

        // Set parameters in animator.
        animator.SetFloat(MovementSpeed, speed);
        animator.SetBool(Grounded, isGrounded);
    }

    #endregion

    #region Interaction

    /// <summary>
    /// Interact with the currently <see cref="selectedInteractable"/>.
    /// </summary>
    /// <param name="_">Callback context of the <see cref="InputAction"/>.</param>
    private void Interact(InputAction.CallbackContext _)
    {
        // Only try to interact if an interactable is actually selected.
        if (selectedInteractable != null)
        {
            selectedInteractable.Interact();
        }
    }

    /// <summary>
    /// Try to find and select an <see cref="Interactable"/> on the passed <paramref name="other"/> <see cref="Collider"/>.
    /// </summary>
    /// <param name="other">The collider to test.</param>
    private void TrySelectInteractable(Collider other)
    {
        // Search for an Interactable on the other object.
        Interactable interactable = other.GetComponent<Interactable>();
        // Abort if none found.
        if (interactable == null) { return; }

        // Deselect old interactable in case we already have one selected.
        if (selectedInteractable != null)
        {
            selectedInteractable.Deselect();
        }
        // Save & select.
        selectedInteractable = interactable;
        selectedInteractable.Select();
    }

    /// <summary>
    /// Try to find an <see cref="Interactable"/> on the passed <paramref name="other"/> <see cref="Collider"/> and deselect it, if it is currently selected.
    /// </summary>
    /// <param name="other">The collider to test.</param>
    private void TryDeselectInteractable(Collider other)
    {
        // Search for an Interactable on the other object.
        Interactable interactable = other.GetComponent<Interactable>();
        // Abort if none found.
        if (interactable == null) { return; }

        // Only deselect the selected Interactable.
        if (interactable == selectedInteractable)
        {
            // Deselect & forget.
            selectedInteractable.Deselect();
            selectedInteractable = null;
        }
    }

    #endregion
}
