using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{

    [SerializeField] public BlockSpawnerController m_blockSpawnerController;
    [SerializeField] public EnvController m_envController;
    [SerializeField] private GameObject Arrow, BallPreview;
    [SerializeField] private LineRenderer MouseLR, BallLR;
    [SerializeField] private InferenceAgent m_inferenceAgent;
    
    private ActionController m_actionController;
    private Vector3 dragStartPos, dragSecondPos;   // 드래그 시작 지점
    private Vector3 dragEndPos, gap;
    private Vector2 controlSignal;
    private bool isDragging = false;

    void Awake()
    {
        m_actionController = GetComponent<ActionController>();
    }
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        // 마우스 버튼을 누르는 순간
        if (Input.GetMouseButtonDown(0) && dragStartPos == Vector3.zero)
        {
            dragStartPos = GetMouseWorldPosition() + new Vector3(0, 0, 10); // 10을 더해주는 이유는 카메라가 z10에 위치해있어서
            isDragging = true;
        }

        bool isMouse = Input.GetMouseButton(0);
        if (isMouse)
        {
            // 차이값
            dragSecondPos = Camera.main.ScreenToWorldPoint(Input.mousePosition) + new Vector3(0, 0, 10);

            if ((dragSecondPos - dragStartPos).magnitude < 1) return;
            gap = (dragSecondPos - dragStartPos).normalized;
            gap = new Vector3(gap.y >= 0 ? gap.x : gap.x >= 0 ? 1 : -1, Mathf.Clamp(gap.y, 0.2f, 1), 0);
            
            // 화살표, 공 미리보기
            Arrow.transform.position = m_envController.resetBallPosition;
            Arrow.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(gap.y, gap.x) * Mathf.Rad2Deg);
            BallPreview.transform.position = Physics2D.CircleCast(
                new Vector2(Mathf.Clamp(m_envController.resetBallPosition.x, -10, 10), -4.15f),
                0.2f,
                gap,
                10000,
                LayerMask.GetMask("Wall", "Block")
            ).centroid;

            RaycastHit2D hit = Physics2D.Raycast(
                m_envController.resetBallPosition, 
                gap, 
                10000, 
                LayerMask.GetMask("Wall")
            );
            
            // 라인
            MouseLR.SetPosition(0, dragStartPos);
            MouseLR.SetPosition(1, dragSecondPos);
            BallLR.SetPosition(0, m_envController.resetBallPosition);
            BallLR.SetPosition(1, (Vector3)hit.point);
        }
        BallPreview.SetActive(isMouse);
        Arrow.SetActive(isMouse);

        // 마우스 버튼을 떼는 순간
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            MouseLR.SetPosition(0, Vector3.zero);
            MouseLR.SetPosition(1, Vector3.zero);
            BallLR.SetPosition(0, Vector3.zero);
            BallLR.SetPosition(1, Vector3.zero);
            
            dragEndPos = GetMouseWorldPosition();

            Debug.Log(gap);
            m_actionController.Shoot(gap);
            m_inferenceAgent.UpdateLastAction(gap); // AI Agent를 사용하기 위해
            isDragging = false;

            dragStartPos = Vector3.zero;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        // 카메라와 마우스 위치를 이용해 월드 좌표를 반환
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 0; // Z 값 보정
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }
}