using apiTest.Data.ApplicationModel;
using apiTest.Data.CodeFirstMigration;
using apiTest.Data.DataAccess;
using apiTest.Data.ViewModel;
using apiTest.Data.ViewModel.Response;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace apiTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        #region Variable
        private readonly UserDbContext _userDbContext;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        public AuthenticationController(UserDbContext userDbContext, UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userDbContext = userDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }
        #endregion

        #region Method
        private async Task<List<Claim>> CreateClaims(ApplicationUser IdentityUser, IList<string> roles)
        {
            var authClaim = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,IdentityUser.Id),
                new Claim(ClaimTypes.Name,IdentityUser.UserName!),
                new Claim(ClaimTypes.Email,IdentityUser.Email),
                new Claim(ClaimTypes.MobilePhone,IdentityUser.PhoneNumber),
                new Claim(ClaimTypes.Country,IdentityUser.PasswordHash),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (roles.Count > 0)
            {
                foreach (var role in roles)
                {
                    authClaim.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            return authClaim;
        }
        private JwtSecurityToken CreateToken(List<Claim> claims)
        {
            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]!));
            var signInCredential = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256);

            _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
            var expirationTimeUtc = DateTime.UtcNow.AddMinutes(tokenValidityInMinutes);
            var localTimeZone = TimeZoneInfo.Local;
            var expirationTimeInLocalTimeZone = TimeZoneInfo.ConvertTimeFromUtc(expirationTimeUtc, localTimeZone);

            var token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: signInCredential,
                expires: expirationTimeInLocalTimeZone

                );
            return token;
        }

        #endregion
        #region Event

        [HttpPost("Login")]
        public async Task<ActionResult<Response>> Login(string username = "ishfaq", string password = "Ish1234271@")
        {
            try
            {
                var Identityuser = await _userManager.FindByNameAsync(username);
                if (Identityuser == null)
                    return new Response { IsSuccess = false, Status = "Failed", Message = "No Identity user Found" };
                var user = await _userDbContext.Users.FirstOrDefaultAsync(w => w.UserName == username);
                if (user == null)
                    return new Response { IsSuccess = false, Status = "Failed", Message = "No Registered user Found" };
                if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                    return new Response { IsSuccess = false, Status = "Failed", Message = "Password is not correct" };

                await _signInManager.SignInAsync(Identityuser, isPersistent: false);
                var roles = await _userManager.GetRolesAsync(Identityuser);
                var authClaim = await CreateClaims(Identityuser, roles);
                var token = CreateToken(authClaim);
                var jwt = new JwtSecurityTokenHandler().WriteToken(token);


                return new Response { IsSuccess = true, Status = "Success", Message = jwt };
            }
            catch (Exception ex)
            {

                return new Response { IsSuccess = false, Status = "Failed", Message = ex.Message };
            }
        }


        [HttpPost("Register")]
        public async Task<ActionResult<Response>> Register( RegisterDto model)
        {
            try
            {
                if(!ModelState.IsValid)
                    return new Response { IsSuccess = false, Status = "Failed", Message = "Model State is not valid" };

                var userCheck = await _userDbContext.Users.FirstOrDefaultAsync(w => w.UserName == model.UserName);
                if (userCheck != null)
                    return new Response { IsSuccess = false, Status = "Failed", Message = "User already exist" };

                string passwordHash= BCrypt.Net.BCrypt.HashPassword(model.Password);
                User user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PasswordHash = passwordHash,
                    DateOfBirth = model.DateOfBirth,
                    PhoneNumber = model.PhoneNumber,
                    InsertedDate=DateTime.Now,
                };
                await _userDbContext.Users.AddAsync(user);
                _userDbContext.SaveChanges();
                if(user.Id<1)
                    return new Response { IsSuccess = false, Status="Failed", Message="User Creation Failed" };

                //var role = "User";
                var role = "Admin";
                var identityUser = new ApplicationUser
                {
                    UserId = user.Id,
                    UserName = model.UserName,
                    NormalizedUserName = model.UserName,
                    Email = model.Email,
                    NormalizedEmail = model.Email,
                    ConcurrencyStamp= new Guid().ToString(),
                    SecurityStamp= new Guid().ToString(),
                    PhoneNumber= model.PhoneNumber,
                };
                var resultUser = await _userManager.CreateAsync(identityUser,model.Password);
                if (!resultUser.Succeeded)
                    return new Response { IsSuccess = false, Status = "Error", Message = "User failed to create." };
                if(!await _roleManager.RoleExistsAsync(role))
                    return new Response { IsSuccess = false, Status = "Error", Message = "This Role Does Not Exist" };

                var resultRole = await _userManager.AddToRoleAsync(identityUser,role);
                if (!resultRole.Succeeded)
                    return new Response { IsSuccess = false, Status = "Error", Message = "Failed to assign Role" };

                return new Response { IsSuccess = true ,Status="Success",Message="User Registration Successfully done",ObjResponse=user};

            }
            catch (Exception ex)
            {

                return new Response { IsSuccess = false, Status = "Failed", Message = "Exception Error" };
            }
        }

        [Authorize(Roles ="User,Admin")]
        [HttpGet("UserDetails")]
        public IEnumerable<User> UserDeatils()
        {
            var userDeatils =  _userDbContext.Users.ToList();
            return userDeatils;
        }
        #endregion


    }
}
