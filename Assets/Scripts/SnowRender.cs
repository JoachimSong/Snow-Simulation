using UnityEngine;

public class SnowRender : MonoBehaviour
{
	public Shader depthBlurShader;
	public Shader snowRenderShader;
	public GameObject mainCamera;
	public float bilaterFilterFactor = 0.5f;
	public float blurRadius = 1.5f;

	Material blurMaterial;
	Material renderMaterial;

	Camera camera;
	void Awake()
	{
		blurMaterial = new Material(depthBlurShader);
		renderMaterial = new Material(snowRenderShader);
		camera = GetComponent<Camera>();
		transform.parent = mainCamera.transform;
	}
	void OnEnable()
	{
		camera.depthTextureMode |= DepthTextureMode.DepthNormals;
		Shader.SetGlobalFloat("_CameraDepthTextureFormat", (float)RenderTextureFormat.R16);
	}
	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		RenderTexture tempRT = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
		RenderTexture tempRT2 = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
		RenderTexture tempRT3 = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
		RenderTexture tempRT4 = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
		RenderTexture tempRT5 = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);
		RenderTexture tempRT6 = RenderTexture.GetTemporary(src.width, src.height, 0, src.format);


		//depth
		Graphics.Blit(src, tempRT, blurMaterial, 0);
		//blur

		blurMaterial.SetFloat("_BilaterFilterFactor", bilaterFilterFactor);
		blurMaterial.SetVector("_BlurRadius", new Vector4(blurRadius, 0, 0, 0));
		Graphics.Blit(tempRT, tempRT2, blurMaterial, 1);
		blurMaterial.SetVector("_BlurRadius", new Vector4(0, blurRadius, 0, 0));
		Graphics.Blit(tempRT2, tempRT3, blurMaterial, 1);
		//depth to normal
		Graphics.Blit(tempRT3, tempRT4, blurMaterial, 2);
		//render
		renderMaterial.SetTexture("_DepthTex", tempRT3);
		renderMaterial.SetTexture("_NormalTex", tempRT4);
		Graphics.Blit(tempRT5, tempRT6, renderMaterial, 0);

		mainCamera.GetComponent<BlendTexture>().depthTexture = tempRT3;
		mainCamera.GetComponent<BlendTexture>().snowTexture = tempRT6;

		RenderTexture.ReleaseTemporary(tempRT);
		RenderTexture.ReleaseTemporary(tempRT2);
		RenderTexture.ReleaseTemporary(tempRT3);
		RenderTexture.ReleaseTemporary(tempRT4);
		RenderTexture.ReleaseTemporary(tempRT5);
		RenderTexture.ReleaseTemporary(tempRT6);

	}
	void FixedUpdate()
    {
		
    }
}