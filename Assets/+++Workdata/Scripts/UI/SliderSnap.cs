using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Snaps the value of a <see cref="Slider"/> to the defined snap increments.
/// </summary>
[RequireComponent(typeof(Slider))]
public class SliderSnap : MonoBehaviour
{
    [Min(0)]
    [Tooltip("Snap the slider value to the chosen increments.")]
    [SerializeField] private float snap = 0.1f;

    private Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        slider.onValueChanged.AddListener(SnapValue);
    }

    public void SnapValue(float value)
    {
        value = Mathf.Round(value / snap) * snap;
        slider.SetValueWithoutNotify(value);
    }
}
