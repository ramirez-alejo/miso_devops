namespace BlackListApi.Models;

public class BlackList
{
	public Guid Id { get; set; }
	public string Email { get; set; }
	public Guid AppUuid { get; set; }
	public string BlockedReason { get; set; }
	public DateTime CreatedAt { get; set; }
	public string SourceIp { get; set; }
}