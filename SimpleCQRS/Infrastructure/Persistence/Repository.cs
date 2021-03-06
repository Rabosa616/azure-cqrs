﻿using SimpleCQRS.Domain;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleCQRS.Infrastructure.Persistence
{
    /// <summary>
    /// Repository implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Repository<T> : IRepository<T> where T : class, IAggregateRoot, new()
    {
        /// <summary>
        /// Event Store
        /// </summary>
        private readonly IEventStore _eventStore;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="eventStore"></param>
        public Repository(IEventStore eventStore)
        {
            _eventStore = eventStore;
        }

        /// <summary>
        /// Save all changes
        /// </summary>
        /// <param name="aggregate"></param>
        /// <param name="handleEventsSynchronously"></param>
        public async Task SaveAsync(IAggregateRoot aggregate)
        {
            await _eventStore.SaveEventsAsync(aggregate.Id, aggregate.CurrentVersion, aggregate.GetUncommittedChanges());
            aggregate.MarkChangesAsCommitted();
        }


        /// <summary>
        /// Get an aggregate by it's Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetById(Guid id)
        {
            T aggregate = null;
            var events = _eventStore.GetEvents(id);

            if (events != null && events.Any()) 
            {
                aggregate = new T();
                aggregate.LoadFromHistory(events);
            }
            
            return aggregate;
        }
    }
}
