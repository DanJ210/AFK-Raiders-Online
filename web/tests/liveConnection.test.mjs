import assert from 'node:assert/strict'
import test from 'node:test'
import { clearLiveWarning, liveClosedMessage, stopOwnedConnection } from '../src/liveConnection.ts'

test('intentional stop releases ownership before the close callback', async () => {
  let warning = ''
  const live = {
    async stop() {
      if (owned === live) warning = liveClosedMessage
    },
  }
  let owned = live
  await stopOwnedConnection(owned, () => { owned = null })
  assert.equal(owned, null)
  assert.equal(warning, '')
})

test('recovery clears only the live-close warning, not command errors', () => {
  assert.equal(clearLiveWarning(liveClosedMessage), '')
  assert.equal(clearLiveWarning('Appraisal failed'), 'Appraisal failed')
})

test('stop completion cannot release a replacement connection', async () => {
  let finish
  const old = { stop: () => new Promise(resolve => { finish = resolve }) }
  let owned = old
  const stopping = stopOwnedConnection(old, () => { owned = null })
  const replacement = { stop: async () => {} }
  owned = replacement
  finish()
  await stopping
  assert.equal(owned, replacement)
})

test('failed stop remains an explicit failure with ownership already released', async () => {
  const live = { stop: async () => { throw new Error('transport stop failed') } }
  let owned = live
  await assert.rejects(stopOwnedConnection(live, () => { owned = null }), /transport stop failed/)
  assert.equal(owned, null)
})
