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
                return request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, "No se envió un archivo para procesar");
            }
            string root = HttpContext.Current.Server.MapPath("~/Archivosimagenes");
            var provider = new MultipartFormDataStreamProvider(root);
            try
            {
                bool Existe = false;
                //Lee el contenido de los archivos
                await request.Content.ReadAsMultipartAsync(provider);
                if (provider.FileData.Count > 0)
                {
                    ArchivosImagenes = new List<string>();
                    foreach (MultipartFileData file in provider.FileData)
                    {
                        string fileName = file.Headers.ContentDisposition.FileName;
                        if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                        {
                            fileName = fileName.Trim('"');
                        }
                        if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                        {
                            fileName = Path.GetFileName(fileName);
                        }
                        if (File.Exists(Path.Combine(root, fileName)))
                        {
                            if (Actualizar)
                            {
                                //El archivo ya existe en el servidor, no se va a cargar, se va a eliminar el temporal y se devolverá un error
                                File.Delete(Path.Combine(root, fileName));
                                //actualiza el nombre del primer archivo
                                File.Move(file.LocalFileName, Path.Combine(root, fileName));
                                return request.CreateResponse(System.Net.HttpStatusCode.OK, "Se actualizó la imagen");
                            }
                            else
                            {
                                //El archivo ya existe en el servidor, no se va a cargar, se va a eliminar el temporal y se devolverá un error
                                File.Delete(Path.Combine(root, file.LocalFileName));
                                Existe = true;
                            }
                            return request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, "El archivo ya existe");
                        }
                        else
                        {
                            if (!Actualizar)
                            {
                                Existe = false;
                                //Agrego en una lista el nombre de los archivos que se cargaron 
                                ArchivosImagenes.Add(fileName);
                                //Renombra el archivo temporal
                                File.Move(file.LocalFileName, Path.Combine(root, fileName));
                            }
                            else
                            {
                                return request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, "El archivo no existe, se debe agregar");
                            }
                        }
                    }
                    if (!Existe)
                    {
                        //Se genera el proceso de gestión en la base de datos
                        string RptaBD = ProcesarBD();
                        //Termina el ciclo, responde que se cargó el archivo correctamente
                        return request.CreateResponse(System.Net.HttpStatusCode.OK, "Se cargaron los archivos en el servidor, " + RptaBD);
                    }
                    else
                    {
                        return request.CreateErrorResponse(System.Net.HttpStatusCode.Conflict, "El archivo ya existe");
                    }
                }
                else
                {
                    return request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, "No se envió un archivo para procesar");
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

                if (File.Exists(Archivo))
                {
                    File.Delete(Archivo);
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("Archivo eliminado correctamente")
                    };
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = new StringContent("Archivo no encontrado")
                    };
                }
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