export function narrationLabel(source = 'CanonFallback'): string {
  if (source === 'Generated') return 'AI-enhanced'
  if (source === 'CanonFallback' || source === 'ArchivalLog') return 'Authored'
  return source
}

export function enrichmentExplanation(status = 'NotRequested', reason?: string | null): string {
  if (status === 'NotRequested' && reason === 'LiveBudgetBusy')
    return 'Another live AI aside is pending for this world. This entry stays authored to avoid an inference backlog.'
  switch (status) {
    case 'NotRequested': return 'AI enrichment was not requested. This is not a generation failure.'
    case 'Pending': return 'Optional AI enrichment is queued or running; authored narration remains available.'
    case 'Ready': return 'An accepted AI aside supplements the authoritative facts.'
    case 'Fallback': return 'AI enrichment did not produce an accepted aside (for example, rejection, timeout or queue saturation). Authored narration remains available.'
    case 'Expired': return 'The AI request became stale. Authored narration remains available.'
    default: return `Unrecognized AI enrichment status: ${status}.`
  }
}

export function mergeJournal<T extends { sourceEventId: string; sequence: number }>(
  existing: readonly T[], incoming: readonly T[],
): T[] {
  const entries = new Map(existing.map(entry => [entry.sourceEventId, entry]))
  for (const entry of incoming) entries.set(entry.sourceEventId, entry)
  return [...entries.values()].sort((a, b) => a.sequence - b.sequence)
}
