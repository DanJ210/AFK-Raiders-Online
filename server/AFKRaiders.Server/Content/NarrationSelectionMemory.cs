using AFKRaiders.Server.Domain;

namespace AFKRaiders.Server.Content;

// Presentation metadata only, transferred with the Raider's existing voice checkpoint.
internal sealed class NarrationSelectionMemory
{
    private readonly Dictionary<string, long> lastUses = new(StringComparer.Ordinal);
    private long count;
    private string? lastLine;

    private long Recency(string line) =>
        lastUses.TryGetValue(line, out var use) ? use - count : long.MinValue;

    internal static string Select(IReadOnlyList<string> eligible,
        IReadOnlyList<NarrationSelectionMemory> recipients)
    {
        if (eligible.Count == 0)
            throw new InvalidOperationException("Narration selection requires an eligible line.");
        if (recipients.Count == 0)
            throw new InvalidOperationException("Narration selection requires a recipient.");
        var offset = (int)((ulong)recipients.Max(memory => memory.count) % (ulong)eligible.Count);
        var selected = Enumerable.Range(0, eligible.Count)
            .Select(index => eligible[(offset + index) % eligible.Count])
            .OrderBy(line => recipients.Max(memory => memory.Recency(line)))
            .First();
        foreach (var memory in recipients) memory.Record(selected);
        return selected;
    }

    private void Record(string line)
    {
        lastUses[line] = count++;
        lastLine = line;
    }

    internal RaiderVoiceState Snapshot() => new(
        new Dictionary<string, long>(), lastLine,
        new Dictionary<string, long>(lastUses, StringComparer.Ordinal), count);

    internal void Restore(RaiderVoiceState state)
    {
        lastUses.Clear();
        if (state.LineUses is not null)
            foreach (var (line, use) in state.LineUses) lastUses.Add(line, use);
        count = state.QuoteCount;
        lastLine = state.LastLine;
    }
}
