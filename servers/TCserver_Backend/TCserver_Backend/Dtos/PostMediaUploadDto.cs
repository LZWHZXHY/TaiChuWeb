namespace TCserver_Backend.Dtos
{
    public class PostMediaUploadDto
    {
        public List<IFormFile> Images { get; set; }
        public List<IFormFile> Videos { get; set; }
    }
}
