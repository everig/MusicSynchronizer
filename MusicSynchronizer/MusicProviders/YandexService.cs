using AutoMapper;
using MusicSynchronizer.MusicProviders.Interfaces;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using MusicSynchronizer.Entities;
using Yandex.Music.Api.Models.Library;
using VkNet.Model;
using Yandex.Music.Api.Models.Track;

namespace MusicSynchronizer.MusicProviders
{
    public class YandexService : IMusicService
    {
        private readonly YandexMusicApi _api;
        private readonly AuthStorage _storage;
        private readonly IMapper _mapper;
        public bool IsAuthorized { get; private set; } = false;

        public YandexService(IMapper mapper)
        {
            _api = new YandexMusicApi();
            _storage = new AuthStorage();
            _mapper = mapper;
        }

        public void PasswordAuth(string login, string password, string twofactor)
        {
            var auth = _api.User.CreateAuthSession(_storage, login);
            _api.User.AuthorizeByAppPassword(_storage, password);
            IsAuthorized = true;
        }

        public void TokenAuth(string token)
        {
            _api.User.Authorize(_storage, token);
            IsAuthorized = true;
        }

        public async Task<List<Song>> GetLikedSongs()
        {
            
            string[] search = _api.Library.GetLikedTracks(_storage).Result.Library.Tracks.Select((YLibraryTrack t) => t.Id).ToArray();
            var tracks = _api.Track.Get(_storage, search).Result;
            List<Song> songList = _mapper.Map<List<Song>>(tracks);
            return songList;
        }

        public async Task<List<Song>> AddSongList(List<Song> songs)
        {
            var track = new YTrack();
            var addedSongs = new List<Song>();
            foreach (var song in songs)
            {
                track = _api.Search.Track(_storage, $"{song.Title} - {song.Artist}").Result.Tracks.Results[0];
                _api.Library.AddTrackLike(_storage, track);
                addedSongs.Add(_mapper.Map<Song>(track));
            }
            return addedSongs;
        }
    }
}
