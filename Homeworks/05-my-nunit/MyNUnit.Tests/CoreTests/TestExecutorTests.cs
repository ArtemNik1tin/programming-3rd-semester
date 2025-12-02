// <copyright file="TestExecutorTests.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

using MyNUnit.DataModels;

namespace MyNUnit.Tests.CoreTests;

using System.Reflection;
using MyNUnit.Core;

#pragma warning disable SA1600
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public class TestExecutorTests
{
    [SetUp]
    public void Setup()
    {
        SimpleTestClass.BeforeClassWasCalled = false;
        SimpleTestClass.AfterClassWasCalled = false;
    }

    [Test]
    public Task ExecuteTestsAsync_WithNullArgument_ThrowsArgumentNullException()
    {
        Assert.ThrowsAsync<ArgumentNullException>(() =>
            TestExecutor.ExecuteTestsAsync(null!));
        return Task.CompletedTask;
    }

    [Test]
    public async Task ExecuteTestsAsync_ShouldRunTestsFromAssembly()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;

        var results = await TestExecutor.ExecuteTestsAsync(new[] { assemblyPath });

        Assert.That(results, Is.Not.Null);
        Assert.That(results, Is.Not.Empty);

        var testClassResults = results.Where(r => r.TestClass == nameof(SimpleTestClass)).ToList();
        Assert.That(testClassResults, Has.Count.EqualTo(4));
    }

    [Test]
    public async Task ExecuteTestsAsync_ShouldCallBeforeClassAndAfterClass()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;

        await TestExecutor.ExecuteTestsAsync([assemblyPath]);

        Assert.Multiple(() =>
        {
            Assert.That(SimpleTestClass.BeforeClassWasCalled, Is.True);
            Assert.That(SimpleTestClass.AfterClassWasCalled, Is.True);
        });
    }

    [Test]
    public async Task ExecuteTestsAsync_ShouldCallBeforeAndAfterForEachTest()
    {
        BeforeAfterTestClass.BeforeClassCount = 0;
        BeforeAfterTestClass.AfterClassCount = 0;
        BeforeAfterTestClass.BeforeCount = 0;
        BeforeAfterTestClass.AfterCount = 0;

        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;

        await TestExecutor.ExecuteTestsAsync([assemblyPath]);

        Assert.Multiple(() =>
        {
            Assert.That(BeforeAfterTestClass.BeforeClassCount, Is.EqualTo(1));
            Assert.That(BeforeAfterTestClass.AfterClassCount, Is.EqualTo(1));

            Assert.That(BeforeAfterTestClass.BeforeCount, Is.EqualTo(2));
            Assert.That(BeforeAfterTestClass.AfterCount, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task ExecuteTestsAsync_ShouldIgnoreTestWithIgnoreReason()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;

        var results = await TestExecutor.ExecuteTestsAsync(new[] { assemblyPath });

        var ignoredTestResult = results.FirstOrDefault(r =>
            r.TestClass == nameof(SimpleTestClass) &&
            r.TestMethod == nameof(SimpleTestClass.IgnoredTest));

        Assert.That(ignoredTestResult, Is.Not.Null);
        Assert.That(ignoredTestResult.Status, Is.EqualTo(TestStatus.Ignored));
        Assert.That(ignoredTestResult.Messages, Contains.Item($"Ignored: The test was ignored for a reason"));
    }

    [Test]
    public async Task ExecuteTestsAsync_ShouldPassTestWithExpectedException()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;

        var results = await TestExecutor.ExecuteTestsAsync(new[] { assemblyPath });

        var testResult = results.FirstOrDefault(r =>
            r.TestClass == nameof(SimpleTestClass) &&
            r.TestMethod == nameof(SimpleTestClass.TestWithExpectedException));

        Assert.That(testResult, Is.Not.Null);
        Assert.That(testResult.Status, Is.EqualTo(TestStatus.Passed));
    }

    [Test]
    public async Task ExecuteTestsAsync_ShouldFailTestWithWrongException()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;

        var results = await TestExecutor.ExecuteTestsAsync(new[] { assemblyPath });

        var testResult = results.FirstOrDefault(r =>
            r.TestClass == nameof(SimpleTestClass) &&
            r.TestMethod == nameof(SimpleTestClass.TestWithWrongException));

        Assert.That(testResult, Is.Not.Null);
        Assert.That(testResult.Status, Is.EqualTo(TestStatus.Failed));
        Assert.That(
            testResult.Messages.Any(m =>
            m.Contains("Expected exception InvalidOperationException") &&
            m.Contains("ArgumentException")), Is.True);
    }

    [Test]
    public async Task ExecuteTestsAsync_WhenBeforeClassThrowsException_ShouldMarkAllTestsAsFailed()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;

        var results = await TestExecutor.ExecuteTestsAsync([assemblyPath]);

        var classResults = results.Where(r => r.TestClass == nameof(BeforeAfterClassExceptionTestClass)).ToList();
        Assert.That(classResults, Has.Count.EqualTo(1));
        var testResult = classResults.First();
        Assert.That(testResult.Status, Is.EqualTo(TestStatus.Failed));
        Assert.That(testResult.Messages, Contains.Item($"BeforeClass failed: BeforeClass exception"));
    }

    [Test]
    public async Task ExecuteTestsAsync_WhenAfterClassThrowsException_ShouldAddWarningMessage()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;

        var results = await TestExecutor.ExecuteTestsAsync(new[] { assemblyPath });

        var classResults = results.Where(r => r.TestClass == nameof(BeforeAfterClassExceptionTestClass)).ToList();
        Assert.That(classResults, Has.Count.EqualTo(1));
        var testResult = classResults.First();
        Assert.That(testResult.Messages, Contains.Item($"Warning: AfterClass failed: AfterClass exception"));
    }

    [Test]
    public async Task ExecuteTestsAsync_WhenBeforeThrowsException_ShouldMarkTestAsFailed()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;

        var results = await TestExecutor.ExecuteTestsAsync([assemblyPath]);

        var classResults = results.Where(r => r.TestClass == nameof(BeforeAfterExceptionTestClass)).ToList();
        Assert.That(classResults, Has.Count.EqualTo(1));
        var testResult = classResults.First();
        Assert.That(testResult.Status, Is.EqualTo(TestStatus.Failed));
        Assert.That(testResult.Messages, Contains.Item($"Before method failed: InvalidOperationException"));
    }

    [Test]
    public async Task ExecuteTestsAsync_WhenAfterThrowsException_ShouldAddWarningMessage()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;

        var results = await TestExecutor.ExecuteTestsAsync([assemblyPath]);

        var classResults = results.Where(r => r.TestClass == nameof(BeforeAfterExceptionTestClass)).ToList();
        Assert.That(classResults, Has.Count.EqualTo(1));
        var testResult = classResults.First();
        Assert.That(testResult.Status, Is.EqualTo(TestStatus.Passed));
        Assert.That(testResult.Messages, Contains.Item($"Warning: After method failed: After exception"));
    }

    [Test]
    public async Task ExecuteTestsAsync_ShouldHandleMultipleAssemblies()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;

        var results = await TestExecutor.ExecuteTestsAsync(new[] { assemblyPath, assemblyPath });

        Assert.That(results, Is.Not.Null);
        Assert.That(results, Is.Not.Empty);
    }

    [Test]
    public async Task ExecuteTestsAsync_ShouldSkipNonExistentOrNonDllFiles()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var assemblyPath = assembly.Location;
        var nonExistentPath = "nonexistent.dll";
        var textFilePath = "test.txt";

        await File.WriteAllTextAsync(textFilePath, "test");

        try
        {
            var results = await TestExecutor.ExecuteTestsAsync(new[]
            {
                assemblyPath,
                nonExistentPath,
                textFilePath
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Not.Empty);
        }
        finally
        {
            if (File.Exists(textFilePath))
            {
                File.Delete(textFilePath);
            }
        }
    }
}