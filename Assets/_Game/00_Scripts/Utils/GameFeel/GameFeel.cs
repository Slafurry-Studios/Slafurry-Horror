using UnityEngine;


namespace Slafurry.Utils.GameFeel
{

    public class GameFeel : MonoBehaviour
    {
        private IGameFeelEffect[] gameFeelEffects;

        public void PlayEffect()
        {
            foreach (var gameFeel in gameFeelEffects)
            {
                gameFeel.PlayEffect();
            }
        }

        public void StopEffect()
        {
            foreach (var gameFeel in gameFeelEffects)
            {
                gameFeel.StopEffect();
            }
        }
    }
}