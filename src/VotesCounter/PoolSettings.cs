using System;

namespace VotesCounter
{
    public class PoolSettings
    {
        public int VoteCountsBagPoolSize => 50;

        public int ReadyToFlushBagSize => 50;

        public int MaxFlushSize => 150;

        public int FlushTasksCount => 12;

        public TimeSpan SleepBetweenFlushes => TimeSpan.FromSeconds(2);

        public int BackgroundFlushTasksCount => 2;

        public TimeSpan SleepBetweenBackgroundFlushes => TimeSpan.FromSeconds(4);

        public bool LoggingDisabled => false;

        public AccessMode PoolAccessMode => AccessMode.FIFO;

        public LoadingMode PoolLoadingMode => LoadingMode.Lazy;
    }
}