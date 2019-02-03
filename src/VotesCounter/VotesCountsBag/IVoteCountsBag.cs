using System;

namespace VotesCounter
{
    public interface IVoteCountsBag : IDisposable, IStatisticWriter
    {
        long Size { get; }

        long TotalCount { get; }

        long FlushedCount { get; }

        long FailedToFlushCount { get; }
     
        void PutVoteCounts(string context);

        void FlushToDatabase();
    }
}