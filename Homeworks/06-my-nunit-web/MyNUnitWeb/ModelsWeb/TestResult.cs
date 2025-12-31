// <copyright file="TestResult.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MyNUnitWeb.ModelsWeb;

using MyNUnit.DataModels;

public class TestResult
{
    public string TestClass { get; set; } = string.Empty;

    public string TestMethod { get; set; } = string.Empty;

    public TestStatus? Status { get; set; }

    public double DurationMs { get; set; }

    public List<string> Messages { get; set; } = [];

    public string? ExceptionType { get; set; }

    public string? ExceptionMessage { get; set; }

    public string? StackTrace { get; set; }
}