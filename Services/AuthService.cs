using FinalPPE.Data;
using FinalPPE.DTOs;
using FinalPPE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FinalPPE.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher<Usuario> _passwordHasher;

    public AuthService(
        AppDbContext context,
        IPasswordHasher<Usuario> passwordHasher
    )
    {
        _context = context;
        _passwordHasher = passwordHasher;
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
}