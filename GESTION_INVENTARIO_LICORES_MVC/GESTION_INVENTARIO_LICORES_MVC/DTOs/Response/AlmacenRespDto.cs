namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class AlmacenRespDto
    {
        public long IdAlmacen { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }

    }
}
