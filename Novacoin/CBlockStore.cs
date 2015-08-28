using System;
using System.IO;
using System.Linq;

using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Generic;

namespace Novacoin
{
    [Table("BlockStorage")]
    class CBlockStoreItem
    {
        /// <summary>
        /// Item ID in the database
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int ItemID { get; set; }

        /// <summary>
        /// PBKDF2+Salsa20 of block hash
        /// </summary>
        [Unique]
        public byte[] ScryptHash { get; set; }

        /// <summary>
        /// Serialized representation of block header
        /// </summary>
        public byte[] BlockHeader { get; set; }

        /// <summary>
        /// Block position in file
        /// </summary>
        public long nBlockPos { get; set; }

        /// <summary>
        /// Block size in bytes
        /// </summary>
        public int nBlockSize { get; set; }
    }

    public class CBlockStore : IDisposable
    {
        private bool disposed = false;
        private object LockObj = new object();
        private SQLiteConnection dbConn = null;
        
        /// <summary>
        /// Init the block storage manager.
        /// </summary>
        /// <param name="IndexDB">Path to index database</param>
        /// <param name="BlockFile">Path to block file</param>
        public CBlockStore(string IndexDB = "blockstore.dat", string BlockFile = "blk0001.dat")
        {
            bool firstInit = !File.Exists(IndexDB);
            dbConn = new SQLiteConnection(new SQLitePlatformGeneric(), IndexDB);

            if (firstInit)
            {
                lock (LockObj)
                {
                    dbConn.CreateTable<CBlockStoreItem>(CreateFlags.AutoIncPK);
                }
            }
        }

        public bool ParseBlockFile(string BlockFile = "bootstrap.dat")
        {
            // TODO: Rewrite completely.

            var QueryGet = dbConn.Query<CBlockStoreItem>("select * from [BlockStorage] order by [ItemId] desc limit 1");

            var nOffset = 0L;

            if (QueryGet.Count() == 1)
            {
                var res = QueryGet.First();
                nOffset = res.nBlockPos + res.nBlockSize;
            }

            var fileReader = new BinaryReader(File.OpenRead(BlockFile));
            var fileStream = fileReader.BaseStream;

            var buffer = new byte[1000000]; // Max block size is 1Mb
            var intBuffer = new byte[4];

            fileStream.Seek(nOffset, SeekOrigin.Begin); // Seek to previous offset + previous block length

            dbConn.BeginTransaction();

            while (fileStream.Read(buffer, 0, 4) == 4) // Read magic number
            {
                var nMagic = BitConverter.ToUInt32(buffer, 0);
                if (nMagic != 0xe5e9e8e4)
                {
                    Console.WriteLine("Incorrect magic number.");
                    break;
                }

                var nBytesRead = fileStream.Read(buffer, 0, 4);
                if (nBytesRead != 4)
                {
                    Console.WriteLine("BLKSZ EOF");
                    break;
                }

                var nBlockSize = BitConverter.ToInt32(buffer, 0);

                nOffset = fileStream.Position;

                nBytesRead = fileStream.Read(buffer, 0, nBlockSize);

                if (nBytesRead == 0 || nBytesRead != nBlockSize)
                {
                    Console.WriteLine("BLK EOF");
                    break;
                }

                var block = new CBlock(buffer);

                if (nOffset % 1000 == 0)  // Commit on each 1000th block
                {
                    Console.WriteLine("Offset={0}, Hash: {1}", nOffset, block.header.Hash.ToString());
                    dbConn.Commit();
                    dbConn.BeginTransaction();
                }

                var result = dbConn.Insert(new CBlockStoreItem()
                {
                    ScryptHash = block.header.Hash,
                    BlockHeader = block.header,
                    nBlockPos = nOffset,
                    nBlockSize = nBlockSize
                });
            }

            dbConn.Commit();
            
            fileReader.Dispose();

            return true;
        }

        ~CBlockStore()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                }

                if (dbConn != null)
                {
                    dbConn.Close();
                    dbConn = null;
                }

                disposed = true;
            }
        }

    }
}
