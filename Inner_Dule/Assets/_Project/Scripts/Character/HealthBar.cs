using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace InnerDuel.Characters
{
    public class HealthBar : MonoBehaviour
    {
        [Header("Slider References")]
        public Slider healthSlider;
        [Tooltip("Optional delayed slider. If null or same as healthSlider, one will be generated at runtime.")]
        public Slider delayedHealthSlider;

        [Header("Optional Visual References")]
        public Image healthFillImage;
        public Image delayedFillImage;
        public Image frameImage;
        public TMP_Text hpValueText;

        [Header("Animation")]
        [Min(0f)] public float delayTime = 0.25f;
        [Min(1f)] public float fillSpeed = 220f;
        [Min(1f)] public float immediateFillSpeed = 650f;

        [Header("Color Style")]
        public Gradient healthGradient;
        public Color delayedDamageColor = new Color(1f, 0.8f, 0.25f, 0.95f);
        public Color lowHealthPulseColor = new Color(1f, 0.2f, 0.2f, 1f);
        [Range(0.05f, 0.5f)] public float lowHealthThreshold = 0.25f;
        [Min(0f)] public float lowHealthPulseSpeed = 6f;
        [Range(0f, 1f)] public float lowHealthPulseStrength = 0.45f;

        [Header("Text")]
        public string hpPrefix = "HP";

        private float maxHealth = 100f;
        private float targetHealth = 100f;
        private float displayedHealth = 100f;
        private float delayedHealth = 100f;
        private float delayedStartTimer = 0f;
        private bool visualsInitialized;
        private Color defaultFrameColor = Color.white;
        private InnerCharacterController boundCharacter;

        private void Awake()
        {
            NormalizeLegacySettings();
            EnsureGradientDefaults();
            CacheReferences();
            SetupVisuals();
            SetMaxHealth(maxHealth);
            SetHealth(maxHealth, true);
        }

        private void OnEnable()
        {
            UpdateVisualState();
        }

        private void OnDisable()
        {
            UnbindCharacter();
        }

        private void Update()
        {
            if (!visualsInitialized || healthSlider == null)
            {
                return;
            }

            displayedHealth = Mathf.MoveTowards(displayedHealth, targetHealth, immediateFillSpeed * Time.deltaTime);

            if (targetHealth < delayedHealth)
            {
                delayedStartTimer -= Time.deltaTime;
                if (delayedStartTimer <= 0f)
                {
                    delayedHealth = Mathf.MoveTowards(delayedHealth, targetHealth, fillSpeed * Time.deltaTime);
                }
            }
            else
            {
                delayedStartTimer = 0f;
                delayedHealth = Mathf.MoveTowards(delayedHealth, targetHealth, immediateFillSpeed * Time.deltaTime);
            }

            ApplySliderValues(displayedHealth, delayedHealth);
            UpdateVisualState();
        }

        public void BindToCharacter(InnerCharacterController character)
        {
            if (boundCharacter == character)
            {
                return;
            }

            UnbindCharacter();
            boundCharacter = character;
            if (boundCharacter == null)
            {
                return;
            }

            boundCharacter.OnHealthChanged += HandleCharacterHealthChanged;
            SetMaxHealth(boundCharacter.MaxHealth);
            SetHealth(boundCharacter.CurrentHealth, true);
        }

        public void UnbindCharacter()
        {
            if (boundCharacter != null)
            {
                boundCharacter.OnHealthChanged -= HandleCharacterHealthChanged;
                boundCharacter = null;
            }
        }

        private void HandleCharacterHealthChanged(float current, float max)
        {
            if (!Mathf.Approximately(max, maxHealth))
            {
                SetMaxHealth(max);
            }

            SetHealth(current);
        }

        public void SetMaxHealth(float maxHealth)
        {
            this.maxHealth = Mathf.Max(1f, maxHealth);

            if (healthSlider != null)
            {
                healthSlider.minValue = 0f;
                healthSlider.maxValue = this.maxHealth;
            }

            if (delayedHealthSlider != null)
            {
                delayedHealthSlider.minValue = 0f;
                delayedHealthSlider.maxValue = this.maxHealth;
            }

            targetHealth = this.maxHealth;
            displayedHealth = this.maxHealth;
            delayedHealth = this.maxHealth;
            delayedStartTimer = 0f;
            ApplySliderValues(displayedHealth, delayedHealth);
            UpdateVisualState();
        }

        public void SetHealth(float currentHealth, bool instant = false)
        {
            float clampedHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
            bool wasDamage = clampedHealth < targetHealth;
            targetHealth = clampedHealth;

            if (instant)
            {
                displayedHealth = clampedHealth;
                delayedHealth = clampedHealth;
                delayedStartTimer = 0f;
                ApplySliderValues(displayedHealth, delayedHealth);
                UpdateVisualState();
                return;
            }

            delayedStartTimer = wasDamage ? delayTime : 0f;
        }

        public void ResetHealthBar()
        {
            SetMaxHealth(maxHealth);
            SetHealth(maxHealth, true);
        }

        public void TakeDamage(int damage)
        {
            float newHealth = Mathf.Max(0f, targetHealth - damage);
            SetHealth(newHealth);
        }

        private void CacheReferences()
        {
            if (healthSlider == null)
            {
                healthSlider = GetComponent<Slider>();
            }

            if (healthSlider == null)
            {
                healthSlider = GetComponentInChildren<Slider>(true);
            }

            if (healthSlider == null)
            {
                Debug.LogWarning("[HealthBar] Missing health slider reference.", this);
                visualsInitialized = false;
                return;
            }

            if (delayedHealthSlider == null || delayedHealthSlider == healthSlider)
            {
                delayedHealthSlider = CreateDelayedSlider(healthSlider);
            }

            if (healthFillImage == null && healthSlider.fillRect != null)
            {
                healthFillImage = healthSlider.fillRect.GetComponent<Image>();
            }

            if (delayedFillImage == null && delayedHealthSlider != null && delayedHealthSlider.fillRect != null)
            {
                delayedFillImage = delayedHealthSlider.fillRect.GetComponent<Image>();
            }

            if (frameImage == null)
            {
                frameImage = GetComponent<Image>();
            }

            if (frameImage == null && healthSlider != null)
            {
                Transform background = healthSlider.transform.Find("Background");
                if (background != null)
                {
                    frameImage = background.GetComponent<Image>();
                }
            }

            if (hpValueText == null)
            {
                hpValueText = GetComponentInChildren<TMP_Text>(true);
            }

            if (frameImage != null)
            {
                defaultFrameColor = frameImage.color;
            }

            visualsInitialized = true;
        }

        private void SetupVisuals()
        {
            if (!visualsInitialized)
            {
                return;
            }

            SetupSliderVisual(healthSlider);
            SetupSliderVisual(delayedHealthSlider);

            if (delayedFillImage != null)
            {
                delayedFillImage.color = delayedDamageColor;
                delayedFillImage.raycastTarget = false;
            }

            if (healthFillImage != null)
            {
                healthFillImage.raycastTarget = false;
            }
        }

        private void SetupSliderVisual(Slider slider)
        {
            if (slider == null)
            {
                return;
            }

            slider.interactable = false;
            slider.transition = Selectable.Transition.None;

            if (slider.handleRect != null)
            {
                slider.handleRect.gameObject.SetActive(false);
                slider.handleRect = null;
            }
        }

        private Slider CreateDelayedSlider(Slider source)
        {
            if (source == null)
            {
                return null;
            }

            RectTransform sourceRect = source.GetComponent<RectTransform>();
            GameObject delayedObj = new GameObject("DelayedHealthSlider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Slider));
            delayedObj.transform.SetParent(source.transform.parent, false);

            RectTransform delayedRect = delayedObj.GetComponent<RectTransform>();
            CopyRectTransform(sourceRect, delayedRect);
            delayedObj.transform.SetSiblingIndex(source.transform.GetSiblingIndex());

            Image delayedRootImage = delayedObj.GetComponent<Image>();
            delayedRootImage.color = Color.clear;
            delayedRootImage.raycastTarget = false;

            Slider delayedSlider = delayedObj.GetComponent<Slider>();
            delayedSlider.minValue = source.minValue;
            delayedSlider.maxValue = source.maxValue;
            delayedSlider.wholeNumbers = source.wholeNumbers;
            delayedSlider.direction = source.direction;
            delayedSlider.interactable = false;
            delayedSlider.transition = Selectable.Transition.None;

            RectTransform sourceFillArea = source.fillRect != null ? source.fillRect.parent as RectTransform : null;
            GameObject fillAreaObj = new GameObject("Fill Area", typeof(RectTransform));
            fillAreaObj.transform.SetParent(delayedRect, false);
            RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
            if (sourceFillArea != null)
            {
                CopyRectTransform(sourceFillArea, fillAreaRect);
            }
            else
            {
                fillAreaRect.anchorMin = Vector2.zero;
                fillAreaRect.anchorMax = Vector2.one;
                fillAreaRect.offsetMin = Vector2.zero;
                fillAreaRect.offsetMax = Vector2.zero;
            }

            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillObj.transform.SetParent(fillAreaRect, false);
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            if (source.fillRect != null)
            {
                CopyRectTransform(source.fillRect, fillRect);
            }
            else
            {
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = Vector2.one;
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
            }

            Image sourceFill = source.fillRect != null ? source.fillRect.GetComponent<Image>() : null;
            Image delayedFill = fillObj.GetComponent<Image>();
            if (sourceFill != null)
            {
                delayedFill.sprite = sourceFill.sprite;
                delayedFill.material = sourceFill.material;
                delayedFill.type = sourceFill.type;
                delayedFill.fillMethod = sourceFill.fillMethod;
                delayedFill.fillOrigin = sourceFill.fillOrigin;
                delayedFill.fillClockwise = sourceFill.fillClockwise;
                delayedFill.fillAmount = sourceFill.fillAmount;
                delayedFill.preserveAspect = sourceFill.preserveAspect;
            }

            delayedFill.color = delayedDamageColor;
            delayedFill.raycastTarget = false;

            delayedSlider.fillRect = fillRect;
            delayedSlider.handleRect = null;
            delayedSlider.targetGraphic = delayedFill;

            return delayedSlider;
        }

        private static void CopyRectTransform(RectTransform source, RectTransform target)
        {
            if (source == null || target == null)
            {
                return;
            }

            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.anchoredPosition = source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
        }

        private void ApplySliderValues(float main, float delayed)
        {
            if (healthSlider != null)
            {
                healthSlider.value = main;
            }

            if (delayedHealthSlider != null)
            {
                delayedHealthSlider.value = delayed;
            }
        }

        private void UpdateVisualState()
        {
            if (!visualsInitialized)
            {
                return;
            }

            float ratio = maxHealth > 0f ? Mathf.Clamp01(displayedHealth / maxHealth) : 0f;

            if (healthFillImage != null)
            {
                healthFillImage.color = healthGradient.Evaluate(ratio);
            }

            if (delayedFillImage != null)
            {
                delayedFillImage.color = delayedDamageColor;
            }

            if (hpValueText != null)
            {
                int current = Mathf.CeilToInt(targetHealth);
                int max = Mathf.CeilToInt(maxHealth);
                hpValueText.text = $"{hpPrefix} {current}/{max}";
            }

            if (frameImage != null)
            {
                if (ratio <= lowHealthThreshold)
                {
                    float pulse = (Mathf.Sin(Time.unscaledTime * lowHealthPulseSpeed) + 1f) * 0.5f;
                    float amount = pulse * lowHealthPulseStrength;
                    frameImage.color = Color.Lerp(defaultFrameColor, lowHealthPulseColor, amount);
                }
                else
                {
                    frameImage.color = defaultFrameColor;
                }
            }
        }

        private void EnsureGradientDefaults()
        {
            if (healthGradient == null || healthGradient.colorKeys == null || healthGradient.colorKeys.Length == 0)
            {
                healthGradient = new Gradient();
                healthGradient.SetKeys(
                    new[]
                    {
                        new GradientColorKey(new Color(0.85f, 0.2f, 0.2f), 0f),
                        new GradientColorKey(new Color(0.96f, 0.78f, 0.18f), 0.5f),
                        new GradientColorKey(new Color(0.2f, 0.86f, 0.35f), 1f)
                    },
                    new[]
                    {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 1f)
                    }
                );
            }
        }

        private void NormalizeLegacySettings()
        {
            // Scene cũ dùng cấu hình slider mặc định; tự nâng tuning để phù hợp thanh máu theo maxHealth.
            if (delayTime >= 0.9f)
            {
                delayTime = 0.25f;
            }

            if (fillSpeed <= 10f)
            {
                fillSpeed = 220f;
            }

            if (immediateFillSpeed <= 10f)
            {
                immediateFillSpeed = 650f;
            }
        }
    }
}
