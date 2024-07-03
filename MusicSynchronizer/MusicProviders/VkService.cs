using AutoMapper;
using MusicSynchronizer.Entities;
using MusicSynchronizer.MusicProviders.Interfaces;
using VkNet.Abstractions;
using VkNet.Model;
using VkNet.AudioBypassService;
using Microsoft.Extensions.DependencyInjection;
using VkNet;
using VkNet.AudioBypassService.Extensions;
using VkNet.Exception;

namespace MusicSynchronizer.MusicProviders
{
    public class VkService : IMusicService
    {
        private VkApi _api;
        private readonly IMapper _mapper;
        public bool IsAuthorized { get; private set; } = false;

        public VkService(VkApi api, IMapper mapper)
        {
            _api = api;
            _mapper = mapper;
        }

        public void PasswordAuth(string login, string password, string twofactor)
        {
            _api.Authorize(new ApiAuthParams
            {
                Login = login,
                Password = password,
                TwoFactorAuthorization = () =>
                {
                    return twofactor;
                }
            });
            IsAuthorized = true;
        }

        public void TokenAuth(string token)
        {
            _api.Authorize(new ApiAuthParams
            {
                AccessToken = token
            });
        }

        public async Task<List<Song>> GetLikedSongs()
        {
            var audios = _api.Audio.Get(new AudioGetParams { Count = 5 });
            var songList = new List<Song>();

            foreach (var audio in audios)
            {
                songList.Add(_mapper.Map<Song>(audio));
            }
            return songList;
        }

        public async Task<List<Song>> AddSongList(List<Song> songs)
        {
            var foundSongs = new List<Audio>();
            var audioToAdd = new Audio();
            var audioIds = new List<string>();

            foreach (var song in songs)
            {
                foundSongs.AddRange(_api.Audio.Search(new AudioSearchParams { Query = $"{song.Title} - {song.Artist}", Count = 1 }));
                audioToAdd = foundSongs[foundSongs.Count - 1];
                audioIds.Add(audioToAdd.Id.ToString());
                Thread.Sleep(300);
            }

            var playList = _api.Audio.CreatePlaylist((long)_api.UserId, "Музыка из сервисов");

            _api.Audio.AddToPlaylist(audioToAdd.OwnerId.Value, (long)playList.Id, audioIds);
            List<Song> addedSongs = _mapper.Map<List<Song>>(foundSongs);

            return addedSongs;
        }

        
    }
}
