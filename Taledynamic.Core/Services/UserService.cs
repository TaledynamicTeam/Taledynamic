using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using Taledynamic.Core.Interfaces;
using Taledynamic.Core.Exceptions;
using Taledynamic.Core.Helpers;
using Taledynamic.DAL.Entities;
using Taledynamic.DAL.Models.DTOs;
using Taledynamic.DAL.Models.Requests.UserRequests;
using Taledynamic.DAL.Models.Responses.UserResponses;

namespace Taledynamic.Core.Services
{
    public class UserService : BaseService<User>, IUserService
    {
        private TaledynamicContext _context { get; set; }
        private IOptions<AppSettings> _appSettings { get; set; }
        private UserHelper _userHelper { get; set; }

        public UserService(TaledynamicContext context, IOptions<AppSettings> appSettings) : base(context)
        {
            _context = context;
            _appSettings = appSettings;
            _userHelper = new UserHelper(_appSettings);
        }

        public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request, string ipAddress)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'AuthenticateAsync' started.");
            
            var response = new AuthenticateResponse();

            var validator = request.IsValid();
            if (!validator.Status)
            {
                throw new BadRequestException(validator.Message);
            }

            User user = await GetUserByEmailAndPassword(request.Email, request.Password);

            var jwtToken = _userHelper.GenerateJwtToken(user);
            var refreshToken = _userHelper.GenerateRefreshToken(ipAddress);

            user.RefreshTokens.Add(refreshToken);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            response = new AuthenticateResponse()
            {
                Id = user.Id,
                Email = user.Email,
                JwtToken = jwtToken,
                RefreshToken = refreshToken.Token,
                Message = "Authenticate proccess ended with success.",
                StatusCode = HttpStatusCode.OK
            };
            
            Log.Information($"[{nameof(UserService)}]: Method 'AuthenticateAsync' ended.");
            
            return response;
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(string token, string ipAddress)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'RefreshTokenAsync' started.");
            
            var response = new RefreshTokenResponse();

            var user = await _context
                .Users
                .AsQueryable()
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token) && u.IsActive);

            if (user == null)
            {
                throw new NotFoundException("User with token is not found.");
            }

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!(refreshToken.Revoked == null || refreshToken.IsExpired))
            {
                throw new NotFoundException("Token is not active.");
            }

            var newRefreshToken = _userHelper.GenerateRefreshToken(ipAddress);
            if (newRefreshToken == null)
            {
                throw new InternalServerErrorException("New jwt token is not generated properly");
            }

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            refreshToken.IsActive = false;
            user.RefreshTokens.Add(newRefreshToken);
            _context.Update(user);
            await _context.SaveChangesAsync();

            var jwtToken = _userHelper.GenerateJwtToken(user);
            
            Log.Information($"[{nameof(UserService)}]: Method 'RefreshTokenAsync' ended.");

            return new RefreshTokenResponse
            {
                Id = user.Id,
                Email = user.Email,
                JwtToken = jwtToken,
                RefreshToken = newRefreshToken.Token,
                Message = "Refresh proccess ended with success.",
                StatusCode = HttpStatusCode.OK
            };
        }

        public async Task<RevokeTokenResponse> RevokeTokenAsync(string token, string ipAddress)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'RevokeTokenAsync' started.");
            
            var response = new RevokeTokenResponse
            {
                StatusCode = HttpStatusCode.OK,
                IsSuccess = false
            };

            var user = await _context
                .Users
                .AsQueryable()
                .SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token) && u.IsActive);

            if (user == null)
            {
                throw new NotFoundException("User with token is not found.");
            }

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!(refreshToken.Revoked == null || refreshToken.IsExpired))
            {
                throw new NotFoundException("Token is not active.");
            }

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _context.Update(user);
            await _context.SaveChangesAsync();

            response.IsSuccess = true;
            response.Message = "Success.";
            
            Log.Information($"[{nameof(UserService)}]: Method 'RevokeTokenAsync' ended.");
            
            return response;
        }

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest request, string ipAddress)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'CreateUserAsync' started.");
            
            var validator = request.IsValid();
            if (!validator.Status)
            {
                throw new BadRequestException(validator.Message);
            }

            var isExist = await IsUserEmailExistAsync(request.Email);

            if (isExist)
            {
                throw new BadRequestException("User with the same email already exist.");
            }

            User user = new User
            {
                IsActive = true,
                Email = request.Email,
                Password = request.Password,
                RefreshTokens = new List<RefreshToken>()
            };

            var refreshToken = _userHelper.GenerateRefreshToken(ipAddress);
            user.RefreshTokens.Add(refreshToken);
            await this.CreateAsync(user);
            var response = new CreateUserResponse
            {
                StatusCode = (HttpStatusCode) 200,
                Message = "Success."
            };
            
            Log.Information($"[{nameof(UserService)}]: Method 'CreateUserAsync' ended.");

            return response;
        }

        private async Task<bool> IsUserEmailExistAsync(string email)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'IsUserEmailExistAsync' started.");
            
            var isExist = await _context
                .Users
                .AsQueryable()
                .AnyAsync(u => u.Email == email && u.IsActive);
            
            Log.Information($"[{nameof(UserService)}]: Method 'IsUserEmailExistAsync' ended.");

            return isExist;
        }
        public async Task<DeleteUserResponse> DeleteUserAsync(DeleteUserRequest request)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'DeleteUserAsync' started.");
            
            var validator = request.IsValid();
            if (!validator.Status)
            {
                throw new BadRequestException(validator.Message);
            }

            var userId = request.UserId;
            await this.DeleteAsync(userId);
            var response = new DeleteUserResponse()
            {
                StatusCode = (HttpStatusCode) 200,
                Message = "Success."
            };
            
            Log.Information($"[{nameof(UserService)}]: Method 'DeleteUserAsync' ended.");

            return response;
        }

        private async Task<User> GetUserByEmailAndPassword(string email, string password)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'GetUserByEmailAndPassword' started.");
            
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                throw new BadRequestException(
                    $"These parameters: email: {email}; passwordHash: {password} is not set properly");
            }
            
            var user = await _context
                .Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password && u.IsActive);

            if (user == null)
            {
                throw new NotFoundException("Active user with this email is not found.");
            }
            
            Log.Information($"[{nameof(UserService)}]: Method 'GetUserByEmailAndPassword' ended.");

            return user;
        }

        private async Task<User> GetUserByIdAndPassword(int id, string password)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'GetUserByIdAndPassword' started.");
            
            if (id == default || string.IsNullOrEmpty(password))
            {
                throw new BadRequestException(
                    $"These parameters: id: {id}; passwordHash: {password} is not set properly");
            }
            
            var user = await _context
                .Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id && u.Password == password && u.IsActive);

            if (user == null)
            {
                throw new NotFoundException("Active user with this id is not found.");
            }
            
            Log.Information($"[{nameof(UserService)}]: Method 'GetUserByIdAndPassword' ended.");

            return user;
        }
        public async Task<UpdateUserResponse> UpdateUserAsync(UpdateUserRequest request)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'UpdateUserAsync' started.");
            
            var validator = request.IsValid();
            if (!validator.Status)
            {
                throw new BadRequestException(validator.Message);
            }

            var isExist = await IsUserEmailExistAsync(request.Email);

            if (isExist)
            {
                throw new BadRequestException("User with the same email already exist.");
            }
            
            await using var transation = await _context.Database.BeginTransactionAsync();
            var userId = request.Id;

            var oldUser =await GetUserByIdAndPassword(request.Id, request.Password);

            var linkedRefreshTokens = oldUser.RefreshTokens.ToList();
            oldUser.RefreshTokens.RemoveRange(0, oldUser.RefreshTokens.Count);
            await _context.SaveChangesAsync();

            
            User newUser = new User
            {
                IsActive = true,
                Email = oldUser.Email,
                Password = oldUser.Password,
                RefreshTokens = linkedRefreshTokens,
            };

            _context.ChangeTracker.Clear();
            await DeleteAsync(userId);

            newUser.Email = request.Email ?? newUser.Email;
            newUser.Password = request.NewPassword ?? newUser.Password;
            
            await this.CreateAsync(newUser);
            await UpdateWorkspacesForUser(oldUser, newUser);
            await transation.CommitAsync();
            
            var response = new UpdateUserResponse()
            {
                StatusCode = (HttpStatusCode) 200,
                Message = "Success.",
                User = new UserDto(newUser) 
            };
            
            Log.Information($"[{nameof(UserService)}]: Method 'UpdateUserAsync' ended.");

            return response;
        }

        private async Task UpdateWorkspacesForUser(User oldUser, User newUser)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'UpdateWorkspacesForUser' started.");
            
            if (oldUser == null || newUser == null)
            {
                throw new BadRequestException("User is not set");
            }
            
            var workspaces = _context.Workspaces.Where(w => w.User.Equals(oldUser));

            foreach (var workspace in workspaces)
            {
                workspace.User = newUser;
                _context.Update(workspace);
            }

            await _context.SaveChangesAsync();
            
            Log.Information($"[{nameof(UserService)}]: Method 'UpdateWorkspacesForUser' ended.");
        }
        public async Task<GetUserResponse> GetUserByIdAsync(GetUserRequest request)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'GetUserByIdAsync' started.");
            
            var validator = request.IsValid();
            if (!validator.Status)
            {
                throw new BadRequestException(validator.Message);
            }

            var userId = request.Id;
            var user = await this.GetByIdAsync(userId);

            //TODO move in validate for query method
            if (user == null || !user.IsActive)
            {
                throw new NotFoundException("User is not found.");
            }

            var response = new GetUserResponse()
            {
                StatusCode = (HttpStatusCode) 200,
                Message = "Success.",
                User = new UserDto()
                {
                    Email = user.Email,
                    Id = user.Id
                }
            };
            
            Log.Information($"[{nameof(UserService)}]: Method 'GetUserByIdAsync' ended.");

            return response;
        }

        public async Task<GetUsersResponse> GetUsersAsync(GetUsersRequest request)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'GetUsersAsync' started.");
            
            var users = (await this.GetAllAsync())
                .Where(u => u.IsActive)
                .Select(u => new UserDto()
                {
                    Id = u.Id,
                    Email = u.Email
                }).ToList();


            var response = new GetUsersResponse()
            {
                StatusCode = (HttpStatusCode) 200,
                Message = "Success.",
                Users = users
            };
            
            Log.Information($"[{nameof(UserService)}]: Method 'GetUsersAsync' ended.");

            return response;
        }

        public async Task<IsEmailUsedResponse> IsEmailUsedAsync(IsEmailUsedRequest request)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'IsEmailUsedAsync' started.");
            
            var validator = request.IsValid();
            if (!validator.Status)
            {
                throw new BadRequestException(validator.Message);
            }

            var user = await _context
                .Users
                .AsQueryable()
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            var response = new IsEmailUsedResponse()
            {
                StatusCode = (HttpStatusCode) 200,
                Message = "Success.",
                IsEmailUsed = user != null
            };
            
            Log.Information($"[{nameof(UserService)}]: Method 'IsEmailUsedAsync' ended.");

            return response;
        }

        public async Task<GetUserResponse> GetActiveUserByEmailAsync(GetActiveUserByEmailRequest request)
        {
            Log.Information($"[{nameof(UserService)}]: Method 'GetActiveUserByEmailAsync' started.");
            
            var validator = request.IsValid();
            if (!validator.Status)
            {
                throw new BadRequestException(validator.Message);
            }

            var user = await _context
                .Users
                .AsQueryable()
                .SingleOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null)
            {
                throw new NotFoundException("User is not found.");
            }

            UserDto userDto = new UserDto()
            {
                Email = user.Email,
                Id = user.Id,
            };


            var response = new GetUserResponse()
            {
                StatusCode = (HttpStatusCode) 200,
                Message = "User was found.",
                User = userDto
            };
            
            Log.Information($"[{nameof(UserService)}]: Method 'GetActiveUserByEmailAsync' ended.");

            return response;
        }
    }
}