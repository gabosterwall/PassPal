using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassPal
{
    internal class JsonVault
    {
        public byte[] Vault { get; set; }
        public byte[] IV { get; set; }

        public JsonVault(byte[] vault, byte[] iv)
        {
            Vault = vault;
            IV = iv;
        }
    }
}
