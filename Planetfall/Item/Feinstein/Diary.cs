public class Diary : ItemBase, ICanBeExamined, ICanBeRead, ICanBeTakenAndDropped
{

    public byte MessageNumber { get; set; }

    public string ReadDescription => Read();

    public string ExaminationDescription => "You've used this battered old recording machine as a diary for years. It includes a little button, which is flashing, and a microphone/speaker. To read its screen, type READ DIARY. ";

    public override string[] NounsForMatching => ["diary"];

    public string OnTheGroundDescription(ILocation currentLocation)
    {
        return "There is a diary here. ";
    }

    public override string NeverPickedUpDescription(ILocation currentLocation)
    {
        return OnTheGroundDescription(currentLocation);
    }

    public override string GenericDescription(ILocation? currentLocation)
    {
        return "A diary ";
    }

    private string Read()
    {
        MessageNumber++;
        return Messages[MessageNumber];
    }

    private string[] Messages = [
"11,344 July 22 -- Transferred from S.P.S. Trilobyte to S.P.S. Feinstein for the third of my four tours of duty. I\'m truly going to miss my commander,\nEnsign First Class Lim. He was a friend in every respect -- someone you could always go to with a problem, someone I could really look up to. We would\nsometimes talk long into the night. He would tell me about his home world of Ash-Down V, and I would talk about growing up on Gallium. I\'d get pretty\nhomesick sometimes, even though Gallium is not exactly one of the garden spots of the universe. I just hope my new commander is half as nice as Lim.\n\nThis new ship seems pretty swell. I\'m in a cabin with only five other ensigns, and I\'ve got one-and-a-half cubic meters of locker space! ",
"11,344 July 23 -- Met my new commander today -- Ensign Cadet First Class Blather. He seems like a real krip. (Excuse the language, Diary.) But that might just be a bad first impression.",
"11,344 July 25 -- One of my cabin mates, Gorund, organized a Double Fanucci tournament among all the Ensigns Seventh Class. We were playing during the 150-millichron rec period after lunch, and Blather burst in and confiscated the sets and told us that playing war games was a violation of patrol regulations. But Ensign Whirp, who's studying to be a patrol lawyer, said she couldn't find anything about it in the regulations anywhere. BLATHER IS REALLY A TOTAL MEGAKRIP!!!",
"11,344 July 28 -- I went to see the personnel officer today to find out what my new duties would involve. He showed me a list of all the open assignments, and I decided to put in for the grotch-feeding detail. We picked up a few grotches when we were on Crassus, and we're taking them to the Zoology Labs on Tremain so that maybe they can figure out how an animal can produce 47 times its weight in trot every day.",
"11,344 Bozbar 7 -- Everyone from the P.O. to the ship's cook has approved my application for the grotch-feeding detail -- except Blather. I have an appointment to see him tomorrow. Wish me luck.",
"11,344 Bozbar 8 -- TROT!! Blather rejected my application! And to make it worse, he said that since I seem to love grotches so much, he's assigning me to clean out their cages. TROT AND DOUBLE TROT!!",
"11,344 Bozbar 26 -- I haven't had time to keep my diary lately, because Blather's been watching us all like a Teleran bird. Also, last week he found the diary during a surprise inspection, gave me 200 demerits, and told me that diaries were against regulations. But I'll be frobbed if I'm going to stop. I've started hiding the diary inside my official documents file, and I keep that hidden in the air duct. From now on I'll have to sneak away somewhere to use it.",
"11,344 Bozbar 27 -- Greeting from the Deck Four Supply Closet of the S.P.S. Feinstein. I hope I'm not tempting fate, sneaking around with my diary this way. I used to be as much of a disbeliever in destiny as the next guy, but not anymore. Not since the time my mom warned my dad not to tempt fate by walking across the astral plains after dark, when the computerized analysis showed a 43% chance of resulting injury. My dad, stubborn as always, just laughed at her and went right on taking his nightly strolls. THE VERY NEXT SUMMER HE WENT WALKING AT NIGHT ON THE PLAINS AND STUMBLED OVER A CRATER AND BRUISED HIS KNEE! Gosh!",
"11,344 Bozbar 28 -- We entered planetary orbit today, a non-human world called Accardi-3 (although the natives call it something like Blow'k-bibben-gordo). They're not officially part of the union. The rumors say that we're picking up a special ambassador to take back to Tremain for negotiations on joining the union. Tomorrow we have to put on our dress uniforms for some special welcoming ceremony.",
"11,344 August 2 -- I caught a glimpse of the alien ambassador during the welcoming ceremonies yesterday. He looks like a cross between a tree trunk and a melting ice cream cone. But at least the ceremony got me out of cleaning the grotch cages today.",
"11,344 August 7 -- Went to the mandatory Patrol Informational Tri-vision Triple Feature last night. We saw 'Treatment For Space Lice Infestation,''Shoreleave Shirley: How to Guard Against Contracting Alien Diseases,' and 'The Oxygen Tank: Your Galvanized Buddy in the Vacuum.' Blather confined half the ensigns to quarters for hooting during the second feature. (The other half had fallen asleep during the first feature.)",
"11,344 August 24 -- TROT THAT TROTTING KRIP!! I applied for astrophysics training for the next quarter, but Blather says my work for the special\nassignment task force hasn\'t been good enough, so not only did he reject my astrophysics application, but he says I\'ll have to take remedial scrubbing\nnext quarter. WHAT A TROTTING KRIP!\n\nYou know, for the first time I\'m beginning to have doubts about whether I\'m really cut out for the patrol. When I was growing up on Gallium, it was\nalways taken for granted that I would join up when I came of age. My family has served in the patrol for five generations. In fact, my great-great-\ngrandfather was a high admiral and one of the founding fathers of the Patrol! But I seem to be permanently stuck at Ensign 7th, and Blather is making\nmy life miserable...",
"11,344 Septem 4 -- We left hyperspace today at about 7600; weren't scheduled to for about another two weeks. The grapevine says we have special orders to investigate a planetary system here. Apparently, some of the archaeologists back on Varshon think it might have been part of the Second Union. I can't imagine why anyone would settle out here in this remote corner of the galaxy.",
"11,344 Septem 5 -- That krip has done it again! I missed two little pellets of trot when I was when I was cleaning out the grotch cages yesterday, and Blather gave me 100 demerits andassigned me two extra shifts of deck scrubbing -- including Deck Nine, the filthiest deck on the ship! I'm considering asking for a transfer -- or if things get worse, I might evenabandon ship!",
"\"END OF DIARY -- REWINDING\" flashes across the screen; the machine whirrs, stops, and the little button flickers off."
    ];
}