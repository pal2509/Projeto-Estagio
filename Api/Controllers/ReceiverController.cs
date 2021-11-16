using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;
using System.Text.Encodings.Web;
using System.IO;
using Npgsql;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Http;
using System;
using System.Web;

namespace API.Controllers
{
    /// <summary>
    /// Controller to receive data from stations
    /// </summary>
    [Route("4meteo/[controller]")]
    public class ReceiverController : Controller
    {
        private readonly fourmeteoContext _context;

        public ReceiverController(fourmeteoContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Allows a Stationrecord insert to the database
        /// </summary>
        /// <param name="data">Stationrecord object</param>
        /// <returns></returns>
        [HttpPost]
        [Route("InsertRecord")]
        public async Task<IActionResult> InsertRecord([FromBody] Stationrecord data)
        {
            //Verifica se o modelo está correto
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Inserir os dados na base de dados
                var response = await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO stationrecord VALUES({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25})"
                    , data.Stationid, data.Time, data.Temperature, data.Humidity, data.Windspeed, data.Winddir, data.Pressure, data.Precipitation, data.Radiation, data.Leafwetness, data.Soilmoisture1, data.Soilmoisture2, data.Soilmoisture3, data.Soiltemperature1, data.Soiltemperature2, data.Soiltemperature3, data.Customd1, data.Customd1, data.Customd2, data.Customd3, data.Customd4, data.Customd5, data.Customt1, data.Customt2, data.Customt3, data.Customt4, data.Customt5);

                //Verificar a resposta da base de dados
                if (response == 1) return CreatedAtAction("Insert Record", data);
                else return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
            catch (DbUpdateException ex)
            {
                throw new BadHttpRequestException("Ooopss errro!!!", 500, ex);

            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException("Ooopss errro!!!", 500, ex);
            }
        }

        /// <summary>
        /// Allows a Stationrecord insert to the database
        /// </summary>
        /// <param name="station">Stationrecord object</param>
        /// <returns></returns>
        [HttpPost]
        [Route("InsertStation")]
        public async Task<IActionResult> InsertStation([FromBody] newStation station)
        {
            //Verficar se o modelo está correto
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                //Criação da estação
                Station aux = new Station();
                aux.Createdby = station.Createdby;
                aux.Latitude = station.Latitude;
                aux.Longitude = station.Longitude;
                aux.Name = station.Name;
                aux.Updatedby = station.Createdby;
                aux.Createdat = DateTime.Now;
                aux.Updatedat = DateTime.Now;
                aux.Tempunit = station.Tempunit;
                aux.Precipitationunit = station.Precipitationunit;
                aux.Pressureunit = station.Pressureunit;
                aux.Radiationunit = station.Radiationunit;
                aux.Soilmoistunit = station.Soilmoistunit;
                aux.Soiltempunit = station.Soiltempunit;
                aux.Windspeedunit = station.Windspeedunit;
                aux.Leafwetnessunit = station.Leafwetnessunit;


                //Adicionar a estação á base dados
                _context.Stations.Add(aux);
                //Guardar as alterações
                await _context.SaveChangesAsync();

                //Retorno do resultado
                return Ok(aux);

            }
            catch (DbUpdateException ex)
            {
                throw new BadHttpRequestException("Ooopss errro!!!", 500, ex);

            }
            catch (Exception ex)
            {
                throw new BadHttpRequestException("Ooopss errro!!!", 500, ex);
            }
        }
    }
}
