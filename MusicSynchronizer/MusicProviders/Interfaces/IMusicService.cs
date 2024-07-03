using MusicSynchronizer.Entities;

namespace MusicSynchronizer.MusicProviders.Interfaces
{
    public interface IMusicService
    {
        public bool IsAuthorized { get; init; }
        public void PasswordAuth(string login, string password, string twofactor);
        public void TokenAuth(string token);
        public Task<List<Song>> GetLikedSongs();
        public Task<List<Song>> AddSongList(List<Song> songs);
    }
}
