using Azure;
using Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TMF.SmartAssistant.FuncApps.Application.Model;
using TMF.SmartAssistant.FuncApps.Configuration;

namespace TMF.SmartAssistant.FuncApps.Infrastructure.Data
{
    internal interface IOrganizationMemberOverTimeRecordRepository
    {
        Task<EmployeeOverTimeRecord> AddAsync(EmployeeOverTimeRecord employeeOverTimeRecord);
        Task DeleteAsync(EmployeeOverTimeRecord employeeOverTimeRecord);
        Task<EmployeeOverTimeRecord> GetAsync(EmployeeOverTimeRecord employeeOverTimeRecord);
        Task<EmployeeOverTimeRecord> UpdateAsync(EmployeeOverTimeRecord employeeOverTimeRecord);
        Task<List<EmployeeOverTimeRecord>> GetAllAsync();
    }

    internal class OrganizationMemberOverTimeRecordRepository : IOrganizationMemberOverTimeRecordRepository
    {
        private readonly ILogger<OrganizationMemberOverTimeRecordRepository> _logger;
        protected readonly ICosmosDbConfiguration _cosmosDbConfiguration;
        protected readonly CosmosClient _client;

        public OrganizationMemberOverTimeRecordRepository(ILogger<OrganizationMemberOverTimeRecordRepository> logger,
                                      ICosmosDbConfiguration cosmosDbConfiguration,
                                      CosmosClient client)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cosmosDbConfiguration = cosmosDbConfiguration
                    ?? throw new ArgumentNullException(nameof(cosmosDbConfiguration));

            _client = client
                    ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<EmployeeOverTimeRecord> AddAsync(EmployeeOverTimeRecord employeeOverTimeRecord)
        {
            try
            {
                CosmosContainer container = GetContainer();
                ItemResponse<EmployeeOverTimeRecord> createResponse = await container.CreateItemAsync(employeeOverTimeRecord,
                                                                                           new PartitionKey(employeeOverTimeRecord.EmployeeId));
                return createResponse.Value;
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"New entity with ID: {employeeOverTimeRecord.Id} was not added successfully - error details: {ex.Message}");

                if (ex.Status != (int)HttpStatusCode.NotFound)
                {
                    throw;
                }

                return null;
            }
        }

        public async Task DeleteAsync(EmployeeOverTimeRecord employeeOverTimeRecord)
        {
            try
            {
                CosmosContainer container = GetContainer();

                await container.DeleteItemAsync<EmployeeOverTimeRecord>(employeeOverTimeRecord.Id, new PartitionKey(employeeOverTimeRecord.EmployeeId));
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {employeeOverTimeRecord.Id} was not removed successfully - error details: {ex.Message}");

                if (ex.Status != (int)HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
        }

        public async Task<EmployeeOverTimeRecord> GetAsync(EmployeeOverTimeRecord employeeOverTimeRecord)
        {
            try
            {
                CosmosContainer container = GetContainer();

                ItemResponse<EmployeeOverTimeRecord> entityResult = await container.ReadItemAsync<EmployeeOverTimeRecord>(employeeOverTimeRecord.Id,
                                                                                                    new PartitionKey(employeeOverTimeRecord.EmployeeId));
                return entityResult.Value;
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {employeeOverTimeRecord.Id} was not retrieved successfully - error details: {ex.Message}");

                if (ex.Status != (int)HttpStatusCode.NotFound)
                {
                    throw;
                }

                return null;
            }
        }

        public async Task<EmployeeOverTimeRecord> UpdateAsync(EmployeeOverTimeRecord employeeOverTimeRecord)
        {
            try
            {
                CosmosContainer container = GetContainer();

                ItemResponse<EmployeeOverTimeRecord> entityResult = await container
                                                           .ReadItemAsync<EmployeeOverTimeRecord>(employeeOverTimeRecord.Id,
                                                                                                  new PartitionKey(employeeOverTimeRecord.EmployeeId));

                if (entityResult != null)
                {
                    await container
                          .ReplaceItemAsync(employeeOverTimeRecord, employeeOverTimeRecord.Id, new PartitionKey(employeeOverTimeRecord.EmployeeId));
                }
                return employeeOverTimeRecord;
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {employeeOverTimeRecord.Id} was not updated successfully - error details: {ex.Message}");

                if (ex.Status != (int)HttpStatusCode.NotFound)
                {
                    throw;
                }

                return null;
            }
        }

        public async Task<List<EmployeeOverTimeRecord>> GetAllAsync()
        {
            try
            {
                CosmosContainer container = GetContainer();
                AsyncPageable<EmployeeOverTimeRecord> queryResultSetIterator = container.GetItemQueryIterator<EmployeeOverTimeRecord>();
                List<EmployeeOverTimeRecord> entities = new List<EmployeeOverTimeRecord>();

                await foreach (var entity in queryResultSetIterator)
                {
                    entities.Add(entity);
                }

                return entities;

            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entities were not retrieved successfully - error details: {ex.Message}");

                if (ex.Status != (int)HttpStatusCode.NotFound)
                {
                    throw;
                }

                return null;
            }
        }


        protected CosmosContainer GetContainer()
        {
            var database = _client.GetDatabase(_cosmosDbConfiguration.DatabaseName);
            var container = database.GetContainer(_cosmosDbConfiguration.EmployeeOverTimeRecordContainerName);
            return container;
        }
    }
}
