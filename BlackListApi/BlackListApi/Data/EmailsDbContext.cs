using BlackListApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BlackListApi.Data;

public class EmailsDbContext(DbContextOptions<EmailsDbContext> options) : DbContext(options)
{
	public DbSet<BlackList> BlackList { get; set; }
}