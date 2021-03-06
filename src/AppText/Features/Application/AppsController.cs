﻿using AppText.Shared.Infrastructure;
using AppText.Shared.Infrastructure.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace AppText.Features.Application
{
    [Route("apps")]
    public class AppsController : ControllerBase
    {
        private readonly IDispatcher _dispatcher;

        public AppsController(IDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery]AppQuery query)
        {
            return Ok(await _dispatcher.ExecuteQuery(query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOne(string id)
        {
            var result = await _dispatcher.ExecuteQuery(new AppQuery { Id = id });
            if (result.Length == 0)
            {
                return NotFound();
            }
            return Ok(result.First());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody]CreateAppCommand command)
        {
            var result = await _dispatcher.ExecuteCommand(command);
            var id = (result.ResultData as App)?.Id;
            return this.HandleCreateCommandResult(result, id, result.ResultData);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody]UpdateAppCommand command)
        {
            command.Id = id;
            var result = await _dispatcher.ExecuteCommand(command);
            return this.HandleUpdateCommandResult(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _dispatcher.ExecuteCommand(new DeleteAppCommand(id));
            return this.HandleDeleteCommandResult(result);
        }
    }
}