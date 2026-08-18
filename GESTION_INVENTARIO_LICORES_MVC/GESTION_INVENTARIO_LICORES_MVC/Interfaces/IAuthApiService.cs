using GESTION_INVENTARIO_LICORES_MVC.DTOs.Request;
using GESTION_INVENTARIO_LICORES_MVC.DTOs.Response;

namespace GESTION_INVENTARIO_LICORES_MVC.Services
{
    public interface IAuthApiService
    {
        Task<LoginRespDto?> LoginAsync(LoginReqDto request);
    }
}