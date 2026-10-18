using Api.Extensions;
using Application.Interfaces.Services;
using Application.Requests.Trade;
using Application.Responses;
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
        var trades = await tradeService.GetAllAsync(User.GetUserId());
        return Ok(trades);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var trade = await tradeService.GetByIdAsync(User.GetUserId(), id);

        if (trade is null)
            return NotFound(new ErrorResponse(404, "Trade not found"));

        return Ok(trade);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTradeRequest request)
    {
        var trade = await tradeService.AddAsync(User.GetUserId(), request);

        return CreatedAtAction(nameof(GetById), new { id = trade.Id }, trade);
    }

    [HttpPost("import")]
    public async Task<IActionResult> Import(ImportTradesRequest request)
    {
        var imported = await tradeService.ImportAsync(User.GetUserId(), request);

        return Ok(new { imported });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateTradeRequest request)
    {
        await tradeService.UpdateAsync(User.GetUserId(), id, request);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await tradeService.DeleteAsync(User.GetUserId(), id);

        return NoContent();
    }
}
