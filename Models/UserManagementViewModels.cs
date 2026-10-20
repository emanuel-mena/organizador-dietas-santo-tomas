using System.ComponentModel.DataAnnotations;

namespace organizador_dietas_santo_tomas.Models;

public class CreateUserViewModel
{
    [Required(ErrorMessage = "Ingrese el nombre.")]
    [StringLength(150)]
    [Display(Name = "Nombre completo")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese el correo.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese una contraseña inicial.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña inicial")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Seleccione un rol.")]
    [Display(Name = "Rol")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "Médicos asociados (opcional)")]
    public List<string> DoctorIds { get; set; } = [];
    public List<UserListItem> AvailableDoctors { get; set; } = [];
}

public class AssociatePatientViewModel
{
    [Required(ErrorMessage = "Ingrese el correo del paciente.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    [Display(Name = "Correo electrónico del paciente")]
    public string Email { get; set; } = string.Empty;
}

public record UserListItem(string Id, string FullName, string Email, string Role, bool CreatedByCurrentDoctor = false);
