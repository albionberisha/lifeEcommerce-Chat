using lifeEcommerce.Data.UnitOfWork;
using lifeEcommerce.Services.IService;
using AutoMapper;

namespace lifeEcommerce.Services
{
    public class ChatService : IChatService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChatService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
    }
}
