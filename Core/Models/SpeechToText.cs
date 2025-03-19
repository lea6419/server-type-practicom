using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class SpeechToText:IEntity
{
    public int Id { get; set; }

    [Required]
    public int FileId { get; set; }

    [ForeignKey("FileId")]
    public UserFile UserFile { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}