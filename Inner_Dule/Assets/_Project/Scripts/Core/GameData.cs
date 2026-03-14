using UnityEngine;
using InnerDuel.Characters;

namespace InnerDuel.Core
{
    /// <summary>
    /// Static class to transfer data between scenes.
    /// Stores selected characters and match results.
    /// </summary>
    public static class GameData
    {
        // Selected characters
        public static CharacterData player1Character;
        public static CharacterData player2Character;

        // Match results
        public static int winnerPlayerID = 0; // 1 for P1, 2 for P2
        public static string winnerName = "";

        // Scene names (to avoid magic strings)
        public const string MainMenuScene = "MainMenuScene";
        public const string CharacterSelectScene = "CharacterSelectScene";
        public const string LoadingScene = "LoadingScene";
        public const string MainGameScene = "MainGameScene";
        public const string ResultScene = "ResultScene";

        public static void ResetData()
        {
            player1Character = null;
            player2Character = null;
            winnerPlayerID = 0;
            winnerName = "";
        }
    }
}
