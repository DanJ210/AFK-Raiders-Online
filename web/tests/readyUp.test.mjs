import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'
import test from 'node:test'
import ts from 'typescript'

const app = readFileSync(new URL('../src/App.vue', import.meta.url), 'utf8')
const program = readFileSync(new URL('../../server/AFKRaiders.Server/Program.cs', import.meta.url), 'utf8')
const script = app.split('<script setup lang="ts">')[1].split('</script>')[0]
const source = ts.createSourceFile('App.ts', script, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS)
const functions = source.statements.filter(node => ts.isFunctionDeclaration(node)
  && ['readyUp', 'request', 'isRecord'].includes(node.name?.text)).map(node => node.getText(source)).join('\n')
const compiled = ts.transpileModule(functions, {
  compilerOptions: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext },
}).outputText

function harness(statuses = [200]) {
  const session = { value: { accessToken: 'local-test-token', slot: 'raider-one' } }
  const handlerProjection = { value: { canReadyUp: true, readyUpSignalCost: 2 } }
  const signalCurrent = { value: 3 }
  const pending = { value: false }
  const error = { value: '' }
  const calls = []
  const accepted = []
  const cleared = []
  const fetch = async (path, init) => {
    assert.ok(program.includes(`app.MapPost("${path}"`))
    assert.equal(path, '/api/dev/handler/ready-up')
    assert.equal(init.method, 'POST')
    assert.equal(init.headers.get('Authorization'), `Bearer ${session.value.accessToken}`)
    assert.equal(pending.value, true)
    calls.push(JSON.parse(init.body))
    const status = statuses.shift() ?? 200
    if (status === 401) return new Response(null, { status })
    return Response.json(status === 200 ? { canReadyUp: false } : { error: 'RaiderNotAtHome' }, { status })
  }
  const readyUp = new Function('session', 'handlerProjection', 'readyUpPending', 'error',
    'acceptProjection', 'clearSession', 'fetch', 'signalCurrent',
    `let readyUpIntent = null;\n${compiled}\nreturn readyUp;`)(
    session, handlerProjection, pending, error,
    (value, access) => accepted.push({ value, access }),
    (...args) => cleared.push(args), fetch, signalCurrent)
  return { readyUp, session, handlerProjection, pending, error, calls, accepted, cleared, signalCurrent }
}

test('Ready Up posts authenticated intent and accepts the authoritative projection', async () => {
  const h = harness()
  await h.readyUp()
  assert.equal(h.calls.length, 1)
  assert.ok(h.calls[0].commandId)
  assert.equal(h.accepted[0].value.canReadyUp, false)
  assert.equal(h.pending.value, false)
  assert.equal(h.error.value, '')
})

test('Ready Up reuses its command ID after an uncertain failure', async () => {
  const h = harness([503, 200, 200])
  await h.readyUp()
  assert.match(h.error.value, /RaiderNotAtHome/)
  assert.equal(h.pending.value, false)
  await h.readyUp()
  assert.equal(h.calls[0].commandId, h.calls[1].commandId)
  await h.readyUp()
  assert.notEqual(h.calls[1].commandId, h.calls[2].commandId)
})

test('Ready Up ignores unavailable or already-pending intent', async () => {
  const h = harness()
  h.pending.value = true
  await h.readyUp()
  h.pending.value = false
  h.handlerProjection.value.canReadyUp = false
  await h.readyUp()
  h.session.value = null
  await h.readyUp()
  assert.equal(h.calls.length, 0)
})

test('Ready Up clears expired sessions and exposes the reason', async () => {
  const h = harness([401])
  await h.readyUp()
  assert.equal(h.cleared[0][0], 'raider-one')
  assert.match(h.cleared[0][1], /expired/)
  assert.equal(h.accepted.length, 0)
  assert.equal(h.pending.value, false)
})

test('Ready Up creates fresh intent when switching Handler identity', async () => {
  const h = harness([503, 200])
  await h.readyUp()
  h.session.value = { accessToken: 'second-local-test-token', slot: 'raider-two' }
  await h.readyUp()
  assert.notEqual(h.calls[0].commandId, h.calls[1].commandId)
})

test('Ready Up requires the projected Signal cost before sending intent', async () => {
  const h = harness()
  h.signalCurrent.value = 1
  await h.readyUp()
  assert.equal(h.calls.length, 0)
  h.signalCurrent.value = 2
  await h.readyUp()
  assert.equal(h.calls.length, 1)
  assert.match(app, /Ready Up \(\$\{handlerProjection\.readyUpSignalCost\} Signal\)/)
  assert.match(app, /:disabled="readyUpPending \|\| !handlerProjection.canReadyUp \|\| signalCurrent < handlerProjection.readyUpSignalCost"/)
})
