using Slafurry.Utils.Attributes;
using UnityEngine;

namespace Slafurry.System.Audio
{
    [GameAssetCreator("Audio/Music", "Music Data", order: 1)]
    [CreateAssetMenu (fileName = "New Music Data", menuName = "Audio/Music Data", order = 1)]
    public class MusicData : ScriptableObject
    {
        public MusicTrack[] tracks;

        public MusicTrack GetTrack(string trackName)
        {
            foreach (var track in tracks)
            {
                if (track.trackName == trackName)
                    return track;
            }
            return default;
        }

        public AudioClip GetClipFromName(string trackName)
        {
            return GetTrack(trackName).clip;
        }
    }
}