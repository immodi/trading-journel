using Api.Extensions;
using Application.Interfaces.Services;
using Application.Requests.Market;
using Application.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MarketController(IMarketService marketService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var markets = await marketService.GetAllAsync(User.GetUserId());
        return Ok(markets);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var market = await marketService.GetByIdAsync(User.GetUserId(), id);

        if (market is null)
            return NotFound(new ErrorResponse(404, "Market not found"));

        return Ok(market);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMarketRequest request)
    {
        var market = await marketService.AddAsync(User.GetUserId(), request);

        return CreatedAtAction(nameof(GetById), new { id = market.Id }, market);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateMarketRequest request)
    {
        await marketService.UpdateAsync(User.GetUserId(), id, request);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await marketService.DeleteAsync(User.GetUserId(), id);

        return NoContent();
    }
}
