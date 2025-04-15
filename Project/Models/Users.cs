//GPT: Help me with my project that I sent you. I need a login page
//Store user login information in a file named users.json located under wwwroot/data/
//You must define a User class (Models/User.cs) to match the structure of this JSON file. I sent you the representation.

//When the login form is submitted, read the users from the JSON file.
//Check whether the given credentials match an active user in the list.

/*Upon successful login:
Generate a simple token
▪ Store the following in the session:
▪ username
▪ token
▪ session_id (use HttpContext.Session.Id)
▪ Store the same values in cookies using the following cookie settings:
▪ Expires in 30 minutes
▪ HttpOnly = true
▪ Secure = true
▪ SameSite = Strict*/

//Also a logout button is necessarry.

//On all protected pages, check whether the token, username, and session_id from cookies match those in the session.
//If both token and username values match between the session and cookie, then you may consider the login valid.
//If the check fails, use errors and warnings to say “username or password is incorrect.” Or something like this message

//I also sent you the screenshots of format in sessions and cookies

namespace Week2.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
