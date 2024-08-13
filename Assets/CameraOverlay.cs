using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(Camera))]
public class CameraOverlay : MonoBehaviour
{
    public Renderer r;

    private void OnPreRender()
    {
        if (XRSettings.enabled)
            XRSettings.useOcclusionMesh = false; // Need to disable occlusion mesh for mirror camera
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        RenderTextureDescriptor desc;
        if (XRSettings.enabled)
            desc = XRSettings.eyeTextureDesc;
        else
            desc = new RenderTextureDescriptor(Screen.width, Screen.height);

        RenderTexture rt = RenderTexture.GetTemporary(desc);
        Graphics.Blit(source, rt);
        Graphics.Blit(rt, destination);
        r.material.SetTexture("_ReflectionTex", rt);
        RenderTexture.ReleaseTemporary(rt);
    }

    private void OnPostRender()
    {
        if (XRSettings.enabled)
            XRSettings.useOcclusionMesh = true;
    }
}