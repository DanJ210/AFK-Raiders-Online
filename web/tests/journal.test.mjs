import assert from 'node:assert/strict'
import test from 'node:test'
import { readFileSync } from 'node:fs'
import { diaryPresentation, enrichmentExplanation, journalCategories, matchesJournalFilter, mergeJournal, narrationLabel } from '../src/journal.ts'

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

test('personality deltas leave the sentence but every committed number stays in the detail record', () => {
  const text = 'Staying deeper: Greed 9 -> 10; Mood -1 -> 2; resilience 3.\nRaider: My concerns remain provisional.'
  const event = { kind: 'PersonalityChanged', text }
  assert.deepEqual(diaryPresentation(event), {
    main: 'Staying deeper.\nRaider: My concerns remain provisional.', record: text, changed: true,
  })
  assert.equal(event.text, text)
  assert.equal(diaryPresentation({ kind: 'PersonalityChanged', text: 'Future format: 123' }).main, 'Future format: 123')
})

test('area diary hides only exact recurring boilerplate and retains actual co-presence in details', () => {
  const text = 'Raider One checking straps at Market. Raider Two is also actually present here. No loot, damage or new actor created.\nRaider: My socks object.'
  const result = diaryPresentation({ kind: 'AreaDiary', text })
  assert.equal(result.main, 'Raider One checking straps at Market.\nRaider: My socks object.')
  assert.equal(result.record, text)
  const otherKind = { kind: 'Unknown', text }
  assert.equal(diaryPresentation(otherKind).main, text)
  assert.equal(diaryPresentation({ kind: 'AreaDiary', text: 'Lost 4 HP. No loot recovered.' }).main, 'Lost 4 HP. No loot recovered.')
})

test('exit decisions, travel and activity duration keep their numbers available without altering outcomes', () => {
  for (const [kind, text, main] of [
    ['ExtractionDecision', 'Greed Check: 12 % exit chance; staying for now. Greed 39.', 'Greed Check: staying for now.'],
    ['ExtractionDecision', 'Deadline urgency: 100% exit chance; heading for extraction. Greed 99.', 'Deadline urgency: heading for extraction.'],
    ['Travel', 'One leaves Market for Salvage Yard. ETA 12s. Navigation by questionable instinct.', 'One leaves Market for Salvage Yard. Navigation by questionable instinct.'],
    ['AreaActivityStarted', 'One begins surveying at Market (65s). Reconnaissance only; no new loot opportunity.', 'One begins surveying at Market. Reconnaissance only; no new loot opportunity.'],
  ]) {
    assert.deepEqual(diaryPresentation({ kind, text }), { main, record: text, changed: true })
  }
  for (const kind of ['ShieldAbsorbed', 'UnmitigatedDamage', 'RobotRound', 'HandlerAction']) {
    const text = 'Shield lost 4 charge, 3 HP got through.'
    assert.deepEqual(diaryPresentation({ kind, text }), { main: text, record: text, changed: false })
  }
})

test('the diary uses native expandable records and wraps long populated content', () => {
  const component = readFileSync(new URL('../src/components/DiaryEntryText.vue', import.meta.url), 'utf8')
  const app = readFileSync(new URL('../src/App.vue', import.meta.url), 'utf8')
  const css = readFileSync(new URL('../src/style.css', import.meta.url), 'utf8')
  assert.match(component, /<details v-if="presentation.changed"/)
  assert.match(component, /<summary>Event details<\/summary>/)
  assert.match(component, /\{\{ presentation.record \}\}/)
  assert.match(app, /<DiaryEntryText :event="event"\s*\/>/)
  assert.match(css, /\.diary-entry-text\s*\{[^}]*min-width:\s*0[^}]*overflow-wrap:\s*anywhere/)
})
