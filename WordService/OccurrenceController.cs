using Microsoft.AspNetCore.Mvc;

namespace WordService;

[ApiController]
[Route("[controller]")]
public class OccurrenceController : ControllerBase
{
    private Database database = Database.GetInstance();

    [HttpPost]
    public void Post(int docId, [FromBody] ISet<int> wordIds)
    {
        database.InsertAllOcc(docId, wordIds);
    }
}
