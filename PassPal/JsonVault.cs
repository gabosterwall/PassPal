using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassPal
{
    public class JsonVault  //Detta är objeket som ska innehålla en dictionary<string,string>, dvs "valvet", plus det generarade IV:t
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
