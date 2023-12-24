using System;
using UnityEngine;
using UnityEngine.Rendering;

public class TempSimulation : MonoBehaviour
{
    public ComputeShader cs;
    public RenderTexture tempTexture;
    const int THREAD_X = 8;
    const int THREAD_Y = 8;
    const int THREAD_Z = 8;
    int _kernelSetTemp;
    int _kernelAddHeatSource;
    int _groupX;
    int _groupY;
    int _groupZ;

    public void Init(Vector3Int gridCount, float gridSize, Vector3 worldPos)
    {
        _groupX = gridCount.x / THREAD_X;
        _groupY = gridCount.y / THREAD_Y;
        _groupZ = gridCount.z / THREAD_Z;
        if (gridCount.x % THREAD_X != 0) _groupX++;
        if (gridCount.y % THREAD_Y != 0) _groupY++;
        if (gridCount.z % THREAD_Z != 0) _groupZ++;

        _kernelSetTemp = cs.FindKernel("SetTemp");
        _kernelAddHeatSource = cs.FindKernel("AddHeatSource");

        tempTexture = new RenderTexture(gridCount.x, gridCount.y, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
        tempTexture.dimension = TextureDimension.Tex3D;
        tempTexture.volumeDepth = gridCount.z;
        tempTexture.useMipMap = false;
        tempTexture.enableRandomWrite = true;
        tempTexture.wrapMode = TextureWrapMode.Clamp;
        tempTexture.filterMode = FilterMode.Bilinear;
        tempTexture.Create();
        Debug.Log("temp");

        Debug.Log(gridCount);
        Debug.Log(gridSize);
        Debug.Log(worldPos);
        Vector3 gridStartPos = new Vector3(worldPos.x - gridCount.x * gridSize / 2, worldPos.y - gridCount.y * gridSize / 2, worldPos.z - gridCount.z * gridSize / 2);
        cs.SetVector("gridCount", new Vector3(gridCount.x, gridCount.y, gridCount.z));
        cs.SetFloat("gridSize", gridSize);
        cs.SetVector("gridStartPos", gridStartPos);

        cs.SetTexture(_kernelSetTemp, "tempTexture", tempTexture);
        cs.SetFloat("temp", 0);
        cs.Dispatch(_kernelSetTemp, _groupX, _groupY, _groupZ);
    }
    public RenderTexture AddHeatSource(Vector3 position, float radius, float temp)
    {
        cs.SetTexture(_kernelAddHeatSource, "tempTexture", tempTexture);
        cs.SetVector("sourcePosition", position);
        cs.SetFloat("sourceRadius", radius);
        cs.SetFloat("sourceTemp", temp);
        cs.Dispatch(_kernelAddHeatSource, _groupX, _groupY, _groupZ);
        return tempTexture;
    }
    public RenderTexture SetTemp(float temp)
    {
        cs.SetFloat("temp", temp);
        cs.Dispatch(_kernelSetTemp, _groupX, _groupY, _groupZ);
        return tempTexture;
    }

    public void Dispose()
    {

    }
}
