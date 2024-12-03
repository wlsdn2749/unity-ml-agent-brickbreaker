using TMPro;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    private TextMeshPro tmp;

    [SerializeField] private int score;

    [SerializeField] private int maxScore = 0;
    [SerializeField] private ParticleSystem p_ParticleRed;
    private Animator _animator;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        tmp = GetComponentInChildren<TextMeshPro>();
        tmp.color=new Color32(255,255,255,255);
        _animator = GetComponent<Animator>();
    }
    
    public void SetScore(int newScore)
    {
        if (maxScore == 0)
        {
            maxScore = newScore;
        }
        
        score = newScore;
        _animator.SetTrigger("Shock");
        
        
        if (tmp != null)
        {
            tmp.text = score.ToString();
        }
        else
        {
            Debug.LogError("TextMeshPro component is not found!");
        }

        if (score <= 0)
        {
            Destroy(gameObject);
            Instantiate(p_ParticleRed, transform.position, Quaternion.identity);
        }
    }

    public int GetScore()
    {
        return score;
    }

    public int GetMaxScore()
    {
        return maxScore;
    }

}
