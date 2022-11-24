using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using LogServerLogic;
using DTOs;
using System;

namespace WebApiRabbitMQ.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private LogLogic _logic = new LogLogic();

        [HttpGet]
        public IActionResult GetAllLogs()
        {
            return Ok(_logic.GetLogs());
        }

        [HttpGet]
        public IActionResult GetFilteredLogs([FromBody]FilterDTO filter)
        {
            switch (filter.Option)
            {
                case 0:
                    return Ok(_logic.FilterByDate(DateTime.Parse(filter.Text)));
                    break;
                case 1:
                    return Ok(_logic.FilterByCategory(filter.Text));
                    break;
                case 2:
                    return Ok(_logic.FilterByContent(filter.Text));
                    break;
                default:
                    return BadRequest("Codigo de filtro invalido");
                    break;
            }
        }
    }
}
