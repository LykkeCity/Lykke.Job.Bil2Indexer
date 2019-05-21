using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    public class TestController:Controller
    {
        [HttpGet("/api/test")]
        public ActionResult Test()
        {
            return Ok("test");
        }
    }
}
