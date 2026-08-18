namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class CategoriaRespDto
    {

        public long IdCategoria { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; }

    }
}
