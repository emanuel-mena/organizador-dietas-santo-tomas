using Microsoft.AspNetCore.Identity;

namespace organizador_dietas_santo_tomas.Data;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? CreatedByDoctorId { get; set; }
    public ApplicationUser? CreatedByDoctor { get; set; }
    public ICollection<DoctorPatient> DoctorPatients { get; set; } = new List<DoctorPatient>();
    public ICollection<DoctorPatient> PatientDoctors { get; set; } = new List<DoctorPatient>();
}
