namespace organizador_dietas_santo_tomas.Data;

public static class AppRoles
{
    public const string Administrator = "Administrador";
    public const string Doctor = "Medico";
    public const string Patient = "Paciente";
    public const string All = Administrator + "," + Doctor + "," + Patient;
}
