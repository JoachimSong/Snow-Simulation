using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    public Transform player;
    public float sensitivity;
    public float distance;
    public float xRotation;
    public float yRotation;
    //放置方块相关
    private Ray ray;
    public float raycastRange;
    public VoidEventSO mouseRightButtonEvent;
    public VoidEventSO mouseLeftButtonEvent;
    public RaycastHitEventSO createBlockEvent;
    public RaycastHitEventSO destroyBlockEvent;

    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        instance = this;
    }
    void Start()
    {
        xRotation = 45;
        yRotation = 0;
    }

    void Update()
    {
        if(!GameSceneUI.instance.bagOpen)
        {
            float Mouse_X = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
            float Mouse_Y = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
            xRotation -= Mouse_Y;
            yRotation += Mouse_X;
            xRotation = Mathf.Clamp(xRotation, 0f, 90f);
            player.Rotate(Vector3.up * Mouse_X);
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            transform.position = player.position + distance * new Vector3(
                -Mathf.Cos(xRotation * Mathf.PI / 180) * Mathf.Sin(yRotation * Mathf.PI / 180),
                Mathf.Sin(xRotation * Mathf.PI / 180) + 0.5f,
                -Mathf.Cos(xRotation * Mathf.PI / 180) * Mathf.Cos(yRotation * Mathf.PI / 180));

            distance -= Input.GetAxis("Mouse ScrollWheel") * sensitivity * Time.deltaTime;
            distance = Mathf.Clamp(distance, 1, 10);
        }
        
    }

    private void OnEnable()
    {
        mouseLeftButtonEvent.OnEventRaised += GetDestroyRaycastHit;
        mouseRightButtonEvent.OnEventRaised += GetCreateRaycastHit;
    }


    private void OnDisable()
    {
        mouseLeftButtonEvent.OnEventRaised -= GetDestroyRaycastHit;
        mouseRightButtonEvent.OnEventRaised -= GetCreateRaycastHit;
    }

    private void GetDestroyRaycastHit()
    {
        
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, raycastRange))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            destroyBlockEvent.RaiseEvent(hit);
        }
    }

    private void GetCreateRaycastHit()
    {
        
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, raycastRange))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.red);
            createBlockEvent.RaiseEvent(hit);
        }
    }
}
