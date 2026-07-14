using System;
using System.Collections;
using UnityEngine;
using TMPro;
using Slafurry.System.Audio;

public class DialoguePlayer : MonoBehaviour
{
    public static DialoguePlayer instance;

    [Header("Referensi")]
    [SerializeField] private DialogueUI ui;
    [SerializeField] private FirstPersonLook playerLook;

    [Header("Typewriter")]
    [SerializeField] private float charInterval = 0.03f;
    [SerializeField] private string typeSfxCategory = "UI";
    [SerializeField] private string typeSfxId = "Type_Effect";

    public bool IsPlaying { get; private set; }
    private Func<float> _freezeFunc;

    private void Awake() => instance = this;

    public void Play(DialogueData data, Action onComplete = null)
    {
        if (IsPlaying) return;
        if (data == null || data.lines == null || data.lines.Length == 0) return;
        StartCoroutine(RunDialogue(data, onComplete));
    }

    private IEnumerator RunDialogue(DialogueData data, Action onComplete)
    {
        IsPlaying = true;
        ui.Show();
        if (data.lockPlayer) LockPlayer();

        foreach (DialogueLine line in data.lines)
            yield return ShowLine(line);

        if (data.lockPlayer) UnlockPlayer();
        ui.Hide();
        IsPlaying = false;
        onComplete?.Invoke();
    }

    private IEnumerator ShowLine(DialogueLine line)
    {
        ui.SetSpeaker(line.speaker);
        ui.SetContinueVisible(false);

        TMP_Text body = ui.Body;
        body.text = line.text;
        body.ForceMeshUpdate();
        int total = body.textInfo.characterCount;
        body.maxVisibleCharacters = 0;

        while (Input.GetMouseButton(0)) yield return null;

        Audio.PlaySFX2D(typeSfxCategory, typeSfxId, loop: true);
        int visible = 0;
        float timer = 0f;
        while (visible < total)
        {
            if (Input.GetMouseButtonDown(0)) { visible = total; break; }
            timer += Time.deltaTime;
            if (timer >= charInterval)
            {
                timer -= charInterval;
                visible++;
                body.maxVisibleCharacters = visible;
            }
            yield return null;
        }
        body.maxVisibleCharacters = total;
        Audio.StopSFX(typeSfxCategory, typeSfxId);

        ui.SetContinueVisible(true);
        if (line.autoAdvanceDelay > 0f)
        {
            yield return new WaitForSeconds(line.autoAdvanceDelay);
        }
        else
        {
            while (Input.GetMouseButton(0)) yield return null;
            while (!Input.GetMouseButtonDown(0)) yield return null;
        }
    }

    private void LockPlayer()
    {
        if (FirstPersonMovement.instance != null)
        {
            _freezeFunc = () => 0f;
            FirstPersonMovement.instance.speedOverrides.Add(_freezeFunc);
        }
        if (playerLook != null) playerLook.LockRotasi();
    }

    private void UnlockPlayer()
    {
        if (FirstPersonMovement.instance != null && _freezeFunc != null)
        {
            FirstPersonMovement.instance.speedOverrides.Remove(_freezeFunc);
            _freezeFunc = null;
        }
        if (playerLook != null) playerLook.UnlockRotasi();
    }
}