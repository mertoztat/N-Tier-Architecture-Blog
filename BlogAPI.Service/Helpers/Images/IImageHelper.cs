using BlogAPI.Entity.Dtos.Images;
using BlogAPI.Entity.Enums;
using Microsoft.AspNetCore.Http;
namespace BlogAPI.Service.Helpers.Images;

public interface IImageHelper
{
    Task<ImageUploadedDto> UploadImage(string name, IFormFile imageFile, ImageType imageType, string foldernName = null);
    void DeleteImage(string imageName);
}