using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading;
using System.Web.Mvc;

using PerformcanceWatcherFramework;

namespace Example.cs
{
    class Program
    {
        static void Main(string[] args)
        {
            var watcher = PerformanceWatcher.StartNewWatcher();

            #region Executions in the single thread.

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
        }
    }

    #region MVC & Watches every action execution

    /// <summary>
    /// Watches every action execution.
    /// </summary>
    public class PerformanceWatcherActionFilterAttribute : ActionFilterAttribute, IActionFilter
    {
        PerformanceWatcher watcher = null;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            watcher = PerformanceWatcher.StartNewWatcher();

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (watcher != null)
            {
                var watchedResult = watcher.StopWatcher();
                if (watchedResult.ElapsedTime >= 3000)
                {
                    var controller = filterContext.RouteData.Values["controller"];
                    var action = filterContext.RouteData.Values["action"];
                    // logger.Log(Level.Warning, string.Format("Action: {0}/{1}, Results: {2}", controller, action, watchedResult.ToString()));
                }
            }

            base.OnActionExecuted(filterContext);
        }
    }

    #endregion

    #region SQL & SqlHelper

    //private static SqlDataReader ExecuteReader(SqlConnection connection, SqlTransaction transaction, CommandType commandType, string commandText, SqlParameter[] commandParameters, SqlConnectionOwnership connectionOwnership)
    //{
    //    //create a command and prepare it for execution
    //    SqlCommand cmd = new SqlCommand();
    //    PrepareCommand(cmd, connection, transaction, commandType, commandText, commandParameters);

    //    //create a reader
    //    SqlDataReader dr;

    //    // call ExecuteReader with the appropriate CommandBehavior
    //    if (connectionOwnership == SqlConnectionOwnership.External)
    //    {
    //        dr = PerformanceWatcher.Watch(commandText, () => cmd.ExecuteReader());
    //    }
    //    else
    //    {
    //        dr = PerformanceWatcher.Watch(commandText, () => cmd.ExecuteReader(CommandBehavior.CloseConnection));
    //    }

    //    // detach the SqlParameters from the command object, so they can be used again.
    //    cmd.Parameters.Clear();

    //    return dr;
    //}

    #endregion

    #region WCF Inspector

    public class WatchServerThresholdMessageInspector : IDispatchMessageInspector
    {
        #region IDispatchMessageInspector

        private PerformanceWatcher watcher = null;

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return watcher = PerformanceWatcher.StartNewWatcher();
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var watchedResult = watcher.StopWatcher();
            if (watchedResult.ElapsedTime >= 1000)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    //logger.Log(Level.Warning, string.Format("Action: {0}, Request: {1}, Response: {2}, TotalMilliseconds: {3}",
                    //                               messageWatcher.Action, messageWatcher.Message,
                    //                               responseMessage.ToString(), elapsedTime));
                }, reply.ToString());

                using (var messageBuffer = reply.CreateBufferedCopy(int.MaxValue))
                {
                    reply = messageBuffer.CreateMessage();
                }
            }
        }

        #endregion
    }

    #endregion
}
