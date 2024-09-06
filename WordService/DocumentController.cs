using Microsoft.AspNetCore.Mvc;

namespace WordService;

[ApiController]
[Route("[controller]")]
public class DocumentController : ControllerBase
{
    private Database database = Database.GetInstance();

    [HttpGet("GetByDocIds")]
    public List<string> GetByDocIds([FromQuery] List<int> docIds)
    {
        return database.GetDocDetails(docIds);
    }

    [HttpGet("GetByWordIds")]
    public Dictionary<int, int> GetByWordIds([FromQuery] List<int> wordIds)
    {
        return database.GetDocuments(wordIds);
    }

    [HttpPost]
    public void Post(int id, string url)
    {
        database.InsertDocument(id, url);
    }
}
