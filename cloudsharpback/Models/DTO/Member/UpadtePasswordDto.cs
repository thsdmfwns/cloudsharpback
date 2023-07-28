namespace cloudsharpback.Models.DTO.Member
{
    public class UpadtePasswordDto
    {
        public UpadtePasswordDto(string original, string changeTo)
        {
            Original = original;
            ChangeTo = changeTo;
        }

        public string Original { get; set; }
        public string ChangeTo { get; set; }
    }
}
