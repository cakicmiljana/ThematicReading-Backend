using backend.model;

namespace backend.controllers;

[ApiController]
[Route("[controller]")]
public class ReviewController : ControllerBase
{
    
    private readonly IMongoClient _client;
    private readonly IMongoDatabase _db;
    public ReviewController(IMongoClient client)
    {
        _client = client;
        _db = _client.GetDatabase("Books");
    }

    [HttpPost("CreateReview")]
    public async Task<ActionResult> CreateReview([FromBody] Review review)
    {
        review.Id = ObjectId.GenerateNewId();
        await _db.GetCollection<Review>("ReviewCollection").InsertOneAsync(review);
        return Ok("Review created!" + review.Id);
    }

    [HttpGet("GetReviewById/{id}")]
    public async Task<ActionResult> GetReview(string id)
    {
        try{var filter = Builders<Review>.Filter.Eq(b => b.Id, new ObjectId(id));
        var review = await _db.GetCollection<Review>("ReviewCollection").Find(filter).FirstOrDefaultAsync();
        return Ok(new{
            Id = review.Id.ToString(),
            review.UserId,
            review.ThemeId,
            review.Rating,
            review.Comment
        });}
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("Get review failed!");
        }
    }

    [HttpPut("UpdateReview/{id}/{rating}/{comment}")]
    public async Task<ActionResult> UpdateReview(string id, int rating, string comment)
    {
        try{var filter = Builders<Review>.Filter.Eq(b => b.Id, new ObjectId(id));
        var review = await _db.GetCollection<Review>("ReviewCollection").Find(filter).FirstOrDefaultAsync();
        if (rating < 1 || rating > 5)
        {
            return BadRequest("Rating must be between 1 and 5");
        }
        review.Rating = rating;
        review.Comment = comment;
        await _db.GetCollection<Review>("ReviewCollection").ReplaceOneAsync(filter, review);
        return Ok("Review updated!");}
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("Update review failed!");
        }
    }

    [HttpDelete("DeleteReview/{id}")]
    public async Task<ActionResult> DeleteReview(string id)
    {
        try{var filter = Builders<Review>.Filter.Eq(b => b.Id, new ObjectId(id));
        await _db.GetCollection<Review>("ReviewCollection").DeleteOneAsync(filter);
        return Ok("Review deleted!");}
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("Review deletion failed!");
        }
    }

    [HttpGet("GetAllReviews")]
    public async Task<ActionResult> GetAllReviews()
    {
        try{var reviews = await _db.GetCollection<Review>("ReviewCollection").Find(b => true).ToListAsync();
        return Ok(reviews.Select(r => new {
            Id = r.Id.ToString(),
            r.UserId,
            r.ThemeId,
            r.Rating,
            r.Comment
        }).ToList());}
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("Get all reviews failed!");
        }
    }

    [HttpPost("LeaveReview/{userId}/{themeId}/{rating}/{comment}")]
    public async Task<ActionResult> LeaveReview(string userId, string themeId, int rating, string comment)
    {
        try{if (rating < 1 || rating > 5)
        {
            return BadRequest("Rating must be between 1 and 5");
        }
        Review review = new Review
        {
            Id = ObjectId.GenerateNewId(),
            UserId = userId,
            ThemeId = themeId,
            Rating = rating,
            Comment = comment
        };
        var themeFilter = Builders<Theme>.Filter.Eq(t => t.Id, new ObjectId(themeId));
        var updateFilter = Builders<Theme>.Update.Push(t => t.Reviews, review);
        await _db.GetCollection<Theme>("ThemeCollection").UpdateOneAsync(themeFilter, updateFilter);

        var theme = await _db.GetCollection<Theme>("ThemeCollection").Find(themeFilter).FirstOrDefaultAsync();
        double themeRating = theme.Reviews.Select(r => r.Rating).Average();
        theme.Rating = (decimal)themeRating;
        var updateRatingFilter = Builders<Theme>.Update.Set(t => t.Rating, theme.Rating);
        await _db.GetCollection<Theme>("ThemeCollection").UpdateOneAsync(themeFilter, updateRatingFilter);

        await _db.GetCollection<Review>("ReviewCollection").InsertOneAsync(review);
        return Ok("Review left!");}
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("Leave review failed!");
        }
    }
}