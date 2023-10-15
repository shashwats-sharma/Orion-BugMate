﻿using BugTracker.Application.Contracts.Data;
using BugTracker.Application.Contracts.Identity;
using FluentValidation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BugTracker.Application.Features.Tickets.Commands.Update
{
    public class UpdateTicketCommandValidator : AbstractValidator<UpdateTicketCommand>
    {
        private readonly ITicketRepository _ticketRepository;
        private readonly IIdentityService _identityService;

        public UpdateTicketCommandValidator(ITicketRepository ticketRepository, IIdentityService identityService)
        {
            _ticketRepository = ticketRepository ?? throw new ArgumentNullException(nameof(ticketRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));


            RuleFor(p => p.Name)
                .NotNull()
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(30).WithMessage("{PropertyName} can't exceed 30 characters.");

            RuleFor(p => p.Description)
                .MaximumLength(100).WithMessage("{PropertyName} can't exceed 100 characters.");

            RuleFor(p => p.EstimatedAmountOfHours)
                .NotNull()
                .NotEmpty()
                .InclusiveBetween(1, 100);

            RuleFor(p => p.PriorityId)
                .NotNull();

            RuleFor(p => p.TypeId)
                .NotNull();

            RuleFor(p => p.StatusId)
                .NotNull();

            RuleFor(e => e)
                .MustAsync(NameIsUnique).WithMessage("A ticket with the same given name already exists.")
                .MustAsync(UserIdsAreValid).WithMessage("One of the selected user, does not hold a valid Id");
        }

        private async Task<bool> NameIsUnique(UpdateTicketCommand e, CancellationToken c)
        {
            return await _ticketRepository.NameIsUnique(e.Name, true, e.TicketId);
        }

        private async Task<bool> UserIdsAreValid(UpdateTicketCommand e, CancellationToken c)
        {
            if (e.Team.Any())
            {
                foreach (var Id in e.Team)
                {
                    if (!await _identityService.UserIdExists(Id.ToString()))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
