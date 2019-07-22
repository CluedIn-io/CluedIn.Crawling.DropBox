using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CluedIn.Core;
using CluedIn.Core.Accounts;
using CluedIn.Core.Data.Relational;
using CluedIn.Core.DataStore;

namespace Crawling.DropBox.Integration.Test.Stubs
{
    public class TokenDataStoreStub : IRelationalDataStore<Token>
    {
        public void CreateDataStore(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public void DeleteDataStore(ExecutionContext context)
        {
            throw new NotImplementedException();
        }

        public void DeleteAll(ExecutionContext context, bool canBeSystemContext = false)
        {
            throw new NotImplementedException();
        }

        public string Name { get; }
        public DataShardType DataShardType { get; }
        public Token GetById(ExecutionContext context, Guid id)
        {
            throw new NotImplementedException();
        }

        public void Insert(ExecutionContext context, Token entity)
        {
            throw new NotImplementedException();
        }

        public void InsertOrUpdate(ExecutionContext context, Token entity)
        {
            throw new NotImplementedException();
        }

        public void Update(ExecutionContext context, Token entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(ExecutionContext context, Token entity)
        {
            throw new NotImplementedException();
        }

        public void DeleteById(ExecutionContext context, Guid id)
        {
            throw new NotImplementedException();
        }

        public void DeleteById(ExecutionContext context, Guid id, int? version)
        {
            throw new NotImplementedException();
        }

        public IDataStoreConnectionManager<TConnection> CreateConnectionManager<TConnection>(IOrganization organization)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Token> Select(ExecutionContext context, Expression<Func<Token, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Token>> SelectAsync(ExecutionContext context, Expression<Func<Token, bool>> predicate)
        {
            return await Task.FromResult(new List<TokenTemp> {
                new TokenTemp
                {
                    AccessToken = ConfigurationManager.AppSettings["AccessToken"]
                }

            });
        }

        public bool Any(ExecutionContext context, Expression<Func<Token, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public void Insert(ExecutionContext context, IEnumerable<Token> objects)
        {
            throw new NotImplementedException();
        }

        public void InsertOrUpdate(ExecutionContext context, IEnumerable<Token> objects)
        {
            throw new NotImplementedException();
        }

        public void Delete(ExecutionContext context, IEnumerable<Token> objects)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public class TokenTemp : Token
        {
            public new string AccessToken { get; set; }
        }
    }

    
}
