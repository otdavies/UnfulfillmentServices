using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnnounceText : MonoBehaviour
{
    public Text announceText;
    public RectTransform offScreenAbove;
    public RectTransform offScreenBelow;
    public RectTransform onCenterScreen;
    private RectTransform textRectTransform;

    private void Start()
    {
        textRectTransform = announceText.GetComponent<RectTransform>();
    }

    public void SetText(string txt)
    {
        announceText.text = txt;
        textRectTransform.position = offScreenAbove.position;
        MoveToAnchor(0, onCenterScreen.position, 0.5f);
        MoveToAnchor(2, offScreenBelow.position, 0.5f);
        Vector3.Lerp(textRectTransform.position, offScreenAbove.position, Time.deltaTime * 10);
    }

    private void MoveToAnchor(float delay, Vector3 target, float time)
    {
        StartCoroutine(MoveAnchor(delay, target, time));
    }

    private IEnumerator MoveAnchor(float delay, Vector3 target, float time)
    {
        yield return new WaitForSeconds(delay);
        float percentCompletion = 0;
        float t = 0;
        Vector3 originalPosition = textRectTransform.position;

        while (percentCompletion < 1)
        {
            percentCompletion += ( Time.fixedDeltaTime / time );
            t = percentCompletion;
            t = t * t * t * (t * (6f * t - 15f) + 10f);
            textRectTransform.position = Vector3.Lerp(originalPosition, target, t);
            yield return new WaitForEndOfFrame();
        }
    }
}
