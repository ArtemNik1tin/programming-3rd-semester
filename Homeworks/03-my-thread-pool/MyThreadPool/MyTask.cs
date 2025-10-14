// <copyright file="MyTask.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MyThreadPool;

/// <inheritdoc/>
/// <summary>
/// Initializes a new instance of the <see cref="MyTask{TResult}"/> class.
/// </summary>
/// <param name="function">The function to execute asynchronously.</param>
/// <param name="threadPool">The thread pool that will execute this task.</param>
internal class MyTask<TResult>(Func<TResult> function, MyThreadPool threadPool) : IMyTask<TResult>
{
    private readonly Func<TResult> function = function ?? throw new ArgumentNullException(nameof(function));
    private readonly MyThreadPool threadPool = threadPool ?? throw new ArgumentNullException(nameof(threadPool));
    private readonly ManualResetEvent completionEvent = new(false);
    private TResult? result = default;
    private Exception? exception = null;
    private volatile bool isCompleted = false;

    /// <inheritdoc/>
    public bool IsCompleted
        => this.isCompleted;

    /// <inheritdoc/>
    public TResult Result
    {
        get
        {
            this.completionEvent.WaitOne();

            if (this.exception != null)
            {
                throw new AggregateException(this.exception);
            }

            if (this.result == null)
            {
                throw new InvalidOperationException("Task completed without result");
            }

            return this.result;
        }
    }

    /// <summary>
    /// Executes the task function and handles completion logic including continuations.
    /// </summary>
    public void Execute()
    {
        try
        {
            this.result = this.function();
        }
        catch (Exception ex)
        {
            this.exception = ex;
        }
        finally
        {
            this.isCompleted = true;
            this.completionEvent.Set();
        }
    }

    /// <inheritdoc/>
    public IMyTask<TNewResult> ContinueWith<TNewResult>(Func<TResult, TNewResult> function)
    {
        ArgumentNullException.ThrowIfNull(function);

        TNewResult ContinuationFunction()
        {
            TResult sourceResult = this.Result;
            return function(sourceResult);
        }

        return this.threadPool.Submit(ContinuationFunction);
    }
}