using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;
    public GameObject playerModel;
    public GameObject playerHand;
    [SerializeField] float runSpeed;
    [SerializeField] float jumpSpeed;
    [SerializeField] float throwSpeed;
    public GameObject item;
    public string itemType;
    Rigidbody rb;
    Animator anim;
    RaycastHit hitInfo;
    public VoidEventSO mouseRightButtonEvent;
    public VoidEventSO mouseLeftButtonEvent;
    public RaycastHitEventSO createBlockEvent;
    public RaycastHitEventSO destroyBlockEvent;
    public GameObject gameManager;
    public AudioSource FXSource;
    public AudioClip destroyAudioClip;
    public AudioClip createAudioClip;
    public AudioClip throwAudioClip;
    private GameController gameControllerScript;

    public GameObject ballPrefab;
    public float throwForce;

    private GameObject currentBall;

    private void Awake()
    {
        gameControllerScript = gameManager.GetComponent<GameController>();
    }

    private void OnEnable()
    {
        createBlockEvent.OnEventRaised += CreateBlock;
        destroyBlockEvent.OnEventRaised += DestroyBlock;
    }


    private void OnDisable()
    {
        createBlockEvent.OnEventRaised -= CreateBlock;
        destroyBlockEvent.OnEventRaised -= DestroyBlock;
    }
    void Start()
    {
        if (instance != null)
            Destroy(gameObject);
        instance = this;
        rb = GetComponent<Rigidbody>();
        anim = playerModel.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameSceneUI.instance.bagOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
        if (Input.GetMouseButtonDown(0)) 
        {
            if (itemType == "throw")
                ThrowItem();
            else if (itemType == "put" && gameControllerScript.isBuild)
            {
                //PutItem();
                mouseLeftButtonEvent.RaiseEvent();
            }
                
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (itemType == "put" && gameControllerScript.isBuild)
            {
                mouseRightButtonEvent.RaiseEvent();
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (currentBall == null)
            {
                // 在玩家位置的前上方生成球体 放置在边缘处与墙体发生挤压
                Vector3 forwardDirection = transform.forward;
                forwardDirection.y = 0;
                forwardDirection.Normalize();

                Vector3 spawnPosition = transform.position + new Vector3(0f, 1.5f, 0f) + forwardDirection * 0.5f;
                currentBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
                currentBall.SetActive(true);

                // 施加斜抛力给球体
                Rigidbody ballRigidbody = currentBall.GetComponent<Rigidbody>();
                Vector3 throwDirection = Quaternion.Euler(-30f, 0f, 0f) * transform.forward;
                ballRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            }
            else
            {
                // 销毁当前球体
                Destroy(currentBall);

                // 在玩家位置的前上方生成新球体 放置在边缘处与墙体发生挤压
                Vector3 forwardDirection = transform.forward;
                forwardDirection.y = 0;
                forwardDirection.Normalize();

                Vector3 spawnPosition = transform.position + new Vector3(0f, 1.5f, 0f) + forwardDirection * 0.5f;
                currentBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
                currentBall.SetActive(true);

                // 施加斜抛力给球体
                Rigidbody ballRigidbody = currentBall.GetComponent<Rigidbody>();
                Vector3 throwDirection = Quaternion.Euler(-30f, 0f, 0f) * transform.forward;
                ballRigidbody.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            }
            FXSource.clip = throwAudioClip;
            FXSource.Play();
        }
    }

    private void FixedUpdate()
    {
        playerModel.transform.localPosition = new Vector3(0, 0, 0);
        if (!GameSceneUI.instance.bagOpen)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            if (vertical == 1)
                playerModel.transform.localRotation = Quaternion.Euler(0, 45 * horizontal, 0);

            else if (vertical == 0)
                playerModel.transform.localRotation = Quaternion.Euler(0, 90 * horizontal, 0);

            else if (vertical == -1)
                playerModel.transform.localRotation = Quaternion.Euler(0, 180 - 45 * horizontal, 0);

            Vector3 movement = (transform.forward * vertical + transform.right * horizontal) * runSpeed;
            if (Input.GetAxis("Jump") == 1)
                movement.y += jumpSpeed;
            rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
            anim.SetFloat("speed", movement.magnitude);
            anim.SetBool("putItem", false);
            anim.SetBool("throwItem", false);
        }
        else
        {
            anim.SetFloat("speed", 0);
        }
    }
    public void PutItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Physics.Raycast(ray, out hitInfo);
        if (hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
            {
                Vector3 point = hitInfo.point;
                point.y += item.transform.localScale.y / 2;
                Instantiate(item, point, Quaternion.identity);
                anim.SetBool("putItem", true);
            }
        }
    }
    public void ThrowItem()
    {
        anim.SetBool("throwItem", true);
        Invoke("Throw", 0.2f);
    }
    void Throw()
    {
        playerModel.transform.localRotation = Quaternion.Euler(0, 0, 0);
        GameObject go = Instantiate(item, playerHand.transform.position, Quaternion.identity);
        Vector3 dir = transform.forward + Mathf.Sin(-CameraController.instance.xRotation / 180 * Mathf.PI) * Vector3.up;
        go.GetComponent<Rigidbody>().velocity = dir * throwSpeed;
        Destroy(go, 10);
    }

    private void DestroyBlock(RaycastHit hit)
    {
        if (hit.collider.CompareTag("SnowBlock"))
        {
            FXSource.clip = destroyAudioClip;
            FXSource.Play();
            Destroy(hit.collider.gameObject);
        }
    }
    private void CreateBlock(RaycastHit hit)
    {
        //Item tmp = bagController.toolSlotGridBottom.transform.GetChild(currentToolNum).GetComponent<Slot>().slotItem;
        //if (tmp != null)
        //{
        //    currentBlock = tmp.itemPrefab;
        //}
        //else
        //{
        //    currentBlock = null;
        //}
        if (item == null)
        {
            return;
        }
        Vector3 position = hit.point;
        Vector3 blockPosition = hit.collider.transform.position;
        Vector3 createPosition = blockPosition;
        Vector3 dir = position - blockPosition;
        if (hit.collider.CompareTag("Ground"))
        {
            createPosition = position;
            createPosition.x = Mathf.RoundToInt(createPosition.x);
            createPosition.z = Mathf.RoundToInt(createPosition.z);
            createPosition.y = Mathf.RoundToInt(createPosition.y) + 0.5f;
            Instantiate(item, createPosition, Quaternion.identity);
            anim.SetBool("putItem", true);
            FXSource.clip = createAudioClip;
            FXSource.Play();
            return;
        }
        if (!hit.collider.CompareTag("SnowBlock"))
        {
            return;
        }
        
        float normalDir = Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y), Mathf.Abs(dir.z));
        if (normalDir == Mathf.Abs(dir.x))
        {
            //Debug.Log("x");
            if (dir.x > 0)
            {
                createPosition.x += 1.0f;
            }
            else
            {
                createPosition.x -= 1.0f;
            }
        }
        else if (normalDir == Mathf.Abs(dir.y))
        {
            //Debug.Log("y");
            if (dir.y > 0)
            {
                createPosition.y += 1.0f;
            }
            else
            {
                createPosition.y -= 1.0f;
            }
        }
        else if (normalDir == Mathf.Abs(dir.z))
        {
            //Debug.Log("z");
            if (dir.z > 0)
            {
                createPosition.z += 1.0f;
            }
            else
            {
                createPosition.z -= 1.0f;
            }
        }

        //Debug.Log(normalDir);
        //Debug.Log(position);
        //Debug.Log(blockPosition);
        if (Mathf.Abs(createPosition.x - transform.position.x) < 0.95f && Mathf.Abs(createPosition.z - transform.position.z) < 0.95f && Mathf.Abs(createPosition.y - transform.position.y) < 1.39f)
        {
            return;
        }
        else
        {
            GameObject go = Instantiate(item, createPosition, Quaternion.identity);
            anim.SetBool("putItem", true);
            FXSource.clip = createAudioClip;
            FXSource.Play();
            //go.transform.parent = allBlocks.transform;
        }

    }
}
