namespace Domain.Interfaces
{
    public interface IUser
    {
        string Username { get; set; }
        string GivenName { get; set; }
        string LastName { get; set; }
        string Email { get; set; }
        string Password { get; set; }
    }
}
