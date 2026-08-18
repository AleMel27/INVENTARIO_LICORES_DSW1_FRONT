namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class ProductoResumenRespDto
    {

        public long IdProducto { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;

    }
}
