// <copyright file="SimpleTestClass.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MyNUnit.Tests;

using MyNUnit.UsersAttributes;

#pragma warning disable SA1600
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class SimpleTestClass
{
    public static bool BeforeClassWasCalled { get; set; }

    public static bool AfterClassWasCalled { get; set; }

    private int beforeWasCalledCount = 0;
    private int afterWasCalledCount = 0;

    [BeforeClass]
    public static void MyBeforeClass()
    {
        BeforeClassWasCalled = true;
    }

    [AfterClass]
    public static void MyAfterClass()
    {
        AfterClassWasCalled = true;
    }

    [Before]
    public void MyBefore()
    {
        this.beforeWasCalledCount++;
    }

    [After]
    public void MyAfter()
    {
        this.afterWasCalledCount++;
    }

    [UsersAttributes.Test(null, null)]
    public void PassingTest()
    {
    }

    [UsersAttributes.Test("The test was ignored for a reason", null)]
    public void IgnoredTest()
    {
    }

    [UsersAttributes.Test(null, typeof(InvalidOperationException))]
    public void TestWithExpectedException()
    {
        throw new InvalidOperationException("Expected exception");
    }

    [UsersAttributes.Test(null, typeof(InvalidOperationException))]
    public void TestWithWrongException()
    {
        throw new ArgumentException("Wrong exception");
    }
}