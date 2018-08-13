using System.Threading.Tasks;
using TestAmericanFootball2.Models;

namespace TestAmericanFootball2.Service.Interface
{
    public interface IGameService
    {
        Game InitializeGame();

        Task<Game> GameAsync(
            string player1Id,
            string player2Id,
            string method
            );
    }
}