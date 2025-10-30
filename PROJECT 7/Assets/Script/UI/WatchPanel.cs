using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardBattleGame
{
    /// <summary>
    /// 怀表面板UI
    /// </summary>
    public class WatchPanel : MonoBehaviour
    {
        [Header("视觉组件")]
        public Image watchResultImage;
        public Image numberImage;
        public TextMeshProUGUI effectDescriptionText;
        public Animator watchAnimator;

        [Header("按钮")]
        public Button continueButton;

        private System.Action onContinue;

        private void Start()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        /// <summary>
        /// 显示怀表结果
        /// </summary>
        public void ShowResult(Sprite watchSprite, Sprite numberSprite, string description, string animTrigger, System.Action onContinueCallback)
        {
            gameObject.SetActive(true);

            if (watchResultImage != null)
            {
                watchResultImage.sprite = watchSprite;
            }

            if (numberImage != null)
            {
                numberImage.sprite = numberSprite;
            }

            if (effectDescriptionText != null)
            {
                effectDescriptionText.text = description;
            }

            if (watchAnimator != null && !string.IsNullOrEmpty(animTrigger))
            {
                watchAnimator.SetTrigger(animTrigger);
            }

            onContinue = onContinueCallback;
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 继续按钮点击
        /// </summary>
        private void OnContinueClicked()
        {
            Hide();
            onContinue?.Invoke();
        }
    }
}

