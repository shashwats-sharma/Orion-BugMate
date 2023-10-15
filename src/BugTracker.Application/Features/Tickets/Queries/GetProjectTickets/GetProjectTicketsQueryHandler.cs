﻿using AutoMapper;
using BugTracker.Application.Contracts.Data;
using BugTracker.Application.Contracts.Identity;
using BugTracker.Application.Model.Pagination;
using BugTracker.Application.Responses;
using BugTracker.Application.ViewModel;
using BugTracker.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BugTracker.Application.Features.Tickets.Queries.GetProjectTickets
{
    public class GetProjectTicketsQueryHandler : IRequestHandler<GetProjectTicketsQuery, ApiResponse<ProjectWithTicketVm>>
    {
        private readonly IMapper _mapper;
        private readonly ITicketRepository _ticketRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IIdentityService _identityService;
        private readonly ILoggedInUserService _loggedInUserService;

        public GetProjectTicketsQueryHandler(
            IMapper mapper,
            ITicketRepository ticketRepository,
            IProjectRepository projectRepository,
            IIdentityService identityService,
            ILoggedInUserService loggedInUserService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));
            _loggedInUserService = loggedInUserService ?? throw new ArgumentNullException(nameof(loggedInUserService));
        }

        public async Task<ApiResponse<ProjectWithTicketVm>> Handle(GetProjectTicketsQuery request, CancellationToken cancellationToken)
        {
            var response = new ApiResponse<ProjectWithTicketVm>();
            if (! await IsAllowedToAccessTickets(response,request.ProjectId))
            {
                return response;
            }

            var project = await _projectRepository.GetByIdAsync(request.ProjectId);
            var setCount = await CountTicketsForUser(project.Id, request.SearchString);
            var tickets = await GetTicketsForUser(request, project.Id);

            if (setCount <= 6)
            {
                request.Page = 1;
            }
            var pager = new Pager(setCount,request.Page) {RelatedId = project.Id };

            response.Data = new ProjectWithTicketVm(project.Id, project.Name, _mapper.Map<List<TicketVm>>(tickets), pager);
            await AssignValueToHumanReadable(response, tickets);

            return response; 
        }

        private async Task<bool> IsAllowedToAccessTickets(ApiResponse<ProjectWithTicketVm> response, Guid projectId)
        {
            bool isAdmin = _loggedInUserService.Roles.Any(str => str.Contains("Admin"));
            bool isProjectManager = _loggedInUserService.Roles.Any(str => str == "Project Manager");
            var isSubmitter = _loggedInUserService.Roles.Any(str => str == "Submitter");

            if (isAdmin)
            {
                return true;
            }
            else
            {
                return await _projectRepository.UserBelongsToProjectTeam(_loggedInUserService.UserId, projectId);
            }

        }
    
        private async Task AssignValueToHumanReadable(ApiResponse<ProjectWithTicketVm> response, List<Ticket> tickets)
        {
            for (int i = 0; i < response.Data.Tickets.Count(); i++)
            {
                var target = response.Data.Tickets[i];
                target.Author = await _identityService.GetUserNameById(tickets[i].CreatedBy.ToString());
                target.Priority = tickets[i].Priority.Name;
                target.Status = tickets[i].Status.Name;
                target.Type = tickets[i].Type.Name;
            }
        }
        
        private async Task<List<Ticket>>GetTicketsForUser(GetProjectTicketsQuery request, Guid projectId)
        {
            if (_loggedInUserService.Roles.Any(str => str.Contains("Dev")))
            {
                return (await _ticketRepository.GetDevTicketByProject(_loggedInUserService.UserId, projectId, request.Page, request.SearchString)).ToList();
            }
            else
            {
                return (await _ticketRepository.GetTicketsByProject(request.ProjectId, request.Page, request.SearchString)).ToList();
            }
        }
        private async Task<int> CountTicketsForUser(Guid projectId, string searchString)
        {
            if (_loggedInUserService.Roles.Any(str => str.Contains("Dev")))
            {
                return await _ticketRepository.GetDevTicketAmountByProject(_loggedInUserService.UserId, projectId, searchString);
            }
            else
            {
                return await _ticketRepository.CountProjectTicket(searchString, projectId);
            }
        }
    }
}
