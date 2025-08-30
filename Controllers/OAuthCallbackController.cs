using FreedomITAS.API_Serv;
using FreedomITAS.API_Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace FreedomITAS.Controllers
{
    
    [Route("oauth")]
    public class OAuthCallbackController : Controller
    {
        private readonly GoHighLevelService _ghlService;
        private readonly GoHighLevelSettings _settings;

       
        public OAuthCallbackController(GoHighLevelService ghlService, IOptions<GoHighLevelSettings> settings)
        {
            _ghlService = ghlService;
            _settings = settings.Value;
        }

        
        [HttpGet("start")]
        public IActionResult Start()
        {
           
            const string redirectUri = "https://app.freedomit.com.au/oauth/callback";

            var url =
                $"https://marketplace.gohighlevel.com/oauth/chooselocation?response_type=code" +
                $"&client_id={Uri.EscapeDataString(_settings.ClientId)}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";

            return Redirect(url);
        }

        
        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Missing authorization code.");

            try
            {
                await _ghlService.ExchangeCodeForTokensAsync(code);
                return Content("Authorization successful. You can now close this tab.");
            }
            catch (Exception ex)
            {
                return Content("Authorization failed: " + ex.Message);
            }
        }
    }
}
