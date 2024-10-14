using System.Collections.Generic;
using System.Linq;

using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

public class MenuButtonSelectAnimation : MonoBehaviour
{
    #region Inspector

    [SerializeField] private List<Graphic> graphics;

    [Min(0)]
    [SerializeField] private float extendDuration = 0.75f;

    [SerializeField] private Ease extendEase = Ease.OutQuart;

    [Min(0)]
    [SerializeField] private float retractDuration = 0.75f;

    [SerializeField] private Ease retractEase = Ease.OutQuart;

    [Min(0)]
    [SerializeField] private float spacing = 12f;

    #endregion

    private List<RectTransform> graphicRectTransforms;

    #region Unity Events Functions

    private void Reset()
    {
        graphics = GetComponentsInChildren<Graphic>().ToList();
    }

    private void OnValidate()
    {
        for (int i = 0; i < graphics.Count; i++)
        {
            var rectTransform = graphics[i].GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 0.5f);
            rectTransform.anchoredPosition = new Vector2(-spacing * (i + 1), 0);
        }
    }

    private void Awake()
    {
        graphicRectTransforms = new List<RectTransform>(graphics.Count);

        foreach (Graphic graphic in graphics)
        {
            Color color = graphic.color;
            color.a = 0;
            graphic.color = color;

            RectTransform rectTransform = graphic.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = Vector2.zero;

            graphicRectTransforms.Add(rectTransform);
        }
    }

    private void OnDisable()
    {
        // Prevents DOTween from acting when stopping play mode
        if (!this.gameObject.scene.isLoaded) { return; }
        DODeselect().Complete();
    }

    private void OnDestroy()
    {
        this.DOKill();
    }

    #endregion

    public void Select() => DOSelect();

    public void Deselect() => DODeselect();

    private Tween DOSelect()
    {
        this.DOKill();

        Sequence sequence = DOTween.Sequence(this).SetUpdate(true);

        float durationSegment = extendDuration / graphics.Count;
        float alphaSegment = 1.0f / graphics.Count;

        for (int i = 0; i < graphics.Count; i++)
        {
            float duration = durationSegment * (i + 1);
            sequence.Insert(0, graphicRectTransforms[i].DOAnchorPosX(-spacing * (i + 1), duration).From(Vector2.zero).SetEase(extendEase));
            sequence.Insert(0, graphics[i].DOFade(1 - alphaSegment * i, duration).From(0).SetEase(Ease.Linear));
        }

        return sequence.Play();
    }

    private Tween DODeselect()
    {
        this.DOKill();

        Sequence sequence = DOTween.Sequence(this).SetUpdate(true);

        for (int i = 0; i < graphics.Count; i++)
        {
            sequence.Insert(0, graphicRectTransforms[i].DOAnchorPos(Vector2.zero, retractDuration).SetEase(retractEase));
            sequence.Insert(0, graphics[i].DOFade(0, retractDuration).SetEase(Ease.Linear));
        }

        return sequence.Play();
    }
}
