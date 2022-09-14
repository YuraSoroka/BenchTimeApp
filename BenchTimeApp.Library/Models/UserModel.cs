namespace BenchTimeApp.Library.Models;
public class UserModel
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string ObjectIdentifier { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public string EmailAddress { get; set; }
    public List<BasicSuggestionModel> AuthoredSuggestions { get; set; } = new List<BasicSuggestionModel>();
    public List<BasicSuggestionModel> VotedOnSuggestions { get; set; } = new List<BasicSuggestionModel>();
}
