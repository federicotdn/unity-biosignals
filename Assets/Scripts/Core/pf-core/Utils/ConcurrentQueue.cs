using System;
using System.Collections.Generic;
using System.Threading;

namespace pfcore {
    public class ConcurrentQueue<T> {
        private Queue<T> queue;

        public int Count {
            get {
                Monitor.Enter(queue);
                int count = queue.Count;
                Monitor.Exit(queue);
                return count;
            }
        }

        public ConcurrentQueue() {
            queue = new Queue<T>();
        }

        public void Enqueue(T elem) {
            Monitor.Enter(queue);
            queue.Enqueue(elem);
            Monitor.Exit(queue);
        }

        public bool TryDequeue(out T elem) {
            bool locked = Monitor.TryEnter(queue);
            if (!locked || queue.Count == 0) {

                if (locked) {
                    Monitor.Exit(queue);
                }

                elem = default(T);
                return false;
            }

            elem = queue.Dequeue();

            Monitor.Exit(queue);
            return true;
        }
    }
}
