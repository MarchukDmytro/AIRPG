namespace AIRPG.Core.IDGenerationService;

public class IdGenerationService
{
    public static IdGenerationService Instance { get; } = new IdGenerationService();
    private int _currentId = 0;
    public int GetNextIdPlaceholder()
    {
        return _currentId++;
    }
}