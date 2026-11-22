using BlogAPI.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogAPI.Data.Mappings;

public class ArticleMap : IEntityTypeConfiguration<Article>
{
    public void Configure(EntityTypeBuilder<Article> builder)
    {
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
                UserId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee")
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
                UserId = Guid.Parse("eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee")
            });
    }
}
