using UnityEngine;

/// <summary>
/// Manages a quest entry on the UI.
/// </summary>
public class QuestEntry : MonoBehaviour
{
    #region Inspector

    [Tooltip("The icon to indicate the status of the quest.")]
    [SerializeField] private GameObject statusIcon;

    #endregion

    /// <summary>
    /// Set the status of the quest.
    /// </summary>
    /// <param name="fulfilled">If the quest conditions are fulfilled or not.</param>
    /// <remarks>If fulfilled the quest can be completed, either automatically or by "delivering it" at the appropriate location.</remarks>
    public void SetQuestStatus(bool fulfilled)
    {
        statusIcon.SetActive(fulfilled);
    }
}
