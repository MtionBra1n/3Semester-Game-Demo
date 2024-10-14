using UnityEngine;
using UnityEditor;

public class RandomizeObjTransform : MonoBehaviour
{
    [Range(0.1f, 5.0f)]
    public float minScale = 0.5f;
    [Range(0.1f, 5.0f)]
    public float maxScale = 2.0f;

    public void RandomizeChildrenYRotation()
    {
        foreach (Transform child in transform)
        {
            float randomYRotation = Random.Range(0f, 360f);
            child.rotation = Quaternion.Euler(child.rotation.eulerAngles.x, randomYRotation, child.rotation.eulerAngles.z);
        }
    }

    public void RandomizeChildrenScale()
    {
        foreach (Transform child in transform)
        {
            float randomScale = Random.Range(minScale, maxScale);
            child.localScale = new Vector3(randomScale, randomScale, randomScale);
        }
    }
}

[CustomEditor(typeof(RandomizeObjTransform))]
public class RandomizeObjTransformEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RandomizeObjTransform script = (RandomizeObjTransform)target;

        if (GUILayout.Button("Randomize Children Y Rotation"))
        {
            script.RandomizeChildrenYRotation();
        }

        if (GUILayout.Button("Randomize Children Scale"))
        {
            script.RandomizeChildrenScale();
        }
    }
}