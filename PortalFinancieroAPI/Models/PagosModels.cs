using System.ComponentModel.DataAnnotations;

namespace PortalFinancieroAPI.Models
{
    // DTO entrada: Registrar pago
    public class PagoRequest
    {
        [Required(ErrorMessage = "Usuario requerido")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Tipo de servicio requerido")]
        [RegularExpression("^(agua|luz|telefonía|internet|otros)$", ErrorMessage = "Servicio inválido")]
        public string TipoServicio { get; set; }

        [Required(ErrorMessage = "Monto requerido")]
        [Range(0.01, 50000, ErrorMessage = "Monto entre 0.01 y 50,000")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "Referencia requerida")]
        public string Referencia { get; set; }

        public DateTime? FechaVencimiento { get; set; }

        public string? Observaciones { get; set; }
    }

    // DTO salida: Respuesta de pago
    public class PagoResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string TipoServicio { get; set; }
        public decimal Monto { get; set; }
        public string Referencia { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public DateTime? FechaPago { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Modelo de BD
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

    // Respuesta genérica
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
    }
}
