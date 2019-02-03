using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace VotesCounter
{
    public class VotesCountsBag : IVoteCountsBag
    {
        private List<VoteCount> bag;
        private readonly VoteCountsParser parser;
        private readonly PoolSettings settings;
        private long total;
        private long flushedCount;
        private long failedToFlushCount;
       
        public long Size
        {
            get
            {
                lock (this)
                {
                    return bag.Count;
                }
            }
        }

        public long TotalCount
        {
            get { return total; }
        }

        public long FlushedCount
        {
            get { return flushedCount; }
        }

        public long FailedToFlushCount
        {
            get { return failedToFlushCount; }
        }
        
        public VotesCountsBag(PoolSettings settings)
        {
            this.settings = settings;

            bag = new List<VoteCount>();
            parser = new VoteCountsParser();
        }

        public void PutVoteCounts(string context)
        {
            lock (this)
            {
                var voteCount = parser.Parse(context);
                if (voteCount != null && voteCount.Count > 0)
                {
                    var beforeAddRangeCount = bag.Count;
                    bag.AddRange(voteCount);
                    total += bag.Count - beforeAddRangeCount;
                }
            }
        }

        public void FlushToDatabase()
        {
            lock (this)
            {
                if (bag.Count == 0)
                {
                    return;
                }
                
                List<VoteCount> marker;

                if (bag.Count > settings.MaxFlushSize)
                {
                    marker = bag.GetRange(0, settings.MaxFlushSize);
                }
                else
                {
                    marker = bag.GetRange(0, bag.Count);
                }

                int tryCount = 0;
                do
                {
                    try
                    {
                        var logs = new List<VoteCount>();
                        marker.ForEach(logs.Add);

                        VoteCount.SaveCollection(logs);
                        flushedCount += logs.Count;
                        logs.ForEach(f => bag.Remove(f));

                        return;
                    }
                    catch (Exception ex)
                    {
                        tryCount++;
                        Trace.WriteLine("Failed to save collection. " + ex.Message + ". Retry " + tryCount);
                        Thread.Sleep(TimeSpan.FromSeconds(2 + (2*tryCount)));
                    }
                } 
                while (tryCount < 5);

                if (tryCount >= 5)
                {
                    failedToFlushCount += marker.Count;
                }
            }
        }

        public void WriteStatistic(StringBuilder sb)
        {
            lock (this)
            {
                sb.AppendFormat("Bag size: {0}. Total count: {1}. Flushed: {2}. Failed: {3}. ", Size, TotalCount, FlushedCount, FailedToFlushCount);
                sb.AppendLine();
            }
        }

        public void Dispose()
        {
            bag = null;            
        }
    }
}