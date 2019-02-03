using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace VotesCounter
{
    public sealed class VoteCountsLogger : IDisposable, IStatisticWriter
    {
        private readonly PoolSettings _settings;
        private readonly Pool<IVoteCountsBag> voteCountsPool;
        private IList<Worker> workers;

        public VoteCountsLogger(PoolSettings settings)
        {
            _settings = settings;

            voteCountsPool = new Pool<IVoteCountsBag>(_settings.VoteCountsBagPoolSize, f => new VoteCountsBagPooled(f, _settings), settings.PoolLoadingMode, settings.PoolAccessMode);

            StartFlushingTasks();
        }

        public void ProcessVoteCount(string context)
        {
            if (!_settings.LoggingDisabled)
            {
                using (var voteCountsBag = voteCountsPool.Acquire())
                {
                    voteCountsBag.PutVoteCounts(context);
                }
            }
        }

        public void Dispose()
        {
            StopFlushingTasks();

            // TODO save in DB remaining vote counts
            
            if (voteCountsPool != null && !voteCountsPool.IsDisposed)
            {
                voteCountsPool.Dispose();
            }
        }

        public void WriteStatistic(StringBuilder sb)
        {
            sb.AppendLine("Workers count: " + workers.Count);
            sb.AppendLine("[Pool statistics]");
            voteCountsPool.WriteStatistic(sb);
        }

        private void StartFlushingTasks()
        {
            workers = new List<Worker>();
            for (int i = 0; i < _settings.FlushTasksCount; i++)
            {
                var worker = new Worker(i + 1, _settings.SleepBetweenFlushes,
                                        () =>
                                            {
                                                using (var log = voteCountsPool.Acquire())
                                                {
                                                    if (log.Size >= _settings.ReadyToFlushBagSize)
                                                    {
                                                        log.FlushToDatabase();
                                                    }
                                                }
                                            });
                workers.Add(worker);
                worker.Start();
                Thread.Sleep(50);
            }

            for (int i = 0; i < _settings.BackgroundFlushTasksCount; i++)
            {
                var worker = new Worker(_settings.FlushTasksCount + i + 1, _settings.SleepBetweenBackgroundFlushes,
                                        () =>
                                            {
                                                int tryCount = 0;
                                                do
                                                {
                                                    using (var log = voteCountsPool.Acquire())
                                                    {
                                                        if (log.Size > 0)
                                                        {
                                                            log.FlushToDatabase();
                                                            break;
                                                        }
                                                    }
                                                }
                                                while (tryCount++ < _settings.VoteCountsBagPoolSize);
                                            });
                workers.Add(worker);
                worker.Start();
                Thread.Sleep(100);
            }
        }

        private void StopFlushingTasks()
        {
            foreach (var worker in workers)
            {
                worker.Stop();
            }
        }
    }
}