using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class InferenceAgent : Agent
{
    [SerializeField] public EnvController m_envController;
    [SerializeField] private BufferSensorComponent m_BlockBufferSensor;
    [SerializeField] private BufferSensorComponent m_ItemBufferSensor;
    [SerializeField] public GameObject m_blockSpawner;
    [SerializeField] public AIAssistButton m_AIAssistButton;

    // actionLine Render
    [SerializeField] private LineRenderer actionLineRenderer;
    [SerializeField] private GameObject actionPreview;
    private Vector3 predictedDirection;

    private const int blockBufferSensorObservationSize = 4;
    private const int itemBufferSensorObservationSize = 2;
    private float lastAngle = 90f;
    private int lastActionIndex = 0;
    private GameObject parentObject;
    public bool inferenceMode;

    private void Awake()
    {
        parentObject = transform.parent.gameObject;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GetActionInference()
    {
        GetActionInference(inferenceMode);
    }

    public void GetActionInference(bool toggle)
    {
        if (toggle)
        {
            RequestDecision();
        }
        else
        {
            RemoveActionInference();
        }
        
        inferenceMode = toggle;
    }

    public void RemoveActionInference()
    {
        actionPreview.SetActive(false);
        actionLineRenderer.SetPosition(0, Vector3.zero);
        actionLineRenderer.SetPosition(1, Vector3.zero);
    }
    
    public void UpdateLastAction(Vector3 gap)
    {
        lastAngle = CalculateAngleFromGap(gap);
        lastActionIndex = GetActionIndexFromAngle(lastAngle);

        Debug.Log($"lastAngle = {lastAngle}, lastActionIndex = {lastActionIndex}");
    }
    
    private Vector2 Normalize(Vector3 position)
    {

        float basisX = parentObject.transform.position.x;
        float minX = basisX - 4.3f, maxX = basisX + 4.3f;
        float minY = -4.3f, maxY = 4.6f;
        
        float normalizedX = math.clamp((position.x - minX) / (maxX - minX), 0f, 1f);
        float normalizedY = math.clamp((position.y - minY) / (maxY - minY), 0f, 1f);

        return new Vector2(normalizedX, normalizedY);
    }

    private float CalculateAngleFromGap(Vector3 gap)
    {
        // Vector3에서 x와 y를 이용하여 각도 계산 0~180도... 각도는 오른쪽에서 왼쪽으로 돎
        float angleRadians = Mathf.Atan2(gap.y, gap.x);
        float angleDegrees = angleRadians * Mathf.Rad2Deg;

        // 10~170도 범위로 제한
        float mappedAngle = Mathf.Clamp(angleDegrees, 10f, 170f);

        return mappedAngle;
    }

    private int GetActionIndexFromAngle(float angle)
    {
        // 각도를 Index로 변환
        float minAngle = 10f;
        float maxAngle = 170f;
        float step = 8f;

        // 범위를 10~170도로 제한
        angle = Mathf.Clamp(angle, minAngle, maxAngle);

        // Index 계산
        int index = Mathf.RoundToInt((angle - minAngle) / step) + 1;

        // Index 범위를 1~21로 제한
        return Mathf.Clamp(index, 1, 21);
    }
    
    private Vector3 CalculateGapFromAngle(float angleDegrees)
    {
        float angleRadians = angleDegrees * Mathf.Deg2Rad;
        float gapX = Mathf.Cos(angleRadians);
        float gapY = Mathf.Sin(angleRadians);
        return new Vector3(gapX, gapY, 0f).normalized;
    }
    
    private float GetAngleFromIndex(int index)
    {
        return 10f + ((index-1) * 8f);
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        
        int actionIndex = actionBuffers.DiscreteActions[0];
        
        Debug.Log($"OnActionReceived {actionIndex}");

        if (actionIndex == 0)
        {
            actionIndex = Random.Range(1, 21);
        }
        
        // 행동 인덱스를 각도로 변환하고 Control Signal 계산
        float currentAngle = GetAngleFromIndex(actionIndex);
        Vector3 controlSignal = CalculateGapFromAngle(currentAngle);

        controlSignal = controlSignal.normalized;
        controlSignal = new Vector3(
            controlSignal.y >= 0 ? controlSignal.x : controlSignal.x >= 0 ? 1 : -1,
            Mathf.Clamp(controlSignal.y, 0.2f, 1),
            0
        );

        // 시각화 갱신
        UpdateVisualization(controlSignal);
        
    }
    
    private void UpdateVisualization(Vector3 controlSignal)
    {
        // 에이전트 위치
        Vector3 agentPosition = m_envController.resetBallPosition;

        // Raycast로 충돌 위치 계산
        RaycastHit2D hit = Physics2D.Raycast(
            agentPosition, 
            controlSignal,
            Mathf.Infinity,
            LayerMask.GetMask("Wall")
        );

        // LineRenderer 업데이트
        actionLineRenderer.SetPosition(0, agentPosition);
        if (hit.collider != null)
        {
            actionLineRenderer.SetPosition(1, hit.point);

            // 행동 결과 예상 지점에 미리보기 표시
            actionPreview.transform.position = hit.point;
            actionPreview.SetActive(true);
        }
        else
        {
            Vector3 farPoint = agentPosition + controlSignal * 10f; // 끝 점 설정
            actionLineRenderer.SetPosition(1, farPoint);
            actionPreview.SetActive(false);
        }
    }
    
    
    
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // 현재 플레이어 위치 (정규화) x = 1
        Vector2 agentPosition = Normalize(m_envController.resetBallPosition);
        //Debug.Log(transform.root.gameObject.name + " AgentPosition:  " + agentPosition);
        sensor.AddObservation(agentPosition.x);

        // 현재 플레이어 공 개수 count, 현재 점수 score = 2 -> Test(4,5) 1 = count/score
        int agentCount = m_envController.ballCount;
        int currentScore = m_blockSpawner.GetComponent<BlockSpawnerController>().blockScore;
        sensor.AddObservation(agentCount / (float)currentScore);

        // 1. 현재 각도 정보 추가 = 1
        sensor.AddObservation(lastAngle / 180f);  // 0-180도를 0-1로 정규화

        // 2. 이전 행동 정보 추가 = 1
        sensor.AddObservation(lastActionIndex / 21f);  // 0-21을 0-1로 정규화

        // 3. 전체 게임 상태 정보 추가 = 2
        int totalBlocks = m_blockSpawner.GetComponent<BlockSpawnerController>().GetBlockCount(); // 남은 블록 개수
        int maxBlockScores = m_blockSpawner.GetComponent<BlockSpawnerController>().GetMaxBlockScores();
        int currentBlockScores = m_blockSpawner.GetComponent<BlockSpawnerController>().GetBlockScores();
        
        sensor.AddObservation((float)totalBlocks / m_blockSpawner.GetComponent<BlockSpawnerController>().maxBlocks);  // 남은 블록 수 비율
        sensor.AddObservation((float)currentBlockScores / maxBlockScores);  // 현재 점수 비율

        // 현재 존재하는 블럭과 수치 (x,y,num,weight) = 4
        // Test 2 비활성화, Test 3 활성화
        if (m_blockSpawner != null)
        {
            List<GameObject> blocks = GetChildObjectsWithTag(m_blockSpawner.transform, "Block");
            foreach (GameObject block in blocks)
            {
                float[] listObservation = new float[blockBufferSensorObservationSize];
                Vector2 blockPosition = Normalize(block.transform.position); // 정규화
        
                listObservation[0] = blockPosition.x;
                listObservation[1] = blockPosition.y;
                listObservation[2] = ((float)block.GetComponent<BlockController>().GetScore() / m_blockSpawner.GetComponent<BlockSpawnerController>().blockScore);
                listObservation[3] = 1 - blockPosition.y; // 블럭의 중요도

                // Debug.Log(transform.root.gameObject.name + "Blocks"+ listObservation[0] + " " + listObservation[1] + " " + listObservation[2]);
                m_BlockBufferSensor.AppendObservation(listObservation);
            }
        }
         
        // 현재 존재하는 아이템 (x,y) = 2
         
        if (m_blockSpawner != null)
        {
            List<GameObject> items = GetChildObjectsWithTag(m_blockSpawner.transform, "ItemBall");
            foreach (GameObject item in items)
            {
                float[] listObservation = new float[itemBufferSensorObservationSize];
                Vector2 itemPosition = Normalize(item.transform.position);
        
                listObservation[0] = itemPosition.x;
                listObservation[1] = itemPosition.y;
                
                // Debug.Log(transform.root.gameObject.name + "Items"+ listObservation[0] + " " + listObservation[1]);
                m_ItemBufferSensor.AppendObservation(listObservation);
            }
        }
         
    }
    
    private List<GameObject> GetChildObjectsWithTag(Transform parentTransform, String tag)
    {
        List<GameObject> childObjects = new List<GameObject>();

        if (parentTransform != null)
        {
            foreach (Transform childTransform in parentTransform)
            {
                if(childTransform.gameObject.CompareTag(tag))
                    childObjects.Add(childTransform.gameObject);
            }
        }

        return childObjects;
    }
}
