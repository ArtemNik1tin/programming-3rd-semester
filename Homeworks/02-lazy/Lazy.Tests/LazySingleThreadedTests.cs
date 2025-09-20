// <copyright file="LazySingleThreadedTests.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace Lazy.Tests;

#pragma warning disable CS1591
#pragma warning disable SA1600
public class LazySingleThreadedTests
{
    [Test]
    public void Constructor_WithNullSupplier_ThrowsArgumentNullException()
    {
        Func<string> nullSupplier = null!;

        Assert.Throws<ArgumentNullException>(() => new LazySingleThreaded<string>(nullSupplier));
    }

    [Test]
    public void Get_OnFirstCall_ShouldReturnsCorrectValue()
    {
        var expected = "test value";
        var lazy = new LazySingleThreaded<string>(() => expected);

        var result = lazy.Get();

        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void Get_OnSubsequentCalls_ShouldReturnsSameValue()
    {
        var callCount = 0;
        var lazy = new LazySingleThreaded<int>(() =>
        {
            callCount++;
            return 42;
        });

        var result1 = lazy.Get();
        var result2 = lazy.Get();
        var result3 = lazy.Get();

        Assert.That(result1, Is.EqualTo(42));
        Assert.That(result2, Is.EqualTo(42));
        Assert.That(result3, Is.EqualTo(42));
        Assert.That(callCount, Is.EqualTo(1), "Supplier should be called only once");
    }

    [Test]
    public void Get_WhenSupplierReturnsNull_ShouldReturnsNull()
    {
        var lazy = new LazySingleThreaded<string?>(() => null);

        var result1 = lazy.Get();
        var result2 = lazy.Get();

        Assert.That(result1, Is.Null);
        Assert.That(result2, Is.Null);
    }

    [Test]
    public void Get_WithValueTypes_ShouldWorksCorrectly()
    {
        var lazy = new LazySingleThreaded<int>(() => 123);

        var result = lazy.Get();

        Assert.That(result, Is.EqualTo(123));
    }

    [Test]
    public void Get_WithReferenceTypes_ShouldReturnsSameInstance()
    {
        var expectedObject = new object();
        var lazy = new LazySingleThreaded<object>(() => expectedObject);

        var result = lazy.Get();

        Assert.That(result, Is.SameAs(expectedObject));
    }
}
