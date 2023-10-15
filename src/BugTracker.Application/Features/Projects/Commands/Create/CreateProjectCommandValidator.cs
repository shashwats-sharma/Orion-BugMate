﻿using BugTracker.Application.Contracts.Data;
using BugTracker.Application.Contracts.Identity;
using FluentValidation;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BugTracker.Application.Features.Projects.Commands.Create
{
    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IIdentityService _identityService;

        public CreateProjectCommandValidator(IProjectRepository projectRepository, IIdentityService identityService)
        {
            _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
            _identityService = identityService ?? throw new ArgumentNullException(nameof(identityService));


            RuleFor(p => p.Name)
                .NotNull()
                .NotEmpty().WithMessage("{PropertyName} is required")
                .MaximumLength(30).WithMessage("{PropertyName} can't exceed 30 characters.");

            RuleFor(p => p.Description)
                .MaximumLength(100).WithMessage("{PropertyName} can't exceed 100 characters.");

            RuleFor(e => e)
                .MustAsync(NameIsUnique).WithMessage("A project with the same given name already exists.")
                .MustAsync(UserIdsAreValid).WithMessage("One of the selected user, does not hold a valid Id");
        }

        private async Task<bool> NameIsUnique(CreateProjectCommand e, CancellationToken c)
        {
            return await _projectRepository.NameIsUnique(e.Name);
        }

        private async Task<bool> UserIdsAreValid(CreateProjectCommand e, CancellationToken c)
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
