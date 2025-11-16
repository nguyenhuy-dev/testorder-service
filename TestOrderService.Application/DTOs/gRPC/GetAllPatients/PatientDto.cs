namespace TestOrderService.Application.DTOs.gRPC.GetAllPatients
{
    public class PatientDto
    {
        /// <summary>
        ///     Gets or sets the patient identifier.
        /// </summary>
        public Guid PatientId { get; set; }

        /// <summary>
        ///     Gets or sets the patient name.
        /// </summary>
        public string PatientName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the identity number (CCCD/CMND).
        /// </summary>
        public string? IdentityNumber { get; set; }

        /// <summary>
        ///     Gets or sets the gender (true = Male, false = Female).
        /// </summary>
        public bool Gender { get; set; }

        /// <summary>
        ///     Gets or sets the date of birth.
        /// </summary>
        public DateOnly DateOfBirth { get; set; }

        /// <summary>
        ///     Gets or sets the phone number.
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the email.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        ///     Gets or sets the address.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets a value indicating whether this patient is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        ///     Gets or sets the user identifier (linked to IAM service).
        /// </summary>
        public Guid? UserId { get; set; }
    }
}
