using UnityEngine;
using Core;
using Logger = Core.Logger;

namespace Audio
{
    public class TrackController : MonoBehaviour
    {
        public void SetTrackAndPlay(TrackData track)
        {
            AudioManager.Instance.SelectTrack(track);
            
            Logger.Instance.LogInfo("SetTrackAndPlay");
        }

        public void PauseCall()
        {
            AudioManager.Instance.PauseMusic();
        }

        public void ResumeCall()
        {
            AudioManager.Instance.ResumeMusic();
        }

        public void StopCall()
        {
            AudioManager.Instance.StopMusic();
        }
        
        public void PlaySelectedTrack()
        {
            var track = AudioManager.Instance.SelectedTrack;
            if (track != null)
            {
                AudioManager.Instance.PlayTrack(track);
            }
        }
        
        public void SetInputActive(bool active)
        {
            InputManager.Instance.SetInputActive(active);
        }
    }
}
