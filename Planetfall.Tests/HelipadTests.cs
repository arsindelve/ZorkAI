using FluentAssertions;
using GameEngine.Item;
using Model.AIGeneration;
using Model.AIParsing;
using Model.Intent;
using Model.Interaction;
using Moq;
using Planetfall.Location.Kalamontee.Tower;

namespace Planetfall.Tests;

/// <summary>
///     Issue #519 — the Helipad described four things (the vehicle, the fence, the spiral staircase and
///     the rotor blades) and let you examine none of them: every noun the room's own prose put in front
///     of the player fell through to the narrator. The original attaches the vehicle and the stairway as
///     local-globals and the fence as a pseudo (<c>planetfall-source/compone.zil:2740-2765</c>), so all
///     four are real, referenceable nouns while standing on the pad.
///     Like <see cref="SceneryInvariantTests" /> and <see cref="SanfacTests" />, the examine tests call
///     the room's real override chain directly: the test parser only knows real item nouns, and scenery
///     nouns are not items.
/// </summary>
public class HelipadTests : EngineTestsBase
{
    [TestFixture]
    public class ExaminingWhatTheRoomDescribes : EngineTestsBase
    {
        [TestCase("fence")]
        [TestCase("perimeter")]
        public async Task ExamineFence_DescribesTheFence(string noun)
        {
            var result = await Examine(noun);

            result.Should().BeOfType<PositiveInteractionResult>();
            result.InteractionMessage.Should().Contain("fence");
        }

        [TestCase("vehicle")]
        [TestCase("helicopter")]
        [TestCase("chopper")]
        [TestCase("copter")]
        public async Task ExamineVehicle_DescribesTheWeatheredVehicle(string noun)
        {
            var result = await Examine(noun);

            result.Should().BeOfType<PositiveInteractionResult>();
            result.InteractionMessage.Should().Contain("weathered");
        }

        [TestCase("staircase")]
        [TestCase("spiral staircase")]
        [TestCase("stairs")]
        [TestCase("stairway")]
        public async Task ExamineStaircase_DescribesTheStairsDownIntoTheTower(string noun)
        {
            var result = await Examine(noun);

            result.Should().BeOfType<PositiveInteractionResult>();
            result.InteractionMessage.Should().Contain("tower");
        }

        [TestCase("rotor blades")]
        [TestCase("rotors")]
        [TestCase("blades")]
        public async Task ExamineRotorBlades_DescribesTheBlades(string noun)
        {
            var result = await Examine(noun);

            result.Should().BeOfType<PositiveInteractionResult>();
            result.InteractionMessage.Should().Contain("blades");
        }

        /// <summary>
        ///     The narrator sentinel: an unmatched noun yields <see cref="NoNounMatchInteractionResult" />,
        ///     which is what every noun in this room used to produce. Pinning it here keeps the test above
        ///     honest about what it is proving.
        /// </summary>
        [Test]
        public async Task ExamineSomethingTheRoomNeverMentions_StillFallsThroughToTheNarrator()
        {
            var result = await Examine("grue");

            result.Should().BeOfType<NoNounMatchInteractionResult>();
        }

        private async Task<InteractionResult> Examine(string noun)
        {
            var target = GetTarget();
            var location = GetLocation<Helipad>();
            target.Context.CurrentLocation = location;

            return await location.RespondToSimpleInteraction(
                new SimpleIntent { Verb = "examine", Noun = noun },
                target.Context, Mock.Of<IGenerationClient>(),
                new ItemProcessorFactory(Mock.Of<IAITakeAndAndDropParser>()));
        }
    }

    /// <summary>
    ///     "enter helicopter" only ever worked because the destination ROOM happens to be titled
    ///     "Helicopter"; "vehicle" — the word the Helipad's own description uses — missed entirely and got
    ///     "You cannot go that way." Both synonyms name the same object in the original
    ///     (<c>SYNONYM VEHICLE HELICOPTER</c>, <c>compone.zil:2767-2772</c>), so both must board it.
    /// </summary>
    [TestFixture]
    public class BoardingTheVehicle : EngineTestsBase
    {
        [TestCase("enter helicopter")]
        [TestCase("enter vehicle")]
        public async Task EnterTheVehicle_FromTheHelipad_PutsYouInsideIt(string input)
        {
            var target = GetTarget();
            StartHere<Helipad>();

            var response = await target.GetResponse(input);

            response.Should().Contain("large vehicle with a lot of cargo space");
            target.Context.CurrentLocation.Should().BeOfType<Helicopter>();
        }
    }
}
