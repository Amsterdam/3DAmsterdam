using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelMover : MonoBehaviour
{
    private Coroutine activeMoveCoroutine;
    public RectTransform rectTransform;
    private Vector2 velocity = Vector2.zero;
    private bool isAtStartPosition = true;
    [SerializeField] Vector2 startPosition;
    [SerializeField] private Vector2 endPosition;
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private float thresholdDistance = 0.1f;
    [SerializeField] private bool jumpToEndPositionAtStart;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (jumpToEndPositionAtStart)
            JumpToEndPosition();
    }

    public void TogglePosition()
    {
        if (isAtStartPosition)
            MoveToEndPosition();
        else
            MoveToStartPosition();
    }

    public void MoveToStartPosition()
    {
        if (activeMoveCoroutine != null)
            StopCoroutine(activeMoveCoroutine);
        StartMovePanel(startPosition, smoothTime);
        isAtStartPosition = true;
    }

    public void MoveToEndPosition()
    {
        if (activeMoveCoroutine != null)
            StopCoroutine(activeMoveCoroutine);
        StartMovePanel(endPosition, smoothTime);
        isAtStartPosition = false;
    }

    public void StartMovePanel(Vector2 newPosition, float transitionTime)
    {
        activeMoveCoroutine = StartCoroutine(MovePanel(newPosition, transitionTime));
    }

    private IEnumerator MovePanel(Vector2 targetPosition, float speed)
    {
        //var dir = newPosition - rectTransform.anchoredPosition;
        while (true)
        {
            var distance = Vector2.Distance(rectTransform.anchoredPosition, targetPosition);
            if (distance < thresholdDistance)
                break;

            rectTransform.anchoredPosition = Vector2.SmoothDamp(rectTransform.anchoredPosition, targetPosition, ref velocity, smoothTime); //dir * speed * Time.deltaTime;
            yield return null; //wait until next frame
        }
        rectTransform.anchoredPosition = targetPosition;
    }

    public void SetStartPosition(Vector2 pos)
    {
        startPosition = pos;
    }

    public void SetEndPosition(Vector2 pos)
    {
        endPosition = pos;
    }

    public void JumpToStartPosition()
    {
        rectTransform.anchoredPosition = startPosition;
    }

    public void JumpToEndPosition()
    {
        rectTransform.anchoredPosition = endPosition;
    }
}
