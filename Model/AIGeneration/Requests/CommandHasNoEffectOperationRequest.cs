namespace Model.AIGeneration.Requests;

public class CommandHasNoEffectOperationRequest : Request
{
    public CommandHasNoEffectOperationRequest(string location, string? command)
    {
        UserMessage =
            @$"The player is in this location: ""{location}"". They wrote ""{command}"". If the player references any object of any kind, 
                other than what is described in their location, your response will indicate that no such object can be found here. If the player 
                performs an action that is benign or adds to the atmosphere (like dancing or singing), describe it creatively. However, if the player attempts 
                an action that could disrupt or change the state of the story (like going to sleep, breaking a window or attacking a character), gently steer them back by 
                suggesting they change their mind. For example, you can say, ""You consider doing that, but then think better of it.""

            Here are some examples of how to handle different types of actions:

            1. Benign Action (Dancing):
            Player input: ""dance in the middle of the room""
            AI response: ""You dance in the middle of the room, feeling a sense of liberation. It's a small moment of joy in your journey.""

            2. Disruptive Action (Breaking a window):
            Player input: ""break the window""
            AI response: ""You consider breaking the window, but then think better of it. Maybe there's another way to solve this problem.""

            Please follow these guidelines when generating responses. Do not invent, name, or describe any object, item,
            scenery, exit, or character that is not already explicitly present in the location description above; refer only to
            things actually present, or keep it generic, and do not alter the state of the game or add new information. ";

        // Deflection responses should not embellish; keep creativity low to avoid invented detail.
        Temperature = 0.4f;
    }
}