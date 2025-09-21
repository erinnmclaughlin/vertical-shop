namespace VerticalShop;

public interface IDateProvider
{
    DateTimeOffset UtcNow { get; }
}

internal sealed class SystemDateProvider : IDateProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
