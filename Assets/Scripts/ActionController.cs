using System.Collections;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    [SerializeField] private GameObject m_ballPrefab;

    [SerializeField] private EnvController m_envController;
    [SerializeField] private InferenceAgent m_inferenceAgent;

    private Vector3 _shootDirection;

    private Vector3 _resetBallPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Shoot(Vector3 shootDirection)
    {
        if (m_envController.isShootEnabled)
        {
            _shootDirection = shootDirection;
            _resetBallPosition = m_envController.resetBallPosition;
            m_envController.isShootEnabled = false;
            m_inferenceAgent.RemoveActionInference();
            StartCoroutine(ShootBallsWithDelay());
            
        }
        Destroy(m_envController.guideBall);
        m_envController.textBallCount.gameObject.SetActive(false); // 발사 후, UI의 직관성을 위해 공이 몇개 있는지 표기 x
        
    }
    private IEnumerator ShootBallsWithDelay()
    {
        int ballCount = m_envController.ballCount;
        // Debug.Log($"BallCount : {ballCount}");
        for (int i = 0; i < ballCount; i++)
        {
            // Debug.Log(i + " start : " + Time.time);
            GameObject ball = Instantiate(m_ballPrefab, _resetBallPosition, Quaternion.identity, transform);
            BallMovement ballMovement = ball.GetComponent<BallMovement>();
            ballMovement.Shoot(_shootDirection);
            
            float delay = 0.1f;
            yield return new WaitForSeconds(delay);
        }

    }
    
}
