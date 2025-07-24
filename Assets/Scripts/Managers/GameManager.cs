using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform gridParent;
    public CardData[] allCardData; // Assign all CardData assets in Inspector
    public int rows = 3;
    public int cols = 4;
    public UIManager uiManager; // Reference to UIManager
    public TextMeshProUGUI timerText; // Assign in Inspector
    public LevelData[] levels; // Assign LevelData assets in Inspector
    public SummaryScreen summaryScreen; // Assign in Inspector
    private int currentLevel = 0;
    private List<LevelMetrics> allMetrics = new List<LevelMetrics>();
    private LevelMetrics currentMetrics;
    private float flipStartTime = 0f;
    private float elapsedTime = 0f;
    private bool gameActive = false;
    private int matchCount = 0;
    private int totalMatches = 6; // 6 pairs

    private Card firstFlipped;
    private Card secondFlipped;
    private bool canFlip = true;
    private List<int> cardIds = new List<int>();
    private List<CardData> selectedCardData = new List<CardData>();
    private float lastCardClickTime = 0f;

    void OnEnable()
    {
        Card.OnAnyCardClicked += OnCardClicked;
        Card.OnAnyCardFlipped += OnCardFlipped;
    }

    void OnDisable()
    {
        Card.OnAnyCardClicked -= OnCardClicked;
        Card.OnAnyCardFlipped -= OnCardFlipped;
    }

    void Start()
    {
        currentLevel = 0;
        allMetrics.Clear();
        StartLevel();
    }

    void StartLevel()
    {
        if (currentLevel >= levels.Length)
        {
            ShowSummary();
            return;
        }
        LevelData level = levels[currentLevel];
        SetupLevel(level);
        currentMetrics = new LevelMetrics();
        flipStartTime = 0f;
        lastCardClickTime = 0f;
        // Optionally show preview for level.previewTime
    }

    void SetupLevel(LevelData level)
    {
        // Use level.rows, level.cols, level.numPairs for grid setup
        int rows = level.rows;
        int cols = level.cols;
        int numPairs = level.numPairs;
        // Select numPairs unique CardData at random
        selectedCardData.Clear();
        List<CardData> pool = new List<CardData>(allCardData);
        for (int i = 0; i < numPairs; i++)
        {
            int idx = Random.Range(0, pool.Count);
            selectedCardData.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        // Prepare pairs (2 of each)
        cardIds.Clear();
        List<CardData> cardDataPairs = new List<CardData>();
        foreach (var data in selectedCardData)
        {
            cardDataPairs.Add(data);
            cardDataPairs.Add(data);
        }
        // Shuffle
        for (int i = 0; i < cardDataPairs.Count; i++)
        {
            int rnd = Random.Range(i, cardDataPairs.Count);
            var temp = cardDataPairs[i];
            cardDataPairs[i] = cardDataPairs[rnd];
            cardDataPairs[rnd] = temp;
        }
        // Instantiate cards
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
        for (int i = 0; i < rows * cols; i++)
        {
            GameObject cardObj = Instantiate(cardPrefab, gridParent);
            Card card = cardObj.GetComponent<Card>();
            CardData data = cardDataPairs[i];
            card.SetCard(data.id, data.image);
            cardObj.GetComponent<UnityEngine.UI.Button>().onClick.RemoveAllListeners();
            cardObj.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(card.OnCardClicked);
        }
        // Update matchCount and totalMatches for this level
        matchCount = 0;
        totalMatches = numPairs;
        elapsedTime = 0f;
        gameActive = true;
        UpdateTimerUI();
        // Set grid layout columns
        var grid = gridParent.GetComponent<UnityEngine.UI.GridLayoutGroup>();
        if (grid != null)
        {
            grid.constraint = UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = cols;
        }
    }

    void Update()
    {
        if (gameActive)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
        }
    }

    void OnCardClicked(Card card)
    {
        if (!canFlip || card == firstFlipped || card == secondFlipped)
            return;

        // Track decision time
        float currentTime = Time.time;
        if (lastCardClickTime > 0)
        {
            float decisionTime = currentTime - lastCardClickTime;
            currentMetrics.decisionTimes.Add(decisionTime);
        }
        lastCardClickTime = currentTime;

        card.Flip();
        if (firstFlipped == null)
        {
            firstFlipped = card;
        }
        else if (secondFlipped == null)
        {
            secondFlipped = card;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        canFlip = false;
        yield return new WaitForSeconds(1f);
        if (firstFlipped.cardId == secondFlipped.cardId)
        {
            firstFlipped.SetMatched();
            secondFlipped.SetMatched();
            if (uiManager != null)
                uiManager.IncrementScore();
            matchCount++;
            if (matchCount >= totalMatches)
            {
                gameActive = false;
                Debug.Log($"Game complete! Time taken: {elapsedTime:F2} seconds");
                OnLevelComplete();
            }
        }
        else
        {
            firstFlipped.Flip();
            secondFlipped.Flip();
            OnMistake(firstFlipped.cardId, secondFlipped.cardId);
        }
        firstFlipped = null;
        secondFlipped = null;
        canFlip = true;
    }

    void OnCardFlipped(Card card)
    {
        OnCardFlipped();
    }

    void OnCardFlipped()
    {
        currentMetrics.flips++;
        if (flipStartTime > 0f)
        {
            float flipTime = Time.time - flipStartTime;
            currentMetrics.flipTimes.Add(flipTime);
        }
        flipStartTime = Time.time;
    }

    void OnMistake(int id1, int id2)
    {
        currentMetrics.mistakes++;
        string key = id1 < id2 ? $"{id1}-{id2}" : $"{id2}-{id1}";
        
        // Track repeated mistakes
        if (!currentMetrics.repeatedMistakes.ContainsKey(key))
            currentMetrics.repeatedMistakes[key] = 0;
        
        currentMetrics.repeatedMistakes[key]++;
        
        // If same mistake is made more than once, count as perseverative error
        if (currentMetrics.repeatedMistakes[key] > 1)
        {
            currentMetrics.perseverativeErrors++;
        }
    }

    void OnLevelComplete()
    {
        currentMetrics.timeTaken = elapsedTime;
        allMetrics.Add(currentMetrics);
        currentLevel++;
        StartLevel();
    }

    void ShowSummary()
    {
        if (summaryScreen != null)
            summaryScreen.gameObject.SetActive(true);
            summaryScreen.ShowSummary(allMetrics);
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
