using UnityEngine;

public class DepthNormalTexture : MonoBehaviour
{
	public enum TextureType
	{
		Depth = 0,
		Normal = 1,
	}
	public Shader shader;
	public TextureType textureType = TextureType.Depth;
	Material material;
	Camera camera;
    void Awake()
    {
		material = new Material(shader);
		camera= GetComponent<Camera>();
	}
    void OnEnable()
	{
		camera.depthTextureMode |= DepthTextureMode.DepthNormals;
	}
	void OnRenderImage(RenderTexture src, RenderTexture dest)
	{
		if (material != null)
		{
			Graphics.Blit(src, dest, material,(int)textureType);
		}
		else
		{
			Graphics.Blit(src, dest);
		}
	}
}