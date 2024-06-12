using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ssvep_stimuli : MonoBehaviour
{
    public RawImage rawImage; // The RawImage component
    public float minLuminance = 0.5f; // Minimum luminance level
    public float maxLuminance = 1.0f; // Maximum luminance level
    public float frequency = 1.0f; // Frequency in Hz

    private float timer;
    private Color baseColor;

    void Start()
    {
        baseColor = rawImage.color; // Store the initial color of the RawImage
    }

    void Update()
    {
        timer += Time.deltaTime;
        float phase = Mathf.Sin(timer * frequency * 2.0f * Mathf.PI);
        float luminance = Mathf.Lerp(minLuminance, maxLuminance, (phase + 1.0f) / 2.0f);
        rawImage.color = baseColor * luminance;
    }
}
