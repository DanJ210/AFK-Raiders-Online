import assert from 'node:assert/strict'
import test from 'node:test'
import { rarityAttributes, rarityName } from '../src/rarity.ts'

test('all six rarity tiers use the exact requested names and color order', () => {
  const expected = [
    [1, 'Uncommon', '#a6abbd'],
    [2, 'Common', '#69db7c'],
    [3, 'Rare', '#4dabf7'],
    [4, 'Epic', '#da77f2'],
    [5, 'Legendary', '#ffd166'],
    [6, 'Augmented', '#ff922b'],
  ]
  for (const [tier, name, color] of expected) {
    assert.equal(rarityName(tier), name)
    assert.deepEqual(rarityAttributes(tier), {
      class: 'rarity-item',
      'data-rarity': name,
      title: `Rarity: ${name}`,
      style: { '--rarity-color': color },
    })
  }
})

test('missing and unsupported rarity stay explicitly unavailable rather than pretending to be Uncommon', () => {
  for (const rarity of [undefined, 0, -1, 7, 1.5, NaN]) {
    assert.equal(rarityName(rarity), 'Rarity unavailable')
    assert.equal(rarityAttributes(rarity)['data-rarity'], 'unavailable')
    assert.equal(rarityAttributes(rarity).title, 'Rarity unavailable')
    assert.equal(rarityAttributes(rarity).style['--rarity-color'], 'var(--muted)')
  }
})

test('presentation attributes cannot mutate the shared rarity palette', () => {
  const attributes = rarityAttributes(3)
  attributes.style['--rarity-color'] = '#000000'
  assert.equal(rarityAttributes(3).style['--rarity-color'], '#4dabf7')
})
