using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Text.Encodings.Web;
using Npgsql;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using API.Models;
using System.Linq;
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Cors;

namespace API.Controllers
{
    [EnableCors]
    [Route("4meteo/[controller]")]
    public class DataController : Controller
    {
        /// <summary>
        /// Variável para o contexto da base de dados
        /// </summary>
        private readonly fourmeteoContext _context;
        /// <summary>
        /// Variável que permite aceder ao ficheiro appsettings.json
        /// </summary>
        private readonly IConfiguration config;

        /// <summary>
        /// Construtor do controller
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="context"></param>
        public DataController(IConfiguration configuration, fourmeteoContext context)
        {
            config = configuration;
            _context = context;
        }

        /// <summary>
        /// Returns all the stations
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAllStations")]
        public async Task<IActionResult> GetAllStations()
        {
            //Retorno de todas as estações na base de dados
            return Ok(await _context.Stations.ToArrayAsync());

            /*             await using var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
                        await conn.OpenAsync();
                        List<Station> stations = new List<Station>();

                        await using (var cmd = new NpgsqlCommand("SELECT stationid, latitude, longitude, name, createdby, createddate, updatedby, updateddate FROM station", conn))
                        {

                            var dataReader = cmd.ExecuteReader();
                            while (dataReader.Read())
                            {
                                stations.Add(new Station()
                                {
                                    Stationid = dataReader.GetGuid(0),
                                    Latitude = dataReader.GetDouble(1),
                                    Longitude = dataReader.GetDouble(2),
                                    Name = dataReader.GetString(3),
                                    Createdby = dataReader.GetString(4),
                                    Createdat = dataReader.GetDateTime(5),
                                    Updatedby = dataReader.GetString(6),
                                    Updatedat = dataReader.GetDateTime(7),
                                });
                            }

             */


        }

        /// <summary>
        /// Returns all records from the database
        /// </summary>
        /// <returns>Stationrecord array</returns>
        [HttpGet]
        [Route("GetAllRecords")]
        public async Task<IActionResult> GetAllRecords()
        {
            //Retorno de todas as observações da base de dados
            var data = await _context.Stationrecords.ToArrayAsync();
            return Ok(data);
        }

        /// <summary>
        /// Returns all records from a specific station
        /// </summary>
        /// <param name="uuid">Station UUID</param>
        /// <returns>Stationrecords array</returns>
        [HttpGet]
        [Route("GetRecords/{uuid}")]
        public async Task<IActionResult> GetRecords(Guid uuid)
        {
            //Expressão LINQ para procurar as todas as observações de uma determinada estações
            var data = await (from d in _context.Stationrecords where d.Stationid == uuid select d).OrderByDescending(x => x.Time).ToArrayAsync();
            return Ok(data);
        }


        //Retornar condições atuais de uma estação
        /// <summary>
        /// Returns the last record of a station
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <returns>Station record</returns>
        [HttpGet]
        [Route("[action]/{uuid}")]
        public async Task<IActionResult> GetLastRecord(Guid uuid)
        {
            //Verificar se existe alguma estação com o uuid recebido
            if (!_context.Stations.Any(x => x.Stationid == uuid)) return BadRequest(uuid);

            //Conexão á base de dados
            var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
            //Abertura da ligação
            await conn.OpenAsync();
            //Variável para os dados
            WeatherData data = new WeatherData();
            //Query
            string query = String.Format("SELECT time, temperature, windspeed, winddir, pressure, precipitation, radiation  FROM stationrecord WHERE stationid = '{0}' ORDER BY time DESC LIMIT 1", uuid);
            try
            {

                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    //Leitor para os dados recibidos da base de dados
                    var dataReader = cmd.ExecuteReader();
                    //Leitura dos dados
                    while (dataReader.Read())
                    {
                        data.Time = dataReader.IsDBNull(0) ? null : dataReader.GetDateTime(0);
                        data.Temperature = dataReader.IsDBNull(1) ? null : dataReader.GetFloat(1);
                        data.Windspeed = dataReader.IsDBNull(2) ? null : dataReader.GetFloat(2);
                        data.WindDir = dataReader.IsDBNull(3) ? null : dataReader.GetString(3);
                        data.Pressure = dataReader.IsDBNull(4) ? null : dataReader.GetFloat(4);
                        data.Precipitation = dataReader.IsDBNull(5) ? null : dataReader.GetFloat(5);
                        data.Radiation = dataReader.IsDBNull(6) ? null : dataReader.GetFloat(6);
                    }

                    //Fechar ligação á db
                    await conn.CloseAsync();
                    //Retorno dos dados
                    return Ok(data);
                }
            }
            catch (NpgsqlException ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }


        //Retornar media,maximo e minimo de temperatura, velociade do vento, pressao,humidade e precipitação durante  1 dia, 1 semana e 1 mes
        /// <summary>
        /// Returns max, average and min temperature, wind speed, pressure, humidity, precipitation from 1 day
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <param name="from">End data for the data</param>
        /// <param name="mode">Mode 1 = days, 2 = weeks, 3 = months</param>
        /// <returns>A array with a time and a corresponding value</returns>
        [HttpGet]
        [Route("[action]/{uuid}_{from}")]
        public async Task<IActionResult> GetSummary(Guid uuid, DateTime from, int mode)
        {
            //Verificar se existe alguma estação com o uuid recebido
            if (!_context.Stations.Any(x => x.Stationid == uuid)) return BadRequest(uuid);

            string m = GetMode(mode);

            if (m == null) return BadRequest("Invalid mode");

            //Conexão á base de dados
            var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
            //Abertura da ligação
            await conn.OpenAsync();
            //Variável para os dados
            SummaryData data = new SummaryData();
            //Query
            string query =
            String.Format(
                "SELECT MAX(temperature) AS maxtemp, AVG(temperature) AS avgtemp, MIN(temperature) AS mintemp, MAX(pressure) AS maxpres, AVG(pressure) AS avgpres, MIN(pressure) AS minpres, MAX(humidity) AS maxh, AVG(humidity) AS avghu, MIN(humidity) AS minhu, MAX(windspeed) AS maxwin, AVG(windspeed) AS avgwin, MIN(windspeed) AS minwin, MAX(radiation) AS maxprad, AVG(radiation) AS avgprad, MIN(radiation) AS minrad, MAX(precipitation) AS maxprec, AVG(precipitation) AS avgprec, MIN(precipitation) AS minprec FROM stationrecord WHERE time >= date '{0}' - INTERVAL '{1}' AND time < date '{2}' + INTERVAl '1 days' AND stationid ='{3}';", from.ToString("o"), m, from.ToString("o"), uuid);
            try
            {

                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    //Leitor para os dados recibidos da base de dados
                    var dataReader = cmd.ExecuteReader();
                    //Leitura dos dados
                    while (dataReader.Read())
                    {
                        data.MaxTemp = dataReader.IsDBNull(0) ? null : dataReader.GetFloat(0);
                        data.AvgTemp = dataReader.IsDBNull(1) ? null : (float)dataReader.GetDouble(1);
                        data.MinTemp = dataReader.IsDBNull(2) ? null : dataReader.GetFloat(2);

                        data.MaxPress = dataReader.IsDBNull(3) ? null : dataReader.GetFloat(3);
                        data.AvgPress = dataReader.IsDBNull(4) ? null : (float)dataReader.GetDouble(4);
                        data.MinPress = dataReader.IsDBNull(5) ? null : dataReader.GetFloat(5);

                        data.MaxHumidity = dataReader.IsDBNull(6) ? null : dataReader.GetFloat(6);
                        data.AvgHumidity = dataReader.IsDBNull(7) ? null : (float)dataReader.GetDouble(7);
                        data.MinHumidity = dataReader.IsDBNull(8) ? null : dataReader.GetFloat(8);

                        data.MaxWind = dataReader.IsDBNull(9) ? null : dataReader.GetFloat(9);
                        data.AvgWind = dataReader.IsDBNull(10) ? null : (float)dataReader.GetDouble(10);
                        data.MinWind = dataReader.IsDBNull(11) ? null : dataReader.GetFloat(11);

                        data.MaxRadiation = dataReader.IsDBNull(12) ? null : dataReader.GetFloat(12);
                        data.AvgRadiation = dataReader.IsDBNull(13) ? null : (float)dataReader.GetDouble(13);
                        data.MinRadiation = dataReader.IsDBNull(14) ? null : dataReader.GetFloat(14);

                        data.MaxPrecipitation = dataReader.IsDBNull(15) ? null : dataReader.GetFloat(15);
                        data.AvgPrecipitation = dataReader.IsDBNull(16) ? null : (float)dataReader.GetDouble(16);
                        data.MinPrecipitation = dataReader.IsDBNull(17) ? null : dataReader.GetFloat(17);

                    }
                    //Fechar ligação á db
                    await conn.CloseAsync();
                    //Retorno dos dados
                    return Ok(data);
                }
            }
            catch (NpgsqlException ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }


        }


        //solo mm/m2
        //Retornar condições atuais de uma estação--
        //Retornar media,maximo e minimo de temperatura, velociade do vento, pressao,humidade e precipitação durante  1 dia, 1 semana e 1 mes
        //Fazer as de forma a que o endpoint possa dar os dados relativos a meses, semanas ou dias
        //--
        //Dados de temperatura relativos a 1 dia, 1 semana, 1 mes --
        //Dados de velociade do vento e direção --
        //Dados da precipitação --
        //Dados da pressão --
        //Dados da radiação --

        //Dados do solo atuais --
        //Dados do solo por intervalo de tempo --
        //Retornar dados de uma estação dentro um derterminado intervalo de tempo --


        //Retornar condições atuais de uma estação
        /// <summary>
        /// Returns the last record of a station
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <returns>Station soil record</returns>
        [HttpGet]
        [Route("[action]/{uuid}")]
        public async Task<IActionResult> GetLastSoilRecord(Guid uuid)
        {
            //Verificar se existe alguma estação com o uuid recebido
            if (!_context.Stations.Any(x => x.Stationid == uuid)) return BadRequest(uuid);

            //Conexão á base de dados
            var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
            //Abertura da ligação
            await conn.OpenAsync();
            //Variável para os dados
            GraphSoil data = new GraphSoil();
            //Query
            string query = String.Format("SELECT time, soilmoisture1, soilmoisture2, soilmoisture3, soiltemperature1, soiltemperature2, soiltemperature3 FROM stationrecord WHERE stationid = '{0}' ORDER BY time DESC LIMIT 1", uuid);
            try
            {

                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    //Leitor para os dados recibidos da base de dados
                    var dataReader = cmd.ExecuteReader();
                    //Leitura dos dados
                    while (dataReader.Read())
                    {
                        data.time = dataReader.IsDBNull(0) ? null : dataReader.GetDateTime(0);
                        data.moist1 = dataReader.IsDBNull(1) ? null : (float)dataReader.GetDouble(1);
                        data.moist2 = dataReader.IsDBNull(2) ? null : (float)dataReader.GetDouble(2);
                        data.moist3 = dataReader.IsDBNull(3) ? null : (float)dataReader.GetDouble(3);
                        data.temp1 = dataReader.IsDBNull(4) ? null : (float)dataReader.GetDouble(4);
                        data.temp2 = dataReader.IsDBNull(5) ? null : (float)dataReader.GetDouble(5);
                        data.temp3 = dataReader.IsDBNull(6) ? null : (float)dataReader.GetDouble(6);
                    }
                    //Fechar ligação á db
                    await conn.CloseAsync();
                    //Retorno dos dados
                    return Ok(data);
                }
            }
            catch (NpgsqlException ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }



        /// <summary>
        /// Returns a array with a time and a corresponding time and a soil data 
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <param name="from">End data for the data</param>
        /// <param name="mode">Mode 1 = days, 2 = weeks, 3 = months </param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]/{uuid}_{from}_{mode}")]
        public async Task<IActionResult> GetSoilRecords(Guid uuid, DateTime from, int mode)
        {
            //Verificar se existe alguma estação com o uuid recebido
            if (!_context.Stations.Any(x => x.Stationid == uuid)) return BadRequest(uuid);

            string m = GetMode(mode);

            if (m == null) return BadRequest("Invalid mode");
            //Conexão á base de dados
            var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
            //Abertura da ligação
            await conn.OpenAsync();
            //Variável para os dados
            List<GraphSoil> data = new List<GraphSoil>();
            //Query
            string query =
            String.Format(
               "SELECT time, soilmoisture1, soilmoisture2, soilmoisture3, soiltemperature1, soiltemperature2, soiltemperature3 FROM stationrecord WHERE stationid = '{0}' AND time >= date '{1}' - INTERVAL '{2}' AND time < date '{3}' + INTERVAl '1 days' ORDER BY time ASC", uuid, from.ToString("o"), m, from.ToString("o"));
            try
            {

                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    //Leitor para os dados recibidos da base de dados
                    var dataReader = cmd.ExecuteReader();
                    //Leitura dos dados
                    while (dataReader.Read())
                    {
                        data.Add(
                            new GraphSoil
                            {
                                time = await dataReader.IsDBNullAsync(0) ? null : dataReader.GetDateTime(0),
                                moist1 = await dataReader.IsDBNullAsync(1) ? null : (float)dataReader.GetDouble(1),
                                moist2 = await dataReader.IsDBNullAsync(2) ? null : (float)dataReader.GetDouble(2),
                                moist3 = await dataReader.IsDBNullAsync(3) ? null : (float)dataReader.GetDouble(3),
                                temp1 = await dataReader.IsDBNullAsync(4) ? null : (float)dataReader.GetDouble(4),
                                temp2 = await dataReader.IsDBNullAsync(5) ? null : (float)dataReader.GetDouble(5),
                                temp3 = await dataReader.IsDBNullAsync(6) ? null : (float)dataReader.GetDouble(6)
                            }
                        );

                    }
                }
                //Fechar ligação á db
                await conn.CloseAsync();
                return Ok(data.ToArray());
            }
            catch (NpgsqlException ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



        /// <summary>
        /// Returns a array with a time and a corresponding windspeed adn direction
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <param name="from">End data for the data</param>
        /// <param name="mode">Mode 1 = days, 2 = weeks, 3 = months </param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]/{uuid}_{from}_{mode}")]
        public async Task<IActionResult> GetWindRecords(Guid uuid, DateTime from, int mode)
        {
            //Verificar se existe alguma estação com o uuid recebido
            if (!_context.Stations.Any(x => x.Stationid == uuid)) return BadRequest(uuid);

            string m = GetMode(mode);

            if (m == null) return BadRequest("Invalid mode");
            //Conexão á base de dados
            var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
            //Abertura da ligação
            await conn.OpenAsync();
            //Variável para os dados
            List<GraphWind> data = new List<GraphWind>();
            //Query
            string query =
            String.Format(
               "SELECT time, windspeed, winddir FROM stationrecord WHERE stationid = '{0}' AND time >= date '{1}' - INTERVAL '{2}' AND time < date '{3}' + INTERVAl '1 days' ORDER BY time ASC", uuid, from.ToString("o"), m, from.ToString("o"));
            try
            {

                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    //Leitor para os dados recibidos da base de dados
                    var dataReader = cmd.ExecuteReader();
                    //Leitura dos dados
                    while (dataReader.Read())
                    {
                        data.Add(
                            new GraphWind
                            {
                                time = await dataReader.IsDBNullAsync(0) ? null : dataReader.GetDateTime(0),
                                value = await dataReader.IsDBNullAsync(1) ? null : (float)dataReader.GetDouble(1),
                                direction = await dataReader.IsDBNullAsync(2) ? null : dataReader.GetString(2)
                            }
                        );

                    }
                }
                //Fechar ligação á db
                await conn.CloseAsync();
                //Retorno dos dados
                return Ok(data.ToArray());
            }
            catch (NpgsqlException ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



        /// <summary>
        /// Returns a array with a time and a corresponding precipitation
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <param name="from">End data for the data</param>
        /// <param name="mode">Mode 1 = days, 2 = weeks, 3 = months </param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]/{uuid}_{from}_{mode}")]
        public async Task<IActionResult> GetPrecipitationRecords(Guid uuid, DateTime from, int mode)
        {
            //Verificar se existe alguma estação com o uuid recebido
            if (!_context.Stations.Any(x => x.Stationid == uuid)) return BadRequest(uuid);

            string m = GetMode(mode);

            if (m == null) return BadRequest("Invalid mode");
            //Conexão á base de dados
            var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
            //Abertura da ligação
            await conn.OpenAsync();
            //Variável para os dados
            List<Graph> data = new List<Graph>();
            //Query
            string query =
            String.Format(
               "SELECT time, precipitation FROM stationrecord WHERE stationid = '{0}' AND time >= date '{1}' - INTERVAL '{2}' AND time < date '{3}' + INTERVAl '1 days' ORDER BY time ASC", uuid, from.ToString("o"), m, from.ToString("o"));
            try
            {

                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    //Leitor para os dados recibidos da base de dados
                    var dataReader = cmd.ExecuteReader();
                    //Leitura dos dados
                    while (dataReader.Read())
                    {
                        data.Add(
                            new Graph
                            {
                                time = await dataReader.IsDBNullAsync(0) ? null : dataReader.GetDateTime(0),
                                value = await dataReader.IsDBNullAsync(1) ? null : (float)dataReader.GetDouble(1)
                            }
                        );
                        // Graph[] p = await _context.Stationrecords.Where(r => r.Stationid == uuid).Select(a => new Graph
                        // {
                        //     time = a.Time,
                        //     value = a.Temperature
                        // }).OrderBy(a => a.time).ToArrayAsync();

                    }
                }
                //Fechar ligação á db
                await conn.CloseAsync();
                //Retorno dos dados
                return Ok(data.ToArray());
            }
            catch (NpgsqlException ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



        /// <summary>
        /// Returns a array with a time and a corresponding temperature
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <param name="from">End data for the data</param>
        /// <param name="mode">Mode 1 = days, 2 = weeks, 3 = months </param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]/{uuid}_{from}_{mode}")]
        public async Task<IActionResult> GetRadiationRecords(Guid uuid, DateTime from, int mode)
        {
            //Verificar se existe alguma estação com o uuid recebido
            if (!_context.Stations.Any(x => x.Stationid == uuid)) return BadRequest(uuid);

            string m = GetMode(mode);

            if (m == null) return BadRequest("Invalid mode");
            //Conexão á base de dados
            var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
            //Abertura da ligação
            await conn.OpenAsync();
            //Variável para os dados
            List<Graph> data = new List<Graph>();
            //Query
            string query =
            String.Format(
               "SELECT time, radiation FROM stationrecord WHERE stationid = '{0}' AND time >= date '{1}' - INTERVAL '{2}' AND time < date '{3}' + INTERVAl '1 days' ORDER BY time ASC", uuid, from.ToString("o"), m, from.ToString("o"));
            try
            {

                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    //Leitor para os dados recibidos da base de dados
                    var dataReader = cmd.ExecuteReader();
                    //Leitura dos dados
                    while (dataReader.Read())
                    {
                        data.Add(
                            new Graph
                            {
                                time = await dataReader.IsDBNullAsync(0) ? null : dataReader.GetDateTime(0),
                                value = await dataReader.IsDBNullAsync(1) ? null : (float)dataReader.GetDouble(1)
                            }
                        );
                        // Graph[] p = await _context.Stationrecords.Where(r => r.Stationid == uuid).Select(a => new Graph
                        // {
                        //     time = a.Time,
                        //     value = a.Temperature
                        // }).OrderBy(a => a.time).ToArrayAsync();

                    }
                }
                //Fechar ligação á db
                await conn.CloseAsync();
                //Retorno dos dados
                return Ok(data.ToArray());
            }
            catch (NpgsqlException ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }




        /// <summary>
        /// Returns a array with a time and a corresponding humidity
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <param name="from">End data for the data</param>
        /// <param name="mode">Mode 1 = days, 2 = weeks, 3 = months </param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]/{uuid}_{from}_{mode}")]
        public async Task<IActionResult> GetHumidityRecords(Guid uuid, DateTime from, int mode)
        {
            //Verificar se existe alguma estação com o uuid recebido
            if (!_context.Stations.Any(x => x.Stationid == uuid)) return BadRequest(uuid);

            string m = GetMode(mode);

            if (m == null) return BadRequest("Invalid mode");
            //Conexão á base de dados
            var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
            //Abertura da ligação
            await conn.OpenAsync();
            //Variável para os dados
            List<Graph> data = new List<Graph>();
            //Query
            string query =
            String.Format(
               "SELECT time, humidity FROM stationrecord WHERE stationid = '{0}' AND time >= date '{1}' - INTERVAL '{2}' AND time < date '{3}' + INTERVAl '1 days' ORDER BY time ASC", uuid, from.ToString("o"), m, from.ToString("o"));
            try
            {

                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    //Leitor para os dados recibidos da base de dados
                    var dataReader = cmd.ExecuteReader();
                    //Leitura dos dados
                    while (dataReader.Read())
                    {
                        data.Add(
                            new Graph
                            {
                                time = await dataReader.IsDBNullAsync(0) ? null : dataReader.GetDateTime(0),
                                value = await dataReader.IsDBNullAsync(1) ? null : (float)dataReader.GetDouble(1)
                            }
                        );
                        // Graph[] p = await _context.Stationrecords.Where(r => r.Stationid == uuid).Select(a => new Graph
                        // {
                        //     time = a.Time,
                        //     value = a.Temperature
                        // }).OrderBy(a => a.time).ToArrayAsync();

                    }
                }
                //Fechar ligação á db
                await conn.CloseAsync();
                //Retorno dos dados
                return Ok(data.ToArray());
            }
            catch (NpgsqlException ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



        /// <summary>
        /// Returns a array with a time and a corresponding pressure
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <param name="from">End data for the data</param>
        /// <param name="mode">Mode 1 = days, 2 = weeks, 3 = months </param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]/{uuid}_{from}_{mode}")]
        public async Task<IActionResult> GetPressureRecords(Guid uuid, DateTime from, int mode)
        {
            //Verificar se existe alguma estação com o uuid recebido
            if (!_context.Stations.Any(x => x.Stationid == uuid)) return BadRequest(uuid);

            string m = GetMode(mode);

            if (m == null) return BadRequest("Invalid mode");
            //Conexão á base de dados
            var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
            //Abertura da ligação
            await conn.OpenAsync();
            //Variável para os dados
            List<Graph> data = new List<Graph>();
            //Query
            string query =
            String.Format(
               "SELECT time, pressure FROM stationrecord WHERE stationid = '{0}' AND time >= date '{1}' - INTERVAL '{2}' AND time < date '{3}' + INTERVAl '1 days' ORDER BY time ASC", uuid, from.ToString("o"), m, from.ToString("o"));
            try
            {

                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    //Leitor para os dados recibidos da base de dados
                    var dataReader = cmd.ExecuteReader();
                    //Leitura dos dados
                    while (dataReader.Read())
                    {
                        data.Add(
                            new Graph
                            {
                                time = await dataReader.IsDBNullAsync(0) ? null : dataReader.GetDateTime(0),
                                value = await dataReader.IsDBNullAsync(1) ? null : (float)dataReader.GetDouble(1)
                            }
                        );
                        // Graph[] p = await _context.Stationrecords.Where(r => r.Stationid == uuid).Select(a => new Graph
                        // {
                        //     time = a.Time,
                        //     value = a.Temperature
                        // }).OrderBy(a => a.time).ToArrayAsync();

                    }
                }
                //Fechar ligação á db
                await conn.CloseAsync();
                //Retorno dos dados
                return Ok(data.ToArray());
            }
            catch (NpgsqlException ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="from"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]/{uuid}_{from}_{mode}")]
        public async Task<IActionResult> GetTempRecords(Guid uuid, DateTime from, int mode)
        {
        }


        /// <summary>
        /// Returns a array with a time and a corresponding temperature
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <param name="from">End data for the data</param>
        /// <param name="mode">Mode 1 = days, 2 = weeks, 3 = months </param>
        /// <returns></returns>
        [HttpGet]
        [Route("[action]/{uuid}_{from}_{mode}")]
        public async Task<IActionResult> GetTempRecords(Guid uuid, DateTime from, int mode)
        {
            //Verificar se existe alguma estação com o uuid recebido
            if (!_context.Stations.Any(x => x.Stationid == uuid)) return BadRequest(uuid);

            string m = GetMode(mode);

            if (m == null) return BadRequest("Invalid mode");
            //Conexão á base de dados
            var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
            //Abertura da ligação
            await conn.OpenAsync();
            //Variável para os dados
            List<Graph> data = new List<Graph>();
            //Query
            string query =
            String.Format(
               "SELECT time, temperature FROM stationrecord WHERE stationid = '{0}' AND time >= date '{1}' - INTERVAL '{2}' AND time < date '{3}' + INTERVAl '1 days'  ORDER BY time ASC", uuid, from.ToString("o"), m, from.ToString("o"));
            try
            {

                await using (var cmd = new NpgsqlCommand(query, conn))
                {
                    //Leitor para os dados recibidos da base de dados
                    var dataReader = cmd.ExecuteReader();
                    //Leitura dos dados
                    while (dataReader.Read())
                    {
                        data.Add(
                            new Graph
                            {
                                time = await dataReader.IsDBNullAsync(0) ? null : dataReader.GetDateTime(0),
                                value = await dataReader.IsDBNullAsync(1) ? null : (float)dataReader.GetDouble(1)
                            }
                        );
                        // Graph[] p = await _context.Stationrecords.Where(r => r.Stationid == uuid).Select(a => new Graph
                        // {
                        //     time = a.Time,
                        //     value = a.Temperature
                        // }).OrderBy(a => a.time).ToArrayAsync();

                    }
                }
                //Fechar ligação á db
                await conn.CloseAsync();
                //Retorno dos dados
                return Ok(data.ToArray());
            }
            catch (NpgsqlException ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            catch (Exception ex)
            {
                //Fechar ligação á db
                await conn.CloseAsync();
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        /// <summary>
        /// Returns the records from a station during a time interval
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <param name="from">Start date</param>
        /// <param name="to">End date</param>
        /// <returns>Stationrecords Array</returns>
        [HttpGet]
        [Route("GetRecords/{uuid}/{from}_until_{to}")]
        public async Task<IActionResult> GetRecords(Guid uuid, DateTime from, DateTime to)
        {
            //Query para procurar todas as obsevações dentro de um determinado intervalo de tempo de uma estação
            var data = await _context.Stationrecords.FromSqlRaw(
                "SELECT * FROM stationrecord WHERE time > {0} AND time < {1} AND stationid = {2}", from, to, uuid).ToArrayAsync();
            return Ok(data);
        }

        /// <summary>
        /// Returns the records from a station during a time interval in a CSV file
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <param name="from">Start date</param>
        /// <param name="to">End date</param>
        /// <returns>Stationrecords Array</returns>
        [HttpGet]
        [Route("GetRecordsCSV/{uuid}/{from}_until_{to}")]
        public async Task<IActionResult> GetRecordsCSV(Guid uuid, DateTime from, DateTime to)
        {
            //Query para procurar todas as obsevações dentro de um determinado intervalo de tempo de uma estação
            var data = await _context.Stationrecords.FromSqlRaw(
                "SELECT * FROM stationrecord WHERE time > {0} AND time < {1} AND stationid = {2}", from, to, uuid).ToArrayAsync();


            string csv = "stationid;time;temperature;humidity;windspeed;winddir;pressure;precipitation;radiation;leafwetness;soilmoisture1;soilmoisture2;soilmoisture3;soiltemperature1;soiltemperature2;soiltemperature3;customd1;customd2;customd3;customd4;customd5;customt1;customt2;customt3;customt4;customt5\n";


            foreach (Stationrecord r in data)
            {
                string aux = r.Stationid.ToString() + ";" + r.Time.ToString() + ";" + r.Temperature + ";" + r.Humidity + ";" + r.Windspeed + ";" + r.Winddir + ";" + r.Pressure + ";" + r.Precipitation + ";" + r.Radiation + ";" + r.Leafwetness + ";" + r.Soilmoisture1 + ";" + r.Soilmoisture2 + ";" + r.Soilmoisture3 + ";" + r.Soiltemperature1 + ";" + r.Soiltemperature2 + ";" + r.Soiltemperature3 + ";" + r.Customd1 + ";" + r.Customd2 + ";" + r.Customd3 + ";" + r.Customd4 + ";" + r.Customd5 + ";" + r.Customt1 + ";" + r.Customt2 + ";" + r.Customt3 + ";" + r.Customt4 + ";" + r.Customt5 + "\n";
                csv = csv + aux;
            }


            return Ok(csv);
        }



        /// <summary>
        /// Returns the moving average of temperature of a determined station during a specified time interval
        /// </summary>
        /// <param name="uuid">Station UUID</param>
        /// <param name="fr">Start date</param>
        /// <param name="to">End date</param>
        /// <returns>Returns an array with a time index and the temperature</returns>
        [HttpGet]
        [Route("GetMovingTempAvg/{uuid}/{fr}_until_{to}")]
        public async Task<IActionResult> GetMovingTempAvg(Guid uuid, DateTime fr, DateTime to)
        {
            var conn = new NpgsqlConnection(config.GetConnectionString("4meteodb"));
            await conn.OpenAsync();
            Dictionary<DateTime, double> data = new Dictionary<DateTime, double>();
            string query = String.Format("SELECT time, AVG(temperature) AS average  FROM stationrecord WHERE stationid = '{0}' AND time > '{1}' AND time < '{2}' GROUP BY time", uuid, fr.ToString("o"), to.ToString("o"));
            await using (var cmd = new NpgsqlCommand(query, conn))
            {

                var dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    data.Add(dataReader.GetDateTime(0), dataReader.GetDouble(1));
                }

                // var p = await _context.Stationrecords.Where(r => r.Time.CompareTo(fr) > 0 && r.Time.CompareTo(to) < 0 && r.Stationid == uuid).GroupBy(a => a.Time, t => t.Temperature).Select(
                //     a => new
                //     {
                //         time = a.Key,
                //         temp = a.Average(),
                //     }
                // ).OrderByDescending(r => r.time).ToArrayAsync();

                //Fechar ligação á db
                await conn.CloseAsync();
                //Retorno dos dados
                return Ok(data);

            }
        }

        /// <summary>
        /// Returns the properties of a station
        /// </summary>
        /// <param name="uuid">Station uuid</param>
        /// <returns>Station</returns>
        [HttpGet]
        [Route("GetStation/{uuid}")]
        public async Task<IActionResult> GetStation(Guid uuid)
        {
            //Procurar as propriedades de uma estação através do uuid
            var data = await _context.Stations.FindAsync(uuid);
            return Ok(data);
        }

        /// <summary>
        /// Modos do intervalo
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        private string GetMode(int mode)
        {
            switch (mode)
            {
                case 1:
                    return "0 days";
                case 2:
                    return "1 weeks";
                case 3:
                    return "1 months";
            }

            return null;
        }

    }

}