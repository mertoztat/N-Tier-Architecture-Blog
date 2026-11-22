using BlogAPI.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogAPI.Data.Mappings;

public class ArticleMap : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
        // Deleting a user must not delete their articles. Restrict also avoids SQL Server's
        // "multiple cascade paths" error (Image -> Article and Image -> User -> Article).
        builder.HasOne(a => a.User)
            .WithMany(u => u.Articles)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Article
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "First Article",
                Content = "This is the content of the first article.",
                ViewCount = 100,
                ImageId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                CategoryId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                CreatedBy = "Mert",
                CreatedDate = new DateTime(2026, 1, 1),
                IsDeleted = false,
                UserId = Guid.Parse("CB94223B-CCB8-4F2F-93D7-0DF96A7F065C")
            },
            new Article
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title = "Second Article",
                Content = "This is the content of the second article.",
                ViewCount = 200,
                ImageId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                CategoryId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                CreatedBy = "Mert",
                CreatedDate = new DateTime(2026, 1, 1),
                IsDeleted = false,
                UserId = Guid.Parse("CB94223B-CCB8-4F2F-93D7-0DF96A7F065C")
            });
    }
}
