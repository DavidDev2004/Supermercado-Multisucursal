using System.Data;
using Microsoft.Data.SqlClient;
using SupermercadoMVC.Models;

namespace SupermercadoMVC.Data;

public sealed class PaymentRuleException(string message) : Exception(message);

public sealed class PaymentsDataService(DatabaseConnectionFactory connections)
{
    // Cada agregado devuelve una fila por venta: no unir directamente las dos colecciones.
    // DECIMAL(37,2) deja un dígito para la resta; (38,2)-(38,2) reduciría la escala a 1 en SQL Server.
    private const string Sales = """
        WITH Saldos AS (
          SELECT v.CodigoVenta,v.NumeroVenta,v.FechaVenta,c.Nombres+' '+c.Apellidos AS Cliente,c.NIT,
            s.Nombre AS Sucursal,v.Estado,
            CAST(ROUND(COALESCE(d.Total,0),2) AS decimal(37,2)) AS TotalVenta,
            CAST(COALESCE(p.Total,0) AS decimal(37,2)) AS TotalPagado
          FROM dbo.Tbl_Ventas v JOIN dbo.Tbl_Clientes c ON c.CodigoCliente=v.CodigoCliente
          JOIN dbo.Tbl_Sucursales s ON s.CodigoSucursal=v.CodigoSucursal
          OUTER APPLY (SELECT SUM(Cantidad*PrecioUnitario-Descuento) AS Total
            FROM dbo.Tbl_VentasDetalle WHERE CodigoVenta=v.CodigoVenta) d
          OUTER APPLY (SELECT SUM(Monto) AS Total FROM dbo.Tbl_Pagos
            WHERE CodigoVenta=v.CodigoVenta AND Estado='Aplicado') p
        )
        """;
    private const string Filter = """
        (@State='' OR Estado=@State) AND
        (NumeroVenta LIKE @Search OR Cliente LIKE @Search OR NIT LIKE @Search OR EXISTS
          (SELECT 1 FROM dbo.Tbl_Pagos p WHERE p.CodigoVenta=Saldos.CodigoVenta AND p.Comprobante LIKE @Search)) AND
        (@Balance='' OR (@Balance='pendiente' AND TotalVenta>TotalPagado) OR
          (@Balance='pagado' AND TotalVenta=TotalPagado) OR (@Balance='negativo' AND TotalVenta<TotalPagado))
        """;
    private static SqlCommand Command(SqlConnection connection, SqlTransaction transaction, string sql) => new(sql, connection, transaction);
    private static void Int(SqlCommand command, string name, int value) => command.Parameters.Add(name, SqlDbType.Int).Value = value;
    private static void Money(SqlCommand command, string name, decimal value)
    {
        var p = command.Parameters.Add(name, SqlDbType.Decimal); p.Precision = 12; p.Scale = 2; p.Value = value;
    }
    private static PaymentSale Sale(SqlDataReader r) => new(r.GetInt32(0), r.GetString(1), r.GetDateTime(2),
        r.GetString(3), r.GetString(4), r.GetString(5), r.GetDecimal(6), r.GetDecimal(7));
    private static async Task RollbackAsync(SqlTransaction transaction)
    {
        try { await transaction.RollbackAsync(CancellationToken.None); }
        catch (InvalidOperationException) { }
        catch (SqlException) { }
    }

    public async Task<PaymentsPage> GetPageAsync(string search, string state, string balance, int page, CancellationToken ct)
    {
        var model = new PaymentsPage { Search = search, State = state, Balance = balance, Page = Math.Max(1, page) };
        await using var connection = connections.Create(); await connection.OpenAsync(ct);
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            await using (var command = Command(connection, transaction, Sales + $"SELECT COUNT_BIG(*) FROM Saldos WHERE {Filter};"))
            {
                Filters(command, model); model.Total = Convert.ToInt64(await command.ExecuteScalarAsync(ct));
            }
            model.Page = Math.Min(model.Page, model.PageCount);
            await using (var command = Command(connection, transaction, Sales + """
                SELECT COALESCE(SUM(TotalVenta),0),COALESCE(SUM(TotalPagado),0),
                  COALESCE(SUM(TotalVenta-TotalPagado),0),COUNT_BIG(CASE WHEN TotalVenta>TotalPagado THEN 1 END)
                FROM Saldos WHERE Estado='Confirmada';
                """))
            {
                await using var r = await command.ExecuteReaderAsync(ct); await r.ReadAsync(ct);
                model.TotalConfirmed = r.GetDecimal(0); model.PaidConfirmed = r.GetDecimal(1);
                model.BalanceConfirmed = r.GetDecimal(2); model.PendingConfirmed = r.GetInt64(3);
            }
            await using (var command = Command(connection, transaction, Sales + $"""
                SELECT CodigoVenta,NumeroVenta,FechaVenta,Cliente,Sucursal,Estado,TotalVenta,TotalPagado
                FROM Saldos WHERE {Filter} ORDER BY CodigoVenta DESC OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY;
                """))
            {
                Filters(command, model); Int(command, "@Offset", (model.Page - 1) * model.PageSize); Int(command, "@Size", model.PageSize);
                var rows = new List<PaymentSale>(); await using var r = await command.ExecuteReaderAsync(ct);
                while (await r.ReadAsync(ct)) rows.Add(Sale(r)); model.Rows = rows;
            }
            await transaction.CommitAsync(ct); return model;
        }
        catch { await RollbackAsync(transaction); throw; }
    }
    private static void Filters(SqlCommand command, PaymentsPage model)
    {
        command.Parameters.Add("@Search", SqlDbType.VarChar, 202).Value = $"%{model.Search}%";
        command.Parameters.Add("@State", SqlDbType.VarChar, 20).Value = model.State;
        command.Parameters.Add("@Balance", SqlDbType.VarChar, 20).Value = model.Balance;
    }

    public async Task LoadDetailAsync(PaymentsPage model, int id, CancellationToken ct)
    {
        await using var connection = connections.Create(); await connection.OpenAsync(ct);
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            await using (var command = Command(connection, transaction, Sales + """
                SELECT CodigoVenta,NumeroVenta,FechaVenta,Cliente,Sucursal,Estado,TotalVenta,TotalPagado
                FROM Saldos WHERE CodigoVenta=@Id;
                """))
            {
                Int(command, "@Id", id); await using var r = await command.ExecuteReaderAsync(ct);
                if (await r.ReadAsync(ct)) model.Sale = Sale(r);
            }
            if (model.Sale is null) throw new PaymentRuleException("La venta no existe.");
            await using (var command = Command(connection, transaction, """
                SELECT CodigoPago,Comprobante,FechaPago,MetodoPago,Monto,Comision,Autorizacion,Estado
                FROM dbo.Tbl_Pagos WHERE CodigoVenta=@Id ORDER BY FechaPago DESC,CodigoPago DESC;
                """))
            {
                Int(command, "@Id", id); var rows = new List<PaymentRow>(); await using var r = await command.ExecuteReaderAsync(ct);
                while (await r.ReadAsync(ct)) rows.Add(new(r.GetInt32(0), r.GetString(1), r.GetDateTime(2), r.GetString(3),
                    r.GetDecimal(4), r.GetDecimal(5), r.IsDBNull(6) ? null : r.GetString(6), r.GetString(7)));
                model.History = rows;
            }
            await transaction.CommitAsync(ct);
        }
        catch { model.Sale = null; await RollbackAsync(transaction); throw; }
    }

    public async Task RegisterAsync(PaymentInput input, CancellationToken ct)
    {
        Validate(input);
        await using var connection = connections.Create(); await connection.OpenAsync(ct);
        using var transaction = connection.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            // Mismo primer bloqueo que Ventas: serializa pagos sobre una venta y sus cambios de estado.
            await using (var command = Command(connection, transaction,
                "SELECT Estado FROM dbo.Tbl_Ventas WITH (UPDLOCK,HOLDLOCK) WHERE CodigoVenta=@Id;"))
            {
                Int(command, "@Id", input.CodigoVenta!.Value);
                var state = await command.ExecuteScalarAsync(ct) as string;
                if (state is null) throw new PaymentRuleException("La venta ya no existe.");
                if (state != "Confirmada") throw new PaymentRuleException("Solo se pueden registrar pagos para ventas Confirmadas.");
            }
            decimal total, paid;
            await using (var command = Command(connection, transaction, """
                SELECT CAST(ROUND(COALESCE(SUM(Cantidad*PrecioUnitario-Descuento),0),2) AS decimal(37,2))
                FROM dbo.Tbl_VentasDetalle WHERE CodigoVenta=@Id;
                SELECT COALESCE(SUM(Monto),0) FROM dbo.Tbl_Pagos WHERE CodigoVenta=@Id AND Estado='Aplicado';
                """))
            {
                Int(command, "@Id", input.CodigoVenta.Value); await using var r = await command.ExecuteReaderAsync(ct);
                await r.ReadAsync(ct); total = r.GetDecimal(0);
                await r.NextResultAsync(ct); await r.ReadAsync(ct); paid = r.GetDecimal(0);
            }
            var balance = total - paid;
            if (balance <= 0) throw new PaymentRuleException("La venta no tiene saldo positivo disponible. Consulta el historial.");
            if (input.Monto!.Value > balance) throw new PaymentRuleException("El monto supera el saldo actual. Consulta el saldo y ajusta el pago.");
            // UNIQUE(Comprobante) es la garantía final incluso entre pagos de ventas diferentes.
            await using (var command = Command(connection, transaction, """
                INSERT INTO dbo.Tbl_Pagos(CodigoVenta,Comprobante,FechaPago,MetodoPago,Monto,Comision,Autorizacion,Estado)
                VALUES(@Id,@Receipt,@Date,@Method,@Amount,@Fee,@Authorization,'Aplicado');
                """))
            {
                Int(command, "@Id", input.CodigoVenta.Value);
                command.Parameters.Add("@Receipt", SqlDbType.VarChar, 50).Value = input.Comprobante.Trim();
                command.Parameters.Add("@Date", SqlDbType.Date).Value = input.FechaPago!.Value.Date;
                command.Parameters.Add("@Method", SqlDbType.VarChar, 50).Value = input.MetodoPago.Trim();
                Money(command, "@Amount", input.Monto.Value); Money(command, "@Fee", input.Comision!.Value);
                command.Parameters.Add("@Authorization", SqlDbType.VarChar, 50).Value = string.IsNullOrWhiteSpace(input.Autorizacion) ? DBNull.Value : input.Autorizacion.Trim();
                if (await command.ExecuteNonQueryAsync(ct) != 1) throw new PaymentRuleException("No se pudo registrar el pago.");
            }
            await transaction.CommitAsync(ct);
        }
        catch { await RollbackAsync(transaction); throw; }
    }

    private static void Validate(PaymentInput input)
    {
        if (!input.CodigoVenta.HasValue || input.CodigoVenta <= 0) throw new PaymentRuleException("Selecciona una venta válida.");
        if (string.IsNullOrWhiteSpace(input.Comprobante) || input.Comprobante.Length > 50) throw new PaymentRuleException("Comprobante es obligatorio y permite hasta 50 caracteres.");
        if (string.IsNullOrWhiteSpace(input.MetodoPago) || input.MetodoPago.Length > 50) throw new PaymentRuleException("Método de pago es obligatorio y permite hasta 50 caracteres.");
        if (input.Autorizacion?.Length > 50) throw new PaymentRuleException("Autorización permite hasta 50 caracteres.");
        if (!input.FechaPago.HasValue || input.FechaPago.Value.TimeOfDay != TimeSpan.Zero) throw new PaymentRuleException("Selecciona una fecha de pago válida sin hora.");
        if (!ValidMoney(input.Monto, true)) throw new PaymentRuleException("Monto debe ser positivo, con hasta dos decimales y máximo 9999999999.99.");
        if (!ValidMoney(input.Comision, false) || input.Comision > input.Monto) throw new PaymentRuleException("Comisión debe estar entre cero y el monto, con hasta dos decimales.");
    }
    public static bool ValidMoney(decimal? value, bool positive) => value.HasValue && value >= 0 && value <= 9999999999.99m
        && (!positive || value > 0) && decimal.Round(value.Value, 2) == value.Value;
}
