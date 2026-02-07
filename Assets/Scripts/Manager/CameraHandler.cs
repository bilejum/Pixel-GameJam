using Cinemachine;
using UnityEngine;


public class CameraHandler : MonoBehaviour
{
    public static CameraHandler Instance {  get; private set; }

    [SerializeField]

    private CinemachineVirtualCamera cinemachineVirtualCamera;

    [SerializeField]

    private float zoomAmount;

    [SerializeField]

    private float minOrthograhicSize, maxOrthographicSize;

    private float orthographicSize;

    private float targetOrthographicSize;


    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        orthographicSize = cinemachineVirtualCamera.m_Lens.OrthographicSize;
        cinemachineVirtualCamera.Follow = GameManager.Instance._playerShip.transform;
    }



    private void Update()
    {
        //HandleMove();
        HandleZoom();
    }

    private void HandleMove()

    {

        float moveSpeed = 5f;

        float x = Input.GetAxisRaw("Horizontal");

        float y = Input.GetAxisRaw("Vertical");



        Vector2 moveDir = new Vector2(x, y).normalized;

        transform.position += (Vector3)moveDir * (moveSpeed * Time.deltaTime);
    }
    private void HandleZoom()
    {

        targetOrthographicSize += -Input.mouseScrollDelta.y * zoomAmount;

        targetOrthographicSize = Mathf.Clamp(targetOrthographicSize, minOrthograhicSize, maxOrthographicSize);

        orthographicSize = Mathf.Lerp(orthographicSize, targetOrthographicSize, Time.deltaTime* 5f);

        cinemachineVirtualCamera.m_Lens.OrthographicSize = orthographicSize;

    }

    public void ZoomInToShip(float amount)
    {
        targetOrthographicSize = amount;
    }

    public float GetOrthographicSize()
    {
        return orthographicSize;
    }

}