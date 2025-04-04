using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Taller2ServiciosWeb.Clases;

namespace Taller2ServiciosWeb.Controllers
{
    [RoutePrefix("api/Uploads")]
    public class UploadsController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> CargarArchivo(HttpRequestMessage request, string Datos, string Proceso)
        {
            clsUpload upload = new clsUpload();
            upload.Datos = Datos;
            upload.Proceso = Proceso;
            upload.request = request;
            return await upload.GrabarArchivo(false);
        }

        [HttpPut]
        [Route("Actualizar")]
        
        public async Task<HttpResponseMessage> ActualizarArchivo(HttpRequestMessage request)
        {
            clsUpload upload = new clsUpload();
            upload.request = request;                    
            return await upload.GrabarArchivo(true);
        }

        [HttpDelete]
        [Route("Eliminar")]
        public HttpResponseMessage EliminarArchivo(string NombreImagen)
        {
            clsUpload upload = new clsUpload();
            return upload.EliminarArchivo(NombreImagen);
        }
    }
}