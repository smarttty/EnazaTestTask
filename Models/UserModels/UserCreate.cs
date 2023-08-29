namespace EnazaTestTask.Models
{
    public class UserCreate
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public int UserGroupId { get; set; }
    }
}
