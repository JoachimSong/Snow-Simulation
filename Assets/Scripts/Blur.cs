using UnityEngine;

public class Blur : MonoBehaviour
{
    public enum BlurType
    {
        GaussianBlur = 0,
        BilateralColorFilter = 1,
        BilateralNormalFilter = 2,
        BilateralDepthFilter = 3,
    }

    public Shader shader;
    public BlurType blurType = BlurType.GaussianBlur;
    [Range(1, 4)]
    public int BlurRadius = 1;
    [Range(0, 1.0f)]
    public float bilaterFilterStrength = 0.15f;
    Material material;
    Camera camera;

    void Awake()
    {
        material = new Material(shader);
        camera = GetComponent<Camera>();
    }

    void OnEnable()
    {
        camera.depthTextureMode |= DepthTextureMode.DepthNormals;
    }

    void OnDisable()
    {
        camera.depthTextureMode &= ~DepthTextureMode.DepthNormals;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        var tempRT = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
        material.SetFloat("_BilaterFilterFactor", bilaterFilterStrength);

        material.SetVector("_BlurRadius", new Vector4(BlurRadius, 0, 0, 0));
        Graphics.Blit(src, tempRT, material, (int)blurType);

        material.SetVector("_BlurRadius", new Vector4(0, BlurRadius, 0, 0));
        Graphics.Blit(tempRT, dest, material, (int)blurType);

        RenderTexture.ReleaseTemporary(tempRT);
    }
}