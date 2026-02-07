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
            // 這裡可以直接回傳 ModelService 抓回來的 Hashtable 清單
            return _modelService.GetUsers(userKeyword);
        }
    }
}
