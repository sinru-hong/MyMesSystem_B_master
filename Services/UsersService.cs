using System.Collections;
using MyMesSystem_B.ModelServices;

namespace MyMesSystem_B.Services
{
    public class UsersService
    {
        private readonly UsersModelService _modelService;
        public UsersService(UsersModelService modelService)
        {
            _modelService = modelService;
        }
        public IList GetUsers(string userKeyword = "")
        {
            return _modelService.GetUsers(userKeyword);
        }
    }
}
