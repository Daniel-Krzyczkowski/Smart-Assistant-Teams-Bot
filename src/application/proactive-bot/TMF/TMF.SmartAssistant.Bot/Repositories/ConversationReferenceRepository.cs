using Azure;
using Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using TMF.SmartAssistant.Bot.Configuration;
using TMF.SmartAssistant.Bot.Model;

namespace TMF.SmartAssistant.Bot.Repositories
{
    public interface IConversationReferenceRepository
    {
        Task<UserConversationReference> AddAsync(UserConversationReference userConversationReference);
        Task DeleteAsync(UserConversationReference userConversationReference);
        Task<UserConversationReference> GetAsync(string id);
        Task<UserConversationReference> UpdateAsync(UserConversationReference userConversationReference);
        Task<List<UserConversationReference>> GetAllAsync();
    }

    internal class ConversationReferenceRepository : IConversationReferenceRepository
    {
        private readonly ILogger<ConversationReferenceRepository> _logger;
        protected readonly CosmosDbConfiguration _cosmosDbConfiguration;
        protected readonly CosmosClient _client;

        public ConversationReferenceRepository(ILogger<ConversationReferenceRepository> logger,
                                      IOptions<CosmosDbConfiguration> cosmosDbConfiguration,
                                      CosmosClient client)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cosmosDbConfiguration = cosmosDbConfiguration.Value
                    ?? throw new ArgumentNullException(nameof(cosmosDbConfiguration));

            _client = client
                    ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<UserConversationReference> AddAsync(UserConversationReference userConversationReference)
        {
            try
            {
                CosmosContainer container = GetContainer();
                ItemResponse<UserConversationReference> createResponse = await container.CreateItemAsync(userConversationReference,
                                                                                           new PartitionKey(userConversationReference.Id));
                return createResponse.Value;
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"New entity with ID: {userConversationReference.Id} was not added successfully - error details: {ex.Message}");

                if (ex.Status != (int)HttpStatusCode.NotFound)
                {
                    throw;
                }

                return null;
            }
        }

        public async Task DeleteAsync(UserConversationReference userConversationReference)
        {
            try
            {
                CosmosContainer container = GetContainer();

                await container.DeleteItemAsync<UserConversationReference>(userConversationReference.Id, new PartitionKey(userConversationReference.Id));
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {userConversationReference.Id} was not removed successfully - error details: {ex.Message}");

                if (ex.Status != (int)HttpStatusCode.NotFound)
                {
                    throw;
                }
            }
        }

        public async Task<UserConversationReference> GetAsync(string id)
        {
            try
            {
                CosmosContainer container = GetContainer();

                ItemResponse<UserConversationReference> entityResult = await container.ReadItemAsync<UserConversationReference>(id,
                                                                                                    new PartitionKey(id));
                return entityResult.Value;
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {id} was not retrieved successfully - error details: {ex.Message}");

                if (ex.Status != (int)HttpStatusCode.NotFound)
                {
                    throw;
                }

                return null;
            }
        }

        public async Task<UserConversationReference> UpdateAsync(UserConversationReference userConversationReference)
        {
            try
            {
                CosmosContainer container = GetContainer();

                ItemResponse<UserConversationReference> entityResult = await container
                                                           .ReadItemAsync<UserConversationReference>(userConversationReference.Id,
                                                                                                  new PartitionKey(userConversationReference.Id));

                if (entityResult != null)
                {
                    await container
                          .ReplaceItemAsync(userConversationReference, userConversationReference.Id, new PartitionKey(userConversationReference.Id));
                }
                return userConversationReference;
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Entity with ID: {userConversationReference.Id} was not updated successfully - error details: {ex.Message}");

                if (ex.Status != (int)HttpStatusCode.NotFound)
                {
                    throw;
                }

                return null;
            }
        }

        public async Task<List<UserConversationReference>> GetAllAsync()
        {
            try
            {
                CosmosContainer container = GetContainer();
                AsyncPageable<UserConversationReference> queryResultSetIterator = container.GetItemQueryIterator<UserConversationReference>();
                List<UserConversationReference> entities = new List<UserConversationReference>();

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
            var container = database.GetContainer(_cosmosDbConfiguration.BotConversationReferenceContainerName);
            return container;
        }
    }
}
