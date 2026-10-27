using System.ComponentModel.DataAnnotations;

namespace SupermercadoMVC.Models;

public sealed class PaymentInput
{
    [Required(ErrorMessage = "Selecciona una venta.")]
    public int? CodigoVenta { get; set; }
    [Required(ErrorMessage = "Comprobante es obligatorio.")]
    [StringLength(50, ErrorMessage = "Comprobante permite hasta 50 caracteres.")]
    public string Comprobante { get; set; } = "";
    [Required(ErrorMessage = "Fecha de pago es obligatoria.")]
    public DateTime? FechaPago { get; set; }
    [Required(ErrorMessage = "Método de pago es obligatorio.")]
    [StringLength(50, ErrorMessage = "Método de pago permite hasta 50 caracteres.")]
    public string MetodoPago { get; set; } = "";
    [Required(ErrorMessage = "Monto es obligatorio.")]
    public decimal? Monto { get; set; }
    [Required(ErrorMessage = "Comisión es obligatoria.")]
    public decimal? Comision { get; set; } = 0;
    [StringLength(50, ErrorMessage = "Autorización permite hasta 50 caracteres.")]
    public string? Autorizacion { get; set; }
}

public sealed record PaymentSale(int CodigoVenta, string NumeroVenta, DateTime FechaVenta, string Cliente,
    string Sucursal, string Estado, decimal TotalVenta, decimal TotalPagado)
{
    public decimal Saldo => TotalVenta - TotalPagado;
    public bool PuedePagar => Estado == "Confirmada" && Saldo > 0;
}

public sealed record PaymentRow(int CodigoPago, string Comprobante, DateTime FechaPago, string MetodoPago,
    decimal Monto, decimal Comision, string? Autorizacion, string Estado);

public sealed class PaymentsPage
{
    public IReadOnlyList<PaymentSale> Rows { get; set; } = [];
    public IReadOnlyList<PaymentRow> History { get; set; } = [];
    public PaymentSale? Sale { get; set; }
    public PaymentInput Input { get; set; } = new() { FechaPago = DateTime.Today };
    public string Search { get; set; } = "";
    public string State { get; set; } = "";
    public string Balance { get; set; } = "";
    public int Page { get; set; } = 1;
    public int PageSize => 8;
    public long Total { get; set; }
    public int PageCount => (int)Math.Max(1, (Total + PageSize - 1) / PageSize);
    public decimal TotalConfirmed { get; set; }
    public decimal PaidConfirmed { get; set; }
    public decimal BalanceConfirmed { get; set; }
    public long PendingConfirmed { get; set; }
    public bool Register { get; set; }
    public string? Error { get; set; }
    public string? Notice { get; set; }
}
