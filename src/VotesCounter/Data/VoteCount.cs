using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;

namespace VotesCounter
{
    public sealed class VoteCount
    {
        public Guid Id { get; }
        public string Data { get; }

        public VoteCount(Guid id, string data)
        {
            Id = id;
            Data = data;
        }

        public static void SaveCollection(List<VoteCount> voteCounts)
        {
            var sb = new StringBuilder();

            try
            {
                foreach (var voteCount in voteCounts)
                {
                    sb.AppendLine(JsonConvert.SerializeObject(voteCount));
                }

                File.AppendAllText("database_file.txt", sb.ToString());
            }
            catch (Exception ex)
            {
                Console.Write("Error saving vote counts: " + ex.Message);
            }
        }
    }
}