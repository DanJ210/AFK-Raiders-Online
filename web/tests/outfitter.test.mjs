import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'
import test from 'node:test'
import ts from 'typescript'
import { computed, effectScope, nextTick, ref, watch } from 'vue'

const app = readFileSync(new URL('../src/App.vue', import.meta.url), 'utf8')
const program = readFileSync(new URL('../../server/AFKRaiders.Server/Program.cs', import.meta.url), 'utf8')
const script = app.split('<script setup lang="ts">')[1].split('</script>')[0]
const source = ts.createSourceFile('App.ts', script, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS)
const actions = ['purchaseShopItem', 'equipWeapon', 'repairWeapon', 'saveStagedLoadout']
const functions = source.statements
  .filter(node => ts.isFunctionDeclaration(node)
    && [...actions, 'request', 'isRecord'].includes(node.name?.text))
  .map(node => node.getText(source)).join('\n')
const compiled = ts.transpileModule(functions, {
  compilerOptions: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext },
}).outputText

function harness(failure = false) {
  const access = { accessToken: 'test-access' }
  const session = { value: access }
  const pending = { value: false }
  const message = { value: '' }
  const stagedDraft = { value: { bandage_white: 1, fizz_cell: 0 } }
  const calls = []
  let refreshes = 0
  const fetch = async (path, init) => {
    assert.ok(program.includes(`app.MapPost("${path}"`), `No server POST route for ${path}`)
    assert.equal(init.method, 'POST')
    assert.equal(init.headers.get('Authorization'), `Bearer ${access.accessToken}`)
    assert.equal(init.headers.get('Content-Type'), 'application/json')
    assert.equal(pending.value, true)
    calls.push({ path, body: JSON.parse(init.body) })
    return Response.json(failure ? { error: 'InsufficientCoins' } : { repairCostPaid: 18 }, {
      status: failure ? 400 : 200,
    })
  }
  const handlers = new Function(
    'session', 'outfitterActionPending', 'outfitterMessage', 'stagedDraft', 'refreshProjection', 'fetch',
    `${compiled}\nreturn { ${actions.join(', ')} };`,
  )(session, pending, message, stagedDraft, async () => { refreshes++ }, fetch)
  return { handlers, session, pending, message, calls, get refreshes() { return refreshes } }
}

const cases = [
  ['purchaseShopItem', ['aspperigo', 1], '/api/dev/handler/shop/purchase', { catalogId: 'aspperigo', quantity: 1 }, 'buy:', /Acquisition authorized/, 'Purchase'],
  ['purchaseShopItem', ['bandage_white', 5], '/api/dev/handler/shop/purchase', { catalogId: 'bandage_white', quantity: 5 }, 'buy:', /Acquisition authorized/, 'Purchase'],
  ['equipWeapon', ['owned-weapon'], '/api/dev/handler/equipment/equip', { weaponInstanceId: 'owned-weapon' }, 'equip:', /equipped successfully/, 'Equip'],
  ['repairWeapon', ['owned-weapon'], '/api/dev/handler/equipment/repair', { weaponInstanceId: 'owned-weapon' }, 'repair:', /18 .*deducted.*restored/, 'Repair'],
  ['saveStagedLoadout', [], '/api/dev/handler/loadout/stage', { stagedSupplies: [{ catalogId: 'bandage_white', quantity: 1 }] }, 'stage:', /staged for next deployment/, 'Staging'],
]

for (const [name, args, path, payload, prefix, success, errorPrefix] of cases) {
  test(`${name} ${args[0] ?? ''} posts the authenticated server contract and refreshes`, async () => {
    const h = harness()
    await h.handlers[name](...args)
    assert.equal(h.calls.length, 1)
    const { commandId, ...body } = h.calls[0].body
    assert.equal(h.calls[0].path, path)
    assert.ok(commandId.startsWith(prefix))
    assert.deepEqual(body, payload)
    assert.equal(h.refreshes, 1)
    assert.match(h.message.value, success)
    assert.equal(h.pending.value, false)
  })

  test(`${name} ${args[0] ?? ''} surfaces server errors and re-enables retry`, async () => {
    const h = harness(true)
    await h.handlers[name](...args)
    assert.equal(h.message.value, `${errorPrefix} failed: InsufficientCoins`)
    assert.equal(h.refreshes, 0)
    assert.equal(h.pending.value, false)
    await h.handlers[name](...args)
    assert.equal(h.calls.length, 2)
    assert.notEqual(h.calls[0].body.commandId, h.calls[1].body.commandId)
  })

  test(`${name} ${args[0] ?? ''} ignores absent sessions and pending actions`, async () => {
    const h = harness()
    h.session.value = null
    await h.handlers[name](...args)
    h.session.value = { accessToken: 'test-access' }
    h.pending.value = true
    await h.handlers[name](...args)
    assert.equal(h.calls.length, 0)
    assert.equal(h.refreshes, 0)
  })
}

const stagingStateNames = [
  'shopCatalog', 'stagedDraft', 'ownedSupplies', 'stagedSupplies',
  'stagedMedicalCount', 'stagedRechargerCount',
]
const stagingSource = source.statements.filter(node =>
  (ts.isVariableStatement(node)
    && node.declarationList.declarations.some(declaration => stagingStateNames.includes(declaration.name.getText(source))))
  || (ts.isFunctionDeclaration(node) && ['getOwnedSupplyQuantity', 'adjustStagedDraft'].includes(node.name?.text))
  || (ts.isExpressionStatement(node) && ts.isCallExpression(node.expression)
    && node.expression.expression.getText(source) === 'watch' && node.getText(source).includes('stagedDraft')),
).map(node => node.getText(source)).join('\n')
const compiledStaging = ts.transpileModule(stagingSource, {
  compilerOptions: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext },
}).outputText

function stagingHarness(t) {
  const handlerProjection = ref(null)
  const scope = effectScope()
  t.after(() => scope.stop())
  return scope.run(() => new Function(
    'handlerProjection', 'computed', 'ref', 'watch',
    `${compiledStaging}
    return { handlerProjection, stagedDraft, stagedSupplies, stagedMedicalCount, stagedRechargerCount,
      getOwnedSupplyQuantity, adjustStagedDraft };`,
  )(handlerProjection, computed, ref, watch))
}

function supplyProjection() {
  return {
    playerId: 'raider-one',
    supplyStock: [
      { catalogId: 'bandage_white', category: 'Medical', quantity: 5 },
      { catalogId: 'fizz_cell', category: 'ShieldRecharger', quantity: 3 },
    ],
    stagedSupplies: [],
    shopCatalog: {
      supplies: [
        { id: 'bandage_white', category: 'Medical' },
        { id: 'fizz_cell', category: 'ShieldRecharger' },
      ],
    },
  }
}

test('purchased server supplyStock enables staging and retains stock/category limits', async t => {
  const h = stagingHarness(t)
  assert.equal(h.getOwnedSupplyQuantity('bandage_white'), 0)
  h.handlerProjection.value = supplyProjection()
  await nextTick()
  assert.equal(h.getOwnedSupplyQuantity('bandage_white'), 5)
  assert.equal(h.getOwnedSupplyQuantity('missing'), 0)
  for (let i = 0; i < 6; i++) h.adjustStagedDraft('bandage_white', 1, 'Medical')
  for (let i = 0; i < 4; i++) h.adjustStagedDraft('fizz_cell', 1, 'ShieldRecharger')
  assert.equal(h.stagedMedicalCount.value, 4)
  assert.equal(h.stagedRechargerCount.value, 2)
  assert.equal(h.getOwnedSupplyQuantity('bandage_white'), 5, 'draft does not consume stock')
  for (let i = 0; i < 6; i++) h.adjustStagedDraft('bandage_white', -1, 'Medical')
  assert.equal(h.stagedDraft.value.bandage_white, 0)
  h.handlerProjection.value.supplyStock[0].quantity = 1
  h.adjustStagedDraft('bandage_white', 1, 'Medical')
  h.adjustStagedDraft('bandage_white', 1, 'Medical')
  assert.equal(h.stagedDraft.value.bandage_white, 1)
})

test('unchanged polling snapshots and additional purchases preserve unsaved staging edits', async t => {
  const h = stagingHarness(t)
  h.handlerProjection.value = supplyProjection()
  await nextTick()
  h.adjustStagedDraft('bandage_white', 1, 'Medical')
  for (let i = 0; i < 3; i++) {
    const refreshed = supplyProjection()
    refreshed.supplyStock[0].quantity += i
    h.handlerProjection.value = refreshed
    await nextTick()
    assert.equal(h.stagedDraft.value.bandage_white, 1)
  }
})

test('authoritative staging changes and identity changes resynchronize the draft', async t => {
  const h = stagingHarness(t)
  const projection = supplyProjection()
  projection.stagedSupplies = [{ catalogId: 'bandage_white', category: 'Medical', quantity: 1 }]
  h.handlerProjection.value = projection
  await nextTick()
  assert.equal(h.stagedDraft.value.bandage_white, 1)
  h.adjustStagedDraft('bandage_white', 1, 'Medical')
  h.handlerProjection.value = structuredClone(projection)
  await nextTick()
  assert.equal(h.stagedDraft.value.bandage_white, 2, 'unchanged saved quantities preserve edits')
  const updated = structuredClone(projection)
  updated.stagedSupplies[0].quantity = 3
  h.handlerProjection.value = updated
  await nextTick()
  assert.equal(h.stagedDraft.value.bandage_white, 3)
  h.adjustStagedDraft('bandage_white', 1, 'Medical')
  h.handlerProjection.value = { ...structuredClone(updated), playerId: 'raider-two' }
  await nextTick()
  assert.equal(h.stagedDraft.value.bandage_white, 3, 'another identity cannot inherit unsaved edits')
  h.handlerProjection.value = null
  await nextTick()
  assert.deepEqual(h.stagedDraft.value, {})
})
