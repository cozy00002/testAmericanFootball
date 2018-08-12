using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestAmericanFootball2.Service.Interface;

namespace TestAmericanFootball2.Service
{
    /// <summary>
    /// GameService
    /// </summary>
    public class GameService : IGameService
    {
        public GameService(IConfiguration config)
        {
            // var myStringValue = config["MyStringKey"];
        }

        public Task WriteMessage(string message)
        {
            Console.WriteLine(
                $"GameService.WriteMessage called. Message: {message}");

            return Task.FromResult(0);
        }
    }
}