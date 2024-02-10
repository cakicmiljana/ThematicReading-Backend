using backend.model;

namespace backend.services;

public class ThemeService {


    private readonly IMongoClient _client;
    private readonly IMongoDatabase _db;
    public ThemeService(IMongoClient client)
    {
        _client = client;
        _db = _client.GetDatabase("Books");
    }

    public async Task AddGenresToTheme(Book book, FilterDefinition<Theme> themeFilter){

        var updateGenres = Builders<Theme>.Update.PushEach<string>(p=>p.Genres, book.Genres);
        await _db.GetCollection<Theme>("ThemeCollection").UpdateOneAsync(themeFilter, updateGenres);
    }

}