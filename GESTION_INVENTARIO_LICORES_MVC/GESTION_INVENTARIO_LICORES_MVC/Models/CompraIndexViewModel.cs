using GESTION_INVENTARIO_LICORES_MVC.DTOs.Request;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Response;

namespace GESTION_INVENTARIO_LICORES_MVC.Models
{
    public class CompraIndexViewModel
    {
        public PagedResultRespDto<CompraRespDto> Resultado { get; set; } = new();
        public List<TipoComprobanteRespDto> TiposComprobante { get; set; } = [];
        public List<AlmacenInventarioRespDto> Almacenes { get; set; } = [];
        public CompraFiltroReqDto Filtro { get; set; } = new();
    }
}