using IAMService.API.gRPC.Protos.UserProto;
using TestOrderService.Application.DTOs.gRPC.GetAllUsers;
using TestOrderService.Application.Interfaces.gRPC;
namespace TestOrderService.Infrastructure.gRPC.Clients
{
    public class UserGrpcClient : IUserGrpcClient
    {
        private readonly UserService.UserServiceClient _userClient;

        public UserGrpcClient(UserService.UserServiceClient userClient)
        {
            _userClient = userClient;
        }

        /// <summary>
        ///     Gets all users from IAM service via gRPC
        /// </summary>
        /// <param name="cancellation">Cancellation token</param>
        /// <returns>List of users</returns>
        public async Task<List<UserDto>> GetAllUsers(CancellationToken cancellation)
        {
            var response = await _userClient.GetAllUsersAsync(new GetAllUsersRequest(), cancellationToken: cancellation);

            var users = response.Users
                .Select(u =>
                {
                    try
                    {
                        // Parse required fields
                        if (!Guid.TryParse(u.UserId, out var userId))
                        {
                            return null; // Return null for invalid records
                        }

                        return new UserDto
                        {
                            UserId = userId,
                            FullName = u.FullName ?? string.Empty,
                            Email = u.Email ?? string.Empty,
                            IsActive = u.IsActive,
                            Role = new RoleDto
                            {
                                RoleId = u.Role.RoleId,
                                RoleName = u.Role.RoleName
                            }
                        };
                    }
                    catch
                    {
                        return null; // Skip invalid records
                    }
                })
                .Where(u => u != null) // Filter out nulls
                .ToList();

            return users!;
        }

        public async Task<UserDto?> GetUserById(Guid userId, CancellationToken cancellation)
        {
            var response = await _userClient.GetAllUsersAsync(new GetAllUsersRequest(), cancellationToken: cancellation);

            var user = response.Users
                .Select(u =>
                {
                    try
                    {
                        // Parse required fields
                        if (!Guid.TryParse(u.UserId, out var userId))
                        {
                            return null; // Return null for invalid records
                        }

                        return new UserDto
                        {
                            UserId = userId,
                            FullName = u.FullName ?? string.Empty,
                            Email = u.Email ?? string.Empty,
                            IsActive = u.IsActive,
                            Role = new RoleDto
                            {
                                RoleId = u.Role.RoleId,
                                RoleName = u.Role.RoleName
                            }
                        };
                    }
                    catch
                    {
                        return null; // Skip invalid records
                    }
                })
                .FirstOrDefault(u => u != null && u.UserId == userId); // Filter out nulls

            return user;
        }
    }
}
