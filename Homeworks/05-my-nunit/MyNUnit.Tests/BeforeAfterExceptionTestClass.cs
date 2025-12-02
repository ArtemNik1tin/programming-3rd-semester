// <copyright file="BeforeAfterExceptionTestClass.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MyNUnit.Tests;

using MyNUnit.UsersAttributes;

#pragma warning disable SA1600
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class BeforeAfterExceptionTestClass
{
    [Before]
    public static void Before()
    {
        throw new InvalidOperationException("Before exception");
    }

    [After]
    public static void After()
    {
        throw new InvalidOperationException("After exception");
    }

    [Test(null, null)]
    public static void Test1()
    {
    }
}