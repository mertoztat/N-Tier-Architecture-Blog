using Microsoft.EntityFrameworkCore;
using BlogAPI.Entity.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace BlogAPI.Data.Context;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, Guid, AppUserClaim, AppUserRole, AppUserLogin, AppRoleClaim, AppUserToken>
{
    protected AppDbContext()
    {
        
    }
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Image> Images { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Bu projedeki entity yapılandırmalarını modele uygular.
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
