using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace PerformcanceWatcherFramework
{
    /// <summary>
    /// Starts a performance watcher to watch every execution in every thread.
    /// If threshold is exceeded, it can dump every method and its execution time.
    /// </summary>
    public class PerformanceWatcher
    {
        /// <summary>
        /// Saves all the execution and its execution time.
        /// </summary>
        [ThreadStatic]
        private static WatchedResult watchedResult;

        [ThreadStatic]
        private static DateTime? startTime = null;

        private PerformanceWatcher() { }

        /// <summary>
        /// Initials a new performcane watcher.
        /// </summary>
        /// <returns></returns>
        public static PerformanceWatcher StartNewWatcher()
        {
            startTime = DateTime.Now;
            watchedResult = new WatchedResult();

            return new PerformanceWatcher();
        }

        /// <summary>
        /// Stops to watch performance and gets watched result.
        /// </summary>
        /// <returns>Watched result.</returns>
        public WatchedResult StopWatcher()
        {
            var results = new WatchedResult();
            if (startTime.HasValue && watchedResult != null)
            {
                results.ElapsedTime = (DateTime.Now - startTime.Value).TotalMilliseconds;
                results.Results.AddRange(watchedResult.Results);
            }

            startTime = null;
            watchedResult = null;

            return results;
        }

        /// <summary>
        /// Watches a single method and dumps the method name with its execution time.
        /// </summary>
        /// <param name="watchingCall">Execution text such as RPC name, stored proc name and sql script.</param>
        /// <param name="action"></param>
        public static void Watch(string watchingCall, Action action)
        {
            if (string.IsNullOrEmpty(watchingCall) || action == null || !startTime.HasValue)
            {
                action();

                return;
            }

            var stopWatcher = new Stopwatch();
            try
            {
                stopWatcher.Start();

                action();
            }
            finally
            {
                stopWatcher.Stop();

                if (watchedResult == null)
                {
                    watchedResult = new WatchedResult();
                }

                // Dumps the method name with its execution time.
                watchedResult.Results.Add(new WatchedResult.WatchingResult(watchingCall, stopWatcher.ElapsedMilliseconds));
            }
        }

        /// <summary>
        /// Watches a single method and dumps the method name with its execution time.
        /// </summary>
        /// <param name="watchingCall">Execution text such as RPC name, stored proc name and sql script.</param>
        public static TResult Watch<TResult>(string watchingCall, Func<TResult> func)
        {
            if (string.IsNullOrEmpty(watchingCall) || func == null || !startTime.HasValue)
            {
                return func();
            }

            var stopWatcher = new Stopwatch();
            try
            {
                stopWatcher.Start();

                return func();
            }
            finally
            {
                stopWatcher.Stop();

                if (watchedResult == null)
                {
                    watchedResult = new WatchedResult();
                }

                watchedResult.Results.Add(new WatchedResult.WatchingResult(watchingCall, stopWatcher.ElapsedMilliseconds));
            }
        }

        /// <summary>
        /// Directlly dumps the method name and its execution time.
        /// </summary>
        public static void Watch(string watchingCall, long elapsedTime)
        {
            if (startTime.HasValue && !string.IsNullOrEmpty(watchingCall))
            {
                watchedResult.Results.Add(new WatchedResult.WatchingResult(watchingCall, elapsedTime));
            }
        }
    }

    /// <summary>
    /// Dumps all the execution and its execution time.
    /// </summary>
    public class WatchedResult
    {
        public List<WatchingResult> Results { get; private set; }
        public double ElapsedTime { get; set; }

        public WatchedResult()
        {
            this.Results = new List<WatchingResult>();
        }

        public override string ToString()
        {
            var watcherResultBuilder = new StringBuilder();
            watcherResultBuilder.AppendFormat("ThreadId:{0}, ElapsedTIme:{1}", Thread.CurrentThread.ManagedThreadId, this.ElapsedTime);

            this.Results.ForEach(_ =>
            {
                watcherResultBuilder.AppendLine().Append(_.ToString());
            });


            return watcherResultBuilder.ToString();
        }

        /// <summary>
        /// Defines execution method and its execution time in a single thread.
        /// </summary>
        public class WatchingResult
        {
            #region Ctor

            public string WatchingCall { get; private set; }
            public long ElapsedTime { get; private set; }

            public WatchingResult(string watchingCall, long elapsedTime)
            {
                this.WatchingCall = watchingCall;
                this.ElapsedTime = elapsedTime;
            }

            #endregion

            public override string ToString()
            {
                return string.Format("{0} {1}", this.WatchingCall, this.ElapsedTime);
            }
        }
    }
}
