using GESTION_INVENTARIO_LICORES_MVC.DTOs.Request;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Response;

namespace GESTION_INVENTARIO_LICORES_MVC.Models
{
    public class InventarioIndexViewModel
    {
        public InventarioFiltroReqDto Filtro { get; set; } = new();
        public PagedResultRespDto<InventarioRespDto> Resultado { get; set; } = new();
        public List<AlmacenInventarioRespDto> Almacenes { get; set; } = [];
        public List<ProductoResumenRespDto> Productos { get; set; } = [];
        public List<TipoMovimientoRespDto> TiposMovimiento { get; set; } = [];
    }
}