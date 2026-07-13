using UnityEngine;

// Siapa yang "ngomong" — buat nentuin gaya teks nanti
public enum Speaker
{
    Saya,     // pemain
    Bina,   // coworker
    dalam_hati   // kata dalam hati
}

// Satu baris dialog
[System.Serializable]
public struct DialogueLine
{
    public Speaker speaker;
    [TextArea(1, 4)] public string text;   
    public float autoAdvanceDelay;         
}

[CreateAssetMenu(menuName = "Slafurry/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public bool lockPlayer;      
    public DialogueLine[] lines;
}