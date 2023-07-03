using elasticnest.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
namespace elasticnest.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IElasticClient? _elasticCLient;
    private readonly ILogger<ProductsController>? _logger;
    public ProductsController(IElasticClient elasticClient, ILogger<ProductsController> logger)
    {
        _elasticCLient = elasticClient;
        _logger = logger;
    }
    [HttpGet(Name = "GetProducts")]
    public async Task<IActionResult> Get(String keyword)
    {
        var result = await _elasticCLient.SearchAsync<Product>
        (p => p.Query(
            q => q.QueryString(
                d => d.Query('*' + keyword + '*')
            )).Size(1000)
        );
        return Ok(result.Documents.ToList());
    }
    [HttpPost(Name = "AddProducts")]
    public async Task<IActionResult> Post(Product product)
    {
        await _elasticCLient.IndexDocumentAsync(product);
        return Ok(product);
    }
}
