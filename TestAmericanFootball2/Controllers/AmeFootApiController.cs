using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TestAmericanFootball2.Service;
using TestAmericanFootball2.Service.Interface;

namespace TestAmericanFootball2.Controllers
{
    [Produces("application/json")]
    [Route("AmeFoot/api")]
    public class AmeFootApiController : Controller
    {
        private readonly IGameService _service;

        public AmeFootApiController(IGameService service)
        {
            _service = service;
        }

        // GET: api/AmeFootApi
        [HttpGet]
        public IEnumerable<string> Get()
        {
            _service.WriteMessage("test");
            return new string[] { "value1", "value2" };
        }

        // GET: api/AmeFootApi/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/AmeFootApi
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/AmeFootApi/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}