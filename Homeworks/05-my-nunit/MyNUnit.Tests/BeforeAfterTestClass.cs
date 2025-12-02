// <copyright file="BeforeAfterTestClass.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MyNUnit.Tests;

using MyNUnit.UsersAttributes;

#pragma warning disable SA1600
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public static class BeforeAfterTestClass
{
    public static int BeforeClassCount { get; set; }

    public static int AfterClassCount { get; set; }

    public static int BeforeCount { get; set; }

    public static int AfterCount { get; set; }

    [BeforeClass]
    public static void BeforeClass()
    {
        BeforeClassCount++;
    }

    [AfterClass]
    public static void AfterClass()
    {
        AfterClassCount++;
    }

    [Before]
    public static void Before()
    {
        BeforeCount++;
    }

    [After]
    public static void After()
    {
        AfterCount++;
    }

    [Test(null, null)]
    public static void Test1()
    {
    }

    [Test(null, null)]
    public static void Test2()
    {
    }
}
