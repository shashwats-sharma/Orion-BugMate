﻿using BugTracker.Application.Contracts.Audits;
using BugTracker.Application.Contracts.Data;
using BugTracker.Persistence.Services.Audits;
using BugTracker.Persistence.Services.Data;
using Microsoft.Extensions.DependencyInjection;

namespace BugTracker.Persistence
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceService(this IServiceCollection services)
        {
            services.AddScoped(typeof(IAsyncRepository<>), typeof(BaseRepository<>));

            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ITicketRepository, TicketRepository>();
            services.AddScoped<ITicketConfigurationRepository, TicketConfigurationsRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();

            return services;
        }
    }
}
