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
    [HttpPut(Name = "FilterProducts")]
    public async Task<IActionResult> Filter([FromBody] FilterCriteria filterCriteria, [FromQuery] String keyword)
    {
        var result = await _elasticCLient.SearchAsync<Product>(p => p
        .Query(q => q
            .Bool(b => b
                .Must(
                    m => m.Range(r => r.Field(f => f.Price).GreaterThan(filterCriteria.MinPrice)) ||
                    m.Range(r => r.Field(f => f.Price).LessThan(filterCriteria.MaxPrice))
                )
            )
        ).Size(1000));
        return Ok(result.Documents.ToList());
    }

    public class FilterCriteria
    {
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
    }
}
