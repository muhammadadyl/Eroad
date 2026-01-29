using Xunit;

namespace Eroad.BFF.IntegrationTest.Collections;

/// <summary>
/// Test collection for BFF integration tests that share the same fixture
/// and should not run in parallel to avoid distributed lock contention.
/// Tests within this collection will run sequentially, but different collections
/// can still run in parallel.
/// </summary>
[CollectionDefinition("BFF Collection")]
public class BffTestCollection : ICollectionFixture<BFFTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
