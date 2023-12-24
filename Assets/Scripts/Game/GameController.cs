using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject particleGeneratorPrefab;
    public GameObject particleGenerator;
    public Vector3 particleGeneratorPos;
    public bool isBuild;//1表示正在建造 0表示正在游戏中
    private void Awake()
    {
        isBuild = true;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && (isBuild == true))
        {
            GameObject[] foundSnowBlocks = GameObject.FindGameObjectsWithTag("SnowBlock");
            if (foundSnowBlocks.Length == 0)
            {
                return;
            }
            isBuild =false;
            particleGenerator = Instantiate(particleGeneratorPrefab, particleGeneratorPos, Quaternion.identity);
            GameSceneUI.instance.particleSimulation = particleGenerator.GetComponent<ParticleSimulation>();
        }
        if(Input.GetKeyDown(KeyCode.R) && (isBuild == false))
        {
            isBuild=true;
            Destroy(particleGenerator);
        }
    }
}
