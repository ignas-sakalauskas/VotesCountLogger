using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VotesCounter
{
    // TODO this example is based on a legacy project, and requires proper modernization

    public class Program
    {
        private static VoteCountsLogger VoteCountsLogger = new VoteCountsLogger(new PoolSettings());

        public static void Main(string[] args)
        {
            Console.WriteLine("Vote Counts Logger started");

            var run = 0;
            while (run < 5000)
            {
                for (int i = 0; i < 100; i++)
                {
                    Task.Run(() => VoteCountsLogger.ProcessVoteCount(JsonConvert.SerializeObject(new VoteCount(Guid.NewGuid(), "pw"))));
                }
                ShowStats();
                run++;
            }

            ShowStats();

            VoteCountsLogger.Dispose();
            Console.WriteLine("Exiting...");
        }

        private static void ShowStats()
        {
            var sb = new StringBuilder();
            VoteCountsLogger.WriteStatistic(sb);
            Console.WriteLine(sb.ToString());
        }
    }
}
