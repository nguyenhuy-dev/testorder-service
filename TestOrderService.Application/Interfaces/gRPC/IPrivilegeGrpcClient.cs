using TestOrderService.Application.DTOs.gRPCs.GetPrivileges;
namespace TestOrderService.Application.Interfaces.gRPC
{
    /// <summary>
    ///     Interface for Privilege gRPC Client.
    /// </summary>
    public interface IPrivilegeGrpcClient
    {
        /// <summary>
        ///     Gets the privileges asynchronous.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<PrivilegeDto>> GetPrivilegesAsync();
    }
}
