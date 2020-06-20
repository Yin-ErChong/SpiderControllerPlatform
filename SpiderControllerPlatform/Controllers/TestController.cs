using Microsoft.AspNetCore.Mvc;
using SpiderCore.ServiceInterFace;
using SpiderDataBase.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiderControllerPlatform.Controllers
{
    /// <summary>
    /// 测试控制器
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : Controller
    {
        private IFirstTestService _firstTestService;
        public TestController(IFirstTestService firstTestService)
        {
            _firstTestService = firstTestService;
        }
        [Route(nameof(hehe))]
        [HttpGet]
        public async Task<User> hehe(Guid guid)
        {
            return await _firstTestService.GetUser(guid);
        }
    }
}
