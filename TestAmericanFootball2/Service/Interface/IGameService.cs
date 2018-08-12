using System.Threading.Tasks;

namespace TestAmericanFootball2.Service.Interface
{
    public interface IGameService
    {
        Task WriteMessage(string message);
    }
}