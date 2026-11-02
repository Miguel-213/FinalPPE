using FinalPPE.DTOs;
using FinalPPE.Models;

namespace FinalPPE.Services;

public interface IAuthService
{
    Task<Usuario?> RegisterAsync(RegisterDto dto);
}