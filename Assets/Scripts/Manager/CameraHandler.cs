using Cinemachine;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    public static CameraHandler Instance { get; private set; }

    [SerializeField]
    private CinemachineVirtualCamera cinemachineVirtualCamera;

    [SerializeField]
    private float zoomAmount;

    [SerializeField]
    private float minOrthograhicSize,
        maxOrthographicSize;

    private float orthographicSize;

    private float targetOrthographicSize;

    private Camera _camera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _camera = Camera.main;
    }

    private void Start()
    {
        orthographicSize = cinemachineVirtualCamera.m_Lens.OrthographicSize;
        cinemachineVirtualCamera.Follow = GameManager.Instance._playerShip.transform;
    }

    private void Update()
    {
            HandleZoom();
    }

    //private void HandleMove()
    //{
    //    float moveSpeed = 5f;

    //    float x = Input.GetAxisRaw("Horizontal");

    //    float y = Input.GetAxisRaw("Vertical");

    //    Vector2 moveDir = new Vector2(x, y).normalized;

    //    TogglePauseMode(true);

    //    cinemachineVirtualCamera.transform.position += (Vector3)moveDir * (moveSpeed * Time.unscaledDeltaTime);
    //}

    void TogglePauseMode(bool isPaused)
    {
        if (isPaused)
        {
            cinemachineVirtualCamera.Follow = null;
        }
        else
        {
            cinemachineVirtualCamera.Follow = GameManager.Instance._playerShip.transform;
        }
    }

    private void HandleZoom()
    {
        targetOrthographicSize += -Input.mouseScrollDelta.y * zoomAmount;

        targetOrthographicSize = Mathf.Clamp(
            targetOrthographicSize,
            minOrthograhicSize,
            maxOrthographicSize
        );

        orthographicSize = Mathf.Lerp(
            orthographicSize,
            targetOrthographicSize,
            Time.unscaledDeltaTime * 5f
        );

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

    public void SwitchCinemachineUpdateMode(bool flag)
    {
        if (flag)
        {
            _camera.GetComponent<CinemachineBrain>().m_UpdateMethod = CinemachineBrain
                .UpdateMethod
                .LateUpdate;
        }
        else
        {
            _camera.GetComponent<CinemachineBrain>().m_UpdateMethod = CinemachineBrain
                .UpdateMethod
                .SmartUpdate;
        }
    }
}
