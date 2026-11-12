using FinalPPE.DTOs;
using FinalPPE.Models;
using FinalPPE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalPPE.Controllers;

[ApiController]
[Route("trofeos")]
public class TrofeosController : ControllerBase
{
    private readonly ITrofeosService _trofeosService;

    public TrofeosController(
        ITrofeosService trofeosService
    )
    {
        _trofeosService = trofeosService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Trofeo>), StatusCodes.Status200OK)]
    public async Task<ActionResult> GetAll(
        [FromQuery] string? q = null,
        [FromQuery] int? page = null,
        [FromQuery] int pageSize = 4
    )
    {
        if (pageSize < 1)
        {
            pageSize = 4;
        }

        var trofeos = await _trofeosService.GetAllAsync(
            q,
            page,
            pageSize
        );

        return Ok(trofeos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Trofeo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetById(int id)
    {
        var trofeo =
            await _trofeosService.GetByIdAsync(id);

        if (trofeo is null)
        {
            return NotFound();
        }

        return Ok(trofeo);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(Trofeo), StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(
        CreateTrofeoDto dto
    )
    {
        var trofeo =
            await _trofeosService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = trofeo.Id },
            trofeo
        );
    }

    [Authorize]
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(Trofeo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(
        int id,
        UpdateTrofeoDto dto
    )
    {
        var trofeo =
            await _trofeosService.UpdateAsync(id, dto);

        if (trofeo is null)
        {
            return NotFound();
        }

        return Ok(trofeo);
    }

    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Trofeo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var trofeo =
            await _trofeosService.DeleteAsync(id);

        if (trofeo is null)
        {
            return NotFound();
        }

        return Ok(trofeo);
    }
}