using UnityEngine;
using Core;
using Logger = Core.Logger;

namespace Audio
{
    public class PlayTrack : MonoBehaviour
    {
        [SerializeField] private TrackData _track;
        private void Start()
        {
            Logger.Instance.LogInfo("Play Track: " + _track.trackName);
            AudioManager.Instance.PlayTrack(_track);
        }
    }
}
