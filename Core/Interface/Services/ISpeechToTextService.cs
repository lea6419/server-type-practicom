public interface ISpeechToTextService
{
    Task<SpeechToText?> GetSpeechToTextByFileIdAsync(int fileId);
    Task<SpeechToText> ConvertSpeechToTextAsync(int fileId, string text);
}
