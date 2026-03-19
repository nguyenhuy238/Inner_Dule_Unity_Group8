using UnityEngine;

namespace InnerDuel.Core
{
    [CreateAssetMenu(fileName = "NewMapData", menuName = "InnerDuel/Game/MapData")]
    public class MapData : ScriptableObject
    {
        public string mapName;
        public Sprite previewImage;
        public GameObject mapPrefab;
        [TextArea(3, 10)]
        public string description;
    }
}
