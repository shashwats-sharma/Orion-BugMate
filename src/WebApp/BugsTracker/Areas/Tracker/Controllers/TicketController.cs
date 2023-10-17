﻿using BugTracker.Application.Dto.Comments;
using BugTracker.Application.Dto.Projects;
using BugTracker.Application.Dto.Tickets;
using BugTracker.Application.Enums;
using BugTracker.Application.Features.Audits.Queries;
using BugTracker.Application.Features.Comments.Commands.Create;
using BugTracker.Application.Features.Comments.Queries.GetAll;
using BugTracker.Application.Features.ProjectTeam.Queries.GetAllAccessibleMembers;
using BugTracker.Application.Features.TicketConfigurations.Queries.GetAll;
using BugTracker.Application.Features.Tickets.Commands.Create;
using BugTracker.Application.Features.Tickets.Commands.Delete;
using BugTracker.Application.Features.Tickets.Commands.Update;
using BugTracker.Application.Features.Tickets.Queries.GetProjectTickets;
using BugTracker.Application.Features.Tickets.Queries.GetTicket;
using BugTracker.Application.Features.Tickets.Queries.GetTicketsByUser;
using BugTracker.Application.Features.TicketTeam.Query;
using BugTracker.Application.Features.TicketTeam.Query.GetCurrentTeam;
using BugTracker.Application.Responses;
using BugTracker.Filters.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BugTracker.Areas.Tracker.Controllers
{
    public class TicketController : BaseController
    {
        private const string ModalBasePath = "~/Areas/Tracker/Views/Shared/Partial/Ticket/";
        private const string ModalType = "TicketModalPartial.cshtml";

        private const string CreateModalPath = ModalBasePath + "_create" + ModalType;
        private const string UpdateModalPath = ModalBasePath + "_update" + ModalType;
        private const string DeleteModalPath = ModalBasePath + "_delete" + ModalType;
        private const string DetailsModalPath = ModalBasePath + "_details" + ModalType;
        private const string CommentModalPath = ModalBasePath + "_comment" + ModalType;
        private const string ProjectTeamModalPath = "~/Areas/Tracker/Views/Shared/Partial/Project/_projectTeamModalPartial.cshtml";


        public async Task<IActionResult> ByUser(
            int page = 1,
            string searchString = "",
            bool showOnlyCreated = false, bool showAll = true)
        {
            ViewData["showOnlyCreated"] = showOnlyCreated;
            var response = await Mediator.Send(new GetTicketByUserQuery(page, searchString, showOnlyCreated, showAll));
            response.Data.Pager.SearchText = searchString != null ? searchString : "";
            foreach(var ticket in response.Data.Tickets)
            {
                ticket.FormattedTicketNumber = FormatTicketNumber(ticket.TicketNumber);
            }
            response.Data.ShowOnlyCreated = showOnlyCreated;
            return View(response.Data);
        }
        private static string FormatTicketNumber(int number)
        {
            string formattedNumber = number.ToString("D4"); // Format as a 4-digit number with leading zeros
            string ticketNumber = "TICKET" + formattedNumber;
            return ticketNumber;
        }


        [ValidationFilter]
        public async Task<IActionResult> ByProject(
            Guid projectId,
            List<string> errors,
            int page = 1,
            string searchString ="",
            bool isSuccess = false, bool isFailed = false,
            string type = "ticket", string actionReturned = null)
        {
            ViewBag.isSuccess = isSuccess;
            ViewBag.isFailed = isFailed;
            ViewBag.Type = type;
            ViewBag.actionReturned = actionReturned;
            ViewBag.errors = errors;

            var response = await Mediator.Send(new GetProjectTicketsQuery(projectId, page, searchString));
            response.Data.Pager.SearchText = searchString != null ? searchString : "";
            return View("ProjectTickets", response);
        }
        
        [HttpPost]
        [Authorize(Roles ="Admin, Submitter")]
        public async Task<IActionResult> Create(CreateTicketCommand command, int page)
        {            
            command.CreatedBy = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value); // Get the user ID
            var response = await Mediator.Send(command);
            if (response.Succeeded)
            {
                return RedirectToAction("ByProject", new { projectId = command.ProjectId, isSuccess = true, type = "ticket", actionReturned = "created", page = page });
            }
            return RedirectToAction("ByProject", new { projectId = command.ProjectId, isFailed = true, type = "ticket", actionReturned = "created", page = page });
        }

        [HttpPost]
        [Authorize(Policy ="NoDemo")]
        public async Task<IActionResult> Update(UpdateTicketCommand command, Guid projectId, int page)
        {
            var response = await Mediator.Send(command);
            if (response.Succeeded)
            {
                return RedirectToAction("ByProject", new { projectId = projectId, isSuccess = true, type = "ticket", actionReturned = "updated", page = page });
            }
            return RedirectToAction("ByProject", new { projectId = projectId, isFailed = true, actionReturned = "updated", errors = response.ErrorMessages, page = page});
        }

        [HttpPost]
        [Authorize(Policy = "PriviledgedUser")]
        public async Task<IActionResult> Delete(DeleteTicketCommand command, Guid projectId, int page)
        {
            var response = await Mediator.Send(command);
            if (response.Succeeded)
            {
                return RedirectToAction("ByProject", new { projectId = projectId, isSuccess = true, type = "ticket", actionReturned = "deleted", page = page });
            }
            return RedirectToAction("ByProject", new { isFailed = true, page = page });
        }

        [HttpGet]
        [Authorize(Policy = "NotDev")]
        public async Task<IActionResult> LoadCreateModal(Guid projectId, [FromQuery] int page)
        {
            var dto = new CreateTicketDto(projectId);

            var teamResponse = await Mediator.Send(new GetAllAccessibleTicketMembersQuery(projectId));
            var ticketConfigurationResponse = await Mediator.Send(new GetAllTicketConfigurationsQuery());
            dto.Team = teamResponse.DataList;
            dto.TicketConfigurations = ticketConfigurationResponse.Data;

            return PartialView(CreateModalPath, dto);
        }
        
        [HttpGet]
        public async Task<IActionResult> LoadUpdateModal(Guid id, Guid projectId)
        {
            var dto = new UpdateTicketDto(id);

            var ticket = (await Mediator.Send(new GetTicketQuery(id))).Data;

            var teamResponse = await Mediator.Send(new GetAllAccessibleTicketMembersQuery(projectId));
            var ticketConfigurationResponse = await Mediator.Send(new GetAllTicketConfigurationsQuery());

            dto.ProjectId = projectId;
            dto.Team = teamResponse.DataList;
            dto.TicketConfigurations = ticketConfigurationResponse.Data;

            dto.Command = new UpdateTicketCommand(id);
            dto.Command.Name = ticket.Name;
            dto.Command.Description = ticket.Description;
            dto.Command.PriorityId = ticket.Priority.Id;
            dto.Command.TypeId = ticket.Type.Id;
            dto.Command.StatusId = ticket.Status.Id;
            dto.Command.Team = ticket.TicketsTeamMembers.Select(ttm => ttm.Id).ToList();
            dto.Command.EstimatedAmountOfHours = ticket.EstimatedAmountOfHours;

            return PartialView(UpdateModalPath, dto);
        }
        
        [HttpGet]
        [Authorize(Policy = "NotDev")]
        public IActionResult LoadDeleteModal(Guid id, Guid projectId, string name)
        {
            var dto = new DeleteTicketDto(id, projectId, name);
            return PartialView(DeleteModalPath, dto);
        }

        [HttpGet]
        public async Task<IActionResult> LoadDetailsModal(Guid ticketId)
        {
            var dto = new TicketDetailsDto();

            var historyResponse = await Mediator.Send(new GetAuditLogsQuery(ticketId, AuditableType.Ticket));
            var commentsResponse = await Mediator.Send(new GetAllCommentsQuery(ticketId));
            var teamResponse = await Mediator.Send(new GetCurrentTeamQuery(ticketId));

            dto.TicketId = ticketId;
            dto.History = historyResponse.DataList;
            dto.Comments = commentsResponse;
            dto.Team = teamResponse.DataList;

            return PartialView(DetailsModalPath, dto);
        }

        [HttpGet]
        [Authorize(Policy = "PriviledgedUser")]
        public async Task<IActionResult> LoadProjectTeamModal(Guid projectId)
        {
            var dto = new ProjectTeamManagementDto();
            var teamResponse = await Mediator.Send(new GetAllAccessibleProjectMembersQuery());

            dto.ProjectId = projectId;
            dto.Team = teamResponse.DataList;

            return PartialView(ProjectTeamModalPath);
        }
    
        [HttpPost]
        [Authorize(Policy = "NoDemo")]
        public async Task<PartialViewResult> SendComment(CreateCommentDto request)
        {
            var response = new ApiResponse<CommentDto>();

            var createdCommentResponse = await Mediator.Send(new CreateCommentCommand(request.Message, request.TicketId));
            var commentResponse = await Mediator.Send(new GetAllCommentsQuery(request.TicketId));

            response.DataList = commentResponse.DataList;
            response.Data = commentResponse.Data;

            if (!createdCommentResponse.Succeeded)
            {
                response.Succeeded = false;
                response.ErrorMessages = createdCommentResponse.ErrorMessages;
            }

            return PartialView(CommentModalPath, response);

        }
    }
}
