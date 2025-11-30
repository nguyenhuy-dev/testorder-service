using Mapster;
using MediatR;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Exceptions;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
namespace TestOrderService.Application.Features.TestOrders.Queries.GetDetail
{
    /// <summary>
    ///     Handles the logic for retrieving detailed information of a TestOrder,
    ///     including patient info and related staff from external gRPC services.
    /// </summary>
    public class GetTestOrderDetailQueryHandler
        : IRequestHandler<GetTestOrderDetailQuery, TestOrderDetailDto>
    {
        /// <summary>
        ///     The patient GRPC
        /// </summary>
        private readonly IPatientGrpcClient _patientGrpc;
        /// <summary>
        ///     The repo
        /// </summary>
        private readonly ITestOrderRepository _repo;
        /// <summary>
        ///     The test definition GRPC
        /// </summary>
        private readonly ITestDefinitionGrpcClient _testDefGrpc;
        /// <summary>
        ///     The user GRPC
        /// </summary>
        private readonly IUserGrpcClient _userGrpc;
        /// <summary>
        ///     Initializes a new instance of the <see cref="GetTestOrderDetailQueryHandler" /> class.
        /// </summary>
        /// <param name="repo">The repo.</param>
        /// <param name="patientGrpc">The patient GRPC.</param>
        /// <param name="userGrpc">The user GRPC.</param>
        public GetTestOrderDetailQueryHandler(
            ITestOrderRepository repo,
            IPatientGrpcClient patientGrpc,
            IUserGrpcClient userGrpc,
            ITestDefinitionGrpcClient testDefGrpc)
        {
            _repo = repo;
            _patientGrpc = patientGrpc;
            _userGrpc = userGrpc;
            _testDefGrpc = testDefGrpc;
        }

        /// <summary>
        ///     Handles the specified req.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="TestOrderService.Application.Exceptions.NotFoundException">
        ///     TestOrder with id '{req.TestOrderId}' was not found.
        ///     or
        ///     Patient with id '{t.PatientId}' was not found.
        /// </exception>
        public async Task<TestOrderDetailDto> Handle(GetTestOrderDetailQuery req, CancellationToken cancellationToken)
        {
            var t = await _repo.GetByIdAsync(req.TestOrderId, cancellationToken)
                 ?? throw new NotFoundException($"TestOrder with id '{req.TestOrderId}' was not found.");

            var patients = await _patientGrpc.GetAllPatients(cancellationToken);
            var patient = patients.FirstOrDefault(x => x.PatientId == t.PatientId)
                       ?? throw new NotFoundException($"Patient with id '{t.PatientId}' was not found.");

            var users = await _userGrpc.GetAllUsers(cancellationToken);

            var createdBy = users.FirstOrDefault(u => u.UserId == t.CreateById)?.FullName ?? "Unknown";
            var runBy = users.FirstOrDefault(u => u.UserId == t.RunById)?.FullName;
            var reviewBy = users.FirstOrDefault(u => u.UserId == t.ReviewId)?.FullName;

            var today = DateOnly.FromDateTime(DateTime.Today);
            var age = today.Year - patient.DateOfBirth.Year;
            if (patient.DateOfBirth > today.AddYears(-age)) age--;

            var commentList = new List<CommentDto>();

            foreach (var comment in t.Comments)
            {
                commentList.Add(comment.Adapt<CommentDto>());
            }

            var testDefinitions = await _testDefGrpc.GetAllTestDefinitions(cancellationToken);

            var testResultDtos = t.TestResults.Select(r =>
            {
                var def = testDefinitions.FirstOrDefault(d => d.TestDefinitionId == r.TestDefinitionId);

                return new TestResultDto
                {
                    TestResultId = r.TestResultId,
                    Value = r.Value,
                    Flag = r.Flag,
                    Status = r.Status.ToString(),
                    TestDefinitionId = r.TestDefinitionId,
                    CreatedAt = r.CreatedAt,
                    CreatedBy = r.CreatedBy,
                    ReviewedBy = r.ReviewedBy,
                    ReviewedAt = r.ReviewedAt,
                    TestResultDescription = r.TestResultDescription,

                    //  NEW FIELDS
                    TestName = def?.TestName,
                    Unit = def?.Unit,
                    TestResultComments = r.TestResultComments.Select(c => c.Adapt<TestResultCommentDto>()).ToList()
                };
            }).ToList();

            // BUILD RESULT DTO
            return new TestOrderDetailDto
            {
                TestOrderId = t.TestOrderId,
                Status = t.Status.ToString(),
                CreateAt = t.CreateAt,
                TestOrderDescription = t.TestOrderDescription,

                PatientId = patient.PatientId,
                PatientName = patient.PatientName,
                Phone = patient.PhoneNumber,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                Age = age,
                Address = patient.Address,

                RunAt = t.RunAt,
                ReviewAt = t.ReviewAt,
                CreatedBy = createdBy,
                RunBy = runBy,
                ReviewBy = reviewBy,

                Comments = commentList,
                TestResults = testResultDtos,

                // Include AIReviewSummary
                AIReviewSummary = t.AIReviewSummary
            };
        }
    }
}
