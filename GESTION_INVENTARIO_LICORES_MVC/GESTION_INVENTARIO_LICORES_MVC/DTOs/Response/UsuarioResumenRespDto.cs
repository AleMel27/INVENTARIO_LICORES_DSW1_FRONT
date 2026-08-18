namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class UsuarioResumenRespDto
    {
        public long IdUsuario { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
    }
}
