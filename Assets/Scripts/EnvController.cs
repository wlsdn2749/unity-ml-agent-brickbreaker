using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using TMPro;


public class EnvController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [FormerlySerializedAs("ballAgent")] [SerializeField]
    private BallMovement ballMovement;

    [SerializeField] private BlockSpawnerController blockSpawnerController;
    [SerializeField] private GameObject guideBallPrefab;
    [SerializeField] private InferenceAgent m_InferenceAgent;
    
    public Vector2 resetBallPosition;
    public int ballCount = 1;
    private int previousBallCount = 1;

    private int prevBlockCount = 0;
    private int curBlockCount = 0;
    private int prevBlockScore = 0;
    private int curBlockScore = 0;
    public int gatheringBallCount;
    public Vector3 shootDirection;

    public GameObject guideBall;
    public LineRenderer lineRenderer;

    private bool isEndable = false;
    public bool isShootEnabled;
    private bool shouldEndEpisode = false;
    [SerializeField] private LayerMask _layerMask;

    public TextMeshPro textScore;
    public TextMeshPro textBallCount;
    public TextMeshPro highScore;
    private GameObject parentObject;
    private bool isResetting = false;

    private int _highScore;

    private void Update()
    {
        // 게임 종료
        if (shouldEndEpisode)
        {
            shouldEndEpisode = false;
            m_InferenceAgent.inferenceMode = false;
            GameReset();
        }
            
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        parentObject = transform.parent.gameObject;
    }

    private void Start()
    {
        _highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScore.text = "최고기록 : " + _highScore;
        
        GameReset();
    }
    public void GameReset()
    {
        
        Debug.Log("GameReset!");
        // Ball, Score ReSet
        ballCount = 1;
        blockSpawnerController.blockScore = 0;
        blockSpawnerController.maxBlocks = 0;
        isEndable = false;
        
        // GuideBall Reset
        Destroy(guideBall);
        resetBallPosition = new Vector2(parentObject.transform.position.x, transform.localPosition.y - 4.15f);
        guideBall = Instantiate(guideBallPrefab, resetBallPosition, Quaternion.identity);
        
        // Board Env Reset, 순서가 중요함
        RemoveAllEntities();
        Turn();
        isEndable = true; // RemoveAllEntities가 Frame이 끝날때 사라지게 끔 예약이 되어있기 떄문에, Flag로 제어
    }
    
    public void OnBallGrounded(Transform tr)
    {
        gatheringBallCount++;
        if (gatheringBallCount == 1) // 처음 부딫힌 곳에 생성
        {
            resetBallPosition.x = parentObject.transform.position.x + tr.localPosition.x; // 다중 학습 환경
            guideBall = Instantiate(guideBallPrefab, resetBallPosition, Quaternion.identity);
        }
        
        // Debug.Log($"gatheringBallCount: {gatheringBallCount} previousBallCount: {previousBallCount}");

        if (gatheringBallCount >= previousBallCount)
        {
            Turn();
            gatheringBallCount = 0;
        }
    }
    
    void Turn()
    {
        textBallCount.transform.position = new Vector3(resetBallPosition.x, textBallCount.transform.position.y, 0);
        textScore.text= "현재점수 : " + (++blockSpawnerController.blockScore);
        textBallCount.text= "x"+ (ballCount);

        if (blockSpawnerController.blockScore > _highScore)
        {
            PlayerPrefs.SetInt("HighScore", blockSpawnerController.blockScore);
            highScore.text = "최고기록 : " + blockSpawnerController.blockScore;
            highScore.color = new Color(0.255f, 0.698f, 0.235f, 1f);
        }
        curBlockScore = blockSpawnerController.GetBlockScores();

        MoveBlock();
        
        prevBlockScore = blockSpawnerController.GetBlockScores();
        
        ShootTurnOnEnable();
        m_InferenceAgent.GetActionInference(); // 턴이 끝날때마다 추론모드가 켜져있으면 AI 갱신
        textBallCount.gameObject.SetActive(true); // 발사할떄는 Ball이 몇 개 나오는지 보여줌
    }

    void ShootTurnOnEnable()
    {
        if (!shouldEndEpisode)
        {
            previousBallCount = ballCount;
            isShootEnabled = true;
            // Debug.Log("슛발사가능");
        }
    }

    void MoveBlock()
    {
        List<GameObject> blocks = GetChildObjects(blockSpawnerController.transform);
        
        foreach (GameObject block in blocks) // 블럭 이동
        {
            block.transform.localPosition += new Vector3(0f, -1.03f, 0f);
        }

        foreach (GameObject block in blocks) // 이동 완료 후에, 검사
        {
            if (isEndable && block.transform.localPosition.y <= -3.85) // 3.85가 마지노선 
            {
                if (block.CompareTag("Block"))
                {
                    shouldEndEpisode = true;
                    return;
                }
                if (block.CompareTag("ItemBall"))
                {
                    Destroy(block);
                }
            }
        }
        
        // 모든 이동 끝나고 게임도 안끝났으면 블럭 추가
        if (!shouldEndEpisode)
        {
            blockSpawnerController.SpawnBlockRandom();
        }
        
        blockSpawnerController.BlockColorChange();
    }

    public void RemoveAllEntities()
    {
        List<GameObject> entities = GetChildObjects(blockSpawnerController.transform);
        
        foreach (var entity in entities)
        {
            Destroy(entity);
        }

    }
    
    private List<GameObject> GetChildObjects(Transform parentTransform)
    {
        List<GameObject> childObjects = new List<GameObject>();

        if (parentTransform != null)
        {
            foreach (Transform childTransform in parentTransform)
            {
                childObjects.Add(childTransform.gameObject);
            }
        }

        return childObjects;
    }
    
}
