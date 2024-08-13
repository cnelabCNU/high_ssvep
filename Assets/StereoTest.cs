using UnityEngine;
using UnityEngine.XR;

public class StereoTest : MonoBehaviour
{
    public Camera m_Camera;

    void Awake()
    {
        //m_Camera = GetComponent<Camera>();

        RenderTexture _tex;

        _tex = new RenderTexture(128, 128 , 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        _tex.filterMode = FilterMode.Bilinear;
        //_tex.autoGenerateMips = _generateMipMaps;


        m_Camera.stereoTargetEye = StereoTargetEyeMask.Right;


        m_Camera.targetTexture = _tex;
        m_Camera.Render();


        Debug.Log("eye - " + m_Camera.stereoTargetEye);
        Debug.Log("stereoEnabled - " + m_Camera.stereoEnabled);
        Debug.Log("stereoActiveEye - " + m_Camera.stereoActiveEye);
    }

    void LateUpdate()
    {
        // Tested enabling UNITY_SINGLE_PASS_STEREO because it wasn't being set by the camera.
        // Shader.EnableKeyword("UNITY_SINGLE_PASS_STEREO");
        m_Camera.Render();
        // Shader.DisableKeyword("UNITY_SINGLE_PASS_STEREO");
    }
}