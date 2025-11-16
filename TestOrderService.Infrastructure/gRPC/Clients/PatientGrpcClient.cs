using Patient_TestOrder_Service.API.gRPC.Protos.PatientProto;
using TestOrderService.Application.DTOs.gRPC.GetAllPatients;
using TestOrderService.Application.Interfaces.gRPC;
namespace TestOrderService.Infrastructure.gRPC.Clients
{
    public class PatientGrpcClient : IPatientGrpcClient
    {
        private readonly PatientService.PatientServiceClient _patientClient;

        public PatientGrpcClient(PatientService.PatientServiceClient patientClient)
        {
            _patientClient = patientClient;
        }

        /// <summary>
        ///     Gets all patients from patient service via gRPC
        /// </summary>
        /// <param name="cancellation">Cancellation token</param>
        /// <returns>List of patients</returns>
        public async Task<List<PatientDto>> GetAllPatients(CancellationToken cancellation)
        {
            var response = await _patientClient.GetAllPatientsAsync(new GetAllPatientsRequest(), cancellationToken: cancellation);

            var patients = response.Patients
                .Select(p =>
                {
                    try
                    {
                        // Parse required fields
                        if (!Guid.TryParse(p.PatientId, out var patientId))
                        {
                            return null; // Return null for invalid records
                        }

                        // Parse DateOnly from string
                        if (!DateOnly.TryParse(p.DateOfBirth, out var dateOfBirth))
                        {
                            return null; // Return null if date is invalid
                        }

                        // Parse nullable UserId
                        Guid? userId = null;
                        if (!string.IsNullOrEmpty(p.UserId))
                        {
                            if (Guid.TryParse(p.UserId, out var parsedUserId))
                                userId = parsedUserId;
                        }

                        return new PatientDto
                        {
                            PatientId = patientId,
                            PatientName = p.PatientName ?? string.Empty,
                            IdentityNumber = string.IsNullOrEmpty(p.IdentityNumber) ? null : p.IdentityNumber,
                            Gender = p.Gender,
                            DateOfBirth = dateOfBirth,
                            PhoneNumber = p.PhoneNumber ?? string.Empty,
                            Email = string.IsNullOrEmpty(p.Email) ? null : p.Email,
                            Address = p.Address ?? string.Empty,
                            IsActive = p.IsActive,
                            UserId = userId
                        };
                    }
                    catch
                    {
                        return null; // Skip invalid records
                    }
                })
                .Where(p => p != null) // Filter out nulls
                .ToList();

            return patients!;
        }
    }
}
