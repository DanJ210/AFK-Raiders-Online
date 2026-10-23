import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'
import test from 'node:test'

const css = readFileSync(new URL('../src/style.css', import.meta.url), 'utf8')

function declarations(selector) {
  const rule = css.split(`${selector} {`)[1]?.split('}')[0]
  assert.ok(rule, `Missing layout rule: ${selector}`)
  return rule
}

test('backpack descriptions retain a readable basis and can wrap actions onto another line', () => {
  assert.match(declarations('.backpack-panel .item-row'), /flex-wrap:\s*wrap/)
  assert.match(declarations('.backpack-panel .item-row > span'), /flex:\s*1 1 160px/)
})

test('grouped backpack actions remain intrinsic width instead of inheriting full-width buttons', () => {
  const actions = declarations('.backpack-panel .item-row-actions')
  assert.match(actions, /flex:\s*0 0 auto/)
  assert.match(actions, /max-width:\s*100%/)
  assert.match(declarations('.backpack-panel .item-row-actions > button'), /width:\s*auto/)
  assert.match(declarations('.item-row.rarity-item .item-row-actions'), /flex-wrap:\s*wrap/)
})
