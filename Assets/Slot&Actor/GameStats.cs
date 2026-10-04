using System.Collections.Generic;

namespace CardGame
{
    /// <summary>
    /// Stores historical facts about the current game that are not represented by
    /// the current board state.
    /// </summary>
    public sealed class GameStats
    {
        private readonly Dictionary<(GameManager.P owner, int cardId), int> summonCounts = new();

        public int GetSummonCount(GameManager.P owner, int cardId) =>
            summonCounts.TryGetValue((owner, cardId), out int count) ? count : 0;

        public void RecordSummon(GameManager.P owner, int cardId)
        {
            var key = (owner, cardId);
            summonCounts[key] = GetSummonCount(owner, cardId) + 1;
        }
    }
}
