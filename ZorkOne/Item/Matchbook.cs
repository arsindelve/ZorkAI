namespace ZorkOne.Item;

public class Matchbook : ItemBase, ICanBeRead, ICanBeExamined, ICanBeTakenAndDropped
{
    public override string[] NounsForMatching => ["matches", "matchbook"];

    public string ReadDescription => """
                                     (Close cover before striking)

                                     YOU too can make BIG MONEY in the exciting field of PAPER SHUFFLING!

                                     Mr. Anderson of Muddle, Mass. says: "Before I took this course I was a lowly bit twiddler. Now with what I learned at GUE Tech I feel really important and can obfuscate and confuse with the best."

                                     Dr. Blank had this to say: "Ten short days ago all I could look forward to was a dead-end job as a doctor. Now I have a promising future and make really big Zorkmids."

                                     GUE Tech can't promise these fantastic results to everyone. But when you earn your degree from GUE Tech, your future will be brighter.
                                     """;

    public string ExaminationDescription => "The matchbook isn't very interesting, except for what's written on it. ";

    public string OnTheGroundDescription => NeverPickedUpDescription;

    public override string InInventoryDescription => "A matchbook";

    public override string NeverPickedUpDescription => "There is a matchbook whose cover says \"Visit Beautiful FCD#3\" here. ";
}