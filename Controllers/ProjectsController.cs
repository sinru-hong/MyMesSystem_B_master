using Microsoft.AspNetCore.Mvc;
using MyMesSystem_B.Services;

namespace MyMesSystem_B.Controllers
{
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectsService _service;
        public ProjectsController(ProjectsService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            var data = _service.GetFakeProducts();
            return Ok(data); // 這會回傳 JSON [ { "productID": "A01", ... } ]
        }
    }
}
