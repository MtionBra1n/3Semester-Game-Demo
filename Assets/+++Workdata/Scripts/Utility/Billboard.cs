using UnityEngine;

/// <summary>
/// Mirror the rotation of the main <see cref="Camera"/>. Useful for e.g. billboard sprites.
/// </summary>
public class Billboard : MonoBehaviour
{
    /// <summary>Cached <see cref="Camera"/>.<see cref="Camera.main"/>.</summary>
    private Camera mainCamera;

    #region Inspector

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        // Cache the main camera in the scene.
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Copy the rotation of the main camera to this GameObject.
        transform.rotation = mainCamera.transform.rotation;
    }

    #endregion
}
