using System.Text;
using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using OpenAI_API.Completions;


namespace my_server.Controllers;

[ApiController]
[Route("[controller]")]
public class OpenAIController : ControllerBase
{

    private readonly ILogger<OpenAIController> _logger;

    public OpenAIController(ILogger<OpenAIController> logger)
    {
        _logger = logger;
    }

    [HttpPost("open_ai")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> GetResult([FromBody] string promptText)
    {
        // Replace 'YOUR_API_KEY' with your actual API key
        string apiKey = "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";
        string endpoint = "https://api.openai.com/v1/engines/davinci/completions";


        HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        string jsonData = $"{{\"prompt\": \"{promptText}\", \"max_tokens\": 50}}";

        StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(endpoint, content);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Generated Text:");
            Console.WriteLine(responseBody);
            return Ok(responseBody);

        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            return BadRequest(response.Content);
        }
    }
}
// string apiKey = "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";
// string answer = string.Empty;
// var openAI = new OpenAIAPI(apiKey);
// CompletionRequest completion = new();
// completion.Prompt = prompt;
// completion.Model = OpenAI_API.Models.Model.CurieText;
// completion.MaxTokens = 200;

// var result = await openAI.Completions.CreateCompletionAsync(completion);

// if (result != null)
// {

//     return Ok(result);

// }
// else
// {

//     return BadRequest("Couldn't create completion");
// }


