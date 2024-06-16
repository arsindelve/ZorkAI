namespace Model;

public enum Verbosity
{
    // Never display the description of the room, even on first visit.
    SuperBrief,
    
    // Display the description the room only on the first visit. 
    Brief,
    
    // Always display the description of the room. 
    Verbose
}