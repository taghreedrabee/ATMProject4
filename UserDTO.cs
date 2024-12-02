using ATMAPI.Data;

namespace ATMAPI.Data
{
    public class UserDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime BirthDate { get; set; }
        public UserType Type { get; set; }
        public double Balance { get; set; }


        public enum UserType
        {
            Ordinary,
            VIP
        }
    }

    
}
