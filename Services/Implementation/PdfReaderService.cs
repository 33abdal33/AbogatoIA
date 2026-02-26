using Microsoft.AspNetCore.Http;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig;

namespace Services.Implementation
{ 

        public class PdfReaderService : IPdfReaderService
        {
            public async Task<string> ExtraerTextoDeMultiplesPdfsAsync(List<IFormFile> documentos)
            {
                var textoCompleto = new StringBuilder();
                int contador = 1;

                foreach (var archivo in documentos)
                {
                    // Solo procesamos si es un PDF y tiene contenido
                    if (archivo.Length > 0 && archivo.ContentType == "application/pdf")
                    {
                        textoCompleto.AppendLine($"\n--- INICIO DEL DOCUMENTO {contador}: {archivo.FileName} ---");

                        // Leemos el archivo en memoria RAM
                        using var stream = new MemoryStream();
                        await archivo.CopyToAsync(stream);
                        stream.Position = 0;

                        try
                        {
                            // PdfPig abre el documento
                            using (PdfDocument document = PdfDocument.Open(stream))
                            {
                                // Recorre cada página y extrae las letras
                                foreach (var page in document.GetPages())
                                {
                                    textoCompleto.Append(page.Text + " ");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            textoCompleto.AppendLine($"[Error al leer el archivo {archivo.FileName}: {ex.Message}]");
                        }

                        textoCompleto.AppendLine($"\n--- FIN DEL DOCUMENTO {contador} ---");
                        contador++;
                    }
                }

                return textoCompleto.ToString();
            }
        }
}
