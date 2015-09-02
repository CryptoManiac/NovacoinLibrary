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

namespace Novacoin
{
    /// <summary>
    /// Represents the script identifier. Internal value is calculated as Hash160(script).
    /// </summary>
    public class CScriptID : uint160
    {
        #region Constructors
        public CScriptID() : base()
        {
        }

        public CScriptID(CScriptID KeyID) : base(KeyID as uint160)
        {
        }

        public CScriptID(uint160 pubKeyHash) : base(pubKeyHash)
        {
        }

        public CScriptID(byte[] hashBytes) : base(hashBytes)
        {
        }
        #endregion

        /// <summary>
        /// Generate Pay-to-ScriptHash address
        /// </summary>
        /// <returns>Base58 formatted novacoin address</returns>
        public override string ToString()
        {
            return (new CNovacoinAddress(this)).ToString();
        }

    }
}
