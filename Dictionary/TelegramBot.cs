using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace Dictionary
{
    public class TelegramBot
    {
        public static TelegramBot Instance { get; private set; } = new TelegramBot();
        private TelegramBot() { }

        private enum MessageType
        {
            Success,
            Info,
            Warning,
            Error
        }

        private enum UserAction
        {
            DictionaryTypeInput,



            WordInputForTranslation,
            DictionaryTypeInputForTranslation,



            WordAdd_Input,
            WordAdd_DictionaryInput,
            WordAdd_TranslationInput,
            
            WordChange_Input,
            WordChange_DictionaryInput,
            WordChange_NewWordInput,

            WordRemove_Input,
            WordRemove_DictionaryInput,



            TranslationAdd_Input,
            TranslationAdd_DictionaryInput,
            TranslationAdd_WordInput,

            TranslationChange_Input,
            TranslationChange_DictionaryInput,
            TranslationChange_WordInput,
            TranslationChange_NewTranslationInput,

            TranslationRemove_Input,
            TranslationRemove_DictionaryInput,
            TranslationRemove_WordInput
        }

        private const string Token = "7083929076:AAGjEjRbbvmAj2EPHM6CenjXsCUSlmGXJH8";

        private TelegramBotClient _botClient;
        private CancellationTokenSource _cts;
        private ReceiverOptions _receiverOptions;
        private User _me;

        private readonly Dictionary</* chatId */ long, UserAction> _usersActions = new();
        private readonly Dictionary</* chatId */ long, List<string>> _usersData = new();

        public async Task Start()
        {
            Dictionary.Dictionaries = DictionarySerializerImp.Instance.Deserialize();

            _botClient = new(Token);
            _cts = new();
            _receiverOptions = new() { AllowedUpdates = Array.Empty<UpdateType>() };

            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: _receiverOptions,
                cancellationToken: _cts.Token
            );

            _me = await _botClient.GetMeAsync();
        }

        public void Stop()
        {
            _cts.Cancel();
            DictionarySerializerImp.Instance.Serialize(Dictionary.Dictionaries);
        }

        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;

            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            if (_usersActions.ContainsKey(chatId))
            {
                switch (_usersActions[chatId])
                {
                    case UserAction.DictionaryTypeInput:
                        Dictionary.Dictionaries.Add(new Dictionary(messageText));

                        _usersActions.Remove(chatId);
                        SendMessage("Словарь успешно создан", MessageType.Success, chatId, cancellationToken);
                        break;





                    case UserAction.WordInputForTranslation:
                        SendMessage("Введите тип словаря", MessageType.Info, chatId, cancellationToken);
                        _usersData.Add(chatId, [messageText]);
                        _usersActions[chatId] = UserAction.DictionaryTypeInputForTranslation;
                        break;

                    case UserAction.DictionaryTypeInputForTranslation:
                        if (Dictionary.GetDictionaryByType(messageText) == null)
                        {
                            SendMessage("Словарь не найден", MessageType.Error, chatId, cancellationToken);
                            _usersData.Remove(chatId);
                            _usersActions.Remove(chatId);
                            break;
                        }

                        List<string>? translation = Dictionary.GetDictionaryByType(messageText)?.GetTranslation(_usersData[chatId][0]) ?? null;
                        
                        if (translation != null)
                            SendMessage($"Перевод:\n{ListUtils.FormatTranslations(translation)}", MessageType.Info, chatId, cancellationToken);
                        else
                            SendMessage("Перевод не найден", MessageType.Error, chatId, cancellationToken);
                        
                        _usersData.Remove(chatId);
                        _usersActions.Remove(chatId);
                        break;





                    case UserAction.WordAdd_Input:
                        SendMessage("Введите тип словаря", MessageType.Info, chatId, cancellationToken);
                        _usersData.Add(chatId, [messageText]);
                        _usersActions[chatId] = UserAction.WordAdd_DictionaryInput;
                        break;

                    case UserAction.WordAdd_DictionaryInput:
                        SendMessage("Введите перевод", MessageType.Info, chatId, cancellationToken);
                        _usersData[chatId] = ListUtils.AddAndReturn(_usersData[chatId], messageText);
                        _usersActions[chatId] = UserAction.WordAdd_TranslationInput;
                        break;

                    case UserAction.WordAdd_TranslationInput:
                        if (Dictionary.GetDictionaryByType(_usersData[chatId][1]) == null)
                        {
                            SendMessage("Словарь не найден", MessageType.Error, chatId, cancellationToken);
                            _usersData.Remove(chatId);
                            _usersActions.Remove(chatId);
                            break;
                        }

                        Dictionary.GetDictionaryByType(_usersData[chatId][1])?.AddWord(_usersData[chatId][0], messageText);

                        _usersData.Remove(chatId);
                        _usersActions.Remove(chatId);
                        SendMessage("Слово успешно добавлено", MessageType.Success, chatId, cancellationToken);
                        break;



                    case UserAction.WordChange_Input:
                        SendMessage("Введите тип словаря", MessageType.Info, chatId, cancellationToken);
                        _usersData.Add(chatId, [messageText]);
                        _usersActions[chatId] = UserAction.WordChange_DictionaryInput;
                        break;

                    case UserAction.WordChange_DictionaryInput:
                        SendMessage("Введите новое слово", MessageType.Info, chatId, cancellationToken);
                        _usersData[chatId] = ListUtils.AddAndReturn(_usersData[chatId], messageText);
                        _usersActions[chatId] = UserAction.WordChange_NewWordInput;
                        break;

                    case UserAction.WordChange_NewWordInput:
                        if (Dictionary.GetDictionaryByType(_usersData[chatId][1]) == null)
                        {
                            SendMessage("Словарь не найден", MessageType.Error, chatId, cancellationToken);
                            _usersData.Remove(chatId);
                            _usersActions.Remove(chatId);
                            break;
                        }

                        Dictionary.GetDictionaryByType(_usersData[chatId][1])?.ChangeWord(_usersData[chatId][0], messageText);

                        _usersData.Remove(chatId);
                        _usersActions.Remove(chatId);
                        SendMessage("Слово успешно изменено", MessageType.Success, chatId, cancellationToken);
                        break;



                    case UserAction.WordRemove_Input:
                        SendMessage("Введите тип словаря", MessageType.Info, chatId, cancellationToken);
                        _usersData.Add(chatId, [messageText]);
                        _usersActions[chatId] = UserAction.WordRemove_DictionaryInput;
                        break;

                    case UserAction.WordRemove_DictionaryInput:
                        if (Dictionary.GetDictionaryByType(messageText) == null)
                        {
                            SendMessage("Словарь не найден", MessageType.Error, chatId, cancellationToken);
                            _usersData.Remove(chatId);
                            _usersActions.Remove(chatId);
                            break;
                        }

                        Dictionary.GetDictionaryByType(messageText)?.RemoveWord(_usersData[chatId][0]);

                        _usersData.Remove(chatId);
                        _usersActions.Remove(chatId);
                        SendMessage("Слово успешно удалено", MessageType.Success, chatId, cancellationToken);
                        break;



                    case UserAction.TranslationAdd_Input:
                        SendMessage("Введите тип словаря", MessageType.Info, chatId, cancellationToken);
                        _usersData.Add(chatId, [messageText]);
                        _usersActions[chatId] = UserAction.TranslationAdd_DictionaryInput;
                        break;

                    case UserAction.TranslationAdd_DictionaryInput:
                        SendMessage("Введите слово, связанное с переводом", MessageType.Info, chatId, cancellationToken);
                        _usersData[chatId] = ListUtils.AddAndReturn(_usersData[chatId], messageText);
                        _usersActions[chatId] = UserAction.TranslationAdd_WordInput;
                        break;

                    case UserAction.TranslationAdd_WordInput:
                        if (Dictionary.GetDictionaryByType(_usersData[chatId][1]) == null)
                        {
                            SendMessage("Словарь не найден", MessageType.Error, chatId, cancellationToken);
                            _usersData.Remove(chatId);
                            _usersActions.Remove(chatId);
                            break;
                        }

                        Dictionary.GetDictionaryByType(_usersData[chatId][1])?.AddTranslation(_usersData[chatId][0], messageText);

                        _usersData.Remove(chatId);
                        _usersActions.Remove(chatId);
                        SendMessage("Перевод успешно добавлен", MessageType.Success, chatId, cancellationToken);
                        break;



                    case UserAction.TranslationChange_Input:
                        SendMessage("Введите тип словаря", MessageType.Info, chatId, cancellationToken);
                        _usersData.Add(chatId, [messageText]);
                        _usersActions[chatId] = UserAction.TranslationChange_DictionaryInput;
                        break;

                    case UserAction.TranslationChange_DictionaryInput:
                        SendMessage("Введите слово, связанное с переводом", MessageType.Info, chatId, cancellationToken);
                        _usersData[chatId] = ListUtils.AddAndReturn(_usersData[chatId], messageText);
                        _usersActions[chatId] = UserAction.TranslationChange_WordInput;
                        break;

                    case UserAction.TranslationChange_WordInput:
                        SendMessage("Введите новый перевод", MessageType.Info, chatId, cancellationToken);
                        _usersData[chatId] = ListUtils.AddAndReturn(_usersData[chatId], messageText);
                        _usersActions[chatId] = UserAction.TranslationChange_NewTranslationInput;
                        break;

                    case UserAction.TranslationChange_NewTranslationInput:
                        if (Dictionary.GetDictionaryByType(_usersData[chatId][1]) == null)
                        {
                            SendMessage("Словарь не найден", MessageType.Error, chatId, cancellationToken);
                            _usersData.Remove(chatId);
                            _usersActions.Remove(chatId);
                            break;
                        }

                        Dictionary.GetDictionaryByType(_usersData[chatId][1])?.ChangeTranslation(_usersData[chatId][0], _usersData[chatId][2], messageText);

                        _usersData.Remove(chatId);
                        _usersActions.Remove(chatId);
                        SendMessage("Перевод успешно изменён", MessageType.Success, chatId, cancellationToken);
                        break;



                    case UserAction.TranslationRemove_Input:
                        SendMessage("Введите тип словаря", MessageType.Info, chatId, cancellationToken);
                        _usersData.Add(chatId, [messageText]);
                        _usersActions[chatId] = UserAction.TranslationRemove_DictionaryInput;
                        break;

                    case UserAction.TranslationRemove_DictionaryInput:
                        SendMessage("Введите слово, связанное с переводом", MessageType.Info, chatId, cancellationToken);
                        _usersData[chatId] = ListUtils.AddAndReturn(_usersData[chatId], messageText);
                        _usersActions[chatId] = UserAction.TranslationRemove_WordInput;
                        break;

                    case UserAction.TranslationRemove_WordInput:
                        if (Dictionary.GetDictionaryByType(_usersData[chatId][1]) == null)
                        {
                            SendMessage("Словарь не найден", MessageType.Error, chatId, cancellationToken);
                            _usersData.Remove(chatId);
                            _usersActions.Remove(chatId);
                            break;
                        }

                        if (Dictionary.GetDictionaryByType(_usersData[chatId][1])?.RemoveTranslation(_usersData[chatId][0], messageText) ?? false)
                            SendMessage("Перевод успешно удалён", MessageType.Success, chatId, cancellationToken);
                        else
                            SendMessage("Нельзя удалить последний вариант перевода", MessageType.Error, chatId, cancellationToken);

                        _usersData.Remove(chatId);
                        _usersActions.Remove(chatId);
                        break;
                }
            }
            else
            {
                switch (messageText)
                {
                    case "/start":
                        SendMainMenu(chatId, cancellationToken);
                        break;

                    case "Создать словарь":
                        SendMessage("Введите тип словаря", MessageType.Info, chatId, cancellationToken);
                        _usersActions.Add(chatId, UserAction.DictionaryTypeInput);
                        break;

                    case "Изменить слова/перевод":
                        SendTranslationMenu(chatId, cancellationToken);
                        break;

                        case "Добавить слово":
                            SendMessage("Введите слово", MessageType.Info, chatId, cancellationToken);
                            _usersActions.Add(chatId, UserAction.WordAdd_Input);
                        break;

                        case "Заменить слово":
                            SendMessage("Введите слово для замены", MessageType.Info, chatId, cancellationToken);
                            _usersActions.Add(chatId, UserAction.WordChange_Input);
                        break;

                        case "Удалить слово":
                            SendMessage("Введите слово для удаления", MessageType.Info, chatId, cancellationToken);
                            _usersActions.Add(chatId, UserAction.WordRemove_Input);
                        break;

                        case "Добавить перевод":
                            SendMessage("Введите перевод", MessageType.Info, chatId, cancellationToken);
                            _usersActions.Add(chatId, UserAction.TranslationAdd_Input);
                        break;

                        case "Заменить перевод":
                            SendMessage("Введите перевод для замены", MessageType.Info, chatId, cancellationToken);
                            _usersActions.Add(chatId, UserAction.TranslationChange_Input);
                        break;

                        case "Удалить перевод":
                            SendMessage("Введите перевод для удаления", MessageType.Info, chatId, cancellationToken);
                            _usersActions.Add(chatId, UserAction.TranslationRemove_Input);
                        break;

                        case "Вернуться в меню":
                            SendMainMenu(chatId, cancellationToken);
                            break;

                    case "Искать перевод":
                        SendMessage("Введите слово", MessageType.Info, chatId, cancellationToken);
                        _usersActions.Add(chatId, UserAction.WordInputForTranslation);
                        break;

                    case "Получить файл словарей":
                        SendDictionaryFile(chatId, cancellationToken);
                        break;

                    default:
                        SendMessage("Некорректный ввод", MessageType.Error, chatId, cancellationToken);
                        break;
                }
            }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
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

        private void SendMessage(string text, MessageType type, long chatId,  CancellationToken cancellationToken)
        {
            switch (type)
            {
                case MessageType.Success:
                    text = $"✅ {text}";
                    break;

                case MessageType.Info:
                    text = $"ℹ {text}";
                    break;

                case MessageType.Warning:
                    text = $"⚠ {text}";
                    break;

                case MessageType.Error:
                    text = $"❌ {text}";
                    break;
            }

            _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: text,
                cancellationToken: cancellationToken
            );
        }

        private void SendMainMenu(long chatId, CancellationToken cancellationToken)
        {
            var buttons = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[]
                {
                    "Создать словарь",
                    "Изменить слова/перевод"
                },

                new KeyboardButton[]
                {
                    "Искать перевод",
                    "Получить файл словарей"
                }
            });

            _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "<b>Выберите действие:</b>\n\n📕 Создать словарь\n✏ Изменить слова/перевод\n📖 Искать перевод\n📄 Получить файл словаря",
                parseMode: ParseMode.Html,
                replyMarkup: buttons,
                cancellationToken: cancellationToken
            );
        }

        private void SendTranslationMenu(long chatId, CancellationToken cancellationToken)
        {
            var buttons = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[]
                {
                    "Добавить слово",
                    "Заменить слово",
                    "Удалить слово",
                },

                new KeyboardButton[]
                {
                    "Добавить перевод",
                    "Заменить перевод",
                    "Удалить перевод"
                },

                new KeyboardButton[]
                {
                    "Вернуться в меню"
                }
            });

            _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "<b>Выберите действие:</b>\n\n📖 Добавить слово\n✏ Заменить слово\n❌ Удалить слово\n\n📖 Добавить перевод\n✏ Заменить перевод\n❌ Удалить перевод\n\n",
                parseMode: ParseMode.Html,
                replyMarkup: buttons,
                cancellationToken: cancellationToken
            );
        }

        private void SendDictionaryFile(long chatId, CancellationToken cancellationToken)
        {
            var stream = System.IO.File.OpenRead("dictionaries.json");

            _botClient.SendDocumentAsync(
                chatId: chatId,
                document: InputFile.FromStream(stream: stream, fileName: "dictionaries.json"),
                cancellationToken: cancellationToken
            );
        }
    }
}