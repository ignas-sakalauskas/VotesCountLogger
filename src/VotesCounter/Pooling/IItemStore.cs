namespace VotesCounter
{
    public interface IItemStore<T> : IStatisticWriter where T : IStatisticWriter
    {
        T Fetch();

        void Store(T item);

        int Count { get; }
    }
}