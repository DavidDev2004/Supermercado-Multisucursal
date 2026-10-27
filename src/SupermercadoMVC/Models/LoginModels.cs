using System.ComponentModel.DataAnnotations;

namespace SupermercadoMVC.Models;

// Adaptado de UsuarioAuth y LoginViewModel de la integrante.
public sealed class UsuarioAuth
{
    public int CodigoUsuario { get; init; }
    public string Usuario { get; init; } = "";
    public string PasswordHash { get; init; } = "";
}

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Escribe tu usuario.")]
    [StringLength(50, ErrorMessage = "El usuario admite hasta 50 caracteres.")]
    [Display(Name = "Usuario")]
    public string Usuario { get; set; } = "";

    [Required(ErrorMessage = "Escribe tu contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";
}
