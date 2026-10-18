import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'
import test from 'node:test'
import ts from 'typescript'

const app = readFileSync(new URL('../src/App.vue', import.meta.url), 'utf8')
const catalog = JSON.parse(readFileSync(new URL('../../server/AFKRaiders.Server/Content/shop-catalog.json', import.meta.url), 'utf8'))
const script = app.split('<script setup lang="ts">')[1].split('</script>')[0]
const source = ts.createSourceFile('App.ts', script, ts.ScriptTarget.Latest, true, ts.ScriptKind.TS)
const damageFunction = source.statements.find(node =>
  ts.isFunctionDeclaration(node) && node.name?.text === 'weaponDamage')
const compiled = ts.transpileModule(damageFunction.getText(source), {
  compilerOptions: { target: ts.ScriptTarget.ES2022, module: ts.ModuleKind.ESNext },
}).outputText

function harness(value) {
  const shopCatalog = { value }
  const weaponDamage = new Function('shopCatalog', `${compiled}\nreturn weaponDamage;`)(shopCatalog)
  return { shopCatalog, weaponDamage }
}

test('every owned weapon resolves its damage range and multiplier from the authoritative catalog', () => {
  const { weaponDamage } = harness(catalog)
  for (const weapon of catalog.weapons) {
    assert.equal(weaponDamage(weapon.id),
      `${weapon.damageMin}\u2013${weapon.damageMax} (x${weapon.damageMultiplier})`)
    assert.doesNotMatch(weaponDamage(weapon.id), /undefined|\(x\)/)
  }
  assert.match(app, /weaponDamage\(equippedWeapon\.catalogId\)/)
  assert.match(app, /weaponDamage\(wpn\.catalogId\)/)
  assert.doesNotMatch(app, /(?:equippedWeapon|wpn)\.(?:damageMin|damageMax|damageMultiplier)/)
})

test('damage follows catalog refreshes instead of retaining stale equipment stats', () => {
  const h = harness(catalog)
  h.shopCatalog.value = {
    weapons: [{ ...catalog.weapons[0], damageMin: 10, damageMax: 12, damageMultiplier: 1.5 }],
  }
  assert.equal(h.weaponDamage('tea_kettle'), '10\u201312 (x1.5)')
})

test('missing catalogs and unknown weapon IDs display an explicit unavailable message', () => {
  for (const catalogValue of [null, { weapons: [] }, catalog]) {
    const { weaponDamage } = harness(catalogValue)
    assert.equal(weaponDamage('unknown'), 'Unavailable (weapon catalog entry missing)')
  }
})
