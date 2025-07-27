using System.Collections.Generic;

namespace MiniMatch.Core.Services
{
    public interface IMetricsCollector
    {
        void StartLevel(int levelIndex);
        void RecordCardFlip(float responseTime);
        void RecordMatch(int cardId1, int cardId2);
        void RecordMismatch(int cardId1, int cardId2);
        void CompleteLevel(float totalTime);
        
        LevelMetrics GetCurrentLevelMetrics();
        List<LevelMetrics> GetAllMetrics();
        void Reset();
    }
}