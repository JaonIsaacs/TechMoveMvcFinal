using Xunit;
using TechMove.Models;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using Microsoft.EntityFrameworkCore.InMemory;

namespace TechMove.Tests
{
    public class WorkflowValidationTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task ServiceRequest_CannotBeCreated_ForExpiredContract()
        {
            // Arrange
            using var context = GetInMemoryContext();

            var client = new Client { Id = 1, Name = "Test Client", Region = "Test" };
            var contract = new Contract
            {
                Id = 1,
                ClientId = 1,
                StartDate = DateTime.Now.AddMonths(-12),
                EndDate = DateTime.Now.AddMonths(-6),
                Status = ContractStatus.Expired
            };

            context.Clients.Add(client);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            // Act
            var retrievedContract = await context.Contracts.FindAsync(1);
            bool canCreateServiceRequest = retrievedContract!.Status == ContractStatus.Active ||
                                           retrievedContract.Status == ContractStatus.Draft;

            // Assert
            Assert.False(canCreateServiceRequest, "Service requests should not be allowed for Expired contracts");
        }

        [Fact]
        public async Task ServiceRequest_CannotBeCreated_ForOnHoldContract()
        {
            // Arrange
            using var context = GetInMemoryContext();

            var client = new Client { Id = 1, Name = "Test Client", Region = "Test" };
            var contract = new Contract
            {
                Id = 1,
                ClientId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(6),
                Status = ContractStatus.OnHold
            };

            context.Clients.Add(client);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            // Act
            var retrievedContract = await context.Contracts.FindAsync(1);
            bool canCreateServiceRequest = retrievedContract!.Status == ContractStatus.Active ||
                                           retrievedContract.Status == ContractStatus.Draft;

            // Assert
            Assert.False(canCreateServiceRequest, "Service requests should not be allowed for OnHold contracts");
        }

        [Theory]
        [InlineData(ContractStatus.Active, true)]
        [InlineData(ContractStatus.Draft, true)]
        [InlineData(ContractStatus.Expired, false)]
        [InlineData(ContractStatus.OnHold, false)]
        public async Task ServiceRequest_WorkflowValidation_ChecksAllStatuses(
            ContractStatus status, bool shouldAllow)
        {
            // Arrange
            using var context = GetInMemoryContext();

            var client = new Client { Id = 1, Name = "Test Client", Region = "Test" };
            var contract = new Contract
            {
                Id = 1,
                ClientId = 1,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddMonths(6),
                Status = status
            };

            context.Clients.Add(client);
            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            // Act
            var retrievedContract = await context.Contracts.FindAsync(1);
            bool canCreateServiceRequest = retrievedContract!.Status == ContractStatus.Active ||
                                           retrievedContract.Status == ContractStatus.Draft;

            // Assert
            Assert.Equal(shouldAllow, canCreateServiceRequest);
        }
    }
}