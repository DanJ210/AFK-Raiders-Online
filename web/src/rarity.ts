const rarities = new Map([
  [1, { name: 'Uncommon', color: '#a6abbd' }],
  [2, { name: 'Common', color: '#69db7c' }],
  [3, { name: 'Rare', color: '#4dabf7' }],
  [4, { name: 'Epic', color: '#da77f2' }],
  [5, { name: 'Legendary', color: '#ffd166' }],
  [6, { name: 'Augmented', color: '#ff922b' }],
])

export function rarityName(rarity: number | undefined): string {
  return rarities.get(rarity ?? 0)?.name ?? 'Rarity unavailable'
}

export function rarityAttributes(rarity: number | undefined) {
  const tier = rarities.get(rarity ?? 0)
  return {
    class: 'rarity-item',
    'data-rarity': tier?.name ?? 'unavailable',
    title: tier ? `Rarity: ${tier.name}` : 'Rarity unavailable',
    style: { '--rarity-color': tier?.color ?? 'var(--muted)' },
  }
}
