using UnityEngine;
using UnityEngine.UI;
using Slafurry.System.Audio;

namespace Slafurry.Utils.UI
{
    /// <summary>
    /// Attach to any Button - automatically plays a UI click SFX through
    /// AudioSystem.SFX instead of wiring PlaySFX2D manually per button in
    /// the Inspector's OnClick list.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSFX : MonoBehaviour
    {
        [SerializeField] private string category = "UI";
        [SerializeField] private string effectName = "Click";

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(PlayClickSFX);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(PlayClickSFX);
        }

        private void PlayClickSFX()
        {
            if (AudioSystem.Instance != null)
                AudioSystem.SFX.PlaySFX2D(category, effectName);
        }
    }
}