using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using Taller2ServiciosWeb.Clases;
using Taller2ServiciosWeb.Models;
using static Taller2ServiciosWeb.Clases.clsPesaje;

namespace Taller2ServiciosWeb.Controllers
{
    [RoutePrefix("api/Pesajes")]
    public class PesajesController : ApiController
    {

        public class PesajeRequest
        {
            public Camion camion { get; set; }
            public Pesaje pesaje { get; set; }
        }
        [HttpPost]
        [Route("Registrar")]
        public string Registrar([FromBody] PesajeRequest request)
        {
            clsPesaje clsPesaje = new clsPesaje();
            clsPesaje.pesaje = request.pesaje;
            clsPesaje.camion = request.camion;
            return clsPesaje.RegistrarPesaje();
        }


        [HttpGet]
        [Route("ConsultarPorPlaca")]
        public List<Pesaje> ConsultarPorPlaca(string placa)
        {
            clsPesaje Pesaje = new clsPesaje();
            return Pesaje.ConsultarPorPlaca(placa);
        }

        [HttpGet]
        [Route("ListarPesajesConImagenes")]
        public IQueryable ListarPesajesConImagenes(string placa)
        {
            clsPesaje pesaje = new clsPesaje();
            return pesaje.ListarPesajes(placa);
        }


    }
}