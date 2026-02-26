using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class DocumentAnalysisRequest
    {
        public List<IFormFile> Documentos { get; set; }
        public string? InstruccionPersonalizada { get; set; }
    }
}
