using AutoMapper;
using MusicSynchronizer.MusicProviders.Interfaces;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using MusicSynchronizer.Entities;
using static SpotifyAPI.Web.SearchRequest;
using System.Collections.Generic; 

namespace MusicSynchronizer.MusicProviders
{
    public class SpotifyService : IMusicService
    {
        private static EmbedIOAuthServer _server;
        private static SpotifyClient _api;
        private readonly IMapper _mapper;
        public bool IsAuthorized { get; init; }
        public SpotifyService(IMapper mapper)
        {
            _mapper = mapper;
            IsAuthorized = false;
        }

        public void PasswordAuth(string login, string password, string twofactor)
        {

        }
        
        public async void TokenAuth(string clientID)
        {
            _server = new EmbedIOAuthServer(new Uri("http://localhost:5543/callback"), 5543);
            await _server.Start();

            _server.ImplictGrantReceived += OnImplicitGrantReceived;
            _server.ErrorReceived += OnErrorReceived;

            var request = new LoginRequest(_server.BaseUri, clientID, LoginRequest.ResponseType.Token)
            {
                Scope = new List<string> { Scopes.UserReadEmail, Scopes.UserLibraryRead, Scopes.UserLibraryModify }
            };
            BrowserUtil.Open(request.ToUri());
            IsAuthorized = true;
        }

        public async Task<List<Song>> GetLikedSongs()
        {
            var totalCount = 0;
            var result = new Paging<SavedTrack>();
            var spotifyTracks = new List<SavedTrack>();
            var tracks = new List<Song>();

            while (true)
            {
                result = await _api.Library.GetTracks(new LibraryTracksRequest { Limit = 50 , Offset = totalCount});
                totalCount += 50;
                spotifyTracks = result.Items;
                tracks.AddRange(_mapper.Map<List<Song>>(spotifyTracks));
                if (spotifyTracks.Count < 50) break;
            }

            return tracks;
        }

        public async Task<List<Song>> AddSongList(List<Song> songList)
        {

            var addedSongs = new List<Song>();
            var songsIds = new List<string>();

            foreach (var song in songList)
            {
                var searchResult = await _api.Search.Item(new SearchRequest(Types.Track, $"{song.Artist} - {song.Title}") { Limit = 1 });
                var songToAdd = searchResult.Tracks.Items.FirstOrDefault();
                songsIds.Add(songToAdd.Id);
                await _api.Library.SaveTracks(new LibrarySaveTracksRequest(new List<string> { songToAdd.Id }));
                addedSongs.Add(_mapper.Map<Song>(songToAdd));
                Console.WriteLine($"{songToAdd.Name} - {songToAdd.Artists[0]}");
            }
            
            return addedSongs;    
        }


        private static async Task OnImplicitGrantReceived(object sender, ImplictGrantResponse response)
        {
            await _server.Stop();
            _api = new SpotifyClient(response.AccessToken);
        }

        private static async Task OnErrorReceived(object sender, string error, string state)
        {
            Console.WriteLine($"Aborting authorization, error received: {error}");
            await _server.Stop();
        }
    }
}
