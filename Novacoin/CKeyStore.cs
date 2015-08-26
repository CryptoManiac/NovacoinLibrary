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
    /// </summary>
    class CountQuery
    {
        public int Count { get; set; }
    }

    public class CKeyStore
    {
        private object LockObj = new object();
        private SQLiteConnection dbConn = null;

        public CKeyStore(string strDatabasePath="KeyStore.db")
        {
            bool firstInit = File.Exists(strDatabasePath);

            dbConn = new SQLiteConnection(new SQLitePlatformGeneric(), strDatabasePath);

            if (!firstInit)
            {
                lock(LockObj)
                {
                    dbConn.CreateTable<KeyStorageItem>(CreateFlags.AutoIncPK);
                    dbConn.CreateTable<ScriptStorageItem>(CreateFlags.AutoIncPK);

                    // Generate keys
                    for (int i = 0; i < 1000; i++)
                    {
                        var keyPair = new CKeyPair();

                        var res = dbConn.Insert(new KeyStorageItem()
                        {
                            KeyID = keyPair.KeyID.hashBytes,
                            PublicKey = keyPair.PublicBytes,
                            PrivateKey = keyPair.SecretBytes,
                            IsCompressed = keyPair.IsCompressed,
                            IsUsed = false
                        });

                        // TODO: Additional initialization
                    }
                }
            }
        }

        ~CKeyStore()
        {
            if (dbConn != null)
            {
                dbConn.Close();
                dbConn = null;
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
                    KeyID = keyPair.KeyID.hashBytes,
                    PublicKey = keyPair.PublicBytes,
                    PrivateKey = keyPair.SecretBytes,
                    IsCompressed = keyPair.IsCompressed,
                    IsUsed = true
                });

                if (res == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool HaveKey(CKeyID keyID)
        {
            var QueryCount = dbConn.Query<CountQuery>("select count([ItemID]) as [Count] from [KeyStorage] where [KeyID] = ?", keyID.hashBytes);

            return QueryCount.First().Count == 1;
        }

        /// <summary>
        /// Get the key pair object.
        /// </summary>
        /// <param name="keyID">Hash of public key.</param>
        /// <param name="keyPair">Instance of CKeyPair or null.</param>
        /// <returns>Result</returns>
        public bool GetKey(CKeyID keyID, out CKeyPair keyPair)
        {
            var QueryGet = dbConn.Query<KeyStorageItem>("select * from [KeyStorage] where [KeyID] = ?", keyID.hashBytes);

            if (QueryGet.Count() == 1)
            {
                keyPair = new CKeyPair(QueryGet.First().PrivateKey);
                return true;
            }

            keyPair = null;
            return false;
        }

        public bool AddScript(CScript script)
        {
            lock (LockObj)
            {
                var res = dbConn.Insert(new ScriptStorageItem()
                {
                    ScriptID = script.ScriptID.hashBytes,
                    ScriptCode = script.Bytes
                });

                if (res == 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool HaveScript(CScriptID scriptID)
        {
            var QueryGet = dbConn.Query<CountQuery>("select count([ItemID]) from [ScriptStorage] where [ScriptID] = ?", scriptID.hashBytes);

            return QueryGet.First().Count == 1;
        }

        public bool GetScript(CScriptID scriptID, out CScript script)
        {
            var QueryGet = dbConn.Query<ScriptStorageItem>("select * from [ScriptStorage] where [ScriptID] = ?", scriptID.hashBytes);

            if (QueryGet.Count() == 1)
            {
                script = new CScript(QueryGet.First().ScriptCode);
                return true;
            }

            script = null;
            return false;
        }


    }
}
