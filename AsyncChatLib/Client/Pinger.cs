using System;
using System.Threading;

namespace AsyncChatLib.Client
{
    class Pinger
    {
        #region Variables

        Random rand = new Random();
        Thread thr;

        bool lastAnswer = true;

        #endregion

        // ======== EVENTS ==========
        public delegate void PingVoid(int number);
        public delegate void EmptyVoid();
        // ==========================
        public event PingVoid PingEvent;
        public event EmptyVoid TimeOutEvent;
        // ==========================

        #region Control

        /// <summary>
        ///  Start the Thread
        /// </summary>
        public void Start()
        {
            Kill();
            thr = new Thread(PingThread);
        }

        /// <summary>
        /// Stop the thread
        /// </summary>
        public void Kill()
        {
            if (thr != null && thr.IsAlive)
                thr.Abort();
            thr = null;
        }

        /// <summary>
        /// This will be called if the server answers the own ping in order to avoid timeouts
        /// </summary>
        /// <param name="answer"></param>
        public void PingAnswer(int answer)
        {
            // maybe do something with the number
            lastAnswer = true;
        }

        #endregion

        #region Timing & Shit

        /// <summary>
        /// Will constantly rise ping-events
        /// </summary>
        /// <param name="obj"></param>
        private void PingThread(object obj)
        {
            try
            {
                while (true)
                {
                    // If the server didnt respond properly in 5 Seconds -> Timeout
                    if (!lastAnswer)
                        TimeOutEvent.Invoke();

                    lastAnswer = false;
                    PingEvent.Invoke(rand.Next(1000000000, 2147483647)); // Invoke Ping-Event with Random Int (fuck it)
                    Thread.Sleep(5000);
                }
            }
            catch (ThreadAbortException) {}
        }

        #endregion
    }
}
