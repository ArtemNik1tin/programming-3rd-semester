// <copyright file="MyThreadPoolTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MyThreadPool.Tests;

using System.Collections.Concurrent;

#pragma warning disable SA1600
#pragma warning disable CS1591 // Missing XML comment for public visible type or member.
public class MyThreadPoolTests
{
    [Test]
    public void Constructor_WithZeroThreads_Should_ThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new MyThreadPool(0));
    }

    [Test]
    public void Submit_Should_ExecuteTaskAndReturnResult()
    {
        using var pool = new MyThreadPool(2);

        var task = pool.Submit(() => 2 + 3);

        Assert.Multiple(() =>
        {
            Assert.That(task.IsCompleted, Is.True);
            Assert.That(task.Result, Is.EqualTo(5));
        });

        Assert.That(task.IsCompleted, Is.True);
    }

    [Test]
    public void Submit_WithException_Should_ThrowAggregateExceptionOnResultAccess()
    {
        using var pool = new MyThreadPool(2);

        var task = pool.Submit<int>(() => throw new InvalidOperationException("Test exception"));

        Assert.Throws<AggregateException>(() => _ = task.Result);
        Assert.That(task.IsCompleted, Is.True);
    }

    [Test]
    public void Submit_AfterShutdown_Should_ThrowInvalidOperationException()
    {
        var pool = new MyThreadPool(2);
        pool.Shutdown();

        Assert.Throws<InvalidOperationException>(() => pool.Submit(() => 42));

        pool.Dispose();
    }

    [Test]
    public void ContinueWith_Should_CreateContinuationThatExecutesAfterOriginalTask()
    {
        using var pool = new MyThreadPool(2);
        var task1 = pool.Submit(() => 5);
        _ = task1.Result;

        var task2 = task1.ContinueWith(x => x + 1);

        Assert.That(task2.Result, Is.EqualTo(6));
    }

    [Test]
    public void ContinueWith_MultipleContinuations_Should_AllExecute()
    {
        using var pool = new MyThreadPool(3);
        var task1 = pool.Submit(() => 10);

        var task2 = task1.ContinueWith(x => x + 1);
        var task3 = task1.ContinueWith(x => x * 2);
        var task4 = task1.ContinueWith(x => x.ToString());

        Assert.Multiple(() =>
        {
            Assert.That(task2.Result, Is.EqualTo(11));
            Assert.That(task3.Result, Is.EqualTo(20));
            Assert.That(task4.Result, Is.EqualTo("10"));
        });
    }

    [Test]
    public void ContinueWith_WithNullFunction_Should_ThrowArgumentNullException()
    {
        using var pool = new MyThreadPool(2);
        var task = pool.Submit(() => 42);

        Assert.Throws<ArgumentNullException>(() => task.ContinueWith<int>(null!));
    }

    [Test]
    public void Dispose_Should_CallShutdown()
    {
        var pool = new MyThreadPool(2);

        pool.Dispose();

        Assert.Throws<InvalidOperationException>(() => pool.Submit(() => 42));
    }
}
