using Microsoft.AspNetCore.Mvc;
using PortalFinancieroAPI.Models;
using PortalFinancieroAPI.Services;

namespace PortalFinancieroAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : ControllerBase
    {
        private readonly PagosService _service;
        private readonly ILogger<PagosController> _logger;

        public PagosController(PagosService service, ILogger<PagosController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarPago([FromBody] PagoRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "Datos inválidos" });

                var resultado = await _service.RegistrarPagoAsync(request);

                return Ok(new ApiResponse<PagoResponse>
                {
                    Success = true,
                    Data = resultado,
                    Message = "Pago registrado"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Error al registrar" });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> ObtenerPagos(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "userId inválido" });

                var pagos = await _service.ObtenerPagosUsuarioAsync(userId);

                return Ok(new ApiResponse<List<PagoResponse>>
                {
                    Success = true,
                    Data = pagos,
                    Message = "Pagos obtenidos"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Error al obtener" });
            }
        }

        [HttpGet("pendientes/{userId}")]
        public async Task<IActionResult> ObtenerPendientes(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "userId inválido" });

                var pendientes = await _service.ObtenerPagosPendientesAsync(userId);

                return Ok(new ApiResponse<List<PagoResponse>>
                {
                    Success = true,
                    Data = pendientes,
                    Message = "Pagos pendientes obtenidos"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Error al obtener" });
            }
        }

        [HttpPatch("marcar-pagado/{pagoId}")]
        public async Task<IActionResult> MarcarPagado(Guid pagoId)
        {
            try
            {
                if (pagoId == Guid.Empty)
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "pagoId inválido" });

                var resultado = await _service.MarcarComoPagadoAsync(pagoId);

                return Ok(new ApiResponse<PagoResponse>
                {
                    Success = true,
                    Data = resultado,
                    Message = "Marcado como pagado"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Error al actualizar" });
            }
        }

        [HttpDelete("{pagoId}")]
        public async Task<IActionResult> EliminarPago(Guid pagoId)
        {
            try
            {
                if (pagoId == Guid.Empty)
                    return BadRequest(new ApiResponse<object> { Success = false, Message = "pagoId inválido" });

                var eliminado = await _service.EliminarPagoAsync(pagoId);

                if (!eliminado)
                    return NotFound(new ApiResponse<object> { Success = false, Message = "No encontrado o ya pagado" });

                return Ok(new ApiResponse<object> { Success = true, Message = "Eliminado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Error al eliminar" });
            }
        }
    }
}
