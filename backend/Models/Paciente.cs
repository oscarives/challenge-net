namespace TurnosMedicos.Models;

public class Paciente
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public DateTime? FechaBloqueo { get; set; }
    public DateTime createdAt { get; set; }
    public bool isActive { get; set; }
}
