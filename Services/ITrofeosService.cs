using FinalPPE.DTOs;
using FinalPPE.Models;

namespace FinalPPE.Services;

public interface ITrofeosService
{
    Task<List<Trofeo>> GetAllAsync(
        string? q = null,
        int? page = null,
        int pageSize = 4
    );

    Task<Trofeo?> GetByIdAsync(int id);

    Task<Trofeo> CreateAsync(CreateTrofeoDto dto);

    Task<Trofeo?> UpdateAsync(int id, UpdateTrofeoDto dto);

    Task<Trofeo?> DeleteAsync(int id);
}