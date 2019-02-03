using System.Text;

namespace VotesCounter
{
    public interface IStatisticWriter
    {
        void WriteStatistic(StringBuilder sb);
    }
}