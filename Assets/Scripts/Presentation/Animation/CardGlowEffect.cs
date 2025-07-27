using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MiniMatch.Presentation.Animation
{
    /// <summary>
    /// Handles glow effect for matched cards using simple coroutines
    /// </summary>
    public class CardGlowEffect : MonoBehaviour
    {
        [Header("Glow Settings")]
        [SerializeField] private float glowDuration = 0.5f;
        [SerializeField] private float glowIntensity = 1.5f;
        [SerializeField] private Color glowColor = Color.yellow;
        
        private Image _targetImage;
        private Color _originalColor;
        private Coroutine _glowCoroutine;
        
        private void Awake()
        {
            _targetImage = GetComponent<Image>();
            if (_targetImage != null)
            {
                _originalColor = _targetImage.color;
            }
        }
        
        public void StartGlow()
        {
            if (_targetImage == null) return;
            
            StopGlow();
            _glowCoroutine = StartCoroutine(GlowCoroutine());
        }
        
        public void StopGlow()
        {
            if (_glowCoroutine != null)
            {
                StopCoroutine(_glowCoroutine);
                _glowCoroutine = null;
            }
            
            if (_targetImage != null)
            {
                _targetImage.color = _originalColor;
            }
        }
        
        private IEnumerator GlowCoroutine()
        {
            for (int i = 0; i < 3; i++)
            {
                // Fade to glow color
                yield return StartCoroutine(LerpColor(_originalColor, glowColor * glowIntensity, glowDuration * 0.5f));
                
                // Fade back to original
                yield return StartCoroutine(LerpColor(glowColor * glowIntensity, _originalColor, glowDuration * 0.5f));
            }
        }
        
        private IEnumerator LerpColor(Color from, Color to, float duration)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                _targetImage.color = Color.Lerp(from, to, t);
                yield return null;
            }
            
            _targetImage.color = to;
        }
        
        private void OnDestroy()
        {
            StopGlow();
        }
    }
}