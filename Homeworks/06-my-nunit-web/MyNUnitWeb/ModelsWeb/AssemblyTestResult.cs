// <copyright file="AssemblyTestResult.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MyNUnitWeb.ModelsWeb;

using MyNUnit.DataModels;

/// <summary>
/// A model for the test results of one specific DLL assembly.
/// </summary>
public class AssemblyTestResult
{
    /// <summary>
    /// Gets or sets file name (e.g. "MyTests.dll").
    /// </summary>
    public string AssemblyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets full path to the file on the server.
    /// </summary>
    public string AssemblyPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets results of all tests in this build.
    /// </summary>
    public List<ModelsWeb.TestResult> TestResults { get; set; } = [];

    public int TotalTests => this.TestResults.Count;

    public int Passed => this.TestResults.Count(r => r.Status == TestStatus.Passed);

    public int Failed => this.TestResults.Count(r => r.Status == TestStatus.Failed);

    public int Ignored => this.TestResults.Count(r => r.Status == TestStatus.Ignored);
}