using System.Text.Json.Serialization;
using BlackListApi.Data;
using BlackListApi.Infrastructure;
using BlackListApi.Models;
using Mediator;
using Microsoft.EntityFrameworkCore;
using FluentValidation;

namespace BlackListApi.Commands;



public class BlackListEmailCommand : IRequest<BlackListEmailResponse>
{
	[JsonPropertyName("email")]
	public string Email { get; set; }
	[JsonPropertyName("blocked_reason")]
	public string BlockedReason { get; set; }
	[JsonPropertyName("app_uuid")]
	public string AppUuid { get; set; }

	public string SourceIp { get; set; }
}

public class BlackListEmailResponse
{
	public string Message { get; set; }
}


public class BlackListEmailValidator : RequestValidationBehavior<BlackListEmailCommand, BlackListEmailResponse>
{
	public BlackListEmailValidator()
	{
		RuleFor(x => x.Email).NotNull().NotEmpty().MaximumLength(50);
		RuleFor(x => x.Email).EmailAddress();
		RuleFor(x => x.AppUuid).NotNull().NotEmpty().MaximumLength(50);
		RuleFor(x => x.BlockedReason).MaximumLength(255);
	}
}


public class BlackListEmailHandler(EmailsDbContext dbContext) : IRequestHandler<BlackListEmailCommand, BlackListEmailResponse>
{
	public async ValueTask<BlackListEmailResponse> Handle(BlackListEmailCommand request, CancellationToken cancellationToken)
	{
		
		var existing = await dbContext.BlackList.FirstOrDefaultAsync(b => b.Email == request.Email, cancellationToken);
		if (existing != null)
		{
			throw new Infrastructure.Exceptions.ValidationException("Email already exists in the blacklist");
		}
		
		var blackList = new BlackList
		{
			Email = request.Email,
			BlockedReason = request.BlockedReason,
			AppUuid = Guid.Parse(request.AppUuid),
			CreatedAt = DateTime.UtcNow,
			SourceIp = request.SourceIp
		};

		await dbContext.BlackList.AddAsync(blackList, cancellationToken);
		await dbContext.SaveChangesAsync(cancellationToken);

		return new BlackListEmailResponse
		{
			Message = "Email added to the blacklist"
		};
	}
}