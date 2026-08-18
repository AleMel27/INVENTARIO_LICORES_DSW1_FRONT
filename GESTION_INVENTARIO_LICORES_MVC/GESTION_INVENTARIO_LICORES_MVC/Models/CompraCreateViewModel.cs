using GESTION_INVENTARIO_LICORES_MVC.DTOs.Request;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Response;

namespace GESTION_INVENTARIO_LICORES_MVC.Models
{
    public class CompraCreateViewModel
    {
        public CompraReqDto Compra { get; set; } = new();
        public List<ProveedorResumenRespDto> Proveedores { get; set; } = [];
        public List<TipoComprobanteRespDto> TiposComprobante { get; set; } = [];
        public List<AlmacenInventarioRespDto> Almacenes { get; set; } = [];
        public List<ProductoResumenRespDto> Productos { get; set; } = [];
    }
}