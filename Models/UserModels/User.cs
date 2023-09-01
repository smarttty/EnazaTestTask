using System.ComponentModel.DataAnnotations;

namespace EnazaTestTask.Models
{
    public partial class User : IValidatableObject
    {
        public int UserId { get; set; }
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public int UserGroupId { get; set; }
        public int UserStateId { get; set; }

        public virtual UserGroup? UserGroup { get; set; } = null!;
        public virtual UserState? UserState { get; set; } = null!;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> validationErrors = new List<ValidationResult>();
            var _context = validationContext.GetService<EnazaTestTaskContext>();
            UserGroup adminUserGroup = _context.UserGroups.FirstOrDefault(ug => ug.Code == "Admin");
            UserState blockedUserState = _context.UserStates.FirstOrDefault(ug => ug.Code == "Blocked");
            if (UserGroupId == adminUserGroup.UserGroupId && _context.Users.Any(u => u.UserGroup.Code == "Admin" && u.UserId != UserId))
            {
                validationErrors.Add(new ValidationResult("Пользователь с ролью \"Администратор\" уже существует"));
            }
            bool anyUserWithSameLogin = _context.Users.Any(u => u.Login == Login && u.UserId != UserId);
            if (anyUserWithSameLogin)
            {
                validationErrors.Add(new ValidationResult($"Пользователь с логином \"{Login}\" уже существует"));
            }
            bool adminUserBlock = UserGroupId == adminUserGroup.UserGroupId && UserStateId == blockedUserState.UserStateId;
            if (adminUserBlock)
            {
                validationErrors.Add(new ValidationResult($"Нельзя заблокировать единственного администратора!"));
            }
            return validationErrors;
        }
    }
}
