namespace TestOrderService.Application.DTOs
{
    /// <summary>
    ///     Data transfer object for test order information with patient details
    /// </summary>
    public class TestOrderDto
    {
        /// <summary>
        ///     Gets or sets the test order identifier.
        /// </summary>
        /// <value>
        ///     The test order identifier.
        /// </value>
        public Guid TestOrderId { get; set; }

        /// <summary>
        ///     Gets or sets the patient identifier.
        /// </summary>
        /// <value>
        ///     The patient identifier.
        /// </value>
        public Guid PatientId { get; set; }

        /// <summary>
        ///     Gets or sets the full name of the patient.
        /// </summary>
        /// <value>
        ///     The full name.
        /// </value>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the age of the patient.
        /// </summary>
        /// <value>
        ///     The age calculated from date of birth.
        /// </value>
        public int Age { get; set; }

        /// <summary>
        ///     Gets or sets the phone number of the patient.
        /// </summary>
        /// <value>
        ///     The phone number.
        /// </value>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the gender of the patient.
        /// </summary>
        /// <value>
        ///     The gender (true = Male, false = Female).
        /// </value>
        public bool Gender { get; set; }

        /// <summary>
        ///     Gets or sets the status.
        /// </summary>
        /// <value>
        ///     The status.
        /// </value>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the created date.
        /// </summary>
        /// <value>
        ///     The created date.
        /// </value>
        public DateTime CreateAt { get; set; }

        /// <summary>
        ///     Gets or sets the user who created the test order.
        /// </summary>
        /// <value>
        ///     The create by identifier.
        /// </value>
        public Guid CreateById { get; set; }

        /// <summary>
        ///     Gets or sets the full name of the user who created the test order.
        /// </summary>
        /// <value>
        ///     The create by full name.
        /// </value>
        public string CreateByName { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the run date.
        /// </summary>
        /// <value>
        ///     The run date.
        /// </value>
        public DateTime? RunAt { get; set; }

        /// <summary>
        ///     Gets or sets the user who ran the test.
        /// </summary>
        /// <value>
        ///     The run by identifier.
        /// </value>
        public Guid? RunById { get; set; }

        /// <summary>
        ///     Gets or sets the full name of the user who ran the test.
        /// </summary>
        /// <value>
        ///     The run by full name.
        /// </value>
        public string? RunByName { get; set; }
    }
}
