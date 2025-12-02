// <copyright file="BeforeAfterClassExceptionTestClass.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

// </copyright>
namespace MyNUnit.Tests;

using MyNUnit.UsersAttributes;

#pragma warning disable SA1600
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class BeforeAfterClassExceptionTestClass
{
    [AfterClass]
    public static void AfterClass()
    {
        throw new InvalidOperationException("AfterClass exception");
    }

    [BeforeClass]
    public static void BeforeClass()
    {
        throw new InvalidOperationException("BeforeClass exception");
    }

    [Test(null, null)]
    public static void Test1() { }
}

