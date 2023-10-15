﻿using BugTracker.Application.Contracts.Data;
using BugTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugTracker.Persistence.Services.Data
{
    public class TicketConfigurationsRepository : ITicketConfigurationRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TicketConfigurationsRepository(ApplicationDbContext dbContext)
        {
           _dbContext = dbContext;
        }

        public async Task<List<Priority>> GetPriorities()
        {
            return await _dbContext.Priority.OrderBy(p => p.Order).ToListAsync();
        }

        public async Task<string> GetPriorityName(Guid id)
        {
            return await _dbContext.Priority.Where(s => s.Id == id).Select(s => s.Name).FirstOrDefaultAsync();
        }

        public async Task<List<Status>> GetStatuses()
        {
            return await _dbContext.Status.OrderBy(p => p.Order).ToListAsync();
        }

        public async Task<string> GetStatusName(Guid id)
        {
            return await _dbContext.Status.Where(s => s.Id == id).Select(s => s.Name).FirstOrDefaultAsync();
        }

        public async  Task<List<Domain.Entities.Type>> GetTypes()
        {
            return await _dbContext.Type.OrderBy(p => p.Order).ToListAsync();
        }
        public async Task<string> GetTypeName(Guid id)
        {
            return await _dbContext.Type.Where(s => s.Id == id).Select(s => s.Name).FirstOrDefaultAsync();
        }
    }
}
