using UnityEngine;
using TMPro;
using System.Collections;

namespace ColorChain.UI
{
    /// <summary>
    /// Animates TextMeshPro text with a color wave effect
    /// Each character cycles through a sequence of colors
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ColorWaveText : MonoBehaviour
    {
        [Header("Color Settings")]
        [SerializeField] private Color[] colorSequence = new Color[5]
        {
            new Color(1f, 0.2f, 0.2f),  // Red
            new Color(1f, 0.8f, 0.2f),  // Yellow
            new Color(0.2f, 1f, 0.2f),  // Green
            new Color(0.2f, 0.5f, 1f),  // Blue
            new Color(0.8f, 0.2f, 1f)   // Purple
        };

        [Header("Animation Settings")]
        [SerializeField] private float animationSpeed = 2f;
        [Tooltip("Offset between each character's color cycle (0-1)")]
        [SerializeField] [Range(0f, 1f)] private float characterOffset = 0.1f;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool loop = true;

        [Header("Target Settings")]
        [Tooltip("Leave empty to animate entire text, or specify a word like 'Color'")]
        [SerializeField] private string targetWord = "";
        [SerializeField] private bool caseSensitive = false;

        [Header("Floating Wave Settings")]
        [SerializeField] private bool enableFloatingWave = true;
        [SerializeField] private float waveAmplitude = 10f;
        [SerializeField] private float waveSpeed = 2f;
        [Tooltip("Distance between wave peaks (lower = tighter wave)")]
        [SerializeField] [Range(0.1f, 2f)] private float waveFrequency = 0.5f;

        private TextMeshProUGUI textMesh;
        private Coroutine animationCoroutine;
        private bool isAnimating = false;
        private Vector3[][] originalVertexPositions;

        private void Awake()
        {
            textMesh = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                StartAnimation();
            }
        }

        private void OnDisable()
        {
            StopAnimation();
        }

        /// <summary>
        /// Start the color wave animation
        /// </summary>
        public void StartAnimation()
        {
            if (textMesh == null || colorSequence == null || colorSequence.Length == 0)
                return;

            StopAnimation();
            animationCoroutine = StartCoroutine(AnimateColors());
        }

        /// <summary>
        /// Stop the color wave animation
        /// </summary>
        public void StopAnimation()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
                animationCoroutine = null;
            }
            isAnimating = false;
        }

        /// <summary>
        /// Set a new color sequence at runtime
        /// </summary>
        public void SetColorSequence(Color[] colors)
        {
            if (colors != null && colors.Length > 0)
            {
                colorSequence = colors;
            }
        }

        /// <summary>
        /// Set animation speed at runtime
        /// </summary>
        public void SetAnimationSpeed(float speed)
        {
            animationSpeed = Mathf.Max(0.1f, speed);
        }

        /// <summary>
        /// Enable or disable floating wave effect
        /// </summary>
        public void SetFloatingWave(bool enabled)
        {
            enableFloatingWave = enabled;
        }

        /// <summary>
        /// Set wave amplitude (height)
        /// </summary>
        public void SetWaveAmplitude(float amplitude)
        {
            waveAmplitude = amplitude;
        }

        private IEnumerator AnimateColors()
        {
            isAnimating = true;
            float time = 0f;

            // Force text mesh to update
            textMesh.ForceMeshUpdate();

            // Store original vertex positions for floating wave
            if (enableFloatingWave)
            {
                StoreOriginalVertexPositions();
            }

            while (isAnimating)
            {
                // Update mesh info
                textMesh.ForceMeshUpdate();
                TMP_TextInfo textInfo = textMesh.textInfo;

                if (textInfo == null || textInfo.characterCount == 0)
                {
                    yield return null;
                    continue;
                }

                // Find target range
                int startIndex = 0;
                int endIndex = textInfo.characterCount;

                if (!string.IsNullOrEmpty(targetWord))
                {
                    string sourceText = caseSensitive ? textMesh.text : textMesh.text.ToLower();
                    string searchWord = caseSensitive ? targetWord : targetWord.ToLower();
                    int wordIndex = sourceText.IndexOf(searchWord);

                    if (wordIndex >= 0)
                    {
                        startIndex = wordIndex;
                        endIndex = wordIndex + targetWord.Length;
                    }
                }

                // Animate each character
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                    // Skip if character is not visible or outside target range
                    if (!charInfo.isVisible || i < startIndex || i >= endIndex)
                        continue;

                    // Calculate color index with offset
                    float characterTime = time + (i - startIndex) * characterOffset;
                    Color color = GetColorAtTime(characterTime);

                    // Get vertex data
                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;
                    Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
                    Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                    // Apply color to all 4 vertices of the character
                    vertexColors[vertexIndex + 0] = color;
                    vertexColors[vertexIndex + 1] = color;
                    vertexColors[vertexIndex + 2] = color;
                    vertexColors[vertexIndex + 3] = color;

                    // Apply floating wave effect
                    if (enableFloatingWave && originalVertexPositions != null &&
                        materialIndex < originalVertexPositions.Length)
                    {
                        float waveOffset = Mathf.Sin((time * waveSpeed) + (i * waveFrequency)) * waveAmplitude;

                        // Apply wave offset to all 4 vertices
                        for (int j = 0; j < 4; j++)
                        {
                            int vIndex = vertexIndex + j;
                            if (vIndex < originalVertexPositions[materialIndex].Length)
                            {
                                Vector3 originalPos = originalVertexPositions[materialIndex][vIndex];
                                vertices[vIndex] = originalPos + new Vector3(0, waveOffset, 0);
                            }
                        }
                    }
                }

                // Update vertex data
                for (int i = 0; i < textInfo.meshInfo.Length; i++)
                {
                    textInfo.meshInfo[i].mesh.colors32 = textInfo.meshInfo[i].colors32;

                    if (enableFloatingWave)
                    {
                        textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                    }

                    textMesh.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
                }

                // Update time
                time += Time.deltaTime * animationSpeed;

                // Handle looping
                if (!loop && time >= colorSequence.Length)
                {
                    isAnimating = false;
                    break;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Store original vertex positions for floating wave animation
        /// </summary>
        private void StoreOriginalVertexPositions()
        {
            textMesh.ForceMeshUpdate();
            TMP_TextInfo textInfo = textMesh.textInfo;

            if (textInfo == null || textInfo.meshInfo == null)
                return;

            originalVertexPositions = new Vector3[textInfo.meshInfo.Length][];

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                Vector3[] sourceVertices = textInfo.meshInfo[i].vertices;
                originalVertexPositions[i] = new Vector3[sourceVertices.Length];
                System.Array.Copy(sourceVertices, originalVertexPositions[i], sourceVertices.Length);
            }
        }

        /// <summary>
        /// Get interpolated color at a specific time in the animation
        /// </summary>
        private Color GetColorAtTime(float time)
        {
            if (colorSequence.Length == 0)
                return Color.white;

            if (colorSequence.Length == 1)
                return colorSequence[0];

            // Normalize time to color sequence length
            float normalizedTime = time % colorSequence.Length;

            // Get indices
            int index1 = Mathf.FloorToInt(normalizedTime);
            int index2 = (index1 + 1) % colorSequence.Length;

            // Get interpolation factor
            float t = normalizedTime - index1;

            // Interpolate between colors
            return Color.Lerp(colorSequence[index1], colorSequence[index2], t);
        }

        /// <summary>
        /// Preview animation in editor (call from custom inspector)
        /// </summary>
        public void PreviewAnimation()
        {
            if (Application.isPlaying)
            {
                StartAnimation();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure we have at least 1 color
            if (colorSequence == null || colorSequence.Length == 0)
            {
                colorSequence = new Color[5]
                {
                    new Color(1f, 0.2f, 0.2f),  // Red
                    new Color(1f, 0.8f, 0.2f),  // Yellow
                    new Color(0.2f, 1f, 0.2f),  // Green
                    new Color(0.2f, 0.5f, 1f),  // Blue
                    new Color(0.8f, 0.2f, 1f)   // Purple
                };
            }
        }
#endif
    }
}
