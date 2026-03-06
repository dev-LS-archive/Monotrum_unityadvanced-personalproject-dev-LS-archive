using UnityEngine;

namespace Audio
{
    [CreateAssetMenu(fileName = "TrackData", menuName = "Monotrum/TrackData")]
    public class TrackData : ScriptableObject
    {
        [Header("Basic Info")]
        public string trackName;
        public string trackArtist;
        public AudioClip clip;
        
        [Header("Rhythm Data")]
        [Tooltip("이 곡의 BPM (비트 파악 및 맵 생성 속도에 활용)")]
        public float bpm = 120f;
        
        [Header("Visual Theme")]
        [Tooltip("이 곡이 재생될 때 맵(큐브)이나 배경의 메인 컬러")]
        [ColorUsage(false,true)]
        public Color themeColor = Color.cyan;
        
        [Tooltip("이 곡에서의 큐브 생성/이동 기본 속도")]
        public float trackScrollSpeed = 30f;
    }
}
