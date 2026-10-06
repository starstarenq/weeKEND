using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class DotweenLab : MonoBehaviour
{

    [SerializeField] Transform endPos;
    [SerializeField] float duration = 5f;

    [SerializeField] Vector3 RotationValue = new Vector3(0, 0, -350);
    [SerializeField] float size = 0.5f;
    // DO (움직여라) Move
    // Do (회전해라) Rotate
    // DO (UI 움직여라) AnchorPos

    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] RectTransform imageRectTransform;
    [SerializeField] Vector2 EndRectTransform = new Vector2(15, -750);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.DOMove(endPos.position, duration);
        transform.DORotate(RotationValue, duration);
        transform.DOScale(size, duration);

        spriteRenderer.DOColor(Color.blue, duration);
        spriteRenderer.DOFade(0, duration);

        imageRectTransform.DOAnchorPos(EndRectTransform, 2);
    }
}
