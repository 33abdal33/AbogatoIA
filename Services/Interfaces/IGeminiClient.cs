using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IGeminiClient
    {
        Task<string> ConsultarAsync(string prompt, string modelo = "gemini-2.5-flash");
    }
}
