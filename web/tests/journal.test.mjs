import assert from 'node:assert/strict'
import test from 'node:test'
import { enrichmentExplanation, journalCategories, matchesJournalFilter, mergeJournal, narrationLabel } from '../src/journal.ts'

test('narration badges distinguish authored content from accepted AI without treating unrequested entries as failures', () => {
  assert.equal(narrationLabel(), 'Authored')
  assert.equal(narrationLabel('CanonFallback'), 'Authored')
  assert.equal(narrationLabel('ArchivalLog'), 'Authored')
  assert.equal(narrationLabel('Generated'), 'AI-enhanced')
  assert.equal(narrationLabel('FutureSource'), 'FutureSource')
  assert.match(enrichmentExplanation(), /not requested.*not a generation failure/)
  assert.match(enrichmentExplanation('Pending'), /queued or running/)
  assert.match(enrichmentExplanation('Ready'), /accepted AI aside/)
  assert.match(enrichmentExplanation('Fallback'), /rejection, timeout or queue saturation/)
  assert.match(enrichmentExplanation('Expired'), /became stale/)
  assert.match(enrichmentExplanation('FutureStatus'), /Unrecognized.*FutureStatus/)
  assert.match(enrichmentExplanation('NotRequested', 'LiveBudgetBusy'), /avoid an inference backlog/)
  assert.match(enrichmentExplanation('NotRequested', 'ModelUnavailable'), /Live AI is offline.*restart the backend/)
})

test('diary badges coexist with hub and actual encounter categories and drive the same filters', () => {
  const cases = [
    [{ channel: 'Diary', kind: 'AreaActivityStarted' }, ['Diary']],
    [{ channel: 'Hub', kind: 'ReturnedHome' }, ['Diary', 'Hub']],
    [{ channel: 'Diary', kind: 'CrossedPaths' }, ['Diary', 'Encounter']],
    [{ channel: 'Diary', kind: 'EncounterResolved' }, ['Diary', 'Encounter']],
    [{ channel: 'Diary', kind: 'RobotRound' }, ['Diary', 'Encounter', 'Combat']],
    [{ channel: 'Combat', kind: 'Damage' }, ['Diary', 'Combat', 'Encounter']],
    [{ channel: 'Support', kind: 'HandlerSupportAction' }, ['Diary', 'Support']],
    [{ channel: 'FutureChannel', kind: 'FutureEvent' }, ['Diary', 'FutureChannel']],
    [{ channel: 'Diary', kind: 'Search' }, ['Diary']],
  ]
  for (const [event, categories] of cases) {
    assert.deepEqual(journalCategories(event), categories)
    assert.equal(matchesJournalFilter(event, 'all'), true)
    assert.equal(matchesJournalFilter(event, 'diary'), true)
    assert.equal(matchesJournalFilter(event, 'hub'), categories.includes('Hub'))
    assert.equal(matchesJournalFilter(event, 'encounters'), categories.includes('Encounter'))
  }
})

test('enrichment replaces an existing source without duplicate entries or losing older raids', () => {
  const existing = [
    { sourceEventId: 'raid:old:1', sequence: 1, text: 'old facts' },
    { sourceEventId: 'raid:current:1', sequence: 2, text: 'original facts', enrichmentStatus: 'Pending' },
  ]
  const incoming = [
    { sourceEventId: 'raid:current:1', sequence: 2, text: 'original facts', flavor: 'I feel quietly hopeful about my dignity.', narrativeSource: 'Generated', enrichmentStatus: 'Ready', enrichmentReason: null },
    { sourceEventId: 'raid:current:2', sequence: 3, text: 'next facts' },
  ]
  const merged = mergeJournal(existing, incoming)
  assert.equal(merged.length, 3)
  assert.equal(merged[0].text, 'old facts')
  assert.equal(merged[1].text, 'original facts')
  assert.equal(merged[1].flavor, incoming[0].flavor)
  assert.equal(merged[1].enrichmentStatus, 'Ready')
  assert.equal(merged[1].enrichmentReason, null)
  assert.deepEqual(mergeJournal(merged, incoming), merged)
})

test('failure reason updates survive source-id merging without changing facts', () => {
  const before = { sourceEventId: 'event:1', sequence: 1, text: 'facts', enrichmentStatus: 'Pending' }
  const after = { ...before, enrichmentStatus: 'Fallback', enrichmentReason: 'InferenceDeadlineExceeded' }
  assert.deepEqual(mergeJournal([before], [after]), [after])
})
