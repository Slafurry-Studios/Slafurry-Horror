using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("Referensi UI")]
    [SerializeField] private CanvasGroup panelGroup;        
    [SerializeField] private TMP_Text speakerLabel;         
    [SerializeField] private TMP_Text bodyText;             
    [SerializeField] private GameObject continueIndicator;  

    
    public TMP_Text Body => bodyText;

    private void Awake() => Hide();

    public void Show() => panelGroup.alpha = 1f;

    public void Hide()
    {
        panelGroup.alpha = 0f;
        SetContinueVisible(false);
    }

    public void SetSpeaker(Speaker speaker)
    {
        bool isInner = speaker == Speaker.dalam_hati;

        speakerLabel.gameObject.SetActive(!isInner);
        if (!isInner) speakerLabel.text = speaker.ToString();

        bodyText.fontStyle = isInner ? FontStyles.Italic : FontStyles.Normal;
    }

    public void SetContinueVisible(bool visible) => continueIndicator.SetActive(visible);
}