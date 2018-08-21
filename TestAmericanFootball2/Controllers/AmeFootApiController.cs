using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TestAmericanFootball2.Models;
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
        [HttpPost]
        public async Task<ActionResult> Post([FromBody]GameParam param)
        {
            string player1Id = "k";
            string player2Id = "CPU";

            var ret = await _service.GameAsync(player1Id, player2Id, "");
            return Ok(ret);
        }

        // GET: api/AmeFootApi/5
        [HttpGet("{id}", Name = "Get")]
        public string Get(int id)
        {
            return "value";
        }

        //// POST: api/AmeFootApi
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}

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

    public class Test
    {
        public string zip { get; set; }
    }
}