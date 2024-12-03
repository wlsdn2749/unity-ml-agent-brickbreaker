using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class BlockSpawnerController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject blockPrefab;
    public GameObject ItemBallPrefab;
    public int blockScore;
    public int maxBlocks = 0;
    private GameObject parentObject;
    
    
    float[] xPositions;
    float[] yPositions;

    private List<int> numberPool = new List<int> { 0, 1, 2, 3, 4, 5 };
    private List<int> selectedNumbers = new List<int>();
    private Dictionary<GradientColor, Color> colorDictionary;
    
    public enum GradientColor
    {
        Red,
        OrangeRed,
        Orange,
        LightOrange,
        Peach,
        LightPeach,
        Yellow
    }

    public Color GetColor(GradientColor colorType)
    {
        if (colorDictionary.TryGetValue(colorType, out Color color))
        {
            return color;
        }

        // 기본 색상 또는 에러 핸들링
        return new Color(1.0f, 0.31f, 0.31f);
    }
    void Awake()
    {
        colorDictionary = new Dictionary<GradientColor, Color>
        {
            { GradientColor.Red, new Color(1.0f, 0.31f, 0.31f) },       // Red
            { GradientColor.OrangeRed, new Color(1.0f, 0.39f, 0.31f) }, // OrangeRed
            { GradientColor.Orange, new Color(1.0f, 0.47f, 0.31f) },    // Orange
            { GradientColor.LightOrange, new Color(1.0f, 0.56f, 0.31f) }, // LightOrange
            { GradientColor.Peach, new Color(1.0f, 0.63f, 0.31f) },     // Peach
            { GradientColor.LightPeach, new Color(1.0f, 0.71f, 0.31f) }, // LightPeach
            { GradientColor.Yellow, new Color(1.0f, 0.78f, 0.31f) }     // Yellow
        };
        
        blockScore = 0;
        xPositions = new float[6];
        yPositions = new float[6];
        parentObject = transform.parent.gameObject;

        float startX = parentObject.transform.position.x -3.7f + 0f;
        float startY = parentObject.transform.position.y + 2.25f + 1.085f;
        
        
        // X와 Y의 차이 값 설정
        float diffX = 1.48f;
        float diffY = -1.03f;

        for (int i = 0; i < 6; i++)
        {
            float x = startX + i * diffX;
            float y = startY + i * diffY;

            xPositions[i] = x;
            yPositions[i] = y;
        }

        // SpawnBlockALl();
        // SpawnBlockRandom();
    }

    void SpawnBlockALl()
    {
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
               GameObject block = Instantiate(blockPrefab, new Vector2(xPositions[i], yPositions[j]), Quaternion.identity, transform);

               BlockController blockController = block.GetComponent<BlockController>();
               blockController.SetScore(blockScore);
               
               // Debug.Log(blockScore);
               
            }
        }
    }

    public int GetBlockCount()
    {
        int blockCount = 0;

        foreach (Transform child in transform)
        {
            if (child.CompareTag("Block"))
            {
                blockCount++;
            }
        }

        return blockCount;
    }

    public int GetBlockScores()
    {
        int blockAllScores = 0;
        
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Block"))
            {
                blockAllScores += child.GetComponent<BlockController>().GetScore();
            }
        }

        return blockAllScores;
    }
    
    public int GetMaxBlockScores()
    {
        int allMaxBlockScores = 0;
        
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Block"))
            {
                allMaxBlockScores += child.GetComponent<BlockController>().GetMaxScore();
            }
        }

        return allMaxBlockScores;
    }
    
    public void SpawnBlockRandom()
    {

        List<int> selectNumbers = PickRandomNumbers();
        maxBlocks += selectNumbers.Count; // 최대 갯수 정하기 
        
        foreach (int num in selectNumbers)
        {
            GameObject block = Instantiate(blockPrefab, new Vector2(xPositions[num], yPositions[0]), Quaternion.identity, transform);
            BlockController blockController = block.GetComponent<BlockController>();
            blockController.SetScore(blockScore);
        }
        
        var remainingNumbers = numberPool.Except(selectNumbers).ToList();
        
        int randomIndex = Random.Range(0, remainingNumbers.Count);
        
        Instantiate(ItemBallPrefab, new Vector2(xPositions[remainingNumbers[randomIndex]], yPositions[0]), Quaternion.identity, transform);
    }
    
    private List<int> PickRandomNumbers()
    {
        // 점수에 따른 블럭복사개수 정하기
        int count;
        int randBlock = Random.Range(0, 24);
        if (blockScore <= 10) count = randBlock < 16 ? 1 : 2;
        else if (blockScore <= 20) count = randBlock < 8 ? 1 : (randBlock < 16 ? 2 : 3);
        else if (blockScore <= 40) count = randBlock < 9 ? 2 : (randBlock < 18 ? 3 : 4);
        else count = randBlock < 8 ? 2 : (randBlock < 16 ? 3 : (randBlock < 20 ? 4 : 5));

        // 숫자 풀을 섞어줍니다.
        ShuffleNumberPool();

        // 섞인 숫자 풀에서 앞에서부터 count개의 숫자를 선택합니다.
        selectedNumbers = numberPool.GetRange(0, count);

        return selectedNumbers;
        // 선택된 숫자를 출력합니다.
    }

    private void ShuffleNumberPool()
    {
        // Fisher-Yates 셔플 알고리즘을 사용하여 숫자 풀을 섞어줍니다.
        for (int i = numberPool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = numberPool[i];
            numberPool[i] = numberPool[j];
            numberPool[j] = temp;
        }
    }

    public void BlockColorChange()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Block"))
            {
                float per = int.Parse(child.GetComponentInChildren<TextMeshPro>().text) / (float)blockScore;
                Color curColor;
                if (per <= 0.1428f) curColor = GetColor(GradientColor.Yellow);
                else if (per <= 0.2856f) curColor = GetColor(GradientColor.LightPeach);
                else if (per <= 0.4284f) curColor = GetColor(GradientColor.Peach);
                else if (per <= 0.5172f) curColor = GetColor(GradientColor.LightOrange);
                else if (per <= 0.714f) curColor = GetColor(GradientColor.Orange);
                else if (per <= 0.8268f) curColor = GetColor(GradientColor.OrangeRed);
                else curColor = GetColor(GradientColor.Red);
                child.GetComponent<SpriteRenderer>().color = curColor;

            }
        }
    }
    
}
