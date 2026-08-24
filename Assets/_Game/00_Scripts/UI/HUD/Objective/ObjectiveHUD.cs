using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ObjectiveHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text objectiveText;

    [Header("Display")]
    [SerializeField] private string header = "OBJECTIVES";
    [SerializeField] private string incompleteFormat = "• {0}  {1}/{2}";
    [SerializeField] private string completedFormat = "• {0}  ✓";

    [Header("Completed Objective")]
    [Tooltip("How long a completed objective remains visible before fading out.")]
    [SerializeField] private float completedDisplayDuration = 2f;

    [Tooltip("Duration of the fade-out animation.")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeRoutine;

    private void OnEnable()
    {
        if (ObjectiveManager.Instance == null)
            return;

        ObjectiveManager.Instance.OnObjectivesChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (ObjectiveManager.Instance != null)
            ObjectiveManager.Instance.OnObjectivesChanged -= Refresh;
    }

    private void Refresh()
    {
        if (objectiveText == null)
            return;

        ObjectiveManager manager = ObjectiveManager.Instance;

        if (manager == null)
            return;

        BuildText(manager);

        if (HasCompletedObjective(manager))
        {
            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(
                FadeCompletedObjectiveRoutine()
            );
        }
        else
        {
            SetAlpha(1f);
        }
    }

    private void BuildText(ObjectiveManager manager)
    {
        StringBuilder builder = new StringBuilder();

        if (!string.IsNullOrEmpty(header))
        {
            builder.AppendLine(header);
            builder.AppendLine();
        }

        foreach (Objective objective in manager.Objectives)
        {
            if (objective.IsCompleted)
            {
                builder.AppendLine(
                    string.Format(
                        completedFormat,
                        objective.ObjectiveName
                    )
                );
            }
            else
            {
                builder.AppendLine(
                    string.Format(
                        incompleteFormat,
                        objective.ObjectiveName,
                        objective.Progress,
                        objective.Threshold
                    )
                );
            }
        }

        objectiveText.text = builder.ToString();
    }

    private bool HasCompletedObjective(ObjectiveManager manager)
    {
        foreach (Objective objective in manager.Objectives)
        {
            if (objective.IsCompleted)
                return true;
        }

        return false;
    }

    private IEnumerator FadeCompletedObjectiveRoutine()
    {
        yield return new WaitForSeconds(
            completedDisplayDuration
        );

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = 1f - Mathf.Clamp01(
                elapsed / fadeDuration
            );

            SetAlpha(alpha);

            yield return null;
        }

        SetAlpha(0f);

        RemoveCompletedObjectives();

        SetAlpha(1f);

        fadeRoutine = null;
    }

    private void RemoveCompletedObjectives()
    {
        ObjectiveManager manager = ObjectiveManager.Instance;

        if (manager == null)
            return;

        List<string> completedObjectives = new List<string>();

        foreach (Objective objective in manager.Objectives)
        {
            if (objective.IsCompleted)
                completedObjectives.Add(objective.ObjectiveName);
        }

        foreach (string objectiveName in completedObjectives)
        {
            manager.RemoveObjective(objectiveName);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = objectiveText.color;
        color.a = alpha;
        objectiveText.color = color;
    }
}