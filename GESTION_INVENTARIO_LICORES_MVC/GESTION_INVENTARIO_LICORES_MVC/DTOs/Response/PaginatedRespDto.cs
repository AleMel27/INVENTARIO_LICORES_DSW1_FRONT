namespace GESTION_INVENTARIO_LICORES_MVC.DTOs.Response
{
    public class PaginatedRespDto<T>
    {
        public List<T> Items { get; set; } = [];
        public int PageNumber { get; set; }
        public int PageSize => 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
