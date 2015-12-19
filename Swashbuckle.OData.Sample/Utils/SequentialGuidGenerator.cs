using System;
using System.Security.Cryptography;

namespace SwashbuckleODataSample.Utils
{
    /// <summary>
    /// Generates <see cref="System.Guid" /> values using strategy from Jeremy Todd.
    /// See <a href="http://www.codeproject.com/Articles/388157/GUIDs-as-fast-primary-keys-under-multiple-database">GUIDs as fast primary keys under multiple databases</a>.
    /// </summary>
    public static class SequentialGuidGenerator
    {
        private static readonly RNGCryptoServiceProvider Rng = new RNGCryptoServiceProvider();

        public static Guid Generate(SequentialGuidType sequentialGuidType)
        {
            var randomBytes = new byte[10];
            Rng.GetBytes(randomBytes);

            var timestamp = DateTime.UtcNow.Ticks / 10000L;
            var timestampBytes = BitConverter.GetBytes(timestamp);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
            }

            var guidBytes = new byte[16];

            switch (sequentialGuidType)
            {
                case SequentialGuidType.SequentialAsString:
                case SequentialGuidType.SequentialAsBinary:
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 0, 6);
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10);

                    // If formatting as a string, we have to reverse the order
                    // of the Data1 and Data2 blocks on little-endian systems.
                    if (sequentialGuidType == SequentialGuidType.SequentialAsString && BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(guidBytes, 0, 4);
                        Array.Reverse(guidBytes, 4, 2);
                    }
                    break;
                case SequentialGuidType.SequentialAtEnd:

                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sequentialGuidType), sequentialGuidType, null);
            }

            return new Guid(guidBytes);
        }
    }

    /// <remarks>
    ///     Database 	            GUID Column 	    SequentialGuidType
    ///     Microsoft SQL Server 	uniqueidentifier 	SequentialAtEnd
    ///     MySQL 	                char(36) 	        SequentialAsString
    ///     Oracle 	                raw(16) 	        SequentialAsBinary
    ///     PostgreSQL 	            uuid 	            SequentialAsString
    ///     SQLite  	            varies  	        varies
    /// </remarks>
    public enum SequentialGuidType
    {
        /// <summary>
        /// Use for MySQL char(36)
        /// Use for PostgreSQL uuid
        /// </summary>
        SequentialAsString,

        /// <summary>
        /// Use for Oracle raw(16)
        /// </summary>
        SequentialAsBinary,

        /// <summary>
        /// Use for Microsoft SQL Server uniqueidentifier
        /// </summary>
        SequentialAtEnd
    }
}