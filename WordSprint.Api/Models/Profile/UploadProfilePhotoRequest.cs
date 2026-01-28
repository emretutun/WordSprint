namespace WordSprint.Api.Models.Profile;

public class UploadProfilePhotoRequest
{
    public IFormFile File { get; set; } = default!;
}