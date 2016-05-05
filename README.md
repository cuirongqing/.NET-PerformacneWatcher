Defines a PerformaceWatcher to watch every execution in a single thread.<br/>
It can dump execution text and its execution time. <br/>
It can work in MVC, EF, sql, WCF inspector and everywhere you watch to watch. <br/>

Example:

            var watcher = PerformanceWatcher.StartNewWatcher();

            #region Executions in a single thread.

            PerformanceWatcher.Watch("Test1", () =>
            {
                Thread.Sleep(1234);
            });

            PerformanceWatcher.Watch("Test1", () =>
             {
                 Thread.Sleep(5678);

                 return 1;
             });

            #endregion

            var watchedResult = watcher.StopWatcher();
            Console.WriteLine(watchedResult);

            Console.ReadLine();

            // Expected Result:
            // ThreadId: 10, ElapsedTIme: 6912
            //Test1 1234
            //Test1 5678
