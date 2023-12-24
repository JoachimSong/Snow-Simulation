using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;


public class GameSceneUI : MonoBehaviour
{
    public static GameSceneUI instance;

    [Header("背包")]
    public GameObject bag;
    public Button closeBagButton;
    public GameObject grid;
    public List<GameObject> slots;
    public bool bagOpen;
    public AudioMixer audioMixer;
    public ParticleSimulation particleSimulation;
    public Camera mainCamera;


    [Header("游戏界面")]
    public Image currentItemImage;


    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        closeBagButton.onClick.AddListener(delegate () { BagControl(); });
        CreateSlot();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetKeyDown(KeyCode.C))
            BagControl();
    }
    void BagControl()
    {
        bagOpen = !bagOpen;
        bag.SetActive(bagOpen);
    }
    void CreateSlot()
    {
        foreach(GameObject slot in slots)
        {
            GameObject newSlot = Instantiate(slot, instance.grid.transform.position, Quaternion.identity);
            newSlot.gameObject.transform.SetParent(instance.grid.transform);
        }
    }

    public void SetVolume(float value)
    {
        audioMixer.SetFloat("MainVolume", value);
    }
    public void SetTemp(float value)
    {
        particleSimulation.SetTemp(value);
    }
    public void RenderParticle(bool value)
    {
        particleSimulation.renderVolume = !value;
        mainCamera.GetComponent<BlendTexture>().enabled = !value;
        if (value)
            mainCamera.GetComponent<Camera>().cullingMask |= (1 << 0);
        else
            mainCamera.GetComponent<Camera>().cullingMask &= ~(1 << 0);
    }
}
