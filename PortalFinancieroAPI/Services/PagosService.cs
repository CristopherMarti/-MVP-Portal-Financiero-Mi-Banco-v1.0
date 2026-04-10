using PortalFinancieroAPI.Models;
using PortalFinancieroAPI.Repositories;

namespace PortalFinancieroAPI.Services
{
    public class PagosService
    {
        private readonly PagosRepository _repository;
        private readonly ILogger<PagosService> _logger;

        public PagosService(PagosRepository repository, ILogger<PagosService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<PagoResponse> RegistrarPagoAsync(PagoRequest datos)
        {
            _logger.LogInformation($"Service: Registrando pago de {datos.TipoServicio}");

            if (datos.Monto <= 0)
                throw new ArgumentException("Monto debe ser mayor a 0");

            if (string.IsNullOrWhiteSpace(datos.Referencia))
                throw new ArgumentException("Referencia requerida");

            var pagoData = new Dictionary<string, object>
            {
                { "user_id", datos.UserId },
                { "tipo_servicio", datos.TipoServicio.ToLower() },
                { "monto", datos.Monto },
                { "referencia", datos.Referencia },
                { "estado", "pendiente" },
                { "observaciones", datos.Observaciones ?? "" }
            };

            if (datos.FechaVencimiento.HasValue)
                pagoData["fecha_vencimiento"] = datos.FechaVencimiento.Value;

            return await _repository.InsertarPagoAsync(pagoData);
        }

        public async Task<List<PagoResponse>> ObtenerPagosUsuarioAsync(Guid userId)
        {
            _logger.LogInformation($"Service: Obteniendo pagos de {userId}");
            return await _repository.ObtenerPagosPorUsuarioAsync(userId);
        }

        public async Task<List<PagoResponse>> ObtenerPagosPendientesAsync(Guid userId)
        {
            _logger.LogInformation($"Service: Obteniendo pendientes de {userId}");
            return await _repository.ObtenerPagosPendientesAsync(userId);
        }

        public async Task<PagoResponse> MarcarComoPagadoAsync(Guid pagoId)
        {
            _logger.LogInformation($"Service: Marcando {pagoId} como pagado");
            return await _repository.MarcarComoPagadoAsync(pagoId);
        }

        public async Task<bool> EliminarPagoAsync(Guid pagoId)
        {
            _logger.LogInformation($"Service: Eliminando {pagoId}");
            return await _repository.EliminarPagoAsync(pagoId);
        }
    }
}
