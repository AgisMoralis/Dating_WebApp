using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class DataContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Entities.User> Users { get; set; }

}
