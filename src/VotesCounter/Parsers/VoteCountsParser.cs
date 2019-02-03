using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace VotesCounter
{
    public class VoteCountsParser
    {
        public List<VoteCount> Parse(string context)
        {
            if (string.IsNullOrEmpty(context))
            {
                return null;
            }

            var voteCountLogs = JsonConvert.DeserializeObject<VoteCount>(context);

            return new List<VoteCount> { voteCountLogs };
        }
    }
}