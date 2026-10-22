import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'
import test from 'node:test'

const app = readFileSync(new URL('../src/App.vue', import.meta.url), 'utf8')
const program = readFileSync(new URL('../../server/AFKRaiders.Server/Program.cs', import.meta.url), 'utf8')
const coordinator = readFileSync(new URL('../../server/AFKRaiders.Server/Domain/RaidCoordinator.cs', import.meta.url), 'utf8')
const domainFiles = [
  '../../server/AFKRaiders.Server/Domain/Equipment.cs',
  '../../server/AFKRaiders.Server/Domain/HomeStash.cs',
].map(path => readFileSync(new URL(path, import.meta.url), 'utf8')).join('\n')

const retiredRoutes = [
  '/api/dev/handler/shop/purchase',
  '/api/dev/handler/equipment/equip',
  '/api/dev/handler/equipment/repair',
  '/api/dev/handler/loadout/stage',
  '/api/dev/handler/stash/sell',
]

test('Handler economy mutation endpoints and coordinator commands are retired', () => {
  for (const route of retiredRoutes) {
    assert.ok(!program.includes(route), `Server still maps retired route ${route}`)
    assert.ok(!app.includes(route), `Client still calls retired route ${route}`)
  }

  for (const command of ['PurchaseShopItem', 'EquipWeapon', 'RepairWeapon', 'StageLoadout', 'SellStash']) {
    assert.doesNotMatch(coordinator, new RegExp(`public\\s+[^\\n]*\\b${command}\\s*\\(`))
  }
  for (const request of [
    'PurchaseShopItemRequest',
    'EquipWeaponRequest',
    'RepairWeaponRequest',
    'StageLoadoutRequest',
    'SellStashItemsRequest',
  ]) {
    assert.ok(!domainFiles.includes(request), `Obsolete request type ${request} remains`)
  }
})

test('Loadout and inventory view renders committed Raider state without mutation controls', () => {
  const panelStart = app.indexOf('<CollapsiblePanel class="panel preparation-panel"')
  const panelEnd = app.indexOf('</CollapsiblePanel>', panelStart)
  assert.notEqual(panelStart, -1)
  assert.notEqual(panelEnd, -1)
  const panel = app.slice(panelStart, panelEnd)

  assert.match(panel, /RAIDER-OWNED \/ READ ONLY/)
  assert.match(panel, /SPENDABLE COINS/)
  assert.match(panel, /OWNED WEAPONS/)
  assert.match(panel, /RAIDER-OWNED SUPPLIES/)
  assert.match(panel, /LATEST COMMITTED PREPARATION DECISIONS/)
  assert.doesNotMatch(panel, /<button\b|<input\b|@click=/)
  assert.doesNotMatch(app, /Liquidate|sellQuantity|sellingGroup|executeSell/)
})
