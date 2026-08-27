using UnityEngine;
using UnityEngine.Events;

public class ObjectiveTrigger : BaseTrigger
{
    public enum ObjectiveAction
    {
        Add,
        Remove,
        Complete,
        AddProgress,
        SetProgress,
        Reset
    }

    [Header("Objective")]

    [Tooltip(
        "Operation to perform on the objective.\n" +
        "Add: Creates the objective if it is not already active.\n" +
        "Remove: Removes the objective from the active list.\n" +
        "Complete: Immediately completes the objective.\n" +
        "AddProgress: Adds progress toward the objective threshold.\n" +
        "SetProgress: Sets the objective progress to a specific value.\n" +
        "Reset: Resets the objective progress to zero."
    )]
    [SerializeField] private ObjectiveAction action;

    [Tooltip(
        "Objective definition this trigger will operate on."
    )]
    [SerializeField] private ObjectiveData objectiveData;

    [Header("Progress")]

    [Tooltip(
        "Amount of progress to add or set.\n" +
        "Used by AddProgress and SetProgress actions."
    )]
    [SerializeField] private int amount = 1;

    [Header("Completion Event")]

    [Tooltip(
        "Called when this trigger causes the objective to transition " +
        "from incomplete to completed.\n\n" +
        "Only relevant for AddProgress, SetProgress, and Complete."
    )]
    [SerializeField] private UnityEvent onComplete;

    public void Execute()
    {
        if (!CanTrigger())
            return;

        if (objectiveData == null)
            return;

        ObjectiveManager manager = ObjectiveManager.Instance;

        if (manager == null)
            return;

        Objective objective = manager.GetObjective(
            objectiveData.objectiveName
        );

        bool wasCompleted =
            objective != null && objective.IsCompleted;

        switch (action)
        {
            case ObjectiveAction.Add:

                manager.AddObjective(objectiveData);
                break;

            case ObjectiveAction.Remove:

                manager.RemoveObjective(
                    objectiveData.objectiveName
                );

                break;

            case ObjectiveAction.Complete:

                manager.Complete(
                    objectiveData.objectiveName
                );

                break;

            case ObjectiveAction.AddProgress:

                manager.AddProgress(
                    objectiveData.objectiveName,
                    amount
                );

                break;

            case ObjectiveAction.SetProgress:

                manager.SetProgress(
                    objectiveData.objectiveName,
                    amount
                );

                break;

            case ObjectiveAction.Reset:

                manager.ResetObjective(
                    objectiveData.objectiveName
                );

                break;
        }

        AddTriggerCount();


        if (action != ObjectiveAction.AddProgress &&
            action != ObjectiveAction.SetProgress &&
            action != ObjectiveAction.Complete)
        {
            return;
        }

        objective = manager.GetObjective(
            objectiveData.objectiveName
        );

        bool isCompleted =
            objective != null && objective.IsCompleted;

        if (!wasCompleted && isCompleted)
            onComplete?.Invoke();
    }
}