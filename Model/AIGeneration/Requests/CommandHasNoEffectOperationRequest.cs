namespace Model.AIGeneration.Requests;

public class CommandHasNoEffectOperationRequest : Request
{
    public CommandHasNoEffectOperationRequest(string location, string? command)
    {
        SystemMessage = SystemPrompt;
        UserMessage =
            
            @$"The player is in this location: ""{location}"". They wrote ""{command}"". If the player references any object of any kind, 
                other than what is described in their location, your response will indicate that no such object can be found here. If the player 
                performs an action that is benign or adds to the atmosphere (like dancing), describe it creatively. However, if the player attempts 
                an action that could disrupt or change the state of the story (like breaking a window or attacking a character), gently steer them back by 
                suggesting they change their mind. For example, you can say, ""You consider doing that, but then think better of it.""

            Here are some examples of how to handle different types of actions:

            1. Benign Action (Dancing):
            Player input: ""dance in the middle of the room""
            AI response: ""You dance in the middle of the room, feeling a sense of liberation. It's a small moment of joy in your journey.""

            2. Disruptive Action (Breaking a window):
            Player input: ""break the window""
            AI response: ""You consider breaking the window, but then think better of it. Maybe there's another way to solve this problem.""

            3. Neutral Action (Looking at a painting):
            Player input: ""look at the painting""
            AI response: ""The painting depicts a serene landscape with rolling hills and a distant castle. It seems almost lifelike, as if you could step into it.""

            Please follow these guidelines when generating responses. ";
            
            
            
    }
}