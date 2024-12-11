﻿using Model.Location;

namespace Planetfall.Item.Feinstein;

public class Brochure : ItemBase, ICanBeRead, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A brochure";
    }

    public override string[] NounsForMatching => ["brochure"];

    public string ReadDescription =>
        """
         "The leading export of Blow'k-bibben-Gordo is the adventure game
        
             *** PLANETFALL ***
        
                 written by S. Eric Meretzky.
             Buy one today. Better yet, buy a thousand."
        """;

    public string ExaminationDescription => ReadDescription;

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "\n\nUnfortunately, one of those stupid Blow'k-bibben-Gordo brochures is here. ";
    }
    
    
}