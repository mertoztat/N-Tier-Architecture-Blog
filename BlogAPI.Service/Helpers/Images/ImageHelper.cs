using BlogAPI.Entity.Dtos.Images;
using BlogAPI.Entity.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace BlogAPI.Service.Helpers.Images;

public class ImageHelper : IImageHelper
{

    private readonly IWebHostEnvironment _env;
    private readonly string _wwwroot;
    private const string imgFolder = "images";
    private const string articleImagesFolder = "article-images";
    private const string articleUsersFolder = "users-images";
    

    public ImageHelper(IWebHostEnvironment webHostEnvironment)
    {
        _env = webHostEnvironment;
        _wwwroot = _env.WebRootPath;
    }
    
    private string ReplaceInvalidChars(string fileName)
    {
        return fileName.Replace("İ", "I")
            .Replace("ı", "i")
            .Replace("Ğ", "G")
            .Replace("ğ", "g")
            .Replace("Ü", "U")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("Ş", "S")
            .Replace("Ö", "O")
            .Replace("ö", "o")
            .Replace("Ç", "C")
            .Replace("ç", "c")
            .Replace("é", "")
            .Replace("!", "")
            .Replace("'", "")
            .Replace("^", "")
            .Replace("+", "")
            .Replace("%", "")
            .Replace("/", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("=", "")
            .Replace("?", "")
            .Replace("_", "")
            .Replace("*", "")
            .Replace("æ", "")
            .Replace("ß", "")
            .Replace("@", "")
            .Replace("€", "")
            .Replace("<", "")
            .Replace(">", "")
            .Replace("#", "")
            .Replace("$", "")
            .Replace("½", "")
            .Replace("{", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("}", "")
            .Replace(@"\", "")
            .Replace("|", "")
            .Replace("~", "")
            .Replace("¨", "")
            .Replace(",", "")
            .Replace(";", "")
            .Replace("`", "")
            .Replace(".", "")
            .Replace(":", "")
            .Replace(" ", "");
    }
    
    
    public async Task<ImageUploadedDto>  UploadImage(string name, IFormFile imageFile,
        ImageType imageType, string folderName = null)
    {
        folderName ??= imageType == ImageType.Post ? articleImagesFolder : articleUsersFolder;
        if (!Directory.Exists($"{_wwwroot}/{imgFolder}/{folderName}"))
            Directory.CreateDirectory($"{_wwwroot}/{imgFolder}/{folderName}");

        string oldFileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
        string fileExtension = Path.GetExtension(imageFile.FileName);

        name = ReplaceInvalidChars(name);

        DateTime dateTime = DateTime.Now;

        string newFileName = $"{name}_{dateTime.Millisecond}{fileExtension}";

        var path = Path.Combine($"{_wwwroot}/{imgFolder}/{folderName}", newFileName);

        await using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync: false);
        await imageFile.CopyToAsync(stream);
        await stream.FlushAsync();

        string message = imageType == ImageType.User 
            ? $"{newFileName} isimli kullanıcı resmi başarı ile eklenmiştir." 
            : $"{newFileName} isimli makale resmi başarı ile eklenmiştir";

        return new ImageUploadedDto()
        {
            FullName = $"{folderName}/{newFileName}"
        };
    }

    public void DeleteImage(string imageName)
    {
        var fileToDelete = Path.Combine($"{_wwwroot}/{imgFolder}/{imageName}");
        if (File.Exists(fileToDelete))
            File.Delete(fileToDelete);
    }
}