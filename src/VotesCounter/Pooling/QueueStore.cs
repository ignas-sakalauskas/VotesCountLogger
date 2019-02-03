using System.Collections.Generic;
using System.Text;

namespace VotesCounter
{
    public class QueueStore<T> : Queue<T>, IItemStore<T> where T : IStatisticWriter
    {
        public QueueStore(int capacity)
            : base(capacity)
        {
        }

        public T Fetch()
        {
            return Dequeue();
        }

        public void Store(T item)
        {
            Enqueue(item);
        }

        public void WriteStatistic(StringBuilder sb)
        {
            foreach (var slot in ToArray())
            {
                slot.WriteStatistic(sb);
            }
        }
    }
}