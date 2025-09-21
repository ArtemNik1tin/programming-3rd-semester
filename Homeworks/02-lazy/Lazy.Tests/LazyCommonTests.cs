// <copyright file="LazyCommonTests.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace Lazy.Tests;

#pragma warning disable CS1591
#pragma warning disable SA1600
public class LazyCommonTests
{
    [Test]
    public void Get_Should_ReturnCorrectValue_ForInt()
    {
        foreach (var lazy in GetLazyImplementations(() => 42))
        {
            var result = lazy.Get();
            Assert.That(result, Is.EqualTo(42));
        }
    }

    [Test]
    public void Get_Should_ReturnCorrectValue_ForString()
    {
        foreach (var lazy in GetLazyImplementations(() => "test"))
        {
            var result = lazy.Get();
            Assert.That(result, Is.EqualTo("test"));
        }
    }

    [Test]
    public void Get_Should_ReturnCorrectValue_ForObject()
    {
        var expected = new object();
        foreach (var lazy in GetLazyImplementations(() => expected))
        {
            var result = lazy.Get();
            Assert.That(result, Is.SameAs(expected));
        }
    }

    [Test]
    public void Get_Should_HandleNullResult()
    {
        foreach (var lazy in GetLazyImplementations<string>(() => null!))
        {
            var result = lazy.Get();
            Assert.That(result, Is.Null);
        }
    }

    [Test]
    public void Get_Should_ReturnSameValue_OnSubsequentCalls()
    {
        foreach (var lazy in GetLazyImplementations(() => 42))
        {
            var result1 = lazy.Get();
            var result2 = lazy.Get();
            var result3 = lazy.Get();

            Assert.Multiple(() =>
            {
                Assert.That(result1, Is.EqualTo(42));
                Assert.That(result2, Is.EqualTo(42));
                Assert.That(result3, Is.EqualTo(42));
            });
        }
    }

    private static IEnumerable<ILazy<T>> GetLazyImplementations<T>(Func<T> supplier)
    {
        return
        [
            new LazySingleThreaded<T>(supplier),
            new LazyMultiThreaded<T>(supplier),
        ];
    }
}