using System;
using System.Collections.Generic;
using System.Text;

namespace VotesCounter
{
    public class CircularStore<T> : IItemStore<T> where T : IStatisticWriter
    {
        private readonly List<Slot<T>> slots;
        private int freeSlotCount;
        private int position = -1;

        public CircularStore(int capacity)
        {
            slots = new List<Slot<T>>(capacity);
        }

        public T Fetch()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The buffer is empty.");
            }

            int startPosition = position;
            
            do
            {
                Advance();
                Slot<T> slot = slots[position];
                if (!slot.IsInUse)
                {
                    slot.IsInUse = true;
                    --freeSlotCount;
                    return slot.Item;
                }
            } 
            while (startPosition != position);

            throw new InvalidOperationException("No free slots.");
        }

        public void Store(T item)
        {
            Slot<T> slot = slots.Find(s => Equals(s.Item, item));
            if (slot == null)
            {
                slot = new Slot<T>(item);
                slots.Add(slot);
            }
            slot.IsInUse = false;
            ++freeSlotCount;
        }

        public int Count
        {
            get { return freeSlotCount; }
        }

        public void WriteStatistic(StringBuilder sb)
        {
            foreach (var slot in slots)
            {
               slot.Item.WriteStatistic(sb); 
            }            
        }

        private void Advance()
        {
            position = (position + 1) % slots.Count;
        }
    }
}