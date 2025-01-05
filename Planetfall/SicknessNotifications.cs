namespace Planetfall;

public class SicknessNotifications
{
    private static readonly Dictionary<(int, int), string> Notifications = new()
    {
        { (2, 1850), "You notice that you feel a bit weak and slightly flushed, but you're not sure why. " },
        { (3, 2250), "You notice that you feel unusually weak, and you suspect that you have a fever. " },
        { (4, 2500), "You are now feeling quite under the weather, not unlike a bad flu. " },
        { (5, 2700), "Your fever seems to have gotten worse, and you're developing a bad headache. " },
        { (6, 3000), "Your health has deteriorated further. You feel hot and weak, and your head is throbbing. " },
        { (7, 3000), "You feel very, very sick, and have almost no strength left. " },
        {
            (8, 3000),
            "You feel like you're on fire, burning up from the fever. You're almost too weak to move, and your brain is reeling from the pounding headache. "
        }
    };

    [UsedImplicitly]
    public List<int> DaysNotified { get; set; } = new();

    internal string? GetNotification(int day, int time)
    {
        if (DaysNotified.Contains(day))
            return null;

        var match = Notifications.FirstOrDefault(x => x.Key.Item1 == day && x.Key.Item2 >= time);
        if (default(KeyValuePair<(int, int), string>).Equals(match)) return null;

        DaysNotified.Add(day);
        return match.Value;
    }
}