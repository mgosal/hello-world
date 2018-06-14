    #region Retry.do
        public static class Retry
        {
            public static bool Do(Action action)
            {
                return Do(action, null);
            }
            public static bool Do(Action action, Func<bool> condition)
            {
                return Do(action, condition, TimeSpan.FromMilliseconds(100));
            }

            public static bool Do(Action action, Func<bool> condition, TimeSpan retryInterval, int retryCount = 10)
            {
                var exceptions = new List<Exception>();
                bool firstOrException = true;
                for (int retry = 0; retry < retryCount; ++retry)
                {
                    try
                    {
                        if (firstOrException)
                        {
                            if (action != null)
                            {
                                action();
                                if (condition == null)
                                {
                                    return true;
                                }
                            }
                            firstOrException = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                        firstOrException = true;
                        Loggers.EventsLogger.ErrorFormat("Exception caught during action {0}. Retry count is {1}. Exception: {2}.", action != null ? action.Method.Name : "action null", retry, ex.ToString());
                    }
                    try
                    {
                        if (condition != null)
                        {
                            if (condition())
                            {
                                return true;
                            }
                        }
                    }
                    catch (Exception exc)
                    {
                        Loggers.EventsLogger.ErrorFormat("Exception caught during condition {0}. Retry count is {1}. Exception: {2}.", condition != null ? condition.Method.Name : "condition null", retry, exc.ToString());
                    }
                    Thread.Sleep(retryInterval);
                }
                return false;
            }

            public static void DoAsync(Action action, Func<bool> condition, Action callback, object w)
            {
                if (w != null)
                {


                    BackgroundWorker worker = new BackgroundWorker();
                    worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
                    {
                        if (callback != null)
                        {
                            callback();
                        }
                    };
                    worker.DoWork += delegate(object sender, DoWorkEventArgs e)
                    {
                        lock (w)
                        {
                            action();
                        }
                    };
                    worker.RunWorkerAsync();
                }
            }

            public static void DoAsyncWithPreCondition(Func<bool> preCondition, Action action, Action callback)
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
                {
                    if (callback != null)
                    {
                        callback();
                    }
                };
                worker.DoWork += delegate(object sender, DoWorkEventArgs e)
                {
                    if (Do(null, preCondition, TimeSpan.FromSeconds(1), 20))
                    {
                        action();
                    }
                };
                worker.RunWorkerAsync();
            }
        }
        #endregion
