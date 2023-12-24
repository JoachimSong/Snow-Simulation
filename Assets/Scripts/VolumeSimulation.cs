using System;
using UnityEngine;
using UnityEngine.Rendering;

public class VolumeSimulation: MonoBehaviour
{
    public Material material;
    public ComputeShader cs;
    public RenderTexture volumeTexture;
    public LayerMask layer;

    private const int THREAD_X = 8;
    private const int THREAD_Y = 8;
    private const int THREAD_Z = 8;
    private int _kernelRender;
    private int _kernelClear;
    private int _groupX;
    private int _groupY;
    private int _groupZ;
    private GameObject cube;

    public void Init(Vector3Int gridCount,
        float gridSize, Vector3 worldPos, int particleCount, int particleCountPerDim)
    {
        _groupX = (int)Math.Ceiling((float)particleCountPerDim / THREAD_X);
        _groupY = (int)Math.Ceiling((float)particleCountPerDim / THREAD_Y);
        _groupZ = (int)Math.Ceiling((float)particleCountPerDim / THREAD_Z);
        //_groupX = particleCountPerDim / THREAD_X;
        //_groupY = particleCountPerDim / THREAD_Y;
        //_groupZ = particleCountPerDim / THREAD_Z;

        _kernelRender = cs.FindKernel("Render");
        _kernelClear = cs.FindKernel("Clear");

        volumeTexture = new RenderTexture(gridCount.x, gridCount.y, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        volumeTexture.dimension = TextureDimension.Tex3D;
        volumeTexture.volumeDepth = gridCount.z;
        volumeTexture.useMipMap = false;
        volumeTexture.enableRandomWrite = true;
        volumeTexture.wrapMode = TextureWrapMode.Clamp;
        volumeTexture.filterMode = FilterMode.Bilinear;
        volumeTexture.Create();

        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<MeshRenderer>().sharedMaterial = material;
        cube.GetComponent<BoxCollider>().enabled = false;
        cube.layer = LayerMask.NameToLayer("Object"); ;
        cube.transform.position = worldPos;
        cube.transform.localScale = new Vector3(gridCount.x, gridCount.y, gridCount.z) * gridSize;

        cs.SetVector("gridCount", new Vector3(gridCount.x, gridCount.y, gridCount.z));
        cs.SetFloat("gridSize", gridSize);
        cs.SetVector("gridPosition", cube.transform.position);
        cs.SetInt("particleCount", particleCount);
        cs.SetInt("particleCountPerDim", particleCountPerDim);
        cs.SetTexture(_kernelRender, "volumeTexture", volumeTexture);
        cs.SetTexture(_kernelClear, "volumeTexture", volumeTexture);

        material.SetVector("volumePosition", cube.transform.position);
        material.SetVector("volumeSize", cube.transform.localScale);
        material.SetTexture("volumeTexture", volumeTexture);
    }

    public void Dispose()
    {
        if (cube != null)
        {
            GameObject.DestroyImmediate(cube);
            cube = null;
        }
    }
    //public void ClearRenderTexture()
    //{
    //    RenderTexture curRT = RenderTexture.active;
    //    RenderTexture.active = volumeTexture;
    //    GL.Clear(true, true, Color.clear);
    //    RenderTexture.active = curRT;
    //}
    //void ClearRenderTexture2()
    //{
    //    RenderTexture tmpRT = RenderTexture.GetTemporary(volumeTexture.width, volumeTexture.height, 0, volumeTexture.format);
    //    Graphics.Blit(null, volumeTexture);
    //    RenderTexture.ReleaseTemporary(tmpRT);
    //}
    //void ClearRenderTexture3()
    //{
    //    RenderTexture currentRT = RenderTexture.active;
    //    RenderTexture.active = volumeTexture;
    //    GL.Clear(true, true, Color.clear);
    //    RenderTexture.active = currentRT;
    //    volumeTexture.DiscardContents();
    //}

    public void Render(ComputeBuffer _positionBuffer, ComputeBuffer _sizeBuffer, ComputeBuffer _stateBuffer)
    {
        cs.SetBuffer(_kernelRender, "positions", _positionBuffer);
        cs.SetBuffer(_kernelRender, "sizes", _sizeBuffer);
        cs.SetBuffer(_kernelRender, "states", _stateBuffer);
        cs.Dispatch(_kernelRender, _groupX, _groupY, _groupZ);
    }
    public void Clear(ComputeBuffer _positionBuffer, ComputeBuffer _sizeBuffer, ComputeBuffer _stateBuffer)
    {
        cs.SetBuffer(_kernelClear, "positions", _positionBuffer);
        cs.SetBuffer(_kernelClear, "sizes", _sizeBuffer);
        cs.SetBuffer(_kernelClear, "states", _stateBuffer);
        cs.Dispatch(_kernelClear, _groupX, _groupY, _groupZ);
    }
    public void RenderTest(RenderTexture rt)
    {
        material.SetTexture("volumeTexture", rt);

    }
}
