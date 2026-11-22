using BlogAPI.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogAPI.Data.Mappings;

public class CategoryMap : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasData(
            new Category
            {
                Id = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name = "Category 1",
                CreatedBy = "Admin Test",
                CreatedDate = new DateTime(2026, 1, 1),
                IsDeleted = false
            },
            new Category
            {
                Id = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                Name = "Category 2",
                CreatedBy = "Admin Test",
                CreatedDate = new DateTime(2026, 1, 1),
                IsDeleted = false
            });
    }
}
