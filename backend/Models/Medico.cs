namespace TurnosMedicos.Models;

public class Medico
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
}
