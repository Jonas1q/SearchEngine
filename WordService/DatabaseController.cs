using Microsoft.AspNetCore.Mvc;
using Polly;
using Polly.Retry;
using System;
using System.Threading.Tasks;

namespace WordService;

[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly Database _database;
    private readonly IAsyncPolicy _retryPolicy;

    public DatabaseController()
    {
        _database = Database.GetInstance();


        _retryPolicy = Policy.Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3, 
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    Console.WriteLine($"Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
                });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAsync()
    {
        try
        {
            await _retryPolicy.ExecuteAsync(() => _database.DeleteDatabaseAsync());
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync()
    {
        try
        {
            await _retryPolicy.ExecuteAsync(() => _database.RecreateDatabaseAsync());
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
}
