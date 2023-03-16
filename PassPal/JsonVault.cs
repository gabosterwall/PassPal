using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassPal
{
    public class JsonVault  //The object to be stored in server, which stores the encrypted dictionary<string,string> and the unencrypted, randomly generated IV
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
