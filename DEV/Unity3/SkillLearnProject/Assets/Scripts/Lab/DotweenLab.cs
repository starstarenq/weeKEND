using DG.Tweening;
using System;
using UnityEngine;

public class DotweenLab : MonoBehaviour
{
    [SerializeField] Transform startPos;
    [SerializeField] Transform endPos;
    [SerializeField] float duration = 5f;
    [SerializeField] Vector3 RotationValue = new Vector3(0, 0, -180);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     transform.DOMove(endPos.position, duration);
     transform.DOScale(new Vector3(2, 2, 2), duration);
     transform.DORotate(RotationValue, duration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
