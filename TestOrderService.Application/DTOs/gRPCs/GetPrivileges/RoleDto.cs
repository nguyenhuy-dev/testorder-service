namespace TestOrderService.Application.DTOs.gRPCs.GetPrivileges
{
    /// <summary>
    ///     Role data tranfer object.
    /// </summary>
    public class RoleDto
    {
        /// <summary>
        ///     Gets or sets the role identifier.
        /// </summary>
        /// <value>
        ///     The role identifier.
        /// </value>
        public int RoleId { get; set; }

        /// <summary>
        ///     Gets or sets the role code.
        /// </summary>
        /// <value>
        ///     The role code.
        /// </value>
        public string RoleCode { get; set; } = default!;
    }
}
