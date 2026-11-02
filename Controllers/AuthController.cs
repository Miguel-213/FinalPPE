using FinalPPE.DTOs;
using FinalPPE.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinalPPE.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(
        RegisterDto dto
    )
    {
        var usuario =
            await _authService.RegisterAsync(dto);

        if (usuario is null)
        {
            return Conflict(new
            {
                message = "El email ya está registrado"
            });
        }

        return StatusCode(
            StatusCodes.Status201Created,
            new
            {
                message = "Usuario registrado",
                id = usuario.Id,
                email = usuario.Email
            }
        );
    }
}