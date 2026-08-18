namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class MarcaRespDto
    {

        public long IdMarca { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? PaisOrigen { get; set; }
        public bool Estado { get; set; }

    }
}
