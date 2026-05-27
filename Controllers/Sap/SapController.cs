using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using REST_API_NET8.Models.SAP;
using REST_API_NET8.Services;

namespace REST_API_NET8.Controllers.Sap
{
    /// <summary>
    /// SAP API Controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "Sap")]
    public class SapController : ControllerBase
    {
        protected readonly SapService _sapService;
        public SapController(SapService sapService)
        {
            _sapService = sapService;
        }
        /// <summary>
        /// สำหรับการดึงข้อมูล SAP จาก .NET 4.8 Framework (EXE) มา MAP กับมูล WMS on Database เพื่อหา Model ที่ต้องผลิต
        /// </summary>
        /// <returns></returns>
        [HttpGet("[action]")]
        public string getDataSap([FromQuery] getDataSapFilter filter)
        {

            return _sapService.getDataSapAsync(filter);
        }
    }
}