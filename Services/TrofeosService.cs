using FinalPPE.Data;
using FinalPPE.DTOs;
using FinalPPE.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalPPE.Services;

public class TrofeosService : ITrofeosService
{
    private readonly AppDbContext _context;

    public TrofeosService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Trofeo>> GetAllAsync(
        string? q = null,
        int? page = null,
        int pageSize = 4
    )
    {
        var consulta = _context.Trofeos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            consulta = consulta.Where(t =>
                t.Nombre.ToLower().Contains(q.ToLower())
            );
        }

        consulta = consulta.OrderBy(t => t.Id);

        if (page.HasValue && page.Value > 0)
        {
            var pagina = page.Value;

            consulta = consulta
                .Skip((pagina - 1) * pageSize)
                .Take(pageSize);
        }

        return await consulta.ToListAsync();
    }

    public async Task<Trofeo?> GetByIdAsync(int id)
    {
        return await _context.Trofeos
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Trofeo> CreateAsync(CreateTrofeoDto dto)
    {
        var trofeo = new Trofeo
        {
            Nombre = dto.Nombre,
            Juego = dto.Juego,
            Dificultad = dto.Dificultad,
            Requisito = dto.Requisito,
            Estado = dto.Estado,
            Imagen = dto.Imagen
        };

        _context.Trofeos.Add(trofeo);

        await _context.SaveChangesAsync();

        return trofeo;
    }

    public async Task<Trofeo?> UpdateAsync(
        int id,
        UpdateTrofeoDto dto
    )
    {
        var trofeo = await _context.Trofeos.FindAsync(id);

        if (trofeo is null)
        {
            return null;
        }

        if (dto.Nombre is not null)
            trofeo.Nombre = dto.Nombre;

        if (dto.Juego is not null)
            trofeo.Juego = dto.Juego;

        if (dto.Dificultad is not null)
            trofeo.Dificultad = dto.Dificultad;

        if (dto.Requisito is not null)
            trofeo.Requisito = dto.Requisito;

        if (dto.Estado.HasValue)
            trofeo.Estado = dto.Estado.Value;

        if (dto.Imagen is not null)
            trofeo.Imagen = dto.Imagen;

        await _context.SaveChangesAsync();

        return trofeo;
    }

    public async Task<Trofeo?> DeleteAsync(int id)
    {
        var trofeo = await _context.Trofeos.FindAsync(id);

        if (trofeo is null)
        {
            return null;
        }

        _context.Trofeos.Remove(trofeo);

        await _context.SaveChangesAsync();

        return trofeo;
    }
}