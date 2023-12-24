
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using System;


public class ParticleSimulation : MonoBehaviour
{
    public List<GameObject> blocks;
    public List<GameObject> balls;

    public float temp = 10;
    public float particleSize = 0.15f;
    public float mass = 1;
    public float Cd = 0.5f;
    public float stepTime = 0.002f;
    public float tangentialCOF = 0.1f;
    public float initialDensity = 100.0f;
    public ComputeShader cs;
    public GameObject ball;
    public GameObject cubeCollision;
    public GameObject playerCollision;//add 6.11
    public float playerRadius;
    public Vector3 worldPos;
    public Vector3Int gridCount;//小网格数量
    public float gridSize = 0.15f;
    public VolumeSimulation volumeSimulation;
    public TempSimulation tempSimulation;


    private int particleCount;
    private int particleCountPerDim;
    private Vector3 worldSize;
    private ComputeBuffer _blockBuffer;
    private ComputeBuffer _ballBuffer;
    private ComputeBuffer _positionBuffer;
    private ComputeBuffer _velocityBuffer;
    private ComputeBuffer _stateBuffer;
    private ComputeBuffer _sizeBuffer;
    private ComputeBuffer _parametersBuffer;
    private ComputeBuffer _particleHashBuffer;
    private ComputeBuffer _particleGridBuffer;
    private ComputeBuffer _cellStartEndBuffer;
    private ComputeBuffer _brokenBondsBuffer;
    private ComputeBuffer _totalVolume;
    private RenderTexture tempTexture;


    private const int THREAD_X = 8;
    private const int THREAD_Y = 8;
    private const int THREAD_Z = 8;
    private int _kernelInit;
    private int _kernelParticleHash;
    private int _kernelBitonicSort;
    private int _kernelFindCellStart;
    private int _kernelStepVelocity;
    private int _kernelStepPosition;
    private int _kernelStepQ;
    private int _groupX;
    private int _groupY;
    private int _groupZ;
    private int iterations;
    private int hashBufferSize;
    private int sortThreadSize;
    private int totalGrid;

    //渲染相关
    public Mesh mesh;
    public Material snowIceMaterial;
    public bool renderVolume;

    private int subMeshIndex = 0;
    private ComputeBuffer _argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

    public AsyncGPUReadbackRequest Initialize()
    {
        GetBlocksInScene();
        ComputeParticleCount();
        _groupX = (int)Math.Ceiling((float)particleCountPerDim / THREAD_X);
        _groupY = (int)Math.Ceiling((float)particleCountPerDim / THREAD_Y);
        _groupZ = (int)Math.Ceiling((float)particleCountPerDim / THREAD_Z);

        iterations = Mathf.CeilToInt(Mathf.Log(particleCount, 2));
        hashBufferSize = 1 << iterations;
        sortThreadSize = hashBufferSize >> 1;
        Debug.Log(sortThreadSize);
        _kernelInit = cs.FindKernel("Init");
        _kernelStepVelocity = cs.FindKernel("StepV");
        _kernelStepPosition = cs.FindKernel("StepP");
        _kernelStepQ = cs.FindKernel("StepQ");
        _kernelParticleHash = cs.FindKernel("CalcHash");
        _kernelBitonicSort = cs.FindKernel("BitonicSort");
        _kernelFindCellStart = cs.FindKernel("FindCellStart");

        cs.SetInt("particleCount", particleCount);
        cs.SetInt("particleCountPerDim", particleCountPerDim);
        cs.SetFloat("particleSize", particleSize);
        cs.SetFloat("mass", mass);
        cs.SetFloat("Cd", Cd);
        cs.SetFloat("initialDensity", initialDensity);
        float initialTotalVolume = particleCount * 3.14f * particleSize * particleSize * particleSize / 6f;
        cs.SetFloat("initialTotalVolume", initialTotalVolume);
        cs.SetFloat("tangentialCOF", tangentialCOF);
        cs.SetInts("gridCount", gridCount.x, gridCount.y, gridCount.z);
        cs.SetFloat("gridSize", gridSize);
        cs.SetInt("sortThreadSize", sortThreadSize);
        cs.SetInt("cellOffset", hashBufferSize - particleCount);
        Vector3 gridStartPos = new Vector3(worldPos.x - gridCount.x * gridSize / 2, worldPos.y - gridCount.y * gridSize / 2, worldPos.z - gridCount.z * gridSize / 2);
        cs.SetFloats("gridStartPos", gridStartPos.x, gridStartPos.y, gridStartPos.z);
        Debug.Log(gridStartPos);

        _positionBuffer = new ComputeBuffer(particleCount, 12);
        _velocityBuffer = new ComputeBuffer(particleCount, 12);
        _brokenBondsBuffer = new ComputeBuffer(particleCount, 8);
        _sizeBuffer = new ComputeBuffer(particleCount, 12);
        _stateBuffer = new ComputeBuffer(particleCount, 16);
        _particleHashBuffer = new ComputeBuffer(hashBufferSize, 8);//存储粒子与网格下标对应关系，并用于后续排序
        _particleGridBuffer = new ComputeBuffer(particleCount, 12);//存储粒子与网格编号对应关系
        totalGrid = gridCount.x * gridCount.y * gridCount.z;
        _cellStartEndBuffer = new ComputeBuffer(totalGrid, 8);//大小等同于网格数量
        uint[] resultArray = new uint[hashBufferSize * 2];
        _particleHashBuffer.SetData(resultArray);
        _totalVolume = new ComputeBuffer(1, 4);
        uint[] totalVolumeArray = new uint[1];
        totalVolumeArray[0] = (uint)Mathf.RoundToInt(initialTotalVolume * 100000);
        _totalVolume.SetData(totalVolumeArray);
        void setBufferForKernet(int k)
        {
            cs.SetBuffer(k, "particleHash", _particleHashBuffer);
            cs.SetBuffer(k, "particleGrid", _particleGridBuffer);
            cs.SetBuffer(k, "cellStartEnd", _cellStartEndBuffer);
            cs.SetBuffer(k, "haveBrokenBonds", _brokenBondsBuffer);
            cs.SetBuffer(k, "velocities", _velocityBuffer);
            cs.SetBuffer(k, "positions", _positionBuffer);
            cs.SetBuffer(k, "sizes", _sizeBuffer);
            cs.SetBuffer(k, "states", _stateBuffer);
            cs.SetBuffer(k, "totalVolume", _totalVolume);
        }
        setBufferForKernet(_kernelInit); 
        setBufferForKernet(_kernelStepVelocity);
        setBufferForKernet(_kernelStepPosition);
        setBufferForKernet(_kernelStepQ);
        setBufferForKernet(_kernelParticleHash);
        setBufferForKernet(_kernelBitonicSort);
        setBufferForKernet(_kernelFindCellStart);

        SetBlock();
        DestroyBlocksInScene();
        SetBall();
        InitRender();
        InitTemp();
        cs.Dispatch(_kernelInit, _groupX, _groupY, _groupZ);

        return AsyncGPUReadback.Request(_positionBuffer, (req) => {
            if (req.hasError)
            {
                Debug.LogError("Init error");
            }
            if (req.done && !req.hasError)
            {
                //_initialized = true;
            }
        });
    }


    void ComputeParticleCount()
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            particleCount += 
                (int)(blocks[i].transform.localScale.x / particleSize + 1) * 
                (int)(blocks[i].transform.localScale.y / particleSize + 1) *
                (int)(blocks[i].transform.localScale.z / particleSize + 1);
        }
        for (int i = 0; i < balls.Count; i++)
        {
            particleCount +=
                (int)(balls[i].transform.localScale.x / particleSize + 1) *
                (int)(balls[i].transform.localScale.y / particleSize + 1) *
                (int)(balls[i].transform.localScale.z / particleSize + 1);
        }
        Debug.Log(particleCount);
        particleCountPerDim = (int)Math.Ceiling(Math.Pow(particleCount, 1.0f / 3));
        particleCountPerDim = (int)Math.Ceiling((float)particleCountPerDim / THREAD_X) * 8;
        Debug.Log(particleCountPerDim);
    }
    void SetBlock()
    {
        cs.SetInt("blockCount", blocks.Count);
        Vector3[] blockData = new Vector3[blocks.Count * 3];
        for (int i = 0; i < blocks.Count; i++)
        {
            int index = i * 3;
            blockData[index] = blocks[i].transform.position;
            blockData[index + 1] = blocks[i].transform.rotation.eulerAngles;
            blockData[index + 2] = new Vector3(
                (int)(blocks[i].transform.localScale.x / particleSize + 1),
                (int)(blocks[i].transform.localScale.y / particleSize + 1),
                (int)(blocks[i].transform.localScale.z / particleSize + 1));
        }

        _blockBuffer = new ComputeBuffer(particleCount, 36);
        _blockBuffer.SetData(blockData);
        cs.SetBuffer(_kernelInit, "blocks", _blockBuffer);
    }
    void SetBall()
    {
        cs.SetInt("ballCount", balls.Count);
        Debug.Log(balls.Count);
        Vector3[] ballData = new Vector3[balls.Count * 4];
        for (int i = 0; i < balls.Count; i++)
        {
            int index = i * 4;
            ballData[index] = balls[i].transform.position;
            ballData[index + 1] = balls[i].transform.rotation.eulerAngles;
            ballData[index + 2] = balls[i].transform.localScale;
            ballData[index + 3] = new Vector3(
                (int)(balls[i].transform.localScale.x / particleSize + 1) *
                (int)(balls[i].transform.localScale.y / particleSize + 1) *
                (int)(balls[i].transform.localScale.z / particleSize + 1), 0, 0);
        }
        _ballBuffer = new ComputeBuffer(particleCount, 48);
        _ballBuffer.SetData(ballData);
        cs.SetBuffer(_kernelInit, "balls", _ballBuffer);
    }
    void InitRender()
    {
        _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        if (mesh != null)
        {
            subMeshIndex = Mathf.Clamp(subMeshIndex, 0, mesh.subMeshCount - 1);
            args[0] = (uint)mesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)particleCount;
            args[2] = (uint)mesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)mesh.GetBaseVertex(subMeshIndex);
        }
        else
            args[0] = args[1] = args[2] = args[3] = 0;
        _argsBuffer.SetData(args);

        volumeSimulation.Init(gridCount, gridSize, worldPos, particleCount, particleCountPerDim);
    }
    void InitTemp()
    {
        tempSimulation.Init(gridCount, gridSize, worldPos);
        tempTexture = tempSimulation.tempTexture;
        cs.SetTexture(_kernelStepQ, "tempTexture", tempTexture);
    }
    public void SetTemp(float value)
    {
        tempTexture = tempSimulation.SetTemp(value);
        cs.SetTexture(_kernelStepQ, "tempTexture", tempTexture);
    }
    void Render()
    {
        worldSize.x = gridSize * gridCount.x;
        worldSize.y = gridSize * gridCount.y;
        worldSize.z = gridSize * gridCount.z;
        snowIceMaterial.SetBuffer("positions", _positionBuffer);
        snowIceMaterial.SetBuffer("sizes", _sizeBuffer);
        snowIceMaterial.SetBuffer("states", _stateBuffer);
        Graphics.DrawMeshInstancedIndirect(mesh, subMeshIndex, snowIceMaterial, new Bounds(worldPos, worldSize), _argsBuffer);
        //volumeSimulation.RenderTest(tempTexture);
    }
    
    void UpdateBall()
    {
        if(ball == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("BallCollision");
            if(obj != null )
            {
                ball = obj;
            }
            else
            {
                return;
            }
        }
        var ballParams = (Vector4)ball.transform.position;
        ballParams.w = ball.transform.localScale.x / 2;
        cs.SetVector("collisionBall", ballParams);
    }
    void UpdateCube()
    {
        //// 获取 cube 对象的位置和缩放信息
        //Vector3 cubePosition = cubeCollision.transform.position;
        //Vector3 cubeScale = cubeCollision.transform.localScale;

        //// 将 cube 的位置和缩放信息转换为 Vector4 类型的参数
        //Vector4 cubeParams = new Vector4(cubePosition.x, cubePosition.y, cubePosition.z, cubeScale.x / 2);

        // 将 cubeParams 传递给 ComputeShader 中的 collisionCube 参数
        if (cubeCollision == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("CubeCollision");
            if (obj != null)
            {
                cubeCollision = obj;
            }
            else
            {
                return;
            }
        }
        var cubeParams = (Vector4)cubeCollision.transform.position;
        cubeParams.w = cubeCollision.transform.localScale.x / 2;
        cs.SetVector("collisionCube", cubeParams);
    }
    void UpdatePlayer()
    {
        if (playerCollision == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null)
            {
                playerCollision = obj;
            }
            else
            {
                return;
            }
        }
        var playerParams = (Vector4)playerCollision.transform.position;
        playerParams.w = playerRadius;
        cs.SetVector("collisionPlayer", playerParams);
    }
    public IEnumerator StartAsync()
    {
        yield return Initialize();
        float dt = 0;
        float minDt = stepTime;
        while (true)
        {
            dt += Time.deltaTime;
            while (dt > minDt)
            {
                cs.SetFloat("dt", minDt);

                //计算所在网格  当前初始化很慢
                //uint[] hashBufferArray = new uint[hashBufferSize * 2];
                //_particleHashBuffer.SetData(hashBufferArray);
                cs.Dispatch(_kernelParticleHash, _groupX, _groupY, _groupZ);

                //排序
                for (int i = 0; i < iterations; i++)
                {
                    cs.SetInt("outerLoop", i);
                    for (int j = i; j >= 0; j--)
                    {
                        cs.SetInt("innerLoop", j);
                        cs.Dispatch(_kernelBitonicSort, _groupX, _groupY, _groupZ);
                    }
                }

                //找邻居准备工作
                uint[] cellBufferArray = new uint[totalGrid * 2];
                _cellStartEndBuffer.SetData(cellBufferArray);
                cs.Dispatch(_kernelFindCellStart, _groupX, _groupY, _groupZ);

                if (renderVolume)
                {
                    volumeSimulation.Clear(_positionBuffer, _sizeBuffer, _stateBuffer);
                }
                cs.Dispatch(_kernelStepVelocity, _groupX, _groupY, _groupZ);
                cs.Dispatch(_kernelStepPosition, _groupX, _groupY, _groupZ);
                cs.Dispatch(_kernelStepQ, _groupX, _groupY, _groupZ);
                if (renderVolume)
                {
                    volumeSimulation.Render(_positionBuffer, _sizeBuffer, _stateBuffer);
                }
                else
                    volumeSimulation.Clear(_positionBuffer, _sizeBuffer, _stateBuffer);
                //uint count = 0;
                //uint[] bondBufferArray = new uint[particleCount * 2];
                //_brokenBondsBuffer.GetData(bondBufferArray);
                //for (int i = 1; i < particleCount * 2; i += 2)
                //{
                //    if (bondBufferArray[i] == 0) count++;
                //}
                //Debug.Log(count);

                dt -= minDt;
            }
            yield return null;
            AsyncGPUReadback.WaitAllRequests();
        }
    }
    public void Dispose()
    {
        if (_blockBuffer != null)
        {
            _blockBuffer.Release();
            _blockBuffer = null;
        }
        if (_ballBuffer != null)
        {
            _ballBuffer.Release();
            _ballBuffer = null;
        }
        if (_positionBuffer != null)
        {
            _positionBuffer.Release();
            _positionBuffer = null;
        }
        if (_velocityBuffer != null)
        {
            _velocityBuffer.Release();
            _velocityBuffer = null;
        }
        if (_stateBuffer != null)
        {
            _stateBuffer.Release();
            _stateBuffer = null;
        }
        if (_parametersBuffer !=null)
        {
            _parametersBuffer.Release();
            _parametersBuffer = null;
        }
        if(_particleHashBuffer != null) 
        { 
            _particleHashBuffer.Release();
            _particleHashBuffer = null;
        }
        if(_cellStartEndBuffer != null)
        {
            _cellStartEndBuffer.Release();
            _cellStartEndBuffer = null;
        }
        if (_argsBuffer != null)
        {
            _argsBuffer.Release();
            _argsBuffer = null;
        }
        if (_particleGridBuffer != null)
        {
            _particleGridBuffer.Release();
            _particleGridBuffer = null;
        }
        if(volumeSimulation!=null)
        {
            volumeSimulation.Dispose();
            volumeSimulation = null;
        }
        if (_sizeBuffer != null)
        {
            _sizeBuffer.Release();
            _sizeBuffer = null;
        }
        if (_brokenBondsBuffer != null)
        {
            _brokenBondsBuffer.Release();
            _brokenBondsBuffer = null;
        }
        if(_totalVolume != null)
        {
            _totalVolume.Dispose();
            _totalVolume = null;
        }
    }

    void Start()
    {
        UpdateBall();
        UpdateCube();
        UpdatePlayer();
        StartCoroutine(StartAsync());
        
    }

    void Update()
    {
        UpdateBall();
        UpdateCube();
        UpdatePlayer();
        Render();
    }

    void OnDestroy()
    {
        Dispose();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(worldPos, new Vector3(gridSize * gridCount.x, gridSize * gridCount.y, gridSize * gridCount.z));
    }
    void GetBlocksInScene()
    {
        GameObject[] foundSnowBlocks = GameObject.FindGameObjectsWithTag("SnowBlock");
        foreach (GameObject snowBlock in foundSnowBlocks)
        {
            blocks.Add(snowBlock);
            MeshRenderer snowBlockRenderer = snowBlock.GetComponent<MeshRenderer>();
            BoxCollider snowBlockCollider = snowBlock.GetComponent<BoxCollider>();
            if (snowBlockRenderer != null)
            {
                snowBlockRenderer.enabled = false;
                snowBlockCollider.enabled = false;
            }
        }

    }
    void DestroyBlocksInScene()
    {
        foreach (GameObject snowBlock in blocks)
        {
            Destroy(snowBlock);
        }

        blocks.Clear();
    }
}
