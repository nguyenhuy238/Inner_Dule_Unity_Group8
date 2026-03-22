using UnityEngine;

namespace InnerDuel.Core
{
    [CreateAssetMenu(fileName = "NewMapData", menuName = "InnerDuel/Game/MapData")]
    public class MapData : ScriptableObject
    {
        public string mapName;
        public Sprite previewImage;
        public GameObject mapPrefab;
        [Header("Audio")]
        public AudioClip mapBgmClip;
        [Range(0f, 2f)] public float mapBgmVolume = 1f;
        [TextArea(3, 10)]
        public string description;
    }
}
