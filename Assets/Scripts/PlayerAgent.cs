using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEditor;
using UnityEngine;

public class PlayerAgent : Agent
{
    [SerializeField] public EnvController m_envController;
    [SerializeField] private float lineLength = 20f;
    [SerializeField] private LayerMask m_layerMask;
    [SerializeField] private BufferSensorComponent m_BlockBufferSensor;
    [SerializeField] private BufferSensorComponent m_ItemBufferSensor;
    
    [SerializeField] public GameObject m_blockSpawner;
    private ActionController m_actionController;
    private bool m_wasMouseButtonDown = false;
    private LineRenderer m_lineRenderer;

    private const int blockBufferSensorObservationSize = 4;
    private const int itemBufferSensorObservationSize = 2;
    
    private int currentAngleIndex = 11; // 시작 각도 인덱스 (90도에 해당)
    private float currentAngle = 90f; // 시작 각도
    private GameObject parentObject;
    private int lastActionIndex = 0;

    
    public override void Initialize()
    {
        m_actionController = GetComponent<ActionController>();
        m_lineRenderer = GetComponent<LineRenderer>();
        parentObject = transform.parent.gameObject;

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override void OnEpisodeBegin()
    {
        // Debug.Log("OnEpisodeBegin()");
        m_envController.GameReset();
    }

    private void FixedUpdate()
    {
        if (m_envController.isShootEnabled) // 공을 발사 할 수 있는 상태
        {
            RequestDecision();
        }
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
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int actionIndex = actionBuffers.DiscreteActions[0];
        
        if (actionIndex != 0)
        {
            lastActionIndex = actionIndex; 
            currentAngle = GetAngleFromIndex(actionIndex);
            // Debug.Log("Scaled Before: "  + controlSignal.x + " " + controlSignal.y);
            
            Vector3 controlSignal = CalculateGapFromAngle(currentAngle);
        
            // Debug.Log($"Current Angle: {currentAngle} $Current controlSignal: {controlSignal}");
            // Debug.Log(controlSignal.magnitude);
            
            controlSignal = controlSignal.normalized;
            controlSignal = new Vector3(controlSignal.y >= 0 ? controlSignal.x : controlSignal.x >= 0 ? 1 : -1, Mathf.Clamp(controlSignal.y, 0.2f, 1), 0);
            
            m_actionController.Shoot(controlSignal);
            // Debug.Log("마우스 버튼 눌림");
            // Debug.Log("Scaled After" + controlSignal.x + " " + controlSignal.y + "" + controlSignal.z);
            // Debug.Log(controlSignal.magnitude);
            
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
        //Debug.Log(transform.root.gameObject.name + " agentCount:  " + agentCount + "currentScore: " + currentScore);
        
        // 1. 현재 각도 정보 추가 = 1
        sensor.AddObservation(currentAngle / 180f);  // 0-180도를 0-1로 정규화

        // 2. 이전 행동 정보 추가 = 1
        sensor.AddObservation(lastActionIndex / 21f);  // 0-21을 0-1로 정규화

        // 3. 전체 게임 상태 정보 추가 = 2
        int totalBlocks = m_blockSpawner.GetComponent<BlockSpawnerController>().GetBlockCount(); // 남은 블록 개수
        int maxBlockScores = m_blockSpawner.GetComponent<BlockSpawnerController>().GetMaxBlockScores();
        int currentBlockScores = m_blockSpawner.GetComponent<BlockSpawnerController>().GetBlockScores();
        
        sensor.AddObservation((float)totalBlocks / m_blockSpawner.GetComponent<BlockSpawnerController>().maxBlocks);  // 남은 블록 수 비율
        sensor.AddObservation((float)currentBlockScores / maxBlockScores);  // 현재 점수 비율
        
        #region Test 6 추가 코드, buffersensor비활성화
        // Test 6 (가장 먼것과 가장 가까운것의 위치와, 정규화된 점수 추가)
        // List<GameObject> blocks = GetChildObjectsWithTag(m_blockSpawner.transform, "Block");
        //
        // GameObject nearestBlock = null;
        // GameObject farthestBlock = null;
        // float nearestDistanceY = float.MaxValue;
        // float nearestDistanceX = float.MaxValue;
        // float farthestDistanceY = float.MinValue;
        // float farthestDistanceX = float.MinValue;
        //
        // foreach (GameObject block in blocks)
        // {
        //     Vector2 blockPosition = Normalize(block.transform.position);
        //     float distanceY = Mathf.Abs(blockPosition.y - agentPosition.y);
        //     float distanceX = Mathf.Abs(blockPosition.x - agentPosition.x);
        //     
        //     // Debug.Log(transform.root.gameObject.name + "블럭 현재 점수, 최대 점수:" + (float)block.GetComponent<BlockController>().GetScore() + ", " + m_blockSpawner.GetComponent<BlockSpawnerController>().blockScore);
        //
        //     // y좌표로 가장 가까운 블록 찾기 같을 경우 x좌표 사용
        //     if (distanceY < nearestDistanceY || (Mathf.Approximately(distanceY, nearestDistanceY) && distanceX < nearestDistanceX))
        //     {
        //         nearestDistanceY = distanceY;
        //         nearestDistanceX = distanceX;
        //         nearestBlock = block;
        //     }
        //
        //     // y좌표로 가장 먼 블록 찾기
        //     if (distanceY > farthestDistanceY || (Mathf.Approximately(distanceY, farthestDistanceY) && distanceX > farthestDistanceX))
        //     {
        //         farthestDistanceY = distanceY;
        //         farthestDistanceX = distanceX;
        //         farthestBlock = block;
        //     }
        // }
        //
        // // 가장 가까운 블록 정보 추가: 위치(2) + 점수(1) + 거리(2) 
        // if (nearestBlock != null)
        // {
        //     Vector2 nearestBlockPosition = Normalize(nearestBlock.transform.position);
        //     float nearestBlockScore = ((float)nearestBlock.GetComponent<BlockController>().GetScore() / m_blockSpawner.GetComponent<BlockSpawnerController>().blockScore);
        //     float nearestDistance = Vector2.Distance(agentPosition, nearestBlockPosition) / Mathf.Sqrt(2); // 최대거리는 sqrt(2)
        //     sensor.AddObservation(nearestBlockPosition);
        //     sensor.AddObservation(nearestBlockScore);
        //     sensor.AddObservation(nearestDistance);
        //     // Debug.Log(transform.root.gameObject.name + "가까운거:" + (float)nearestBlock.GetComponent<BlockController>().GetScore() + ", " + m_blockSpawner.GetComponent<BlockSpawnerController>().blockScore);
        //     // Debug.Log(transform.root.gameObject.name + " nearestBlockPosition:  " + nearestBlockPosition + "nearestBlockScore: " + nearestBlockScore + "nearestDistance: " + nearestDistance);
        // }
        // else
        // {
        //     // 블록이 없는 경우 기본값 추가
        //     sensor.AddObservation(new Vector2(0, 0));
        //     sensor.AddObservation(0f);
        //     sensor.AddObservation(1f); // 최대 거리
        // }
        //
        // // 가장 먼 블록 정보 추가 : 위치(2) + 점수(1) + 거리(1)
        // if (farthestBlock != null)
        // {
        //     Vector2 farthestBlockPosition = Normalize(farthestBlock.transform.position);
        //     float farthestBlockScore = ((float)farthestBlock.GetComponent<BlockController>().GetScore() / m_blockSpawner.GetComponent<BlockSpawnerController>().blockScore);
        //     float farthestDistance = Vector2.Distance(agentPosition, farthestBlockPosition) / Mathf.Sqrt(2);// 최대거리는 sqrt(2) 
        //     
        //     sensor.AddObservation(farthestBlockPosition);
        //     sensor.AddObservation(farthestBlockScore);
        //     sensor.AddObservation(farthestDistance);
        //     // Debug.Log(transform.root.gameObject.name + "먼거:" + (float)farthestBlock.GetComponent<BlockController>().GetScore() + ", " + m_blockSpawner.GetComponent<BlockSpawnerController>().blockScore);
        //     // Debug.Log(transform.root.gameObject.name + " farthestBlockPosition:  " + farthestBlockPosition + "farthestBlockScore: " + farthestBlockScore + "farthestDistance" + farthestDistance);
        // }
        // else
        // {
        //     // 블록이 없는 경우 기본값 추가
        //     sensor.AddObservation(new Vector2(0, 0));
        //     sensor.AddObservation(0f);
        //     sensor.AddObservation(0f); // 최소 거리
        // }        
        #endregion

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
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // var continousActionsOut = actionsOut.ContinuousActions;
        
        var discreteActionsOut = actionsOut.DiscreteActions;
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentAngleIndex = Mathf.Max(1, currentAngleIndex - 1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentAngleIndex = Mathf.Min(21, currentAngleIndex + 1);
        }
        
        currentAngle = GetAngleFromIndex(currentAngleIndex);
        Vector3 gap = CalculateGapFromAngle(currentAngle);
        
        // Debug.Log($"Current Angle: {currentAngle} Current Gap: {gap}");
        
        var resetBallPosition = m_envController.resetBallPosition;
        
        DrawGuideline(resetBallPosition, gap);
        
        bool isMouseButtonDown = Input.GetMouseButton(0);

        if (m_wasMouseButtonDown && !isMouseButtonDown)
        {
            discreteActionsOut[0] = currentAngleIndex;
            RemoveGuideLine();
        }
        else
        {
            discreteActionsOut[0] = 0; // 아무것도 안함
        }

        m_wasMouseButtonDown = isMouseButtonDown;

    }

    private void DrawGuideline(Vector3 startPosition, Vector3 direction)
    {
        // Raycast 수행
        RaycastHit2D hit = Physics2D.Raycast(startPosition + direction * 3, direction, lineLength, m_layerMask);

        // LineRenderer 설정
        m_lineRenderer.positionCount = 2;
        m_lineRenderer.SetPosition(0, startPosition);
        m_lineRenderer.SetPosition(1, hit.point);

    }
    
    private void RemoveGuideLine()
    {
        m_lineRenderer.positionCount = 0;
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
