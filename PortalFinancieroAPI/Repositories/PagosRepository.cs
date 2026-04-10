using PortalFinancieroAPI.Models;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace PortalFinancieroAPI.Repositories
{
    public class Pago
    {
        public Guid id { get; set; }
        public Guid user_id { get; set; }
        public string tipo_servicio { get; set; }
        public decimal monto { get; set; }
        public string referencia { get; set; }
        public string estado { get; set; }
        public DateTime? fecha_vencimiento { get; set; }
        public DateTime? fecha_pago { get; set; }
        public string? observaciones { get; set; }
        public DateTime created_at { get; set; }
    }

    public class PagosRepository
    {
        private readonly IConfiguration _config;
        private readonly ILogger<PagosRepository> _logger;
        private readonly HttpClient _httpClient;

        public PagosRepository(IConfiguration config, ILogger<PagosRepository> logger, HttpClient httpClient)
        {
            _config = config;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<PagoResponse> InsertarPagoAsync(Dictionary<string, object> datos)
        {
            try
            {
                var nuevo = new Pago
                {
                    id = Guid.NewGuid(),
                    user_id = (Guid)datos["user_id"],
                    tipo_servicio = (string)datos["tipo_servicio"],
                    monto = (decimal)datos["monto"],
                    referencia = (string)datos["referencia"],
                    estado = "pendiente",
                    observaciones = (string?)datos.GetValueOrDefault("observaciones"),
                    fecha_vencimiento = datos.ContainsKey("fecha_vencimiento") ? (DateTime?)datos["fecha_vencimiento"] : null,
                    created_at = DateTime.UtcNow
                };

                var supabaseUrl = _config["Supabase:Url"];
                var supabaseKey = _config["Supabase:Key"];

                var json = JsonSerializer.Serialize(nuevo);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{supabaseUrl}/rest/v1/pagos";
                var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
                request.Headers.Add("apikey", supabaseKey);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"HTTP {response.StatusCode}: {errorContent}");
                    throw new Exception($"HTTP {response.StatusCode}");
                }

                return new PagoResponse
                {
                    Id = nuevo.id,
                    UserId = nuevo.user_id,
                    TipoServicio = nuevo.tipo_servicio,
                    Monto = nuevo.monto,
                    Referencia = nuevo.referencia,
                    Estado = nuevo.estado,
                    FechaVencimiento = nuevo.fecha_vencimiento,
                    CreatedAt = nuevo.created_at
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error InsertarPago: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PagoResponse>> ObtenerPagosPorUsuarioAsync(Guid userId)
        {
            try
            {
                var supabaseUrl = _config["Supabase:Url"];
                var supabaseKey = _config["Supabase:Key"];

                var url = $"{supabaseUrl}/rest/v1/pagos?user_id=eq.{userId}&order=created_at.desc";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("apikey", supabaseKey);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"HTTP {response.StatusCode} al obtener pagos");
                    return new List<PagoResponse>();
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var pagos = JsonSerializer.Deserialize<List<Pago>>(responseContent);

                return (pagos ?? new()).Select(x => new PagoResponse
                {
                    Id = x.id,
                    UserId = x.user_id,
                    TipoServicio = x.tipo_servicio,
                    Monto = x.monto,
                    Referencia = x.referencia,
                    Estado = x.estado,
                    FechaVencimiento = x.fecha_vencimiento,
                    FechaPago = x.fecha_pago,
                    CreatedAt = x.created_at
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error ObtenerPagos: {ex.Message}");
                return new List<PagoResponse>();
            }
        }

        public async Task<List<PagoResponse>> ObtenerPagosPendientesAsync(Guid userId)
        {
            try
            {
                var supabaseUrl = _config["Supabase:Url"];
                var supabaseKey = _config["Supabase:Key"];

                var url = $"{supabaseUrl}/rest/v1/pagos?user_id=eq.{userId}&estado=eq.pendiente&order=fecha_vencimiento.asc";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("apikey", supabaseKey);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                    return new List<PagoResponse>();

                var responseContent = await response.Content.ReadAsStringAsync();
                var pagos = JsonSerializer.Deserialize<List<Pago>>(responseContent);

                return (pagos ?? new()).Select(x => new PagoResponse
                {
                    Id = x.id,
                    UserId = x.user_id,
                    TipoServicio = x.tipo_servicio,
                    Monto = x.monto,
                    Referencia = x.referencia,
                    Estado = x.estado,
                    FechaVencimiento = x.fecha_vencimiento,
                    CreatedAt = x.created_at
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error ObtenerPendientes: {ex.Message}");
                return new List<PagoResponse>();
            }
        }

        public async Task<PagoResponse> MarcarComoPagadoAsync(Guid pagoId)
        {
            try
            {
                var supabaseUrl = _config["Supabase:Url"];
                var supabaseKey = _config["Supabase:Key"];

                var updateData = new { estado = "pagado", fecha_pago = DateTime.UtcNow };
                var json = JsonSerializer.Serialize(updateData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"{supabaseUrl}/rest/v1/pagos?id=eq.{pagoId}";
                var request = new HttpRequestMessage(HttpMethod.Patch, url) { Content = content };
                request.Headers.Add("apikey", supabaseKey);

                var response = await _httpClient.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                    throw new Exception($"HTTP {response.StatusCode}");

                var responseContent = await response.Content.ReadAsStringAsync();
                var pago = JsonSerializer.Deserialize<List<Pago>>(responseContent)?.FirstOrDefault();

                if (pago == null)
                    throw new Exception("Pago no encontrado");

                return new PagoResponse
                {
                    Id = pago.id,
                    UserId = pago.user_id,
                    TipoServicio = pago.tipo_servicio,
                    Monto = pago.monto,
                    Referencia = pago.referencia,
                    Estado = pago.estado,
                    FechaPago = pago.fecha_pago,
                    CreatedAt = pago.created_at
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error MarcarPagado: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> EliminarPagoAsync(Guid pagoId)
        {
            try
            {
                var supabaseUrl = _config["Supabase:Url"];
                var supabaseKey = _config["Supabase:Key"];

                var url = $"{supabaseUrl}/rest/v1/pagos?id=eq.{pagoId}&estado=eq.pendiente";
                var request = new HttpRequestMessage(HttpMethod.Delete, url);
                request.Headers.Add("apikey", supabaseKey);

                var response = await _httpClient.SendAsync(request);
                
                var success = response.IsSuccessStatusCode;
                _logger.LogInformation($"Eliminar {pagoId}: {(success ? "OK" : "FAIL")}");
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error EliminarPago: {ex.Message}");
                return false;
            }
        }
    }
}
