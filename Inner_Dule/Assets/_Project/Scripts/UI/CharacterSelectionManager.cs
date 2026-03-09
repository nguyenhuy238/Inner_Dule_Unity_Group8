using UnityEngine;
using UnityEngine.Events;
using InnerDuel.Characters;
using InnerDuel.Core;

namespace InnerDuel.UI
{
    /// <summary>
    /// Quản lý logic chọn nhân vật. 
    /// Team có thể kết nối script này với các Button trong Menu Chọn Tướng.
    /// </summary>
    public class CharacterSelectionManager : MonoBehaviour
    {
        [Header("Selection State")]
        public CharacterType player1SelectedType = CharacterType.Discipline;
        public CharacterType player2SelectedType = CharacterType.Spontaneity;
        
        [Header("Events")]
        public UnityEvent<CharacterData> onPlayer1SelectionChanged;
        public UnityEvent<CharacterData> onPlayer2SelectionChanged;

        /// <summary>
        /// Gọi hàm này từ UI Button khi P1 chọn tướng.
        /// </summary>
        public void SelectCharacterP1(int typeIndex)
        {
            player1SelectedType = (CharacterType)typeIndex;
            CharacterData data = CharacterFactory.Instance.CreateCharacter(player1SelectedType, Vector3.zero, 1).GetComponent<InnerCharacterController>().characterData;
            onPlayer1SelectionChanged?.Invoke(data);
            
            Debug.Log($"[Select] Player 1 selected: {player1SelectedType}");
        }

        /// <summary>
        /// Gọi hàm này từ UI Button khi P2 chọn tướng.
        /// </summary>
        public void SelectCharacterP2(int typeIndex)
        {
            player2SelectedType = (CharacterType)typeIndex;
            CharacterData data = CharacterFactory.Instance.CreateCharacter(player2SelectedType, Vector3.zero, 2).GetComponent<InnerCharacterController>().characterData;
            onPlayer2SelectionChanged?.Invoke(data);
            
            Debug.Log($"[Select] Player 2 selected: {player2SelectedType}");
        }

        /// <summary>
        /// Chốt lựa chọn và vào Game.
        /// </summary>
        public void ConfirmSelection()
        {
            // Lưu lựa chọn vào một biến tĩnh (Static) hoặc PlayerPrefs để GameManager đọc
            SelectionData.P1_Type = player1SelectedType;
            SelectionData.P2_Type = player2SelectedType;
            
            // Chuyển sang scene Gameplay
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainGameScene");
        }
    }

    /// <summary>
    /// Class tĩnh để truyền dữ liệu giữa các Scene.
    /// </summary>
    public static class SelectionData
    {
        public static CharacterType P1_Type = CharacterType.Discipline;
        public static CharacterType P2_Type = CharacterType.Spontaneity;
    }
}
