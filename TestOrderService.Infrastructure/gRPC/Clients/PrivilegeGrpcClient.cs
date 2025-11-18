using IAMService.API.gRPC.Protos;
using TestOrderService.Application.DTOs.gRPCs.GetPrivileges;
using TestOrderService.Application.Interfaces.gRPC;
namespace TestOrderService.Infrastructure.gRPC.Clients
{
    /// <summary>
    ///     Privilege gRPC Client.
    /// </summary>
    /// <seealso cref="TestOrderService.Application.Interfaces.gRPC.IPrivilegeGrpcClient" />
    public class PrivilegeGrpcClient(Privilege.PrivilegeClient privilegeClient) : IPrivilegeGrpcClient
    {
        /// <summary>
        ///     The privilege client
        /// </summary>
        private readonly Privilege.PrivilegeClient _privilegeClient = privilegeClient;

        /// <summary>
        ///     Gets the privileges asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<PrivilegeDto>> GetPrivilegesAsync()
        {
            var response = await _privilegeClient.GetAllPrivilegesAsync(new Empty());
            return response.Privileges.Select(p => new PrivilegeDto
            {
                PrivilegeId = p.PrivilegeId,
                PrivilegeName = p.PrivilegeName,
                Roles = p.Roles.Select(r => new RoleDto
                {
                    RoleId = r.RoleId,
                    RoleCode = r.RoleCode
                }).ToList()
            });
        }
    }
}
