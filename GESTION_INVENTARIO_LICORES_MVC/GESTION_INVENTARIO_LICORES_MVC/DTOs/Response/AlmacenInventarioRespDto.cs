namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class AlmacenInventarioRespDto
    {
        public long IdAlmacen { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
    }
}
