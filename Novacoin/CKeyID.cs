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
    /// Represents the key identifier. Internal state is calculated as Hash160(pubkey).
    /// </summary>
    public class CKeyID : uint160
    {
        public CKeyID() : base()
        {
        }

        public CKeyID(CKeyID KeyID) : base(KeyID)
        {
        }

        public CKeyID(uint160 pubKeyHash) : base(pubKeyHash)
        {
        }

        public CKeyID(byte[] hashBytes) : base(hashBytes)
        {
        }

        /// <summary>
        /// Generate Pay-to-PubkeyHash address
        /// </summary>
        /// <returns>Base58 formatted novacoin address</returns>
        public override string ToString()
        {
            return (new CNovacoinAddress(this)).ToString();
        }
    }
}
