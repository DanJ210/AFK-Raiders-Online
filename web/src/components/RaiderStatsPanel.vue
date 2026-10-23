<script setup lang="ts">
import { computed } from 'vue'

export type DangerStatsView = {
  danger: string; raids: number; extractions: number; knockOuts: number
  hpLostToRobot: number; hpLostToHazards: number
  medicalUsed: number; revivesUsed: number; shieldRechargersUsed: number; medicalCarried: number
  robotFights: number; robotsDefeated: number; robotsEscaped: number; robotFightsUnresolved: number
  robotDamageDealt: number; sharedRobotFights: number
  crossedPaths: number; sharedSearches: number; contestsWon: number; contestsLost: number
  itemsSecured: number; valueSecured: number; itemsLost: number; valueLost: number
  weaponsFound: number; weaponsLost: number
}
export type EconomyStatsView = {
  weaponsBought: number; coinsOnWeapons: number; repairs: number; coinsOnRepairs: number
  loanerKettles: number; suppliesBought: number; coinsOnSupplies: number
  itemsSold: number; coinsFromSales: number
}
export type RaiderStatsView = {
  scope: string; since: string | null; total: DangerStatsView
  byDanger: DangerStatsView[]; economy: EconomyStatsView
}

const props = defineProps<{ stats: RaiderStatsView | null | undefined }>()

const columns = computed(() => props.stats
  ? [{ ...props.stats.total, danger: 'All' }, ...props.stats.byDanger]
  : [])

const perRaid = (value: number, raids: number) => raids ? (value / raids).toFixed(1) : '—'
const percent = (value: number, raids: number) => raids ? `${Math.round((value / raids) * 100)}%` : '—'

const rows: { group: string; label: string; value: (s: DangerStatsView) => string | number }[] = [
  { group: 'Survival', label: 'Raids', value: s => s.raids },
  { group: 'Survival', label: 'Extracted / knocked out', value: s => `${s.extractions} / ${s.knockOuts}` },
  { group: 'Survival', label: 'Knockout rate', value: s => percent(s.knockOuts, s.raids) },
  { group: 'Survival', label: 'HP lost to robots (per raid)', value: s => `${s.hpLostToRobot} (${perRaid(s.hpLostToRobot, s.raids)})` },
  { group: 'Survival', label: 'HP lost to hazards (per raid)', value: s => `${s.hpLostToHazards} (${perRaid(s.hpLostToHazards, s.raids)})` },
  { group: 'Healing', label: 'Medical used / carried', value: s => `${s.medicalUsed} / ${s.medicalCarried}` },
  { group: 'Healing', label: 'Revives used', value: s => s.revivesUsed },
  { group: 'Healing', label: 'Shield rechargers used', value: s => s.shieldRechargersUsed },
  { group: 'Robots', label: 'Robot fights (shared)', value: s => `${s.robotFights} (${s.sharedRobotFights})` },
  { group: 'Robots', label: 'Defeated / escaped / unresolved', value: s => `${s.robotsDefeated} / ${s.robotsEscaped} / ${s.robotFightsUnresolved}` },
  { group: 'Robots', label: 'Damage dealt to robots', value: s => s.robotDamageDealt },
  { group: 'Raiders', label: 'Crossed paths', value: s => s.crossedPaths },
  { group: 'Raiders', label: 'Shared searches', value: s => s.sharedSearches },
  { group: 'Raiders', label: 'Contests won / lost', value: s => `${s.contestsWon} / ${s.contestsLost}` },
  { group: 'Loot', label: 'Items / value secured', value: s => `${s.itemsSecured} / ${s.valueSecured}` },
  { group: 'Loot', label: 'Items / value lost', value: s => `${s.itemsLost} / ${s.valueLost}` },
  { group: 'Loot', label: 'Weapons found / lost', value: s => `${s.weaponsFound} / ${s.weaponsLost}` },
]

const economyRows = computed(() => {
  const e = props.stats?.economy
  if (!e) return []
  return [
    { label: 'Weapons bought', value: `${e.weaponsBought} for ${e.coinsOnWeapons} coins` },
    { label: 'Repairs', value: `${e.repairs} for ${e.coinsOnRepairs} coins` },
    { label: 'Supplies bought', value: `${e.suppliesBought} for ${e.coinsOnSupplies} coins` },
    { label: 'Items sold', value: `${e.itemsSold} for ${e.coinsFromSales} coins` },
    { label: 'Loaner Tea Kettles', value: `${e.loanerKettles}` },
  ]
})

const sinceLabel = computed(() => props.stats?.since ? new Date(props.stats.since).toLocaleString() : null)
</script>

<template>
  <div class="raider-stats">
    <p class="eyebrow">RAIDER STATS / COMMITTED EVENTS ONLY</p>
    <p class="stats-scope">
      Counts settled raids and committed trades for this server session<span v-if="sinceLabel"> since {{ sinceLabel }}</span>.
      They reset when the server restarts.
    </p>
    <template v-if="stats && stats.total.raids > 0">
      <div class="table-scroll">
        <table class="stats-table">
          <thead>
            <tr>
              <th scope="col">Metric</th>
              <th v-for="column in columns" :key="column.danger" scope="col">{{ column.danger }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in rows" :key="row.label">
              <th scope="row"><small>{{ row.group }}</small> {{ row.label }}</th>
              <td v-for="column in columns" :key="column.danger">{{ row.value(column) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </template>
    <p v-else class="empty">No settled raids yet. Stats appear after the Raider's first extraction or knockout.</p>
    <p class="eyebrow stats-economy-heading">AUTONOMOUS ECONOMY</p>
    <div class="stats-economy">
      <span v-for="row in economyRows" :key="row.label"><small>{{ row.label }}</small><strong>{{ row.value }}</strong></span>
    </div>
  </div>
</template>

<style scoped>
.stats-scope { margin: 6px 0 10px; color: var(--muted); font-size: 11px; line-height: 1.5; }
.stats-table th[scope="row"] { color: #c8c5d9; font-weight: 400; }
.stats-table th[scope="row"] small { display: inline-block; min-width: 56px; color: var(--muted); font: 9px Consolas, monospace; text-transform: uppercase; }
.stats-economy-heading { margin-top: 16px; }
.stats-economy { display: grid; grid-template-columns: repeat(auto-fit, minmax(170px, 1fr)); gap: 8px; margin-top: 8px; }
.stats-economy span { display: flex; flex-direction: column; gap: 3px; padding: 8px 10px; border: 1px solid var(--line); }
.stats-economy small { color: var(--muted); font-size: 10px; }
.stats-economy strong { color: #e6e5f3; font: 12px Consolas, monospace; }
</style>
