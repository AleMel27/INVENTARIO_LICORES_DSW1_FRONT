namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class ProveedorRespDto
    {

        public long IdProveedor { get; set; }
        public string Ruc { get; set; } = string.Empty;
        public string RazonSocial { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public string? Direccion { get; set; }
        public bool Estado { get; set; }

    }
}
