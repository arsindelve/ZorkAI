namespace ZorkOne.Item;

public class BlackBook : ItemBase, ICanBeExamined, ICanBeTakenAndDropped, ICanBeRead
{
    public override string[] NounsForMatching => ["book", "black book"];

    public string ExaminationDescription => """
                                            Commandment #12592
                                             
                                            Oh ye who go about saying unto each:  "Hello sailor":
                                            Dost thou know the magnitude of thy sin before the gods?
                                            Yea, verily, thou shalt be ground between two stones.
                                            Shall the angry gods cast thy body into the whirlpool?
                                            Surely, thy eye shall be put out with a sharp stick!
                                            Even unto the ends of the earth shalt thou wander and
                                            Unto the land of the dead shalt thou be sent at last.
                                            Surely thou shalt repent of thy cunning.
                                            """;

    public string OnTheGroundDescription => "There is a black book here. ";

    public override string NeverPickedUpDescription => "On the altar is a large black book, open to page 569.";

    public string ReadDescription => ExaminationDescription;

    public override string InInventoryDescription => "A black book";
}