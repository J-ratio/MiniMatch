using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image frontImage; // The icon shown when the card is face up
    public GameObject front;
    public GameObject back;
    public static event System.Action<Card> OnAnyCardClicked;

    [HideInInspector]
    public int cardId; // Used to identify pairs

    private bool isFlipped = false;
    private bool isMatched = false;
    private CardFlipAnimation flipAnim;
    public Material outlineMaterial; // Assign GlowOutline.mat in Inspector or via code
    public AudioClip flipSound; // Assign rollover4.ogg in Inspector
    public AudioClip matchSound; // Assign a match sound in Inspector
    private AudioSource audioSource;

    void Awake()
    {
        flipAnim = GetComponent<CardFlipAnimation>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void SetCard(int id, Sprite icon)
    {
        cardId = id;
        frontImage.sprite = icon;
        isFlipped = false;
        isMatched = false;
        front.SetActive(false);
        back.SetActive(true);
    }

    public void OnCardClicked()
    {
        OnAnyCardClicked?.Invoke(this);
    }

    public void Flip()
    {
        isFlipped = !isFlipped;
        if (flipAnim != null)
        {
            if (isFlipped)
                flipAnim.FlipToFront();
            else
                flipAnim.FlipToBack();
        }
        else
        {
            front.SetActive(isFlipped);
            back.SetActive(!isFlipped);
        }
        if (flipSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(flipSound);
        }
    }

    public void SetMatched()
    {
        isMatched = true;
        if (outlineMaterial != null && frontImage != null)
        {
            frontImage.material = outlineMaterial;
        }
        if (matchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(matchSound);
        }
    }
}
