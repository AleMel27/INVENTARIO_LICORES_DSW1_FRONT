namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class ResponseDto<T>
    {

        public string Message { get; set; } = string.Empty;

        public bool Success { get; set; }

        public T Data { get; set; } = default!;

    }
}
