using lifeEcommerce.Controllers;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Wangkanai.Detection.Services;

namespace Ecommerce.RealTimeCommunication.Controllers.V1._0
{
    [ApiController]
    public class ChatController : Controller
    {
        #region Properties

        //private readonly IChatService _chatService;
        private readonly ILogger<CategoryController> _logger;
        private readonly IStringLocalizer<CategoryController> _localizer;
        private readonly IEmailSender _emailSender;
        private readonly IDetectionService _detection;
        private readonly IHttpContextAccessor _accessor;

        #endregion Properties

        #region Ctor

        public ChatController() 
        {
        }

        #endregion Ctor

        #region Methods
        
        

        #endregion Methods
    }
}