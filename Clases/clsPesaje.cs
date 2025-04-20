using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using System.Web.Http;
using Taller2ServiciosWeb.Models;
using static System.Web.Razor.Parser.SyntaxConstants;

namespace Taller2ServiciosWeb.Clases
{
    public class clsPesaje
    {

        private DBExamenEntities dbPesaje = new DBExamenEntities();
        public Pesaje pesaje { get; set; }
        public Camion camion { get; set; }

        public string RegistrarPesaje()
        {
            try
            {
                
                var cam = dbPesaje.Camions.FirstOrDefault(c => c.Placa == camion.Placa);
                if (cam == null)
                {
                    dbPesaje.Camions.Add(camion);
                    dbPesaje.SaveChanges();
                }

                dbPesaje.Pesajes.Add(pesaje);
                dbPesaje.SaveChanges();
                return "Pesaje registrado correctamente";
            }
            catch (Exception ex)
            {
                return "Error al registrar el pesaje, no existe el camión, por favor registrelo: " + ex.Message;
            }

        }

        public List<Pesaje> ConsultarPorPlaca(string placa)
        {
            return dbPesaje.Pesajes.Where(p => p.PlacaCamion == placa).ToList();
        }

        public string GrabarImagenPesaje(int idPesaje, List<string> Imagenes)
        {
            try
            {
                foreach (string imagen in Imagenes)
                {
                    FotoPesaje nuevaFoto = new FotoPesaje
                    {
                        idPesaje = idPesaje,
                        ImagenVehiculo = imagen
                    };

                    dbPesaje.FotoPesajes.Add(nuevaFoto);
                }

                dbPesaje.SaveChanges();

                return "Se grabó la información en la base de datos";
            }
            catch (Exception ex)
            {
                return "Error al guardar las imágenes: " + ex.Message;
            }
        }

        public IQueryable ListarPesajes(string placa)
        {
            var consulta = from p in dbPesaje.Set<Pesaje>()
                           join c in dbPesaje.Set<Camion>() on p.PlacaCamion equals c.Placa
                           join f in dbPesaje.Set<FotoPesaje>() on p.id equals f.idPesaje
                           where c.Placa == placa
                           orderby p.FechaPesaje
                           select new
                           {
                               Placa = c.Placa,
                               Marca = c.Marca,
                               NumeroEjes = c.NumeroEjes,
                               FechaPesaje = p.FechaPesaje,
                               Peso = p.Peso,
                               Estacion = p.Estacion,
                               NombreImagen = f.ImagenVehiculo
                           };

            return consulta;
        }
        public string EliminarImagenPesaje(string NombreImagen)
        {
            try
            {
                FotoPesaje foto = dbPesaje.FotoPesajes.FirstOrDefault(f => f.ImagenVehiculo == NombreImagen);
                if (foto != null)
                {
                    dbPesaje.FotoPesajes.Remove(foto);
                    dbPesaje.SaveChanges();
                    return "Registro eliminado de la base de datos.";
                }
                else
                {
                    return "No se encontró la imagen en la base de datos.";
                }
            }
            catch (Exception ex)
            {
                return "Error al eliminar registro de la base de datos: " + ex.Message;
            }
        }

    }
}

