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
            try
            {
                return Ok(_logic.ApplyFilters(filter));
            }
            catch (Exception ex)
            { 
                return BadRequest(ex.Message);
            }
            
        }
    }
}
