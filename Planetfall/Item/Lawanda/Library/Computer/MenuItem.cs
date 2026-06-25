namespace Planetfall.Item.Lawanda.Library.Computer;

public class MenuItem
{
    internal virtual List<MenuItem>? Children { get; set; }

    internal virtual string? Text { get; set; }
}