using UnityEngine;

[System.Serializable]
public struct SceneTrack
{
    public string sceneName;
    public string trackName;
}

[System.Serializable]
public struct MusicTrack
{
    public string trackName;
    public AudioClip clip;
    [Range(0f, 10f)]
    public float volume;
}
