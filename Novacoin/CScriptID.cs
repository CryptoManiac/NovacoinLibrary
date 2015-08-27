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

using System;
using System.Diagnostics.Contracts;

namespace Novacoin
{
    public class CScriptID : Hash160
    {
        public CScriptID(Hash160 scriptHash)
        {
            _hashBytes = scriptHash;
        }

        internal CScriptID(byte[] hashBytes)
        {
            Contract.Requires<ArgumentException>(hashBytes.Length == 20, "Your data doesn't seem like a hash160 of some value.");

            _hashBytes = hashBytes;
        }

        public override string ToString()
        {
            return (new CNovacoinAddress(this)).ToString();
        }

    }
}
