using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Taller2ServiciosWeb.Clases
{
    public class clsUpload
    {
        public string Datos { get; set; } 
        public string Proceso { get; set; } 
        public HttpRequestMessage request { get; set; }

        private List<string> ArchivosImagenes;
        public async Task<HttpResponseMessage> GrabarArchivo(bool Actualizar)
        {
            if (!request.Content.IsMimeMultipartContent())
            {
                return request.CreateErrorResponse(HttpStatusCode.InternalServerError, "No se envió un archivo para procesar");
            }

            string root = HttpContext.Current.Server.MapPath("~/Archivosimagenes");
            var provider = new MultipartFormDataStreamProvider(root);

            try
            {
                await request.Content.ReadAsMultipartAsync(provider);
                if (provider.FileData.Count == 0)
                {
                    return request.CreateErrorResponse(HttpStatusCode.BadRequest, "No se encontró ningún archivo en la solicitud.");
                }

                ArchivosImagenes = new List<string>();
                bool archivoExistente = false;

                foreach (MultipartFileData file in provider.FileData)
                {
                    string fileName = file.Headers.ContentDisposition.FileName?.Trim('"') ?? "archivo_sin_nombre";
                    fileName = Path.GetFileName(fileName);

                    string pathDestino = Path.Combine(root, fileName);
                    string archivoTemporal = file.LocalFileName;

                    if (!File.Exists(archivoTemporal))
                    {
                        return request.CreateErrorResponse(HttpStatusCode.InternalServerError, "El archivo temporal no fue creado correctamente.");
                    }

                    if (File.Exists(pathDestino))
                    {
                        if (Actualizar)
                        {
                            File.Delete(pathDestino);
                            File.Move(archivoTemporal, pathDestino);
                            return request.CreateResponse(HttpStatusCode.OK, "Se actualizó la imagen.");
                        }
                        else
                        {
                            File.Delete(archivoTemporal); 
                            archivoExistente = true;
                            break;
                        }
                    }
                    else
                    {
                        if (Actualizar)
                        {
                            File.Delete(archivoTemporal); 
                            return request.CreateErrorResponse(HttpStatusCode.NotFound, "El archivo no existe para actualizar.");
                        }
                        else
                        {
                            File.Move(archivoTemporal, pathDestino);
                            ArchivosImagenes.Add(fileName);
                        }
                    }
                }

                if (archivoExistente)
                {
                    return request.CreateErrorResponse(HttpStatusCode.Conflict, "El archivo ya existe.");
                }

                string respuestaBD = ProcesarBD();
                return request.CreateResponse(HttpStatusCode.OK, "Se cargaron los archivos en el servidor. " + respuestaBD);
            }
            catch (Exception ex)
            {
                return request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        public HttpResponseMessage DescargarArchivo(string Imagen)
        {
            try
            {
                string Ruta = HttpContext.Current.Server.MapPath("~/ArchivosImagenes");
                string Archivo = Path.Combine(Ruta, Imagen);

                if (File.Exists(Archivo))
                {
                    HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                    var stream = new FileStream(Archivo, FileMode.Open);
                    response.Content = new StreamContent(stream);
                    response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                    response.Content.Headers.ContentDisposition.FileName = Imagen;
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    return response;
                }
                else
                {
                    return request.CreateErrorResponse(System.Net.HttpStatusCode.NoContent, "No se encontró el archivo");
                }
            }
            catch (Exception ex)
            {
                return request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public HttpResponseMessage EliminarArchivo(string NombreArchivo)
        {
            try
            {
                string Ruta = HttpContext.Current.Server.MapPath("~/ArchivosImagenes");
                string Archivo = Path.Combine(Ruta, NombreArchivo);

                // Primero eliminar en BD
                clsPesaje pesaje = new clsPesaje();
                string resultadoBD = pesaje.EliminarImagenPesaje(NombreArchivo);

                // Luego eliminar en disco si existe
                if (File.Exists(Archivo))
                {
                    File.Delete(Archivo);
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Archivo eliminado correctamente. " + resultadoBD)
                };
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("Error al eliminar el archivo: " + ex.Message)
                };
            }
        }

        private string ProcesarBD()
        {
            switch (Proceso.ToUpper())
            {
                case "PESAJE":
                    clsPesaje pesaje = new clsPesaje();
                    return pesaje.GrabarImagenPesaje(Convert.ToInt32(Datos), ArchivosImagenes);
                default:
                    return "No se ha definido el proceso en la base de datos";
            }
        }
    }
}