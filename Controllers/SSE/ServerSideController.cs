using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dotnetSSE.Models.SSEModel;

namespace dotnetSSE.Controllers.SSE
{
    [Route("/sse")]
    public class ServerSideController : Controller
    {
        // e.g /sse/5 to get the unique event id
        [HttpGet("{id}")]
        public async Task ListenToMessage(string id)
        {
            Response.Headers["Cache-Control"] = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";
            Response.ContentType = "text/event-stream";

            await SSEModel.ListenToMessagesModel(id, Response.Body, HttpContext.RequestAborted);
        }

        // e.g /sse/5/messages to post a specific message
        [HttpPost("{id}/messages")]
        public async Task SendMessage(string id, [FromBody] string messages) 
        {
            await SSEModel.SendMessagesModel(id, messages);
        }
    }
}