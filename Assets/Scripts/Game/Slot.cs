using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Button selectButton;
    public GameObject item;
    public string type;
    // Start is called before the first frame update
    void Start()
    {
        selectButton.onClick.AddListener(delegate () { Select(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void Select()
    {
        PlayerController.instance.item = item;
        PlayerController.instance.itemType = type;
        GameSceneUI.instance.currentItemImage.sprite = gameObject.GetComponent<Image>().sprite;
    }
}
