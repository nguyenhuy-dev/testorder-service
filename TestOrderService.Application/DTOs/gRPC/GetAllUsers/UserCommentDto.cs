namespace TestOrderService.Application.DTOs.gRPC.GetAllUsers
{
    public class UserCommentDto
    {
        /// <summary>
        ///     Gets or sets the user identifier.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        ///     Gets or sets the full name.
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a value indicating whether this user is active.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
