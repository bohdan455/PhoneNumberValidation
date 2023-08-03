using DataConnection;
using Telegram;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

var botClient = new TelegramBotClient("{bot_token}"); // TODO put bot token here

using CancellationTokenSource cts = new();

ReceiverOptions receiverOptions = new()
{
    AllowedUpdates = new UpdateType[]{
        UpdateType.Message
    }
};
var websiteConnector = new WebsiteConnector("{websiteUrl}"); // TODO put web site url here
botClient.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await botClient.GetMeAsync();

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();

// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
{
    if (update?.Message == null)
        return;

    var chatId = update.Message.Chat.Id;
    await using DataContext dataContext = new DataContext();

    if (update.Message.Text is { } messageText)
    {
        await HandleStartCommand(botClient, chatId, messageText);
    }
    else if(update.Message.Contact is { } contact)
    {
        await HandleContact(botClient, chatId, contact);
    }
}

async Task HandleStartCommand(ITelegramBotClient botClient, long chatId, string messageText)
{
    var messageSplitted= messageText.Split(" ");

    if (messageSplitted.Length == 2 && messageSplitted[0] == "/start")
    {
        // Make user send contact
        await RequestContact(botClient, chatId);
    }

    var code = messageSplitted[1];
    await using DataContext dataContext = new DataContext();
    var user = dataContext.Users.FirstOrDefault(u => u.TelegramId == chatId);

    if(user == null)
    {
        dataContext.Users.Add(new DataConnection.Entities.User
        {
            TelegramId = chatId,
            SignalRCode = code,
        });
    }
    else
    {
        user.SignalRCode = code;
    }
    await dataContext.SaveChangesAsync();
}

async Task HandleContact(ITelegramBotClient botClient, long chatId, Contact contact)
{
    await using DataContext dataContext = new DataContext();

    var user = dataContext.Users.FirstOrDefault(u => u.TelegramId == chatId);
    if (user == null)
    {
        await botClient.SendTextMessageAsync(chatId, "Invalid request");
    }

    await websiteConnector.ValidateNumberOnWebsite(contact.PhoneNumber,user.SignalRCode);
    await botClient.SendTextMessageAsync(chatId, "Your number has been confirmed, now you can return to website");
}

async Task RequestContact(ITelegramBotClient botClient, long chatId)
{
    ReplyKeyboardMarkup requestReplyKeyboard = new(
        new[]
        {
                // KeyboardButton.WithRequestLocation("Location"), // this for the location if you need it
                KeyboardButton.WithRequestContact("Send my phone Number"),
        });

    await botClient.SendTextMessageAsync(chatId: chatId,
                                                text: "Send your phone number",
                                                replyMarkup: requestReplyKeyboard);
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}
