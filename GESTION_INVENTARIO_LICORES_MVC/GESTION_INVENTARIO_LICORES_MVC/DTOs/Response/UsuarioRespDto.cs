namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class UsuarioRespDto
    {
        public long IdUsuario { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public RolRespDto Rol { get; set; } = new();
        public bool Estado { get; set; }
    }
}
