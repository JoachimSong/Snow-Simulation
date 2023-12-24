using UnityEngine;

public class BlendTexture : MonoBehaviour
{
	public Shader shader;
	public RenderTexture snowTexture;
	public RenderTexture depthTexture;
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
	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		material.SetTexture("_DepthTex", depthTexture);
		material.SetTexture("_SnowTex", snowTexture);
		Graphics.Blit(src, dest, material, 0);
	}
}