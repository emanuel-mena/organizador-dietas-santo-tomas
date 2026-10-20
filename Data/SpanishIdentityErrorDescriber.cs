using Microsoft.AspNetCore.Identity;

namespace organizador_dietas_santo_tomas.Data;

public class SpanishIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DuplicateEmail(string email) => Error("CorreoDuplicado", "Ya existe una cuenta con ese correo.");
    public override IdentityError DuplicateUserName(string userName) => Error("UsuarioDuplicado", "Ya existe una cuenta con ese correo.");
    public override IdentityError InvalidEmail(string? email) => Error("CorreoInvalido", "Ingrese un correo válido.");
    public override IdentityError InvalidUserName(string? userName) => Error("UsuarioInvalido", "El correo no es válido.");
    public override IdentityError PasswordTooShort(int length) => Error("ClaveCorta", $"La contraseña debe tener al menos {length} caracteres.");
    public override IdentityError PasswordRequiresDigit() => Error("ClaveNumero", "La contraseña debe incluir un número.");
    public override IdentityError PasswordRequiresLower() => Error("ClaveMinuscula", "La contraseña debe incluir una letra minúscula.");
    public override IdentityError PasswordRequiresUpper() => Error("ClaveMayuscula", "La contraseña debe incluir una letra mayúscula.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Error("ClaveSimbolo", "La contraseña debe incluir un símbolo.");

    private static IdentityError Error(string code, string description) => new() { Code = code, Description = description };
}
