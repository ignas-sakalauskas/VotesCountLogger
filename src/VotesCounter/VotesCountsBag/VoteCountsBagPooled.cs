using System;
using System.Text;

namespace VotesCounter
{
    public class VoteCountsBagPooled : IVoteCountsBag
    {
        private readonly VotesCountsBag internalStore;
        private readonly Pool<IVoteCountsBag> pool;

        public VoteCountsBagPooled(Pool<IVoteCountsBag> pool, PoolSettings settings)
        {
            if (pool == null)
            {
                throw new ArgumentNullException("pool");
            }

            this.pool = pool;
            internalStore = new VotesCountsBag(settings);
        }

        public long Size
        {
            get { return internalStore.Size; }
        }

        public long TotalCount
        {
            get { return internalStore.TotalCount; }
        }

        public long FlushedCount
        {
            get { return internalStore.FlushedCount; }
        }

        public long FailedToFlushCount
        {
            get { return internalStore.FailedToFlushCount; }
        }

        public void FlushToDatabase()
        {
            internalStore.FlushToDatabase();
        }

        public void PutVoteCounts(string context)
        {
            internalStore.PutVoteCounts(context);
        }

        public void WriteStatistic(StringBuilder sb)
        {
            internalStore.WriteStatistic(sb);
        }

        public void Dispose()
        {
            if (pool.IsDisposed)
            {
                internalStore.Dispose();
            }
            else
            {
                pool.Release(this);
            }
        }
    }
}