using System.Collections.Generic;
using System.Text;

namespace VotesCounter
{
    public class StackStore<T> : Stack<T>, IItemStore<T> where T : IStatisticWriter
    {
        public StackStore(int capacity)
            : base(capacity)
        {
        }

        public T Fetch()
        {
            return Pop();
        }

        public void Store(T item)
        {
            Push(item);
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