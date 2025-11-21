namespace TestOrderService.Application.DTOs
{
    public class TestOrderDetailDto
    {
        /// <summary>
        ///     Gets or sets the test order identifier.
        /// </summary>
        /// <value>
        ///     The test order identifier.
        /// </value>
        public Guid TestOrderId { get; set; }
        /// <summary>
        ///     Gets or sets the status.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        public string Status { get; set; } = string.Empty;

        // TestOrder info
        /// <summary>
        ///     Gets or sets the create at.
        /// </summary>
        /// <value>
        ///     The create at.
        /// </value>
        public DateTime CreateAt { get; set; }
        /// <summary>
        ///     Gets or sets the test order description.
        /// </summary>
        /// <value>
        ///     The test order description.
        /// </value>
        public string? TestOrderDescription { get; set; }

        /// <summary>
        ///     Gets or sets the patient identifier.
        /// </summary>
        /// <value>
        ///     The patient identifier.
        /// </value>
        public Guid PatientId { get; set; }

        // Patient info
        /// <summary>
        ///     Gets or sets the name of the patient.
        /// </summary>
        /// <value>
        ///     The name of the patient.
        /// </value>
        public string PatientName { get; set; } = string.Empty;
        /// <summary>
        ///     Gets or sets the phone.
        /// </summary>
        /// <value>
        ///     The phone.
        /// </value>
        public string Phone { get; set; } = string.Empty;
        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="TestOrderDetailDto" /> is gender.
        /// </summary>
        /// <value>
        ///     <c>true</c> if gender; otherwise, <c>false</c>.
        /// </value>
        public bool Gender { get; set; }
        /// <summary>
        ///     Gets or sets the date of birth.
        /// </summary>
        /// <value>
        ///     The date of birth.
        /// </value>
        public DateOnly DateOfBirth { get; set; }
        /// <summary>
        ///     Gets or sets the age.
        /// </summary>
        /// <value>
        ///     The age.
        /// </value>
        public int Age { get; set; }
        /// <summary>
        ///     Gets or sets the address.
        /// </summary>
        /// <value>
        ///     The address.
        /// </value>
        public string Address { get; set; } = string.Empty;

        // Users
        /// <summary>
        ///     Gets or sets the created by.
        /// </summary>
        /// <value>
        ///     The created by.
        /// </value>
        public string CreatedBy { get; set; } = string.Empty;
        /// <summary>
        ///     Gets or sets the run by.
        /// </summary>
        /// <value>
        ///     The run by.
        /// </value>
        public string? RunBy { get; set; }
        /// <summary>
        ///     Gets or sets the review by.
        /// </summary>
        /// <value>
        ///     The review by.
        /// </value>
        public string? ReviewBy { get; set; }

        // Dates
        /// <summary>
        ///     Gets or sets the run at.
        /// </summary>
        /// <value>
        ///     The run at.
        /// </value>
        public DateTime? RunAt { get; set; }
        /// <summary>
        ///     Gets or sets the review at.
        /// </summary>
        /// <value>
        ///     The review at.
        /// </value>
        public DateTime? ReviewAt { get; set; }

        // Comments
        /// <summary>
        ///     Gets or sets the comments.
        /// </summary>
        /// <value>
        ///     The comments.
        /// </value>
        public List<CommentDto> Comments { get; set; } = new List<CommentDto>();
    }
}
