using JetBrains.Annotations;

namespace Lykke.Service.Bil2Indexer.Client
{
    /// <summary>
    /// Bil2Indexer client interface.
    /// </summary>
    [PublicAPI]
    public interface IBil2IndexerClient
    {
        // Make your app's controller interfaces visible by adding corresponding properties here.
        // NO actual methods should be placed here (these go to controller interfaces, for example - IBil2IndexerApi).
        // ONLY properties for accessing controller interfaces are allowed.

        /// <summary>Application Api interface</summary>
        IBil2IndexerApi Api { get; }
    }
}
