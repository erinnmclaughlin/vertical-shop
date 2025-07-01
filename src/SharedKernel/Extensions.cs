using Npgsql;

namespace VerticalShop;

public static class Extensions
{
    public static bool IsUniqueConstraintViolationOnColumn(this PostgresException ex, string columnName) =>
        ex.ErrorCode.ToString() == PostgresErrorCodes.UniqueViolation &&
        ex.ColumnName == columnName;
}
