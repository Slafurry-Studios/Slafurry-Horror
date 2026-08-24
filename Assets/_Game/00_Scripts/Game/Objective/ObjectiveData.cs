using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(
    fileName = "NewObjective",
    menuName = "Game/Objective"
)]
public class ObjectiveData : ScriptableObject
{
    [Header("Objective")]
    public string objectiveName;

    [Min(1)]
    public int threshold = 1;

    [Header("Events")]
    public UnityEvent onCompleted;
}