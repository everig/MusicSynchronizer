using VkNet;
using VkNet.AudioBypassService.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MusicSynchronizer.Common.Mapping;
using AutoMapper;
using MusicSynchronizer.MusicProviders;
using MusicSynchronizer.MusicProviders.Interfaces;
using MusicSynchronizer.Entities;


public class Program
{
    public static async Task Main(string[] args)
    {
        var mappingConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        IMapper mapper = mappingConfiguration.CreateMapper();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddAudioBypass();

        var vkApi = new VkApi(serviceCollection);

        var spotify = new SpotifyService(mapper);
        var vk = new VkService(vkApi, mapper);
        var yandex = new YandexService(mapper);

        var login = "";
        var password = "";
        var twoFactor = "";
        var token = "";
        var songList = new List<Song>();
        var services = new List<IMusicService>();

        Console.WriteLine("Добро пожаловать");
        Console.WriteLine();



        while (true)
        {

            switch (Console.ReadLine().ToLower())
            {

                case ("vk пароль"):
                    Console.WriteLine("Введите логин");
                    login = Console.ReadLine();
                    Console.WriteLine("Введите пароль");
                    password = Console.ReadLine();
                    Console.WriteLine("Код двухфакторной авторизации");
                    twoFactor = Console.ReadLine();
                    vk.PasswordAuth(login, password, twoFactor);
                    Console.WriteLine("Успех!");
                    break;

                case ("spotify"):
                    spotify.TokenAuth("9062459eb29445a395226ad7071a521b");
                    Console.WriteLine("Успех!");
                    break;

                case ("yandex токен"):
                    Console.WriteLine("Введите токен авторизации");
                    token = Console.ReadLine();
                    yandex.TokenAuth(token);
                    Console.WriteLine("Успех!");
                    break;

                case ("yandex пароль"):
                    Console.WriteLine("Введите логин");
                    login = Console.ReadLine();
                    Console.WriteLine("Введите пароль");
                    password = Console.ReadLine();
                    twoFactor = "";
                    yandex.PasswordAuth(login, password, twoFactor);
                    Console.WriteLine("Успех!");
                    break;

                case ("синхронизация vk"):
                    songList = await vk.GetLikedSongs();
                    services.Add(yandex);
                    services.Add(spotify);
                    Synchronize(services, songList);
                    services.Clear();
                    break;

                case ("синхронизация spotify"):
                    songList = await spotify.GetLikedSongs();
                    services.Add(yandex);
                    services.Add(vk);
                    Synchronize(services, songList);
                    services.Clear();
                    break;

                case ("синхронизация yandex"):
                    songList = await yandex.GetLikedSongs();
                    services.Add(vk);
                    services.Add(spotify);
                    Synchronize(services, songList);
                    services.Clear();
                    break;
            }
        }
    }
    public static void Synchronize(List<IMusicService> services, List<Song> songList)
    {
        foreach(var service in services)
        {
            if (service.IsAuthorized)
            {
                var thread = new Thread(async () => await service.AddSongList(songList));
                thread.Start();
            }
        }
    }
}