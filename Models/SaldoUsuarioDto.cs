using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class SaldoUsuarioDto
    {
        public int TokensUsados { get; set; }
        public int TokensMaximos { get; set; }
        public int TokensRestantes { get; set; }
        public bool EsPro { get; set; }
    }
}
