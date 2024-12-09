namespace Planetfall.Item.Feinstein;

public class Graffiti : ItemBase, ICanBeExamined, ICanBeRead
{
    public override string[] NounsForMatching => ["graffiti"];

    public string ExaminationDescription => """
                                            All the graffiti seem to be about Blather. One of the least obscene items reads:                                                                                                                             
                                             
                                            There once was a krip, name of Blather                                                                                                                                                                       
                                            Who told a young Ensign named Smather                                                                                                                                                                        
                                            "I'll make you inherit                                                                                                                                                                                       
                                            A trotting demerit                                                                                                                                                                                           
                                            And ship you off to those stinking fawg-infested tar pools of Krather."                                                                                                                                      
                                                                                                                                                                                                                                                     
                                            It's not a very good limerick, is it?  
                                            """;

    public string ReadDescription => ExaminationDescription;
}