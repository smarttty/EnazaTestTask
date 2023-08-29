using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnazaTestTask.Models;
using EnazaTestTask.Utils;
using System.Security.Claims;

namespace EnazaTestTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly EnazaTestTaskContext _context;

        private CurrentUserTransactions _currentUserTransactions;

        public UsersController(EnazaTestTaskContext context, CurrentUserTransactions currentUserTransactions)
        {
            _context = context;
            _currentUserTransactions = currentUserTransactions;
        }

        /// <summary>
        /// Получение списка пользователей
        /// </summary>
        /// <returns>Список пользователей</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            return await _context.Users
                .Include(u => u.UserGroup)
                .Include(u => u.UserState)
                .ToListAsync();
        }

        /// <summary>
        /// Получение пользователя по ID
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <returns>Данные пользователя</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
            var user = await _context.Users
                .Include(u => u.UserGroup)
                .Include(u => u.UserState)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <param name="user">Обновленные данные пользователя</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UserUpdate userData)
        {
            if (id != userData.UserId)
            {
                return BadRequest();
            }

            User user = await UserUpdateToUser(userData);
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        /// <summary>
        /// Создание пользователя
        /// </summary>
        /// <param name="userData">Данные для создания пользователя</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(UserCreate userData)
        {
            if (_context.Users == null)
            {
                return Problem("Контекст подключения к БД не проинициализирован.");
            }

            if (!CurrentUserIsAdmin())
            {
                return BadRequest("Для создания пользователей необходимы права администратора");
            }

            // Только если в перечне текущих транзакций нет транзакции с данным логином пользователя
            if (!_currentUserTransactions.transactions.Any(t => t == userData.Login)) {
                // Блокируем ресурс, пока добавляем в него данные о транзакции
                lock (_currentUserTransactions.transactions)
                {
                    _currentUserTransactions.transactions.Add(userData.Login);
                }

                User user = UserCreateToUser(userData);
                UserState activeState = await _context.UserStates.FirstOrDefaultAsync(us => us.Code == "Active");

                // Проставляем статус при регистрации
                user.UserState = activeState;
                _context.Users.Add(user);

                try
                {
                    await Task.Delay(5000);
                    await _context.SaveChangesAsync();
                    /* 
                     * Не совсем понял, почему статус необходимо проставлять именно после успешной регистрации, ведь запись в БД - и есть успешная регистрация. Оставил код тут закомментированный.
                     * UserState activeState = await _context.UserStates.FirstOrDefaultAsync(us => us.Code == "Active");
                     * user.UserState = activeState;
                     * await _context.SaveChangesAsync();
                    */
                }
                catch (DbUpdateException e)
                {
                    if (UserExists(user.UserId))
                    {
                        return Conflict();
                    }
                    else
                    {
                        throw;
                    }
                }
                finally
                {
                    // Транзакция успешная, пользователь создан.
                    lock (_currentUserTransactions.transactions)
                    {
                        _currentUserTransactions.transactions.Remove(userData.Login);
                    }
                }

                return CreatedAtAction("GetUser", new { id = user.UserId }, user);
            }
            else
            {
                return BadRequest("Сохранение данного пользователя уже идет.");
            }

        }

        
        /// <summary>
        /// Удаление пользователя
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <returns>Код успеха/неуспеха</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }

            if (!CurrentUserIsAdmin())
            {
                return BadRequest("Для удаления пользователей необходимы права администратора");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            UserState blockedState = await _context.UserStates.FirstOrDefaultAsync(us => us.Code == "Blocked");
            user.UserState = blockedState;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Проверка, существует ли пользователь с таким ID
        /// </summary>
        /// <param name="id">ID пользователя</param>
        /// <returns>True/False</returns>
        private bool UserExists(int id)
        {
            return (_context.Users?.Any(e => e.UserId == id)).GetValueOrDefault();
        }

        /// <summary>
        /// Проверка по JWT токену, является ли текуий пользователь администратором
        /// </summary>
        /// <returns></returns>
        private bool CurrentUserIsAdmin()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            return identity.Claims.FirstOrDefault(c => c.Type == "Role")?.Value == "Admin";
        }

        /// <summary>
        /// Трансформация данных о пользователе в сущность User для сохранения
        /// Необходимо, чтобы убрать лишние поля из запроса на создание
        /// </summary>
        /// <param name="userData">Данные для создания пользователя</param>
        /// <returns>Экземпляр пользователя для сохранения</returns>
        private User UserCreateToUser(UserCreate userData)
        {
            return new User
            {
                Login = userData.Login,
                Password = PasswordHasher.SHA512(userData.Password),
                UserGroupId = userData.UserGroupId,
                CreatedDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Трансформация данных о пользователе в сущность User для сохранения
        /// Необходимо, чтобы убрать лишние поля из запроса на обновление и валидацию входящих данных
        /// </summary>
        /// <param name="userData">Данные для обновления пользователя</param>
        /// <returns>Экземпляр пользователя для сохранения</returns>
        private async Task<User> UserUpdateToUser(UserUpdate userData)
        {
            User user = await _context.Users.FindAsync(userData.UserId);
            user.Login = userData.Login;
            user.Password = PasswordHasher.SHA512(userData.Password);
            user.UserGroupId = userData.UserGroupId;
            user.UserStateId = userData.UserStateId;
            return user;
        }
    }
}
