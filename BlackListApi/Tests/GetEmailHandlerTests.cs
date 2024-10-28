using BlackListApi.Data;
using BlackListApi.Models;
using BlackListApi.Queries;
using Microsoft.EntityFrameworkCore;

namespace Tests;

[TestClass]
public class GetEmailHandlerTests
{
    private EmailsDbContext _dbContext;
    private GetEmailHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<EmailsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new EmailsDbContext(options);
        _handler = new GetEmailHandler(_dbContext);
    }

    [TestMethod]
    public async Task Handle_ShouldReturnEmailDetails()
    {
        var email = "test@example.com";
        var blackList = new BlackList
        {
            Email = email,
            BlockedReason = "Spam",
            AppUuid = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            SourceIp = "127.0.0.1"
        };
        await _dbContext.BlackList.AddAsync(blackList);
        await _dbContext.SaveChangesAsync();

        var query = new GetEmail { Email = email };

        var response = await _handler.Handle(query, CancellationToken.None);

        Assert.IsNotNull(response);
        Assert.AreEqual(email, response.Email);
        Assert.AreEqual("Spam", response.BlockedReason);
        Assert.AreEqual(blackList.AppUuid, response.AppUuid);
        Assert.AreEqual(blackList.CreatedAt, response.CreatedAt);
        Assert.AreEqual("127.0.0.1", response.SourceIp);
    }
}