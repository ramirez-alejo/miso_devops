using System.Text.Json.Serialization;
using BlackListApi.Data;
using BlackListApi.Infrastructure.Exceptions;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BlackListApi.Queries;

public class GetEmail : IRequest<GetEmailResponse>
{
	public string Email { get; set; }
}

public class GetEmailResponse
{
	[JsonPropertyName("email")]
	public string Email { get; set; }
	[JsonPropertyName("blocked_reason")]
	public string BlockedReason { get; set; }
	[JsonPropertyName("app_uuid")]
	public Guid AppUuid { get; set; }
	[JsonPropertyName("created_at")]
	public DateTime CreatedAt { get; set; }
	[JsonPropertyName("source_ip")]
	public string SourceIp { get; set; }
}


public class GetEmailHandler(EmailsDbContext dbContext) 
	: IRequestHandler<GetEmail, GetEmailResponse>
{
	public async ValueTask<GetEmailResponse> Handle(GetEmail request, CancellationToken cancellationToken)
	{
		var existing = await dbContext.BlackList.FirstOrDefaultAsync(b => b.Email == request.Email, cancellationToken);
		if (existing == null)
		{
			throw new NotFoundException("Email not found");
		}
		return new GetEmailResponse
		{
			Email = existing.Email,
			BlockedReason = existing.BlockedReason,
			AppUuid = existing.AppUuid,
			CreatedAt = existing.CreatedAt,
			SourceIp = existing.SourceIp
		};
	}
}