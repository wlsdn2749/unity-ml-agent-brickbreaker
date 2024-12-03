using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class BallMovement : MonoBehaviour
{

    private float shootForce = 6f;
    public float maxDragDistance = 2f;

    public Rigidbody2D rb2d;
    private Vector2 lastPosition;
    private float timeSinceLastMovement;
    private const float MovementThreshold = 0.01f;
    private const float ResetTime = 5f;

    public float minVerticalVelocity = 2f;
    public float horizontalForceMultiplier = 1f;
    public float downwardForceMultiplier = 0.2f;
    
    private EnvController m_envController;
    private BlockSpawnerController m_blockSpawnerController;
    
    // }
    
    private void FixedUpdate()
    {
        AdjustVelocity();
        PositionalExceptionHandling();
    }

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        m_envController = transform.parent.GetComponent<PlayerController>().m_envController;
        m_blockSpawnerController = transform.parent.GetComponent<PlayerController>().m_blockSpawnerController;
    }

    // Ball이 충돌하는 경우
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 가로로 움직일경우 아래로 내림
        // Vector2 pos = rb2d.velocity.normalized;
        // if (pos.magnitude != 0 && pos.y < 0.15f && pos.y > -0.15f)
        // {
        //     rb2d.velocity = Vector2.zero;
        //     rb2d.AddForce(new Vector2(pos.x > 0 ? 1 : -1, -0.2f).normalized * shootForce * 50f);
        // }
        
        if (collision.gameObject.CompareTag("Block"))
        {
            BlockController blockController = collision.gameObject.GetComponent<BlockController>();
            blockController.SetScore(blockController.GetScore() - 1);
            m_blockSpawnerController.BlockColorChange();
        }
        
        else if (collision.gameObject.CompareTag("StartWall")) 
        {
            // Debug.Log("바닥과 충돌");
            Destroy(gameObject);
            m_envController.OnBallGrounded(transform);
        }
        
        if(rb2d.velocity.magnitude == 0) Debug.Log($"{transform.root.gameObject.name} 0이되는 트리거는 Enter");
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(rb2d.velocity.magnitude == 0) Debug.Log($"{transform.root.gameObject.name} 0이되는 트리거는 Stay");
    }
    
    private void OnCollisionExit2D(Collision2D collision)
    {
        if(rb2d.velocity.magnitude == 0) Debug.Log($"{transform.root.gameObject.name} 0이되는 트리거는 Exit");
    }

    void PositionalExceptionHandling()
    {
        if (Math.Abs(transform.position.y) > 10) // y는 절대로 -10과 10을 넘어가서는 안됨.
        {
            Destroy(gameObject);
            m_envController.OnBallGrounded(transform);
            Debug.Log("PositionalExceptionHandling occured! 예외 핸들링..");
        }
    }
    void AdjustVelocity()
    {
        Vector2 velocity = rb2d.velocity;
        
        // 거의 수평 움직임 감지
        if (Mathf.Abs(velocity.y) < 0.15f && velocity.magnitude > 0.1f)
        {
            // 현재 속도 유지하면서 아래쪽 방향 성분 추가
            float horizontalSpeed = velocity.x;
            float verticalSpeed = -minVerticalVelocity;
            
            // 속도 방향 유지하면서 크기 조정
            Vector2 newVelocity = new Vector2(horizontalSpeed, verticalSpeed).normalized * velocity.magnitude;
            
            // 부드러운 전환을 위해 보간 사용
            rb2d.velocity = Vector2.Lerp(velocity, newVelocity, Time.fixedDeltaTime * 5f);
            
            // 추가적인 아래쪽 힘 적용
            Vector2 additionalForce = new Vector2(
                Mathf.Sign(horizontalSpeed) * horizontalForceMultiplier,
                -downwardForceMultiplier
            ).normalized * shootForce;
            
            rb2d.AddForce(additionalForce, ForceMode2D.Impulse);
            
            Debug.Log($"{transform.root.gameObject.name} : 이상감지 강제 힘 적용");
        }
    }
    
    public void Shoot(Vector3 shootDirection)
    {
        rb2d.velocity = shootDirection * shootForce;
        // Debug.Log("rb2d.velocity : " + rb2d.velocity);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ItemBall"))
        {
            Destroy(collision.gameObject);
            m_envController.ballCount++;
        }
    }
}
