/**
 *  Novacoin classes library
 *  Copyright (C) 2015 Alex D. (balthazar.ad@gmail.com)

 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU Affero General Public License as
 *  published by the Free Software Foundation, either version 3 of the
 *  License, or (at your option) any later version.

 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Affero General Public License for more details.

 *  You should have received a copy of the GNU Affero General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */


using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Interop;
using SQLite.Net.Platform.Generic;
using System;
using System.IO;
using System.Linq;

namespace Novacoin
{
    /// <summary>
    /// Key storage item and structure
    /// </summary>
    [Table("KeyStorage")]
    class KeyStorageItem
    {
        /// <summary>
        /// Key item number
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int ItemId { get; set; }

        /// <summary>
        /// Hash160 of pubkey
        /// </summary>
        public byte[] KeyID { get; set; }

        /// <summary>
        /// Public key 
        /// </summary>
        public byte[] PublicKey { get; set; }

        /// <summary>
        /// Private key 
        /// </summary>
        public byte[] PrivateKey { get; set; }

        /// <summary>
        /// Compressed key flag
        /// </summary>
        public bool IsCompressed { get; set; }

        /// <summary>
        /// Is this key a part of KeyPool?
        /// </summary>
        [Indexed]
        public bool IsUsed { get; set; }

        /// <summary>
        /// Item creation time
        /// </summary>
        [Indexed]
        public int nTime { get; set; }
    }

    /// <summary>
    /// Script storage item and structure
    /// </summary>
    [Table("ScriptStorage")]
    class ScriptStorageItem
    {
        /// <summary>
        /// Script item number
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int ItemId { get; set; }

        /// <summary>
        /// Hash160 of script 
        /// </summary>
        public byte[] ScriptID { get; set; }

        /// <summary>
        /// Script code bytes 
        /// </summary>
        public byte[] ScriptCode { get; set; }
    }

    /// <summary>
    /// select count(...) as Count from ... where ...
    /// select number from ... where ...
    /// </summary>
    class NumQuery
    {
        public int Num { get; set; }
    }

    /// <summary>
    /// Key storage
    /// </summary>
    public class CKeyStore : IDisposable
    {
        private bool disposed = false;
        private object LockObj = new object();
        private SQLiteConnection dbConn = null;
        private int nKeyPoolSize = 100;

        /// <summary>
        /// Initialize new instance of key store.
        /// </summary>
        /// <param name="strDatabasePath">Path to database file.</param>
        /// <param name="KeyPoolSize">Number of reserved keys.</param>
        public CKeyStore(string strDatabasePath="KeyStore.db", int KeyPoolSize = 100)
        {
            bool firstInit = !File.Exists(strDatabasePath);

            dbConn = new SQLiteConnection(new SQLitePlatformGeneric(), strDatabasePath);
            nKeyPoolSize = KeyPoolSize;

            if (firstInit)
            {
                lock(LockObj)
                {
                    dbConn.CreateTable<KeyStorageItem>(CreateFlags.AutoIncPK);
                    dbConn.CreateTable<ScriptStorageItem>(CreateFlags.AutoIncPK);

                    GenerateKeys(nKeyPoolSize);
                }
            }
        }

        ~CKeyStore()
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


        /// <summary>
        /// Generate keys and insert them to key store.
        /// </summary>
        /// <param name="n"></param>
        private void GenerateKeys(int n)
        {
            lock (LockObj)
            {
                dbConn.BeginTransaction();

                // Generate keys
                for (int i = 0; i < n; i++)
                {
                    var keyPair = new CKeyPair();

                    var res = dbConn.Insert(new KeyStorageItem()
                    {
                        KeyID = keyPair.KeyID,
                        PublicKey = keyPair.PubKey,
                        PrivateKey = keyPair,
                        IsCompressed = keyPair.IsCompressed,
                        IsUsed = false,
                        nTime = Interop.GetTime()
                    });

                    // TODO: Additional initialization
                }

                dbConn.Commit();
            }
        }

        /// <summary>
        /// Insert key data into table
        /// </summary>
        /// <param name="keyPair">CKeyPair instance</param>
        /// <returns>Result</returns>
        public bool AddKey(CKeyPair keyPair)
        {
            lock(LockObj)
            {
                var res = dbConn.Insert(new KeyStorageItem()
                {
                    KeyID = keyPair.KeyID,
                    PublicKey = keyPair.PubKey,
                    PrivateKey = keyPair,
                    IsCompressed = keyPair.IsCompressed,
                    IsUsed = true,
                    nTime = Interop.GetTime()
                });

                if (res == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check existance of item with provided KeyID
        /// </summary>
        /// <param name="scriptID">Hash160 of public key.</param>
        /// <returns>Checking result</returns>
        public bool HaveKey(CKeyID keyID)
        {
            var QueryCount = dbConn.Query<NumQuery>("select count([ItemID]) from [KeyStorage] where [KeyID] = ?", (byte[])keyID);

            return QueryCount.First().Num == 1;
        }

        /// <summary>
        /// Get the key pair object.
        /// </summary>
        /// <param name="keyID">Hash of public key.</param>
        /// <param name="keyPair">Instance of CKeyPair or null.</param>
        /// <returns>Result</returns>
        public bool GetKey(CKeyID keyID, out CKeyPair keyPair)
        {
            var QueryGet = dbConn.Query<KeyStorageItem>("select * from [KeyStorage] where [KeyID] = ?", (byte[])keyID);

            if (QueryGet.Count() == 1)
            {
                keyPair = new CKeyPair(QueryGet.First().PrivateKey);
                return true;
            }

            keyPair = null;
            return false;
        }

        /// <summary>
        /// Add redeem script to script store.
        /// </summary>
        /// <param name="script">CScript instance</param>
        /// <returns>Result</returns>
        public bool AddScript(CScript script)
        {
            lock (LockObj)
            {
                var res = dbConn.Insert(new ScriptStorageItem()
                {
                    ScriptID = script.ScriptID,
                    ScriptCode = script
                });

                if (res == 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check existance of item with provided ScriptID
        /// </summary>
        /// <param name="scriptID">Hash160 of script code.</param>
        /// <returns>Checking result</returns>
        public bool HaveScript(CScriptID scriptID)
        {
            var QueryGet = dbConn.Query<NumQuery>("select count([ItemID]) from [ScriptStorage] where [ScriptID] = ?", (byte[])scriptID);

            return QueryGet.First().Num == 1;
        }

        /// <summary>
        /// Get redeem script from database.
        /// </summary>
        /// <param name="scriptID">Script ID, evaluated as Hash160(script code).</param>
        /// <param name="script">Instance of CScript</param>
        /// <returns>Result</returns>
        public bool GetScript(CScriptID scriptID, out CScript script)
        {
            var QueryGet = dbConn.Query<ScriptStorageItem>("select * from [ScriptStorage] where [ScriptID] = ?", (byte[])scriptID);

            if (QueryGet.Count() == 1)
            {
                script = new CScript(QueryGet.First().ScriptCode);
                return true;
            }

            script = null;
            return false;
        }

  
        /// <summary>
        /// SQLite return type for ReserveKey
        /// </summary>                      
        class ReservedKey
        {
            public int ItemId { get; set; }
            public byte[] KeyID { get; set; }
        }

        /// <summary>
        /// Reserve key from a list of unused keys.
        /// </summary>
        /// <param name="nKeyIndex">Internal index of key</param>
        /// <returns>CKeyID instance</returns>
        public CKeyID SelectKey(out int nKeyIndex)
        {
            var QueryGet = dbConn.Query<ReservedKey>("select ItemId, KeyID from [KeyStorage] where not [IsUsed] order by [nTime] asc limit 1");

            if (QueryGet.Count() == 1)
            {
                var res = QueryGet.First();

                nKeyIndex = res.ItemId;

                return new CKeyID(res.KeyID);
            }
            else
            {
                // Generate new keys in case if keypool is exhausted.

                GenerateKeys(nKeyPoolSize);

                return SelectKey(out nKeyIndex);
            }
        }

        /// <summary>
        /// Mark key as used.
        /// </summary>
        /// <param name="nIndex">Internal index.</param>
        public void MarkUsed(int nIndex)
        {
            lock (LockObj)
            {
                dbConn.Execute("update [KeyStorage] set [IsUsed] = true where [ItemId] = ?", nIndex);
            }
        }

        /// <summary>
        /// Mark key as unused.
        /// </summary>
        /// <param name="nIndex">Internal index.</param>
        public void MarkUnused(int nIndex)
        {
            lock (LockObj)
            {
                dbConn.Execute("update [KeyStorage] set [IsUsed] = false where [ItemId] = ?", nIndex);
            }
        }

        /// <summary>
        /// Regenerate all unused keys. 
        /// </summary>
        public void ResetPool()
        {
            lock (LockObj)
            {
                dbConn.Execute("delete from [KeyStorage] where not [IsUsed]");
                GenerateKeys(nKeyPoolSize);
            }
        }

        /// <summary>
        /// Timestamp of oldest item in keystore.
        /// </summary>
        public int OldestTime
        {
            get
            {
                var QueryTime = dbConn.Query<NumQuery>("select [nTime] from [KeyStorage] where not [IsUsed] order by [ItemId] asc limit 1");

                return QueryTime.First().Num;
            }
        }
    }
}
