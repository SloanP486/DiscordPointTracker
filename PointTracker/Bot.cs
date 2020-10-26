using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;

namespace Bot
{
    public class PointTracker
    {
        private DiscordSocketClient _discordClient;
        private CommandService _discordCommandService;

        private Tracker.CommandHandler _commandHandler;

        private Tracker.Logger _logger;

        public async Task BotMain()
        {
            _logger = new Tracker.Logger();

            DiscordSocketConfig clientConfiguration = new DiscordSocketConfig { LogLevel = _logger.LogLevel, MessageCacheSize = 1000 };
            CommandServiceConfig commandConfiguration = new CommandServiceConfig { LogLevel = _logger.LogLevel };

            _discordClient = new DiscordSocketClient(clientConfiguration);
            _discordClient.Log += _logger.Log;

            _discordCommandService = new CommandService(commandConfiguration);
            _discordCommandService.Log += _logger.Log;

            _commandHandler = new Tracker.CommandHandler(_discordClient, _discordCommandService);

            string loginToken = new Tracker.BotToken("token.txt", Tracker.BotTokenPathType.Relative).Token;

            await _discordClient.LoginAsync(TokenType.Bot, loginToken);
            await _discordClient.StartAsync();

            await Task.Delay(-1);
        }
    }
}

namespace Tracker
{
    public class Logger
    {
        public LogSeverity LogLevel = LogSeverity.Verbose;

        public Task Log(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }

    public class CommandHandler
    {
        private readonly DiscordSocketClient _discordClient;
        private readonly CommandService _discordCommandService;

        public CommandHandler(DiscordSocketClient socketClient, CommandService commandService)
        {
            _discordClient = socketClient;
            _discordCommandService = commandService;
        }

        public async Task InstallCommandHandler()
        {
            _discordClient.MessageReceived += HandleCommandAsync;
            await _discordCommandService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            SocketUserMessage convertedMessage = message as SocketUserMessage;
            if (convertedMessage == null) return;

            int argumentPosition = 0;

            if (!(convertedMessage.HasStringPrefix("!points ", ref argumentPosition) || convertedMessage.HasMentionPrefix(_discordClient.CurrentUser, ref argumentPosition)) || convertedMessage.Author.IsBot) return;
            SocketCommandContext context = new SocketCommandContext(_discordClient, convertedMessage);
            await _discordCommandService.ExecuteAsync(context: context, argPos: argumentPosition, services: null);
        }
    }

    public class BotToken
    {
        public string Token;

        public BotToken(string tokenPath, BotTokenPathType pathType)
        {
            string path = String.Empty;

            switch (pathType)
            {
                case BotTokenPathType.Absolute:
                    path = tokenPath;
                    break;
                case BotTokenPathType.Relative:
                    path = AppDomain.CurrentDomain.BaseDirectory + tokenPath;
                    break;
            }

            string token = File.ReadAllTextAsync(path).Result;
            Token = token;
        }
    }

    public enum BotTokenPathType
    {
        Relative,
        Absolute
    }
}