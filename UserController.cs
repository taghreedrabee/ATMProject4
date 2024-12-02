using ATMAPI.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using ATMAPI;

namespace ATMAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userRepository;
        private readonly TransactionRepository _transactionRepository;

        public UserController(UserRepository userRepository, TransactionRepository transactionRepository)
        {
            _userRepository = userRepository;
            _transactionRepository = transactionRepository;
        }

        [HttpPost("addUser")]
        public  IActionResult AddNewUser([FromBody] UserDTO userDTO) {

            if (userDTO == null) {
                return BadRequest("User object is null");
            }

            var user = new User
            {
                Username = userDTO.Username,
                PasswordHash = userDTO.PasswordHash,
                Email = userDTO.Email,
                BirthDate = userDTO.BirthDate,
                Type = (User.UserType)(int)userDTO.Type,
                Balance = userDTO.Balance,

            };
            if (_userRepository.UserExists(user.Username))
            {
                return BadRequest("user already exists");


            }
                _userRepository.AddUser(user);

                _transactionRepository.AddTransaction(new TransactionInfo(
                   0,
                    user.UserId,
                    user.Username,
                   "User Creation",
                   0,
                   DateTime.Now,
                   0,
                   0,
                   null,
                   true,
                   "Managerial"
               ));
                return Ok(new { message = "user created  successfully" });
            


        }

        [HttpPut("updateUser")]
        public IActionResult UpdateUser([FromQuery] int UserId, [FromBody] UserDTO userDTO)
        {

            if (userDTO == null)
            {
                return BadRequest("User object is null");
            }

            if (userDTO.UserId != UserId)
            {
                return BadRequest("UserId mismatch.");
            }

            var user = _userRepository.GetUserById(UserId);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.Username = userDTO.Username;
            user.PasswordHash = userDTO.PasswordHash;
            user.Email = userDTO.Email;
            user.BirthDate = userDTO.BirthDate;
            user.Type = (User.UserType)(int)userDTO.Type;
            user.Balance = userDTO.Balance;

            _userRepository.UpdateUser(user);
            _transactionRepository.AddTransaction(new TransactionInfo(
                0,
                user.UserId,
                user.Username,
                "User Info Update",
                0,
                DateTime.Now,
                0,
                0,
                null,
                true,
                "Managerial"
            ));
            return Ok("User information updated successfully.");
        }

        [HttpDelete("deleteUser")]
        public IActionResult DeleteUser([FromQuery] int userId) {
            _userRepository.DeleteUser(userId);
            return Ok("User deleted successfully." );
        }

        [HttpPost("login")]
        public IActionResult login([FromQuery] string username, string password)
        {
            User user = _userRepository.GetUserByUsername(username);

            if (user != null)
            {

                if (_userRepository.VerifyPassword(username, password))
                {
                    _transactionRepository.AddTransaction(new TransactionInfo(
                        0,
                        user.UserId,
                        user.Username,
                        "Login",
                        0,
                        DateTime.Now,
                        0,
                        0,
                        null,
                        true,
                        "Managerial"
                    ));
                    return Ok(new { message = "Logged in successfully" });
                    

                }
                else
                {
                    return BadRequest(new { message = "password is incorrect" });
                }
            }
            else
            {
            
                return BadRequest(new { message = "username is incorrect" });
                
            }

        }
    }   

        



}

