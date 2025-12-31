// <copyright file="TestsController.cs" company="ArtemNikit1n">
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// </copyright>

namespace MyNUnitWeb;

using Microsoft.AspNetCore.Mvc;
using MyNUnitWeb.ModelsWeb;

[ApiController]
[Route("api/[controller]")]
public class TestsController : ControllerBase
{
    private readonly TestService testService;
    private readonly ILogger<TestsController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestsController"/> class.
    /// </summary>
    /// <param name="testService"></param>
    /// <param name="logger"></param>
    public TestsController(TestService testService, ILogger<TestsController> logger)
    {
        this.testService = testService;
        this.logger = logger;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="request"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost("upload")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> UploadAssembly(IFormFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        if (file.Length == 0)
        {
            return this.BadRequest(new { error = "No file or empty file" });
        }

        try
        {
            var filePath = await this.testService.UploadAssembly(file);
            return this.Ok(new { filePath });
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error uploading assembly");
            return this.BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="assemblyPaths"></param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost("run")]
    public async Task<IActionResult> RunTests([FromBody] List<string> assemblyPaths)
    {
        try
        {
            var testRun = await this.testService.RunTests(assemblyPaths);
            return this.Ok(testRun);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error running tests");
            return this.BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("history")]
    public IActionResult GetHistory()
    {
        try
        {
            var history = this.testService.GetTestRunHistory();
            return this.Ok(history);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting history");
            return this.BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("history/{id}")]
    public IActionResult GetTestRun(Guid id)
    {
        try
        {
            var testRun = this.testService.GetTestRun(id);
            if (testRun == null)
            {
                return this.NotFound();
            }

            return this.Ok(testRun);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting test run");
            return this.BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("history")]
    public IActionResult ClearHistory()
    {
        try
        {
            this.testService.ClearHistory();
            return this.Ok();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error clearing history");
            return this.BadRequest(new { error = ex.Message });
        }
    }
}