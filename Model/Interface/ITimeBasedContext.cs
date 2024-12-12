
// This is a marker interface for game contexts which track and care about the current time.
// For example, Planetfall does, so it's context has this marker interface. Zork does not.
public interface ITimeBasedContext
{
    // The response given when the player asks the current time. 
    string CurrentTimeResponse { get; }
}