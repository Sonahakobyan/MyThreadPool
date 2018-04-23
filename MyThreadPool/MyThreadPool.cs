using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyThreadPool
{
    public sealed class MyThreadPool
    {
        public static readonly int MaxCount = 5;
        
        private static List<Thread> threads = new List<Thread>();
        private static ConcurrentQueue<WaitCallback> tasks = new ConcurrentQueue<WaitCallback>();

        private static bool condition = false;

        /// <summary>
        /// Init the list with threads
        /// </summary>
        static MyThreadPool()
        {
            for (int i = 0; i < MaxCount; i++)
            {
                threads.Add(new Thread(ThreadFunk));
            }
        }


        /// <summary>
        /// Queues a method for execution.Notify the threads that there is a new task
        /// </summary>
        /// <param name="callBack"> The method to execute </param>
        public static void QueueUserWorkItem(WaitCallback callBack)
        {
            if (callBack == null)
            {
                throw new ArgumentNullException("CallBack is null!");
            }

            Monitor.Enter(tasks);

            tasks.Enqueue(callBack);
            condition = true;
            Monitor.PulseAll(tasks);

            Monitor.Exit(tasks);
        }

        
        /// <summary>
        /// Thread Function that wait until there is a task, then get the task and do it
        /// </summary>
        private static void ThreadFunk()
        {
            while (true)
            {
                bool empty = false;
                WaitCallback task = null;
                
                // Find if the queue is empty
                lock (tasks)
                {
                    empty = tasks.Count == 0;
                }

                // If there is a task to do, get it
                if (!empty)
                {
                    Monitor.Enter(tasks);

                    task.Invoke(null);

                    while (!condition)
                    {
                        Monitor.Wait(tasks);
                    }

                    Monitor.Exit(tasks);
                }
            }
        }
    }
}