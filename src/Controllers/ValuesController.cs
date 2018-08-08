using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Scoped.logging.Serilog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> logger;

        public ValuesController(ILogger<ValuesController> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET api/values
        //To test regular logging scenario
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var values = new string[] { "value1", "value2" };

            logger.LogInformation($"Returning values '{values[0]}' and '{values[1]}'");

            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        //To test an exception/error scenario
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            if(id > 100)
            {
                var exception = new ArgumentOutOfRangeException(nameof(id), id, "Id should not be greater than 100");
                logger.LogError(exception, "Id value greater than 100");
                return BadRequest(id);
            }

            logger.LogInformation($"Returning values '{id}' that was received");
            return new OkObjectResult(id);
        }
    }
}
