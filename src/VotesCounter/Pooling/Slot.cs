namespace VotesCounter
{
    public class Slot<T>
    {
        public Slot(T item)
        {
            Item = item;
        }

        public T Item { get; private set; }

        public bool IsInUse { get; set; }
    }
}