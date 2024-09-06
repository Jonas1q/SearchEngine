using Microsoft.AspNetCore.Mvc;

namespace WordService;

[ApiController]
[Route("[controller]")]
public class DatabaseController : ControllerBase
{
    private Database database = Database.GetInstance();

    [HttpDelete]
    public void Delete()
    {
        database.DeleteDatabase();
    }

    [HttpPost]
    public void Post()
    {
        database.RecreateDatabase();
    }
}
