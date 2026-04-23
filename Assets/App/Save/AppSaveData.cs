using System;

namespace Game.App.Save
{
    [Serializable]
    public sealed class AppSaveData
    {
        public int BestScore;
        public RunSaveData ActiveRun;
        public DailyChallengesSaveData DailyChallenges;

        public bool HasActiveRun => ActiveRun != null;
    }
}
