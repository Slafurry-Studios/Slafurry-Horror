using UnityEngine;

public class ExamineInfo : MonoBehaviour
{
    public string title;

    [TextArea(2, 5)]
    public string description;
}