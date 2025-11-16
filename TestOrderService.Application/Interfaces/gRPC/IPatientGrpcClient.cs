using TestOrderService.Application.DTOs.gRPC.GetAllPatients;
namespace TestOrderService.Application.Interfaces.gRPC
{
    public interface IPatientGrpcClient
    {
        /// <summary>
        ///     Gets all patients from patient service via gRPC
        /// </summary>
        /// <param name="cancellation">Cancellation token</param>
        /// <returns>List of patients</returns>
        Task<List<PatientDto>> GetAllPatients(CancellationToken cancellation);
    }
}
