using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class SSVEP : MonoBehaviour
{
    [SerializeField] private float frequency;
    [SerializeField] private bool sine;
    [SerializeField] private bool imageStaticOnZeroFreq = false;

    public RawImage objectRenderer;
    private float elapsedTime;

    private void Start()
    {
        //objectRenderer = GetComponent<RawImage>();
        elapsedTime = 0f;
        // if the frequency is 0, make the object invisible
        if (frequency == 0f) objectRenderer.enabled = imageStaticOnZeroFreq;
    }

    void Update()
    {
        // if the frequency is 0, do nothing
        if (frequency == 0f) return;
        // Add the time since the last frame to the elapsed time
        elapsedTime += Time.deltaTime;
        float t = elapsedTime * frequency;

        Debug.Log(Time.deltaTime);
        float value;
        if (sine) value = (Mathf.Sin(t * Mathf.PI * 2f) + 1f) / 2f; // sine wave
        else value = Mathf.Repeat(t / 2, 1f) < 0.5f ? 0f : 1f; // square wave

        // Change the alpha value
        objectRenderer.color = new Color(255f, 255f, 255f, value);
    }

    public void StopSSVEP()
    {
        objectRenderer.color = new Color(1f, 1f, 1f, 1f);
    }

}