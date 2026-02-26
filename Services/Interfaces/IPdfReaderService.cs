using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IPdfReaderService
    {
        Task<string> ExtraerTextoDeMultiplesPdfsAsync(List<IFormFile> documentos);
    }
}
