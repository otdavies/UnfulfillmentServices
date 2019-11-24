using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreText : MonoBehaviour
{
    public Text[] scoreText;

    private RectTransform[] scoreTextRects;

    private void Start()
    {
        scoreTextRects = new RectTransform[scoreText.Length];
        for (int i = 0; i < scoreText.Length; i++)
        {
            scoreTextRects[i] = scoreText[i].GetComponent<RectTransform>();
        }
    }

    public void SetScore(int i, int val)
    {
        scoreText[i].text = val + "";
        ScaleText(0, i, 0.9f, 0.25f);
        ScaleText(0.25f, i, 0.5f, 0.25f);
    }

    private void ScaleText(float d, int i, float s, float t)
    {
        StartCoroutine(Scale(d, i, s, t));
    }

    private IEnumerator Scale(float delay, int id, float scale, float time)
    {
        yield return new WaitForSeconds(delay);
        float percentCompletion = 0;
        float t = 0;
        Vector3 originalScale = scoreTextRects[id].localScale;

        while (percentCompletion < 1)
        {
            percentCompletion += (Time.fixedDeltaTime / time);
            t = percentCompletion;
            t = t * t * t * (t * (6f * t - 15f) + 10f);
            scoreTextRects[id].localScale= Vector3.Lerp(originalScale, scale * Vector3.one, t);
            yield return new WaitForEndOfFrame();
        }
    }
}
