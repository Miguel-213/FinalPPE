using FinalPPE.Data;
using FinalPPE.DTOs;
using FinalPPE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FinalPPE.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    private readonly IPasswordHasher<Usuario> _passwordHasher;

    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext context,
        IPasswordHasher<Usuario> passwordHasher,
        IConfiguration configuration
    )
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<Usuario?> RegisterAsync(RegisterDto dto)
    {
        var usuarioExistente = await _context.Usuarios
            .AnyAsync(u => u.Email == dto.Email);

        if (usuarioExistente)
        {
            return null;
        }

        var usuario = new Usuario
        {
            Email = dto.Email
        };

        usuario.Password = _passwordHasher.HashPassword(
            usuario,
            dto.Password
        );

        _context.Usuarios.Add(usuario);

        await _context.SaveChangesAsync();

        return usuario;
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(
                u => u.Email == dto.Email
            );

        if (usuario is null)
        {
            return null;
        }

        var resultado = _passwordHasher.VerifyHashedPassword(
            usuario,
            usuario.Password,
            dto.Password
        );

        if (resultado == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return GenerarToken(usuario);
    }

    private string GenerarToken(Usuario usuario)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "No se encontró la clave JWT."
            );

        var issuer = _configuration["Jwt:Issuer"];

        var audience = _configuration["Jwt:Audience"];

        var claims = new[]
        {
            new Claim(
                JwtRegisteredClaimNames.Sub,
                usuario.Id.ToString()
            ),

            new Claim(
                JwtRegisteredClaimNames.Email,
                usuario.Email
            )
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)
        );

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}