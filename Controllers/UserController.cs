using backend.model;
using backend.services;
namespace backend.controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{

    private readonly IMongoClient _client;
    private readonly IMongoDatabase _db;
    private readonly StatisticsService _statisticsServices;
    public UserController(IMongoClient client)
    {
        _client = client;
        _db = _client.GetDatabase("Books");
        _statisticsServices = new StatisticsService(_client);
    }

    [HttpPost("CreateUser")]
    public async Task<ActionResult> CreateUser([FromBody] User user)
    {
        user.Id = ObjectId.GenerateNewId();
        user.Statistics = await _statisticsServices.CreateStatistics(user.Id.ToString());
        await _db.GetCollection<User>("UserCollection").InsertOneAsync(user);
        return Ok("User created!" + user.Id);
    }

    [HttpGet("GetUserById/{id}")]
    public async Task<ActionResult> GetUser(string id)
    {
        var filter = Builders<User>.Filter.Eq(b => b.Id, new ObjectId(id));
        var user = await _db.GetCollection<User>("UserCollection").Find(filter).FirstOrDefaultAsync();
        return Ok(user);
    }

    [HttpPut("UpdateUser/{id}/{country}")]
    public async Task<ActionResult> UpdateUser(string id, string country)
    {
        var filter = Builders<User>.Filter.Eq(b => b.Id, new ObjectId(id));
        var user = await _db.GetCollection<User>("UserCollection").Find(filter).FirstOrDefaultAsync();
        user.Country = country;
        await _db.GetCollection<User>("UserCollection").ReplaceOneAsync(filter, user);
        return Ok("User updated!");
    }

    [HttpPut("ChangePassword/{id}")]
    public async Task<ActionResult> ChangePassword(string id, [FromBody] string password)
    {
        var filter = Builders<User>.Filter.Eq(b => b.Id, new ObjectId(id));
        var user = await _db.GetCollection<User>("UserCollection").Find(filter).FirstOrDefaultAsync();
        user.Password = password;
        await _db.GetCollection<User>("UserCollection").ReplaceOneAsync(filter, user);
        return Ok("Password updated!");
    }

    [HttpDelete("DeleteUser/{id}")]
    public async Task<ActionResult> DeleteUser(string id)
    {
        var filter = Builders<User>.Filter.Eq(b => b.Id, new ObjectId(id));
        await _db.GetCollection<User>("UserCollection").DeleteOneAsync(filter);
        return Ok("User deleted!");
    }

    [HttpGet("GetAllUsers")]
    public async Task<ActionResult> GetAllUsers()
    {
        var users = await _db.GetCollection<User>("UserCollection").FindAsync(b => true).Result.ToListAsync();
        return Ok(users);
    }

    [HttpPut("ApplyForTheme/{userId}/{themeId}")]
    public async Task<ActionResult> ApplyForTheme(string userId, string themeId)
    {
        var userFilter = Builders<User>.Filter.Eq(u => u.Id, new ObjectId(userId));
        var themeFilter = Builders<Theme>.Filter.Eq(t => t.Id, new ObjectId(themeId));
        var theme = await _db.GetCollection<Theme>("ThemeCollection").Find(themeFilter).FirstOrDefaultAsync();
        var updateFilter = Builders<User>.Update.Push<string>(p => p.ThemeIDs, themeId);
        await _db.GetCollection<User>("UserCollection").UpdateOneAsync(userFilter, updateFilter);
        return Ok("Applied for theme!");
    }
}