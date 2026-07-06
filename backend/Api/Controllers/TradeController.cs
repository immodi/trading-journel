using Application.Interfaces.Services;
using Application.Requests.Trade;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TradeController(ITradeService tradeService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var trades = await tradeService.GetAllAsync();
        return Ok(trades);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var trade = await tradeService.GetByIdAsync(id);

        if (trade is null)
            return NotFound();

        return Ok(trade);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTradeRequest request)
    {
        await tradeService.AddAsync(request);

        return Created();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTradeRequest request)
    {
        await tradeService.UpdateAsync(id, request);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await tradeService.DeleteAsync(id);

        return NoContent();
    }
}