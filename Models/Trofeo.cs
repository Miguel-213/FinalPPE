namespace FinalPPE.Models;

public class Trofeo
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Juego { get; set; } = string.Empty;

    public string Dificultad { get; set; } = string.Empty;

    public string Requisito { get; set; } = string.Empty;

    public bool Estado { get; set; } = false;

    public string Imagen { get; set; } = string.Empty;
}