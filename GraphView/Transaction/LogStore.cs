﻿namespace GraphView.Transaction
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.Documents.Client;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// A data store for logging
    /// </summary>
    public interface ILogStore
    {
        void WriteCommittedVersion(string tableId, object recordKey, object payload, long txId, long commitTs);
        void WriteCommittedTx(long txId);
    }
    
    public class CosmosDBStore : ILogStore
    {
        private readonly string url;
        private readonly string primaryKey;
        private readonly string databaseId;
        private readonly string collectionId;
        private readonly DocumentClient cosmosDbClient;

        public CosmosDBStore(
            string dbEndpointUrl,
            string dbAuthorizationKey,
            string dbId,
            string dbCollection,
            string preferredLocation = null)
        {
            this.url = dbEndpointUrl;
            this.primaryKey = dbAuthorizationKey;
            this.databaseId = dbId;
            this.collectionId = dbCollection;

            ConnectionPolicy connectionPolicy = new ConnectionPolicy
            {
                ConnectionMode = ConnectionMode.Direct,
                ConnectionProtocol = Protocol.Tcp,
            };

            if (!string.IsNullOrEmpty(preferredLocation))
            {
                connectionPolicy.PreferredLocations.Add(preferredLocation);
            }

            this.cosmosDbClient = new DocumentClient(new Uri(this.url), this.primaryKey, connectionPolicy);
            this.cosmosDbClient.OpenAsync().Wait();
        }

        public void WriteCommittedTx(long txId)
        {
            throw new NotImplementedException();
        }

        public void WriteCommittedVersion(string tableId, object recordKey, object payload, long txId, long commitTs)
        {
            throw new NotImplementedException();
        }
    }
}
