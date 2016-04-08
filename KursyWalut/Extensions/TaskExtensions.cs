using System;
using System.Threading;
using System.Threading.Tasks;

namespace KursyWalut.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        ///     "Cancel non cancelable async operations."<br />
        ///     Credits: Stephen Toub - MSFT @ blogs.msdn.com<br />
        ///     URL: https://social.msdn.microsoft.com/profile/stephen%20toub%20-%20msft/ <br />
        ///     URL: http://blogs.msdn.com/b/pfxteam/archive/2012/10/05/how-do-i-cancel-non-cancelable-async-operations.aspx <br />
        /// </summary>
        public static async Task WithCancellation(
            this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(
                s => ((TaskCompletionSource<bool>) s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                    throw new OperationCanceledException(cancellationToken);
            }
            await task;
        }
    }
}