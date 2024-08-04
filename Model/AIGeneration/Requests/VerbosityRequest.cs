namespace Model.AIGeneration.Requests;

public class MaximumVerbosityRequest : Request
{
    public MaximumVerbosityRequest()
    {
        UserMessage = "The adventurer has asked for maximum verbosity in room descriptions as they" +
                      "move from location to location. Let them know succinctly and sarcastically,  " +
                      "that, as narrator you will henceforth do as they have asked. ";
    }
}

public class MinimumVerbosityRequest : Request
{
    public MinimumVerbosityRequest()
    {
        UserMessage = "The adventurer has asked for the least amount of verbosity in room descriptions as they" +
                      "move from location to location. Let them know succinctly and sarcastically,  " +
                      "that, as narrator you will henceforth do as they have asked. ";
    }
}

public class MediumVerbosityRequest : Request
{
    public MediumVerbosityRequest()
    {
        UserMessage = "The adventurer has asked for a medium amount of verbosity in room descriptions as they" +
                      "move from location to location. Let them know succinctly and sarcastically,  " +
                      "that, as narrator you will henceforth do as they have asked. ";
    }
}