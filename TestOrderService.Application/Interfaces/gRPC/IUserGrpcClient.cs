using TestOrderService.Application.DTOs.gRPC.GetAllUsers;
namespace TestOrderService.Application.Interfaces.gRPC
{
    public interface IUserGrpcClient
    {
        /// <summary>
        ///     Gets all users from IAM service via gRPC
        /// </summary>
        /// <param name="cancellation">Cancellation token</param>
        /// <returns>List of users</returns>
        Task<List<UserDto>> GetAllUsers(CancellationToken cancellation);
    }
}
