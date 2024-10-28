namespace BlackListApi.Models;

public class BlackList
{
	public Guid Id { get; set; }
	public string Email { get; set; } = null!;
	public Guid AppUuid { get; set; }
	public string BlockedReason { get; set; } = null!;
	public DateTime CreatedAt { get; set; }
	public string SourceIp { get; set; } = null!;
}