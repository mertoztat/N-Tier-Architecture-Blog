using BlogAPI.Entity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlogAPI.Data.Mappings;

public class ImageMap : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.HasData(
            new Image
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                FileName = "Image1",
                FileType = "image/jpeg",
                CreatedBy = "Admin Test",
                CreatedDate = new DateTime(2026, 1, 1),
                IsDeleted = false
            },
            new Image
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                FileName = "Image2",
                FileType = "image/jpeg",
                CreatedBy = "Admin Test",
                CreatedDate = new DateTime(2026, 1, 1),
                IsDeleted = false
            },
            new Image
            {
                Id = Guid.Parse("f71f4b9a-aa60-461d-b398-de31001bf214"),
                FileName = "SuperAdminImage",
                FileType = "image/jpeg",
                CreatedBy = "Admin Test",
                CreatedDate = new DateTime(2026, 1, 1),
                IsDeleted = false
            },
            new Image
            {
                Id = Guid.Parse("d16a6ec7-8c50-4ab0-89a5-02b9a551f0fa"),
                FileName = "AdminImage",
                FileType = "image/jpeg",
                CreatedBy = "Admin Test",
                CreatedDate = new DateTime(2026, 1, 1),
                IsDeleted = false
            });
    }
}
