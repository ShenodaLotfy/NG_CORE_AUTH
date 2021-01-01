using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NG_Core_Auth.Models;
using System.Security.Claims;
using NG_Core_Auth.Helpers;
using Microsoft.Extensions.Options;
using System.Text;

namespace NG_Core_Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signinManager;
        private readonly AppSettings _appSettings;

        // concept of dependency injection
        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signinManager, IOptions<AppSettings> appSettings)
        {
            _signinManager = signinManager;
            _userManager = userManager;
            _appSettings = appSettings.Value; // we here used Value because we use IOptions Interface 
        }

        // registration action
        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel formData)
        {
            // make a list to hold errors in registeration 
            List<string> registerErrors = new List<string>();

            var newUser = new IdentityUser
            {
                UserName = formData.UserName,
                Email = formData.Email,
                SecurityStamp = Guid.NewGuid().ToString() // updated whenever changes happens
            };

            // try to create a new user with a password supplied 
            var result = await _userManager.CreateAsync(newUser, formData.Password);

            // if newUser created successfully 
            if (result.Succeeded)
            {
                // if succeeded then add a role 
                await _userManager.AddToRoleAsync(newUser, "Customer");

                // we then return status of Ok, with an object of user data to be stored in local storage
                return Ok(new { usename = formData.UserName, email = formData.Email, status = 1, message = "Registeration Successful!" });
            } 
            // else if newUser unregistered
            else
            {
                // then registeration failed 
                // add each error in ModelState and in the registerError list that we created
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    registerErrors.Add(error.Description);
                }
            }

            // finally return the error list 
            return BadRequest(new JsonResult(registerErrors));

        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel formData)
        {
            // check the user if he's exist in the database 
            var user = await _userManager.FindByNameAsync(formData.UserName);

            // get the roles, use it to assign token Role claim 
            var roles = await _userManager.GetRolesAsync(user);

            // get the Secret key from AppSettings class and encrypt it 
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_appSettings.Secret));

            // now get token ExpireTime from AppSettings class, convert string value to double
            double tokenExpiryTime = Convert.ToDouble(_appSettings.ExpireTime);

            // if user exist and username/password are valid 
            if( user != null && await _userManager.CheckPasswordAsync( user, formData.Password ))
            {
                // check email confirmation..... to be continued

                // now we are going to generate a token 
                // 1- step one is to create token handler 
                var tokenHandler = new JwtSecurityTokenHandler();

                // 2- step two - create token descriptor that has token information
                var tokenDiscriptor = new SecurityTokenDescriptor
                {
                    // add claims =>>>> ClaimsIdentity( array of claims ) 
                    Subject = new ClaimsIdentity(
                        new Claim[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, formData.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // to prevent duplicated tokens 
                            new Claim(ClaimTypes.NameIdentifier, user.Id),
                            new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                            new Claim("LoggedOn" , DateTime.Now.ToString())

                        }),
                    // used to create security tokens 
                    SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _appSettings.Site,
                    Audience = _appSettings.Audience,
                    Expires = DateTime.UtcNow.AddMinutes(tokenExpiryTime)
                };

                // last thing to generate the token 
                var token = tokenHandler.CreateToken(tokenDiscriptor);

                return Ok(new {
                    token = tokenHandler.WriteToken(token),
                    expiration = token.ValidTo, 
                    username = user.UserName, 
                    userRole = roles.FirstOrDefault()
                });
            }


            // if user doesnt exist 
            ModelState.AddModelError("", "Username/Password is not valid!");
            return Unauthorized(new { loginError = "Please check the login credintials - invalid username/password was entered" });
        }

    }
}
