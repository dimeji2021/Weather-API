﻿using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using WeatherAPI.Domain.Dto;
using WeatherAPI.Domain.Enum;
using WeatherAPI.Domain.Model;



namespace WeatherAPI.Domain.Core.Service
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static List<User> _users = new List<User>();

        public AuthService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }
        public async Task<ResponseDto<User>> Register(UserDto request)
        {
            var checkIfEmailAlreadyExist = _users.Where(u => u.Email == request.Email).FirstOrDefault();
            if (checkIfEmailAlreadyExist is not null)
            {
                return ResponseDto<User>.Fail("Email already exist", (int)HttpStatusCode.BadRequest);
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = UserRole.Customer
            };
            _users.Add(user);
            return ResponseDto<User>.Success("Registration is successful", user, (int)HttpStatusCode.OK);
        }
        public async Task<ResponseDto<User>> RegisterAdmin(UserDto request)
        {
            var checkIfEmailAlreadyExist = _users.Where(u => u.Email == request.Email).FirstOrDefault();
            if (checkIfEmailAlreadyExist is not null)
            {
                return ResponseDto<User>.Fail("Email already exist", (int)HttpStatusCode.BadRequest);
            }

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = UserRole.Admin
            };
            _users.Add(user);
            return ResponseDto<User>.Success("Registration is successful", user, (int)HttpStatusCode.OK);
        }
        public async Task<ResponseDto<string>> Login(UserDto request)
        {
            var user = _users.Where(u => u.Email == request.Email).FirstOrDefault();

            if (user is null)
            {
                return ResponseDto<string>.Fail("User not found", (int)HttpStatusCode.NotFound);
            }
            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                return ResponseDto<string>.Fail("Wrong password", (int)HttpStatusCode.BadRequest);
            }

            string token = CreateToken(user, user.Role.ToString());

            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken);

            return ResponseDto<string>.Success("Login is successful", token, (int)HttpStatusCode.OK);
        }
        public async Task<ResponseDto<string>> RefreshToken()
        {
            var refreshToken = _httpContextAccessor?.HttpContext?.Request.Cookies["refreshToken"];
            var user = _users.FirstOrDefault(u => u.RefreshToken.Equals(refreshToken));
            if (user is null)
            {
                return ResponseDto<string>.Fail("Invalid Refresh Token.", (int)HttpStatusCode.Unauthorized);
            }
            else if (user.TokenExpires < DateTime.Now)
            {
                return ResponseDto<string>.Fail("Token expired.", (int)HttpStatusCode.Unauthorized);
            }

            string token = CreateToken(user, user.Role.ToString());
            var newRefreshToken = GenerateRefreshToken();
            SetRefreshToken(newRefreshToken);
            return ResponseDto<string>.Success("Token successfully refreshed.", token, (int)HttpStatusCode.OK);
        }

        private string CreateToken(User user, string role)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("Jwt:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("Jwt:Issuer"),
                audience: _configuration.GetValue<string>("Jwt:Audience"),
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration.GetSection("lifetime").Value)),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };

            return refreshToken;
        }

        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            _httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
            _users.Select(user =>
            {
                user.RefreshToken = newRefreshToken.Token;
                user.TokenCreated = newRefreshToken.Created;
                user.TokenExpires = newRefreshToken.Expires;
                return user;
            }).ToList();
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}
