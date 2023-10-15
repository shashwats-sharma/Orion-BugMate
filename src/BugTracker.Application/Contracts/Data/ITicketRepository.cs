﻿using BugTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BugTracker.Application.Contracts.Data
{
    public interface ITicketRepository : IAsyncRepository<Ticket>
    {
        Task<Ticket> AddTicketAsync(Ticket entity, ICollection<string> teamIds);
        Task<int> CountAllTickets(string searchString);
        Task<int> CountProjectTicket(string searchString, Guid projectId);
        Task<int> CountUserAssignedTickets(string uid, string searchString);
        Task<int> GetDevTicketAmountByProject(string uid, Guid projectId, string searchString);
        Task<IEnumerable<Ticket>> GetDevTicketByProject(string uid, Guid projectId, int page, string searchString);
        Task<int> GetProjectManagerTicketCount(string uid, string searchString);
        Task<IEnumerable<Ticket>> GetProjectManagerTickets(string uid, int page, string searchString);
        Task<IEnumerable<Ticket>> GetTicketsByProject(Guid id, int page, string searchString ="");
        Task<IEnumerable<Ticket>> GetTicketsByUser(string uid, int page, string searchString, bool showOnlyCreated);
        Task<Ticket> GetTicketWithTeamAndConfiguration(Guid id);
        Task<int> GetUserCreatedTicketAmount(string uid, string searchString);
        Task<bool> NameIsUnique(string name, bool isAnUpdate, Guid id = new Guid());
        Task<bool> UpdateTicketAsync(Ticket entity, ICollection<string> teamIds);
        Task<bool> UserBelongsToTicketTeam(string uid, Guid ticketId);
    }
}
