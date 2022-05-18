using IPA.Utilities.Async;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnhancedStreamChat.Utilities
{
    public class MainThreadInvoker
    {
        private static CancellationTokenSource s_cancellationToken = new CancellationTokenSource();

        public static void ClearQueue()
        {
            s_cancellationToken.Cancel();
            s_cancellationToken = new CancellationTokenSource();
        }

        public static Task Invoke(Action? action)
        {
            if (action != null) {
                return UnityMainThreadTaskScheduler.Factory.StartNew(action, s_cancellationToken.Token);
            }
            return Task.CompletedTask;
        }

        public static Task Invoke<TA>(Action<TA?>? action, TA? a)
            where TA : class
        {
            if (action != null) {
                return UnityMainThreadTaskScheduler.Factory.StartNew(() => action(a), s_cancellationToken.Token);
            }
            return Task.CompletedTask;
        }

        public static Task Invoke<TA, TB>(Action<TA?, TB?>? action, TA? a, TB? b)
            where TA : class
            where TB : class
        {
            if (action != null) {
                return UnityMainThreadTaskScheduler.Factory.StartNew(() => action(a, b), s_cancellationToken.Token);
            }
            return Task.CompletedTask;
        }
    }
}