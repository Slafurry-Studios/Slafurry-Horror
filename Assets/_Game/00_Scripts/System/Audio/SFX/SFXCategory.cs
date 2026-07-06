using Slafurry.Utils.Attributes;
using UnityEngine;


[System.Serializable]
public class SFXEffect
{
    public string groupID;
    public AudioClip[] clips;
    [Range(0f, 10f)]
    public float volume = 1f;
    public int maxSimultaneous = 3;
}

namespace Slafurry.System.Audio
{

    [GameAssetCreator("Audio/SFX", "SFX Category", order: 2)]
    [CreateAssetMenu(fileName = "New SFX Category", menuName = "Audio/SFX Category", order = 1)]
    public class SFXCategory : ScriptableObject
    {
        public string categoryName;
        public SFXEffect[] effects;
        public int poolSize = 8;
    }
}