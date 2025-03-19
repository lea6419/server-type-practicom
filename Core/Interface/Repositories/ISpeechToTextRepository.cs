public interface ISpeechToTextRepository : IRepository<SpeechToText>
{
    Task<SpeechToText> GetByFileIdAsync(int fileId);
}
