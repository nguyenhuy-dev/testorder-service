using Microsoft.EntityFrameworkCore;
using TestOrderService.Application.DTOs;
using TestOrderService.Application.Interfaces;
using TestOrderService.Application.Interfaces.gRPC;
using TestOrderService.Domain.Entities;
using TestOrderService.Infrastructure.Data;
namespace TestOrderService.Infrastructure.Repositories
{
    /// <summary>
    ///     Test order repository implement.
    /// </summary>
    /// <seealso cref="TestOrderService.Application.Interfaces.ITestOrderRepository" />
    public class TestOrderRepository : ITestOrderRepository
    {
        /// <summary>
        ///     The database context
        /// </summary>
        private readonly TestOrderServiceDbContext _dbContext;
        private readonly IPatientGrpcClient _patientGrpcClient;
        private readonly IUserGrpcClient _userGrpcClient;

        public TestOrderRepository(TestOrderServiceDbContext dbContext, IPatientGrpcClient patientGrpcClient, IUserGrpcClient userGrpcClient)
        {
            _dbContext = dbContext;
            _patientGrpcClient = patientGrpcClient;
            _userGrpcClient = userGrpcClient;
        }

        /// <summary>
        ///     Creates the test order.
        /// </summary>
        /// <param name="testOrder">The test order.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public async Task<TestOrder> CreateTestOrderAsync(TestOrder testOrder, CancellationToken cancellationToken)
        {
            // Temp
            testOrder.RunById = Guid.Parse("00000000-0000-0000-0000-000000000001");
            testOrder.RunAt = DateTime.UtcNow.AddHours(2);

            await _dbContext.AddAsync(testOrder, cancellationToken);

            return testOrder;
        }

        /// <summary>
        ///     Gets the all test orders using the specified cancellation token
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing an enumerable of test order</returns>
        public async Task<IEnumerable<TestOrder>> GetAllTestOrdersAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.TestOrders.ToArrayAsync(cancellationToken);
        }
        /// <summary>
        ///     Gets the by id using the specified id
        /// </summary>
        /// <param name="id">The id</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task containing the test order</returns>
        public async Task<TestOrder?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.TestOrders
                .FirstOrDefaultAsync(t => t.TestOrderId == id, cancellationToken);
        }

        /// <summary>
        ///     Delete TestOrder (hard delete).
        /// </summary>
        public void Delete(TestOrder entity)
        {
            _dbContext.TestOrders.Remove(entity);
        }
        /// <summary>
        ///     Gets test orders with pagination, filtering, and patient information from gRPC.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Paginated list of test orders with patient details.</returns>
        public async Task<PaginatedList<TestOrderDto>> GetTestOrdersAsync(GetTestOrdersRequest request, CancellationToken cancellationToken = default)
        {
            // Get test orders from database
            var testOrders = await _dbContext.TestOrders.AsNoTracking().ToListAsync(cancellationToken);

            // Get patients from gRPC service
            var patients = await _patientGrpcClient.GetAllPatients(cancellationToken);
            var patientDict = patients.ToDictionary(p => p.PatientId);

            // Get users from gRPC service
            var users = await _userGrpcClient.GetAllUsers(cancellationToken);
            var userDict = users.ToDictionary(u => u.UserId);

            // Join data and create DTOs
            var projected = testOrders
                .Where(t => patientDict.ContainsKey(t.PatientId))
                .Select(t =>
                {
                    var patient = patientDict[t.PatientId];

                    // Calculate age from date of birth
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var age = today.Year - patient.DateOfBirth.Year;
                    if (patient.DateOfBirth > today.AddYears(-age)) age--;

                    // Get creator name
                    var createByName = userDict.ContainsKey(t.CreateById)
                        ? userDict[t.CreateById].FullName
                        : string.Empty;

                    // Get runner name if exists
                    string? runByName = null;
                    if (t.RunById != Guid.Empty && userDict.ContainsKey(t.RunById))
                    {
                        runByName = userDict[t.RunById].FullName;
                    }

                    return new TestOrderDto
                    {
                        TestOrderId = t.TestOrderId,
                        PatientId = t.PatientId,
                        FullName = patient.PatientName,
                        Age = age,
                        DateOfBirth = patient.DateOfBirth,
                        Phone = patient.PhoneNumber,
                        Gender = patient.Gender,
                        Status = t.Status.ToString(),
                        CreateAt = t.CreateAt,
                        CreateById = t.CreateById,
                        CreateByName = createByName,
                        RunAt = t.RunAt,
                        RunById = t.RunById,
                        RunByName = runByName
                    };
                }).ToList();

            // Apply status filter if provided
            var filtered = projected.AsQueryable();
            if (request.Status.HasValue)
            {
                var statusString = request.Status.Value.ToString();
                filtered = filtered.Where(t => t.Status.Equals(statusString, StringComparison.OrdinalIgnoreCase));
            }

            // Sorting by CreateAt (newest first)
            var ordered = filtered.OrderByDescending(t => t.CreateAt);

            // Get total count
            var totalCount = ordered.Count();

            // Apply pagination
            var page = Math.Max(1, request.PageNumber);
            var pageSize = Math.Max(1, request.PageSize);
            var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Return paginated result
            return new PaginatedList<TestOrderDto>(items, totalCount, page, pageSize);
        }
    }
}
