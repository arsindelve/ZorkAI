using NUnit.Framework;

// Tests share mutable static state through Repository (item/location singletons) - see
// Repository.Reset(), which every test calls via EngineTestsBase.GetTarget(). NUnit's default
// parallel fixture execution runs different [TestFixture] classes concurrently on separate
// threads, so one test's Repository.Reset() can race with another test's in-flight assertions
// and corrupt its view of the game world. Force sequential execution for this assembly so tests
// can't interleave and step on each other's Repository state.
[assembly: LevelOfParallelism(1)]
