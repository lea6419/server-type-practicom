public class SpeechToTextService : ISpeechToTextService
{
    private readonly ISpeechToTextRepository _speechToTextRepository;

    public SpeechToTextService(ISpeechToTextRepository speechToTextRepository)
    {
        _speechToTextRepository = speechToTextRepository;
    }

    public async Task<SpeechToText?> GetSpeechToTextByFileIdAsync(int fileId)
    {
        return await _speechToTextRepository.GetByIdAsync(fileId);
    }

    public async Task<SpeechToText> ConvertSpeechToTextAsync(int fileId, string text)
    {
        var speechToText = new SpeechToText
        {
            FileId = fileId,
            Text = text,
            CreatedAt = DateTime.UtcNow
        };

        await _speechToTextRepository.AddAsync(speechToText);
        return speechToText;
    }
}
