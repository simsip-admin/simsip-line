using System;

namespace fNbt {
    /// <summary> Exception thrown when a format violation is detected while
    /// parsing or serializing an NBT file. </summary>
#if !WINDOWS_PHONE && !NETFX_CORE
    [Serializable]
#endif
    public sealed class NbtFormatException : Exception {
        internal NbtFormatException( string message )
            : base( message ) {}
    }
}