using Dapper;
using DevCars.API.Entities;
using DevCars.API.InputModels;
using DevCars.API.Persistence;
using DevCars.API.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace DevCars.API.Controllers
{
    [Route("api/cars")]
    public class CarsController : ControllerBase
    {
        private readonly DevCarsDbContext _dbContext;
        private readonly string _connectionString;
        public CarsController(DevCarsDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _connectionString = configuration.GetConnectionString("DevCarsCs");
        }
        [HttpGet]
        public IActionResult Get()
        {
            //    var cars = _dbContext.Cars;

            //    var carsViewModel = cars
            //        .Where(c => c.Status == CarStatusEnum.Available)
            //        .Select(c => new CarItemViewModel(c.Id, c.Brand, c.Model, c.Price))
            //        .ToList();
            using (var sqlConnection = new SqlConnection(_connectionString))
            {
                var query = "SELECT Id,Brand,Model,Price FROM Cars WHERE Status = 0";
                var carsViewModel = sqlConnection.Query<CarItemViewModel>(query);

                return Ok(carsViewModel);
            }
            
        }

        //api/cars/1
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var car = _dbContext.Cars.SingleOrDefault(c => c.Id == id);

            if (car == null)
            {
                return NotFound();
            }

            var carDetailsViewModel = new CarDetailsViewModel(
                car.Id,
                car.Brand,
                car.Model,
                car.VinCode,
                car.Year,
                car.Price,
                car.Color,
                car.ProductionDate);
            return Ok(carDetailsViewModel);
        }
        /// <summary>
        /// Cadastrar um Carro
        /// </summary>
        /// <remarks>
        /// Requisição de exemplo:
        /// {
        ///     "brand": "Honda",
        ///     "model": "Civic",
        ///     "vinCode":"abc123",
        ///     "year":2021,
        ///     "color": "Cinza",
        ///     "productionDate":"2021-04-06"
        /// }
        /// </remarks>
        /// <param name="model">Dados de um novo carro</param>
        /// <returns>Objeto recém-criado</returns>
        /// <response code="201">Objeto criado com sucesso.</response>
        /// <response code="400">Dados invalidos.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Post([FromBody] AddCarInputModel model)
        {
            if (model.Model.Length > 50)
            {
                return BadRequest("Modelo não pode ter mais do 50 caracteres");
            }

            var car = new Car(model.VinCode, model.Brand, model.Model, model.Year, model.Price, model.Color, model.ProductionDate);

            _dbContext.Cars.Add(car);
            _dbContext.SaveChanges();

            return CreatedAtAction(
                nameof(GetById),
                new { id = car.Id },
                model);
        }

        /// <summary>
        /// Atualizar dados de um carro
        /// </summary>
        /// <remarks>
        /// Requisição de exemplo:
        /// {
        ///   "color": "Vermelho",
        ///   "price":100000
        /// }
        /// </remarks>
        /// <param name="id">Identificador de Carro</param>
        /// <param name="model">Dados de alteração</param>
        /// <returns>Não tem retorno</returns>
        /// <response code="204">Atualização bem-sucedida</response>
        /// <response code="400">Dados invalidos</response>
        /// <response code="404">Carro não encontrado</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult Put(int id, [FromBody] UpdateCarInputModel model)
        {
            var car = _dbContext.Cars.SingleOrDefault(c => c.Id == id);

            if (car == null)
            {
                return NotFound();
            }

            car.Update(model.Color, model.Price);
            using(var sqlConnection = new SqlConnection(_connectionString))
            {
                var query = "UPDATE Cars SET Color = @color, Price = @price WHERE Id = @id";
                sqlConnection.Execute(query, new { color = model.Color, price = model.Price, car.Id });
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var car = _dbContext.Cars.SingleOrDefault(c => c.Id == id);

            if (car == null)
            {
                return NotFound();
            }

            car.SetAsSuspended();
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}
