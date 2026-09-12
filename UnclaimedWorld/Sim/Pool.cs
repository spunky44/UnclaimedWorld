using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.SimSide
{
    public class Pool<T> where T : new()
    {
        int stepSize;

        Queue<T> queue = new Queue<T>();

      

        public Pool(int stepSize)
        {
            this.stepSize = stepSize;
        }

        public Pool(int stepSize, int amountToStartWith)
        {
            this.stepSize = stepSize;

            for (int i = 0; i < amountToStartWith; i++)
            {
                queue.Enqueue(new T());
            }
        }

        public T Get() 
        {
            if (queue.Count == 0)
            {   // add some fresh goals, we've run out:
                for (int i = 0; i < stepSize; i++)
                {
                    queue.Enqueue(new T());
                }
            }

            return queue.Dequeue();
        }

        public void Clear()
        {
            queue.Clear();
        }

        public void Retire(T obj)
        {
            queue.Enqueue(obj);
        }
    }
}
