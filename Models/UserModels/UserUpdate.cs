namespace EnazaTestTask.Models
{
    public class UserUpdate
    {
        public int UserId { get; set; }
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int UserGroupId { get; set; }
        public int UserStateId { get; set; }
    }
}
