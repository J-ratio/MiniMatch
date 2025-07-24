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
    private float elapsedTime = 0f;
    private bool gameActive = false;
    private int matchCount = 0;
    private int totalMatches = 6; // 6 pairs

    private Card firstFlipped;
    private Card secondFlipped;
    private bool canFlip = true;
    private List<int> cardIds = new List<int>();
    private List<CardData> selectedCardData = new List<CardData>();

    void OnEnable()
    {
        Card.OnAnyCardClicked += OnCardClicked;
    }

    void OnDisable()
    {
        Card.OnAnyCardClicked -= OnCardClicked;
    }

    void Start()
    {
        SetupCards();
        elapsedTime = 0f;
        gameActive = true;
        matchCount = 0;
        UpdateTimerUI();
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

    void SetupCards()
    {
        // Select 6 unique CardData at random
        selectedCardData.Clear();
        List<CardData> pool = new List<CardData>(allCardData);
        for (int i = 0; i < totalMatches; i++)
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
    }

    void OnCardClicked(Card card)
    {
        if (!canFlip || card == firstFlipped || card == secondFlipped)
            return;
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
                // Optionally, show a message in the UI
            }
        }
        else
        {
            firstFlipped.Flip();
            secondFlipped.Flip();
        }
        firstFlipped = null;
        secondFlipped = null;
        canFlip = true;
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
