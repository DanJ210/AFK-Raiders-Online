<script setup lang="ts">
import { enrichmentExplanation, mergeJournal, narrationLabel } from './journal'
import { clearLiveWarning, liveClosedMessage, stopOwnedConnection } from './liveConnection'
import { computed, onBeforeUnmount, onMounted, ref, shallowRef, watch } from 'vue'
import { HubConnectionBuilder, type HubConnection } from '@microsoft/signalr'

type PlayerView = {
  playerId: string; displayName: string; disposition: string; location: string
  activity: string; destination: string | null; activityEndsAt: string | null
}
type ItemView = { instanceId: string; catalogId: string; name: string; value: number; flavor: string }
type EncounterView = {
  encounterId: string; resolvedAt: string; location: string; participantIds: string[]; resolution: string; reason: string
  factors: { playerId: string; displayName: string; disposition: string; greed: number
    aptitude: number; backpackValue: number; initiative: number; score: number }[]
  awards: { instanceId: string; itemName: string; recipientId: string; recipientName: string }[]
}
type RobotEncounterView = {
  encounterId: string; startedAt: string; resolvedAt: string; location: string; resolution: string
  participantIds: string[]; damage: { playerId: string; damageDealt: number; damageTaken: number }[]; reason: string
}
type ActiveThreadView = {
  threadId: string; kind: string; title: string; location: string; status: string
  startedAt: string; endsAt: string | null; totalSeconds: number; summary: string; logs: string[]
}
type StashGroupView = {
  catalogId: string; name: string; unitValue: number; quantity: number
  totalValue: number; flavor: string; instanceIds: string[]
}
type AarDebriefView = {
  bullets: string[]
  source: string
  generatedAt: string
}
type WeaponDefinition = {
  id: string; name: string; damage: number; damageMultiplier: number
  damageMin: number; damageMax: number; value: number; rarity: number
  durabilityMax: number; repairCost: number; description: string; flavor: string
}
type SupplyDefinition = {
  id: string; name: string; category: string; value: number; rarity: number
  healAmount: number; reviveAmount: number; chargeAmount: number; maxStack: number
  description: string; flavor: string
}
type OwnedWeapon = {
  instanceId: string; catalogId: string; name: string; currentDurability: number
  maxDurability: number; repairCost: number; damage: number; damageMultiplier: number
  damageMin: number; damageMax: number; rarity: number; flavor: string
}
type OwnedSupplyStack = {
  catalogId: string; name: string; category: string; quantity: number; costPerUnit: number
  healAmount: number; reviveAmount: number; chargeAmount: number; rarity: number; flavor: string
}
type StagedSupplyItem = {
  catalogId: string; name: string; category: string; quantity: number
}
type ShopCatalogView = {
  weapons: WeaponDefinition[]; supplies: SupplyDefinition[]
}
type ItemAppraisalView = {
  catalogId: string; itemName: string; appraisal: string
  strategicClassification: string; source: string; generatedAt: string
}
type HaulPayoffView = {
  settlementId: string; grossSecuredCount: number; grossSecuredValue: number
  securedItems: ItemView[]; unitsRetainedInStash: number; unitsAutoSold: number
  autoSoldItems: ItemView[]; overflowCoinsEarned: number; netStashValueChange: number
  stashUnitsAfter: number; stashValueAfter: number; coinsBalanceAfter: number
}
type ReturnSummaryView = {
  raidId: string; extractedAt: string; securedLootCount: number; totalValue: number
  items: ItemView[]; debriefSummary: string; highlights: string[]
  payoff?: HaulPayoffView | null
  aarDebrief?: AarDebriefView | null
  topItemAppraisal?: ItemAppraisalView | null
  outcome?: string
  causeOfDefeat?: string | null
  lostHaulValue?: number
  pocketSalvagedItem?: ItemView | null
}
type HandlerJournalEntryView = {
  sequence: number; sourceEventId: string; raidId: string | null; at: string
  kind: string; channel: string; text: string; location?: string | null
  activity?: string | null; destination?: string | null; durationSeconds?: number | null
  healthDelta?: number | null; robotDamageDealt?: number | null; robotDamageTaken?: number | null
  robotRemainingHealth?: number | null; itemName?: string | null; itemValue?: number | null
  itemInstanceId?: string | null; opportunityId?: string | null; reason?: string | null
  threadId?: string | null
  factsText?: string | null; flavor?: string | null
  narrativeSource?: string; enrichmentStatus?: string; enrichmentReason?: string | null
}
type Projection = {
  raidId: string; revision: number; rulesVersion: string; serverTime: string; status: string
  departsAt: string; nextActivityAt: string | null; location: string; players: PlayerView[]
  locations: { name: string; neighbors: string[] }[]
  crate: { crateId: string; location: string; status: string; remainingItems: number }
  robot: { robotId: string; name: string; location: string; status: string; health: number; maxHealth: number
    roundsResolved: number; nextRoundAt: string | null; participantIds: string[] }
  myHealth: number; myMaxHealth: number
  myShieldCharge?: number
  myShieldMaxCharge?: number
  myShieldDurability?: number
  myHiddenPocketInstanceId?: string | null
  myFieldSupplies?: StagedSupplyItem[] | null
  myCauseOfDefeat?: string | null
  myLostHaulValue?: number
  myLostHaulCount?: number
  condition?: { id: string; name: string; description: string; danger: string } | null
  deadlineAt?: string | null
  myGreed?: number; myMood?: number; myResilience?: number; myExtractChance?: number
  myAppetite?: number; exposure?: number
  hazardWarnings?: { location: string; resolvesAt: string; rawDamage: number }[]
  opportunities: { opportunityId: string; location: string; status: string; closesAt: string | null
    remainingItems: number; participantIds: string[] }[]
  myInventory: ItemView[]; myExtractedLoot: ItemView[]; encounters: EncounterView[]
  robotEncounter: RobotEncounterView | null
  events: { sequence: number; at: string; kind: string; text: string }[]
  currentThread: ActiveThreadView | null
  recentThreads: ActiveThreadView[]
}
type HandlerAccess = { slot: string; playerId: string; accessToken: string }
type HandlerProjection = {
  slot: string; playerId: string; displayName: string; dutyEnabled: boolean; status: string
  homeAvailableAt: string | null; currentRaidId: string | null; raid: Projection | null
  myLastExtractedLoot: ItemView[]; comms: { sequence: number; at: string; kind: string; text: string }[]
  serverTime: string
  lastReturnSummary?: ReturnSummaryView | null
  fullJournal?: HandlerJournalEntryView[] | null
  coins?: number
  stashValue?: number
  stashCount?: number
  stashCapacity?: number
  stash?: ItemView[] | null
  stashGroups?: StashGroupView[] | null
  equippedWeapon?: OwnedWeapon | null
  ownedWeapons?: OwnedWeapon[] | null
  ownedSupplies?: OwnedSupplyStack[] | null
  stagedSupplies?: StagedSupplyItem[] | null
  lastTopItemAppraisal?: ItemAppraisalView | null
  signalCurrent?: number
  signalMax?: number
  secretHiddenPocketInstanceId?: string | null
}
type HandlerSupportActionResponse = {
  commandId: string
  actionType: string
  success: boolean
  message: string
  signalRemaining: number
}
type DesignateHiddenPocketResponse = {
  commandId: string
  itemInstanceId?: string | null
  success: boolean
  message: string
}
type SellStashResponse = {
  commandId: string; itemsSoldCount: number; coinsGained: number
  newCoinsBalance: number; newStashCount: number; newStashValue: number
  soldItems: ItemView[]
}
type EnrollResponse = { accessToken: string; projection: HandlerProjection }

const expectedRulesVersion = 'desperanza-g6-v1'
const requestedIdentity = new URLSearchParams(window.location.search).get('handler')
const slot = ref<'raider-one' | 'raider-two'>(requestedIdentity === 'two' ? 'raider-two' : 'raider-one')
const session = shallowRef<HandlerAccess | null>(null)
const handlerProjection = ref<HandlerProjection | null>(null)
const projection = ref<Projection | null>(null)
const error = ref('')
const connecting = ref(false)
const serverReady = ref(false)
const serverStatus = ref('Checking local game server')
const aiStatus = ref<{ generator: string; available: boolean; mode: string } | null>(null)
const transport = ref('Disconnected')
const now = ref(Date.now())
const journalEntries = ref<HandlerJournalEntryView[]>([])
const commsFilter = ref<'all' | 'diary' | 'hub' | 'encounters'>('all')
const commsContainerRef = ref<HTMLElement | null>(null)
const userHoldingScroll = ref(false)
const hasNewUnreadComms = ref(false)
const sellingGroup = ref<StashGroupView | null>(null)
const sellQuantity = ref<number>(1)
const selling = ref(false)
const sellSuccessMessage = ref('')
let serverOffset = 0
let connection: HubConnection | null = null
let liveRaidId: string | null = null
let refreshTimer: ReturnType<typeof setInterval> | undefined
let displayTimer: ReturnType<typeof setInterval> | undefined

const me = computed(() => projection.value?.players.find(player => player.playerId === session.value?.playerId))
const currentThread = computed(() => projection.value?.currentThread ?? null)
const recentThreads = computed(() => projection.value?.recentThreads ?? [])
const loot = computed(() => me.value?.activity === 'Extracted'
  ? projection.value?.myExtractedLoot ?? handlerProjection.value?.myLastExtractedLoot ?? []
  : projection.value?.myInventory ?? handlerProjection.value?.myLastExtractedLoot ?? [])
const lootValue = computed(() => loot.value.reduce((sum, item) => sum + item.value, 0))

const stashGroups = computed(() => handlerProjection.value?.stashGroups ?? [])
const coins = computed(() => handlerProjection.value?.coins ?? 0)
const stashCount = computed(() => handlerProjection.value?.stashCount ?? 0)
const stashCapacity = computed(() => handlerProjection.value?.stashCapacity ?? 120)
const stashValue = computed(() => handlerProjection.value?.stashValue ?? 0)
const stashCapacityPercent = computed(() => Math.min(100, Math.round((stashCount.value / stashCapacity.value) * 100)))

// G5 Survival & Handler Support state
const supportActionPending = ref(false)
const supportActionFeedback = ref('')
const supportActionError = ref(false)
const pocketActionPending = ref(false)
const pocketFeedback = ref('')

const isRaidActive = computed(() => !!projection.value && me.value?.activity !== 'Extracted' && me.value?.activity !== 'KnockedOut')
const signalCurrent = computed(() => handlerProjection.value?.signalCurrent ?? 3)
const signalMax = computed(() => handlerProjection.value?.signalMax ?? 5)

const myHealth = computed(() => projection.value?.myHealth ?? 40)
const myMaxHealth = computed(() => projection.value?.myMaxHealth ?? 40)
const hpPercent = computed(() => Math.max(0, Math.min(100, Math.round((myHealth.value / myMaxHealth.value) * 100))))
const shieldCharge = computed(() => projection.value?.myShieldCharge ?? 0)
const shieldMaxCharge = computed(() => projection.value?.myShieldMaxCharge ?? 40)
const shieldPercent = computed(() => Math.max(0, Math.min(100, Math.round((shieldCharge.value / shieldMaxCharge.value) * 100))))
const shieldDurability = computed(() => projection.value?.myShieldDurability ?? 100.0)
const myHiddenPocketInstanceId = computed(() => projection.value?.myHiddenPocketInstanceId ?? handlerProjection.value?.secretHiddenPocketInstanceId ?? null)
const pocketedItem = computed(() => loot.value.find(item => item.instanceId === myHiddenPocketInstanceId.value) ?? null)
const activeFieldSupplies = computed(() => projection.value?.myFieldSupplies ?? [])

async function triggerSupportAction(actionType: 'CallExtract' | 'Calm' | 'Pressure') {
  const access = session.value
  if (!access || supportActionPending.value) return
  supportActionPending.value = true
  supportActionFeedback.value = ''
  supportActionError.value = false
  try {
    const commandId = `support:${crypto.randomUUID()}`
    const res = await request<HandlerSupportActionResponse>('/api/dev/handler/support/action', access, {
      method: 'POST',
      body: JSON.stringify({ commandId, actionType })
    })
    supportActionFeedback.value = res.message
    await refreshProjection()
    setTimeout(() => {
      if (supportActionFeedback.value === res.message) {
        supportActionFeedback.value = ''
      }
    }, 6000)
  } catch (cause) {
    supportActionError.value = true
    supportActionFeedback.value = cause instanceof Error ? cause.message : 'Support action failed'
  } finally {
    supportActionPending.value = false
  }
}

async function designatePocket(itemInstanceId: string | null) {
  const access = session.value
  if (!access || pocketActionPending.value) return
  pocketActionPending.value = true
  pocketFeedback.value = ''
  try {
    const commandId = `pocket:${crypto.randomUUID()}`
    const res = await request<DesignateHiddenPocketResponse>('/api/dev/handler/pocket/designate', access, {
      method: 'POST',
      body: JSON.stringify({ commandId, itemInstanceId })
    })
    pocketFeedback.value = res.message
    await refreshProjection()
    setTimeout(() => {
      if (pocketFeedback.value === res.message) {
        pocketFeedback.value = ''
      }
    }, 4000)
  } catch (cause) {
    pocketFeedback.value = cause instanceof Error ? cause.message : 'Pocket designation failed'
  } finally {
    pocketActionPending.value = false
  }
}

function clearSession(targetSlot?: string, message?: string) {
  const slotToClear = targetSlot ?? session.value?.slot
  if (slotToClear) {
    localStorage.removeItem(accessStorageKey(slotToClear))
  }
  session.value = null
  handlerProjection.value = null
  projection.value = null
  void stopOwnedConnection(connection, () => {
    connection = null
    liveRaidId = null
  }).catch(cause => console.error('Unable to stop the released live connection', cause))
  transport.value = 'Disconnected'
  if (message !== undefined) {
    error.value = message
  }
}

function openSellModal(group: StashGroupView) {
  sellingGroup.value = group
  sellQuantity.value = 1
  sellSuccessMessage.value = ''
}

function closeSellModal() {
  sellingGroup.value = null
  sellQuantity.value = 1
}

async function executeSell(group: StashGroupView, qty: number) {
  const access = session.value
  if (!access) return
  selling.value = true
  error.value = ''
  sellSuccessMessage.value = ''
  try {
    const commandId = `sell:${crypto.randomUUID()}`
    const response = await request<SellStashResponse>('/api/dev/handler/stash/sell', access, {
      method: 'POST',
      body: JSON.stringify({
        commandId,
        catalogId: group.catalogId,
        quantity: qty
      })
    })
    sellSuccessMessage.value = `Sold ${response.itemsSoldCount}x ${group.name} for +${response.coinsGained} spendable coins.`
    sellingGroup.value = null
    await refreshProjection()
  } catch (cause) {
    if (cause instanceof Error && cause.message.includes('401')) {
      clearSession(access.slot, 'Development session expired after server restart. Please start autonomous duty to enroll.')
    } else {
      error.value = cause instanceof Error ? cause.message : 'Unable to complete sale'
    }
  } finally {
    selling.value = false
  }
}

// OUTFITTER, ARMORY & APPRAISAL STATE (G4 & AI-3)
const shopCatalog = ref<ShopCatalogView | null>(null)
const outfitterTab = ref<'armory' | 'supplies' | 'staging'>('armory')
const outfitterMessage = ref('')
const outfitterActionPending = ref(false)
const appraising = ref(false)
const appraisalModal = ref<(ItemAppraisalView & { estimatedValue: number }) | null>(null)
const stagedDraft = ref<Record<string, number>>({})

const equippedWeapon = computed(() => handlerProjection.value?.equippedWeapon ?? null)
const ownedWeapons = computed(() => handlerProjection.value?.ownedWeapons ?? [])
const ownedSupplies = computed(() => handlerProjection.value?.ownedSupplies ?? [])
const stagedSupplies = computed(() => handlerProjection.value?.stagedSupplies ?? [])
const topAppraisedItem = computed(() => {
  const summary = handlerProjection.value?.lastReturnSummary
  return summary?.items.find(item => item.catalogId === summary.topItemAppraisal?.catalogId)
})

const stagedMedicalCount = computed(() => {
  let count = 0
  for (const [id, qty] of Object.entries(stagedDraft.value)) {
    const def = shopCatalog.value?.supplies.find(s => s.id === id)
    if (def && def.category === 'Medical') count += qty
  }
  return count
})

const stagedRechargerCount = computed(() => {
  let count = 0
  for (const [id, qty] of Object.entries(stagedDraft.value)) {
    const def = shopCatalog.value?.supplies.find(s => s.id === id)
    if (def && def.category === 'ShieldRecharger') count += qty
  }
  return count
})

watch(() => handlerProjection.value?.stagedSupplies, (staged) => {
  const draft: Record<string, number> = {}
  if (staged) {
    for (const item of staged) {
      draft[item.catalogId] = item.quantity
    }
  }
  stagedDraft.value = draft
}, { immediate: true })

function getOwnedSupplyQuantity(catalogId: string): number {
  return ownedSupplies.value.find(s => s.catalogId === catalogId)?.quantity ?? 0
}

async function fetchShopCatalog() {
  try {
    const res = await fetch('/api/dev/shop/catalog')
    if (res.ok) {
      shopCatalog.value = await res.json() as ShopCatalogView
    }
  } catch {
    // optional catalog fetch
  }
}

async function purchaseShopItem(catalogId: string, quantity: number = 1) {
  const access = session.value
  if (!access || outfitterActionPending.value) return
  outfitterActionPending.value = true
  outfitterMessage.value = ''
  try {
    const commandId = `buy:${crypto.randomUUID()}`
    await request<any>('/api/dev/shop/purchase', access, {
      method: 'POST',
      body: JSON.stringify({ commandId, catalogId, quantity })
    })
    outfitterMessage.value = `Acquisition authorized: ${catalogId} (x${quantity}).`
    await refreshProjection()
  } catch (cause) {
    outfitterMessage.value = cause instanceof Error ? `Purchase failed: ${cause.message}` : 'Purchase failed'
  } finally {
    outfitterActionPending.value = false
  }
}

async function equipWeapon(weaponInstanceId: string) {
  const access = session.value
  if (!access || outfitterActionPending.value) return
  outfitterActionPending.value = true
  outfitterMessage.value = ''
  try {
    const commandId = `equip:${crypto.randomUUID()}`
    await request<any>('/api/dev/equipment/equip', access, {
      method: 'POST',
      body: JSON.stringify({ commandId, weaponInstanceId })
    })
    outfitterMessage.value = `Armament equipped successfully.`
    await refreshProjection()
  } catch (cause) {
    outfitterMessage.value = cause instanceof Error ? `Equip failed: ${cause.message}` : 'Equip failed'
  } finally {
    outfitterActionPending.value = false
  }
}

async function repairWeapon(weaponInstanceId: string) {
  const access = session.value
  if (!access || outfitterActionPending.value) return
  outfitterActionPending.value = true
  outfitterMessage.value = ''
  try {
    const commandId = `repair:${crypto.randomUUID()}`
    const res = await request<any>('/api/dev/equipment/repair', access, {
      method: 'POST',
      body: JSON.stringify({ commandId, weaponInstanceId })
    })
    outfitterMessage.value = `Weapon serviced: ${res.repairCostPaid} ¢ deducted. Durability restored.`
    await refreshProjection()
  } catch (cause) {
    outfitterMessage.value = cause instanceof Error ? `Repair failed: ${cause.message}` : 'Repair failed'
  } finally {
    outfitterActionPending.value = false
  }
}

function adjustStagedDraft(catalogId: string, delta: number, category: string) {
  const current = stagedDraft.value[catalogId] ?? 0
  const owned = getOwnedSupplyQuantity(catalogId)
  const maxCategory = category === 'Medical' ? 4 : 2
  const currentCategoryCount = category === 'Medical' ? stagedMedicalCount.value : stagedRechargerCount.value

  const nextVal = current + delta
  if (nextVal < 0) return
  if (nextVal > owned) return
  if (delta > 0 && currentCategoryCount >= maxCategory) return

  stagedDraft.value = {
    ...stagedDraft.value,
    [catalogId]: nextVal
  }
}

async function saveStagedLoadout() {
  const access = session.value
  if (!access || outfitterActionPending.value) return
  outfitterActionPending.value = true
  outfitterMessage.value = ''
  try {
    const commandId = `stage:${crypto.randomUUID()}`
    const stagedSupplies = Object.entries(stagedDraft.value)
      .filter(([_, qty]) => qty > 0)
      .map(([catalogId, quantity]) => ({ catalogId, quantity }))
    await request<any>('/api/dev/loadout/stage', access, {
      method: 'POST',
      body: JSON.stringify({ commandId, stagedSupplies })
    })
    outfitterMessage.value = `Field loadout staged for next deployment.`
    await refreshProjection()
  } catch (cause) {
    outfitterMessage.value = cause instanceof Error ? `Staging failed: ${cause.message}` : 'Staging failed'
  } finally {
    outfitterActionPending.value = false
  }
}

async function requestAppraisal(catalogId: string, itemName: string, estimatedValue: number) {
  const access = session.value
  if (!access || appraising.value) return
  appraising.value = true
  error.value = ''
  try {
    const appraisal = await request<ItemAppraisalView>('/api/dev/handler/item/appraise', access, {
      method: 'POST',
      body: JSON.stringify({ catalogId, itemName })
    })
    appraisalModal.value = { ...appraisal, estimatedValue }
  } catch (cause) {
    error.value = cause instanceof Error ? `Appraisal failed: ${cause.message}` : 'Appraisal failed'
  } finally {
    appraising.value = false
  }
}

function closeAppraisalModal() {
  appraisalModal.value = null
}

const filteredComms = computed(() => {
  const all = journalEntries.value
  if (commsFilter.value === 'all') return all
  if (commsFilter.value === 'diary') return all.filter(e => e.channel.toLowerCase() === 'diary')
  if (commsFilter.value === 'hub') return all.filter(e => e.channel.toLowerCase() === 'hub')
  if (commsFilter.value === 'encounters') return all.filter(e =>
    e.channel.toLowerCase() === 'encounter' || e.channel.toLowerCase() === 'combat' || e.kind.toLowerCase().includes('robot'))
  return all
})

function threadProgressPercent(thread: ActiveThreadView) {
  if (!thread.endsAt) return 100
  const total = thread.totalSeconds || 1
  const remaining = secondsRemaining(thread.endsAt)
  const elapsed = Math.max(0, total - remaining)
  return Math.min(100, Math.max(0, Math.round((elapsed / total) * 100)))
}

function onCommsScroll() {
  const el = commsContainerRef.value
  if (!el) return
  const isNearBottom = el.scrollHeight - el.scrollTop - el.clientHeight < 40
  userHoldingScroll.value = !isNearBottom
  if (isNearBottom) {
    hasNewUnreadComms.value = false
  }
}

function scrollToBottom() {
  const el = commsContainerRef.value
  if (!el) return
  el.scrollTop = el.scrollHeight
  userHoldingScroll.value = false
  hasNewUnreadComms.value = false
}

function secondsRemaining(deadline: string) {
  return Math.max(0, Math.ceil((Date.parse(deadline) - now.value - serverOffset) / 1000))
}

function participantNames(ids: string[]) {
  return ids.map(id => projection.value?.players.find(player => player.playerId === id)?.displayName ?? id).join(' / ')
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function assertCurrentProjection(value: unknown): asserts value is Projection {
  if (!isRecord(value) || value.rulesVersion !== expectedRulesVersion
    || !isRecord(value.robot) || !Array.isArray(value.opportunities)
    || typeof value.myHealth !== 'number' || typeof value.myMaxHealth !== 'number'
    || !Array.isArray(value.players) || !Array.isArray(value.locations)
    || !Array.isArray(value.events) || !Array.isArray(value.encounters)
    || !Array.isArray(value.myInventory) || !Array.isArray(value.myExtractedLoot)) {
    throw new Error(`The API returned an incompatible raid snapshot. Restart the AFK Raiders server from this repository; the client requires ${expectedRulesVersion}.`)
  }
}

async function verifyServerVersion() {
  let status: unknown
  try {
    status = await request<unknown>('/api/dev/status', null)
  } catch (cause) {
    const detail = cause instanceof Error ? ` (${cause.message})` : ''
    throw new Error(`Can't reach the matching local game server${detail}. Start or restart it with dotnet run --project server\\AFKRaiders.Server.`)
  }
  if (!isRecord(status) || status.rulesVersion !== expectedRulesVersion) {
    const reported = isRecord(status) && typeof status.rulesVersion === 'string'
      ? ` The server reports ${status.rulesVersion}.` : ''
    throw new Error(`This web client needs ${expectedRulesVersion}.${reported} Restart the server from this repository, then reload the page.`)
  }
  if (isRecord(status) && isRecord(status.ai) && typeof status.ai.generator === 'string') {
    aiStatus.value = {
      generator: status.ai.generator,
      available: Boolean(status.ai.available),
      mode: String(status.ai.mode ?? 'Offline')
    }
  }
}

function isHandlerAccess(value: unknown): value is HandlerAccess {
  return isRecord(value) && typeof value.slot === 'string' && typeof value.playerId === 'string'
    && typeof value.accessToken === 'string'
}

function accessStorageKey(targetSlot: string) {
  return `afk-raiders-handler-session:${targetSlot}`
}

function enrollmentRequestKey(targetSlot: string) {
  return `afk-raiders-handler-enrollment-request:${targetSlot}`
}

async function request<T>(path: string, access: HandlerAccess | null, init: RequestInit = {}): Promise<T> {
  const headers = new Headers(init.headers)
  if (access) headers.set('Authorization', `Bearer ${access.accessToken}`)
  if (init.body) headers.set('Content-Type', 'application/json')
  const response = await fetch(path, { ...init, headers })
  if (!response.ok) {
    let reason = `Request failed (${response.status})`
    if (response.headers.get('content-type')?.includes('application/json')) {
      const body: unknown = await response.json()
      if (isRecord(body) && typeof body.error === 'string') reason = body.error
    }
    throw new Error(reason)
  }
  return await response.json() as T
}

function acceptProjection(value: HandlerProjection, access: HandlerAccess) {
  if (value.raid) assertCurrentProjection(value.raid)
  if (!session.value || session.value.accessToken !== access.accessToken || (projection.value && value.raid
    && value.raid.raidId === projection.value.raidId && value.raid.revision < projection.value.revision)) return
  now.value = Date.now()
  serverOffset = Date.parse(value.serverTime) - now.value
  handlerProjection.value = value
  projection.value = value.raid

  if (value.fullJournal && value.fullJournal.length > 0) {
    const existingIds = new Set(journalEntries.value.map(e => e.sourceEventId))
    const additions: HandlerJournalEntryView[] = []
    for (const entry of value.fullJournal) {
      if (!existingIds.has(entry.sourceEventId)) {
        existingIds.add(entry.sourceEventId)
        additions.push(entry)
      }
    }
    journalEntries.value = mergeJournal(journalEntries.value, value.fullJournal)
    if (additions.length > 0) {
      if (userHoldingScroll.value) {
        hasNewUnreadComms.value = true
      } else {
        setTimeout(scrollToBottom, 30)
      }
    }
  } else {
    const fallbackEvents = [
      ...(value.comms ?? []).map((e, idx) => ({
        sequence: idx + 1,
        sourceEventId: `comms:${e.sequence}`,
        raidId: null,
        at: e.at,
        kind: e.kind,
        channel: 'Hub',
        text: e.text
      })),
      ...(value.raid?.events ?? []).map(e => ({
        sequence: e.sequence,
        sourceEventId: `raid:${value.raid?.raidId}:${e.sequence}`,
        raidId: value.raid?.raidId ?? null,
        at: e.at,
        kind: e.kind,
        channel: 'Diary',
        text: e.text
      }))
    ].sort((a, b) => Date.parse(a.at) - Date.parse(b.at))
    journalEntries.value = fallbackEvents
  }
}

async function refreshProjection() {
  const access = session.value
  if (!access) return
  try {
    const value = await request<HandlerProjection>('/api/dev/handler', access)
    if (session.value?.accessToken !== access.accessToken) return
    acceptProjection(value, access)
    if (value.raid && liveRaidId !== value.raid.raidId) void connectLive()
    if (!value.raid) {
      await stopOwnedConnection(connection, () => {
        connection = null
        liveRaidId = null
        transport.value = 'At home - polling'
        error.value = clearLiveWarning(error.value)
      })
    }
  } catch (cause) {
    if (session.value?.accessToken === access.accessToken) {
      if (cause instanceof Error && cause.message.includes('401')) {
        clearSession(access.slot, 'Development session expired after server restart. Please start autonomous duty to enroll.')
      } else {
        error.value = cause instanceof Error ? cause.message : 'Unable to refresh raid'
      }
    }
  }
}

async function connectLive() {
  const access = session.value
  const raidId = projection.value?.raidId
  if (!access || !raidId || liveRaidId === raidId) return
  try {
    await stopOwnedConnection(connection, () => {
      connection = null
      liveRaidId = raidId
    })
  } catch (cause) {
    if (session.value?.accessToken === access.accessToken && liveRaidId === raidId && !connection) {
      liveRaidId = null
      transport.value = 'Disconnected - polling'
      error.value = cause instanceof Error ? cause.message : 'Unable to stop the previous live connection'
    }
    return
  }
  if (session.value?.accessToken !== access.accessToken || projection.value?.raidId !== raidId) {
    if (!connection && liveRaidId === raidId) liveRaidId = null
    return
  }
  const live = new HubConnectionBuilder()
    .withUrl(`/hubs/raid?raidId=${encodeURIComponent(raidId)}`, {
      accessTokenFactory: () => access.accessToken,
    })
    .withAutomaticReconnect()
    .build()
  connection = live
  liveRaidId = raidId
  transport.value = 'Connecting'
  live.on('RaidChanged', () => {
    if (connection === live && session.value?.accessToken === access.accessToken) void refreshProjection()
  })
  live.onreconnecting(() => {
    if (connection === live && session.value?.accessToken === access.accessToken) transport.value = 'Reconnecting - polling'
  })
  live.onreconnected(() => {
    if (connection === live && session.value?.accessToken === access.accessToken) {
      transport.value = 'Connected live'
      error.value = clearLiveWarning(error.value)
      void refreshProjection()
    }
  })
  live.onclose(() => {
    if (session.value?.accessToken === access.accessToken && connection === live) {
      transport.value = 'Disconnected - polling'
      error.value = liveClosedMessage
      connection = null
      liveRaidId = null
    }
  })
  try {
    await live.start()
    if (connection === live && session.value?.accessToken === access.accessToken) {
      transport.value = 'Connected live'
      error.value = clearLiveWarning(error.value)
      await refreshProjection()
    }
  } catch (cause) {
    const stillOwned = connection === live
    if (stillOwned) {
      connection = null
      liveRaidId = null
    }
    if (stillOwned && session.value?.accessToken === access.accessToken) {
      transport.value = 'Disconnected - polling'
      error.value = cause instanceof Error ? cause.message : 'Unable to connect live'
    }
  }
}

async function startDuty() {
  error.value = ''
  connecting.value = true
  try {
    await verifyServerVersion()
    let requestId = sessionStorage.getItem(enrollmentRequestKey(slot.value))
    if (!requestId) {
      requestId = crypto.randomUUID()
      sessionStorage.setItem(enrollmentRequestKey(slot.value), requestId)
    }
    const result = await request<EnrollResponse>('/api/dev/handlers/enroll', null, {
      method: 'POST',
      body: JSON.stringify({ slot: slot.value, requestId }),
    })
    const access: HandlerAccess = { slot: result.projection.slot, playerId: result.projection.playerId, accessToken: result.accessToken }
    session.value = access
    acceptProjection(result.projection, access)
    localStorage.setItem(accessStorageKey(access.slot), JSON.stringify(access))
    sessionStorage.removeItem(enrollmentRequestKey(access.slot))
    void fetchShopCatalog()
    if (result.projection.raid) await connectLive()
  } catch (cause) {
    const reason = cause instanceof Error ? cause.message : 'Unable to enter raid'
    error.value = reason === 'HandlerAlreadyEnrolled'
      ? 'That development identity is already enrolled in this server session. Restore its Handler session in this browser or restart the local server.'
      : reason
    if (reason.startsWith("Can't reach") || reason.startsWith('This web client needs')) {
      serverReady.value = false
      serverStatus.value = 'SERVER UNAVAILABLE OR OUTDATED'
    }
  } finally {
    connecting.value = false
  }
}

async function toggleDuty() {
  const access = session.value
  const home = handlerProjection.value
  if (!access || !home) return
  error.value = ''
  try {
    const value = await request<HandlerProjection>('/api/dev/handler/duty', access, {
      method: 'PUT',
      body: JSON.stringify({ enabled: !home.dutyEnabled }),
    })
    acceptProjection(value, access)
  } catch (cause) {
    if (cause instanceof Error && cause.message.includes('401')) {
      clearSession(access.slot, 'Development session expired after server restart. Please start autonomous duty to enroll.')
    } else {
      error.value = cause instanceof Error ? cause.message : 'Unable to update autonomous duty'
    }
  }
}

async function restoreHandler(targetSlot: 'raider-one' | 'raider-two') {
  const key = accessStorageKey(targetSlot)
  const stored = localStorage.getItem(key)
  if (!stored) return
  let access: unknown
  try {
    access = JSON.parse(stored)
  } catch {
    localStorage.removeItem(key)
    error.value = 'Stored Handler access is invalid. Start a new autonomous session.'
    return
  }
  if (!isHandlerAccess(access) || access.slot !== targetSlot) {
    localStorage.removeItem(key)
    error.value = 'Stored Handler access is invalid. Start a new autonomous session.'
    return
  }
  try {
    const value = await request<HandlerProjection>('/api/dev/handler', access)
    if (slot.value !== targetSlot) return
    session.value = access
    acceptProjection(value, access)
    if (value.raid) await connectLive()
  } catch (cause) {
    localStorage.removeItem(key)
    if (session.value?.accessToken === access.accessToken) {
      clearSession(targetSlot)
    }
    const is401 = cause instanceof Error && cause.message.includes('401')
    error.value = is401
      ? 'Development session expired after server restart. Please start autonomous duty to enroll.'
      : (cause instanceof Error ? cause.message : 'Unable to restore Handler access')
  }
}

function resumeView() {
  if (document.visibilityState === 'visible') {
    now.value = Date.now()
    void refreshProjection()
  }
}

onMounted(async () => {
  document.addEventListener('visibilitychange', resumeView)
  window.addEventListener('focus', resumeView)
  displayTimer = setInterval(() => { now.value = Date.now() }, 250)
  refreshTimer = setInterval(() => { void refreshProjection() }, 3000)
  try {
    await verifyServerVersion()
  } catch (cause) {
    serverStatus.value = 'SERVER UNAVAILABLE OR OUTDATED'
    error.value = cause instanceof Error ? cause.message : 'Unable to verify the local game server'
    return
  }
  serverReady.value = true
  serverStatus.value = `SERVER READY / ${expectedRulesVersion}`
  void fetchShopCatalog()
  await restoreHandler(slot.value)
})

watch(slot, targetSlot => {
  if (serverReady.value && !session.value) void restoreHandler(targetSlot)
})

onBeforeUnmount(() => {
  document.removeEventListener('visibilitychange', resumeView)
  window.removeEventListener('focus', resumeView)
  clearInterval(refreshTimer)
  clearInterval(displayTimer)
  clearSession()
})
</script>

<template>
  <main class="shell">
    <header class="topbar">
      <a class="brand" href="#" aria-label="AFK Raiders home"><span class="brand-mark">AR</span><span>AFK <strong>RAIDERS</strong></span></a>
      <div style="display: flex; align-items: center; gap: 12px;">
        <span v-if="aiStatus" class="ai-badge" :class="{ live: aiStatus.available }">
          {{ aiStatus.mode }}
        </span>
        <span class="environment">DESPERANZA / AUTONOMOUS DUTY / DEVELOPMENT</span>
      </div>
    </header>

    <section v-if="!session" class="entry">
      <div class="entry-heading">
        <div>
          <p class="eyebrow">HANDLER CONSOLE / DESPERANZA</p>
          <h1>Home is a stop,<br /><span>not the end.</span></h1>
          <p class="intro">Enroll your Raider in autonomous duty once. They join ongoing raids, return to Desperanza, and repeat without per-raid deployment or an invite.</p>
        </div>
        <div class="server-status" :class="{ ready: serverReady }" role="status">
          <span class="status-dot"></span>
          <span>{{ serverStatus }}</span>
        </div>
      </div>

      <div class="entry-dashboard">
        <section class="panel deploy-panel">
          <p class="eyebrow">01 / AUTONOMOUS DUTY</p>
          <h2>Choose your Raider</h2>
          <label for="identity">Development test identity</label>
          <select id="identity" v-model="slot">
            <option value="raider-one">Mira "Refund" Malone</option>
            <option value="raider-two">Dev "Wet Socks" Kowalski</option>
          </select>
          <button class="primary" :disabled="connecting || !serverReady" @click="startDuty">
            {{ connecting ? 'Enrolling...' : 'Start autonomous duty' }}
          </button>
          <p class="panel-note">One-time enrollment. Closing this page does not withdraw your Raider. This development server supports two test identities.</p>
        </section>

        <section class="panel briefing-panel">
          <p class="eyebrow">02 / THE RECURRING LOOP</p>
          <h2>Home, raid, home</h2>
          <ol class="mission-steps">
            <li><span>01</span><div><strong>Join</strong><small>Enter an ongoing server-owned raid when an admission slot opens.</small></div></li>
            <li><span>02</span><div><strong>Act autonomously</strong><small>Search, fight the shared robot, and encounter only actual Raiders who cross paths.</small></div></li>
            <li><span>03</span><div><strong>Return &amp; repeat</strong><small>Extract independently. A home interval precedes another eligible admission.</small></div></li>
          </ol>
          <div class="threat-note"><strong>HANDLER ROLE</strong><span>Monitor comms; pause future admissions. Outcomes never wait on a Handler.</span></div>
        </section>
      </div>
      <p class="warning">LOCAL DEVELOPMENT ONLY / IN-MEMORY SESSION / SERVER RESTARTS ERASE RAID RESULTS</p>
    </section>

    <section v-else class="raid">
      <div class="raid-head">
        <div>
          <p class="eyebrow">HANDLER CONSOLE / {{ handlerProjection?.status }}</p>
          <h1>Desperanza</h1>
          <p class="subhead">{{ handlerProjection?.displayName }} / {{ handlerProjection?.dutyEnabled ? 'AUTONOMOUS DUTY ON' : 'FUTURE ADMISSIONS PAUSED' }}
            <span v-if="me"> / {{ me.activity }} at {{ me.location }}<span v-if="me.destination"> to {{ me.destination }}</span>
              <span v-if="me.activityEndsAt"> / next activity in {{ secondsRemaining(me.activityEndsAt) }}s</span></span>
            <span v-else-if="handlerProjection?.homeAvailableAt && secondsRemaining(handlerProjection.homeAvailableAt) > 0">
              / next eligible in {{ secondsRemaining(handlerProjection.homeAvailableAt) }}s</span>
          </p>
        </div>
        <div class="handler-actions">
          <span class="live-badge">{{ transport }}</span>
          <button class="secondary duty-toggle" @click="toggleDuty">
            {{ handlerProjection?.dutyEnabled ? 'Pause future admissions' : 'Resume autonomous duty' }}
          </button>
        </div>
      </div>
      <div class="session-bar">
        <div><span class="label">RAIDER STATUS</span><strong>{{ handlerProjection?.status }}</strong></div>
        <div><span class="label">RAID WORLD</span><code>{{ handlerProjection?.currentRaidId ?? 'At home / waiting for admission' }}</code></div>
        <div><span class="label">SIGNAL BUDGET</span><strong class="highlight-signal">{{ signalCurrent }} / {{ signalMax }} ⚡</strong></div>
        <div><span class="label">SPENDABLE COINS</span><strong class="highlight-coins">{{ coins }} ¢</strong></div>
        <div><span class="label">ARMED WEAPON</span><strong>{{ equippedWeapon ? `${equippedWeapon.name} (${equippedWeapon.currentDurability}/${equippedWeapon.maxDurability})` : 'Unarmed' }}</strong></div>
        <div><span class="label">FIELD STAGED</span><strong>{{ stagedSupplies.reduce((acc, s) => acc + s.quantity, 0) }} items</strong></div>
        <div><span class="label">HOME STASH</span><strong>{{ stashCount }} / {{ stashCapacity }} ({{ stashCapacityPercent }}%)</strong></div>
        <div><span class="label">CAPACITY / PULSE</span><strong>2 actual Raiders / 30s</strong></div>
      </div>
      <p class="notice">Pausing affects future admissions only. A Raider already in a raid continues, resolves encounters without Handler input, and returns to Desperanza independently.</p>

      <div v-if="!isRaidActive" class="home-dashboard">
        <section class="panel">
          <p class="eyebrow">DESPERANZA / DUTY BOARD</p>
          <h2>{{ handlerProjection?.status }}</h2>
          <p class="panel-note">The Raider is home. When duty is enabled, the server admits them to an eligible ongoing raid without an invite or per-raid deployment.</p>
          <p v-if="handlerProjection?.homeAvailableAt" class="privacy-note">Next re-entry eligibility: {{ new Date(handlerProjection.homeAvailableAt).toLocaleTimeString() }} server time.</p>
          <p class="privacy-note">A world stays open while any admitted Raider is active, then closes when the last returns.</p>
        </section>

        <!-- OUTFITTER & ARMORY WORKSHOP PANEL (G4 & AI-3) -->
        <section class="panel outfitter-panel">
          <div class="outfitter-header">
            <div>
              <p class="eyebrow">EQUIPMENT &amp; LOGISTICS / DESPERANZA OUTFITTER</p>
              <h2>Outfitter &amp; Field Workshop</h2>
              <span class="dev-stash-badge">CANON GEAR / STAGED CUSTODY</span>
            </div>
            <div class="outfitter-balance">
              <span class="label">SPENDABLE COINS</span>
              <strong class="coin-display">{{ coins }} ¢</strong>
            </div>
          </div>

          <p class="panel-note">Requisition weapons, procure medical/shield field supplies, repair damaged armaments, and stage tactical consumables for field deployment.</p>

          <p v-if="outfitterMessage" class="success-notice">{{ outfitterMessage }}</p>

          <div class="outfitter-tabs">
            <button :class="{ active: outfitterTab === 'armory' }" class="outfitter-tab" @click="outfitterTab = 'armory'">Armory &amp; Workshop</button>
            <button :class="{ active: outfitterTab === 'supplies' }" class="outfitter-tab" @click="outfitterTab = 'supplies'">Field Supplies</button>
            <button :class="{ active: outfitterTab === 'staging' }" class="outfitter-tab" @click="outfitterTab = 'staging'">
              Field Staging ({{ stagedSupplies.reduce((acc, s) => acc + s.quantity, 0) }} items)
            </button>
          </div>

          <!-- TAB 1: ARMORY & WORKSHOP -->
          <div v-if="outfitterTab === 'armory'" class="outfitter-content">
            <div class="outfitter-subheading">
              <p class="eyebrow">ACTIVE ARMAMENT</p>
            </div>
            <div v-if="equippedWeapon" class="weapon-card equipped-weapon-card">
              <div class="weapon-main-info">
                <div class="weapon-title-row">
                  <span class="equipped-tag">EQUIPPED</span>
                  <strong>{{ equippedWeapon.name }}</strong>
                  <span class="rarity-badge" :class="`rarity-${equippedWeapon.rarity}`">Tier {{ equippedWeapon.rarity }}</span>
                </div>
                <small class="weapon-flavor">{{ equippedWeapon.flavor }}</small>
                <div class="weapon-stats-row">
                  <span>Dmg: <strong>{{ equippedWeapon.damageMin }}–{{ equippedWeapon.damageMax }}</strong> (x{{ equippedWeapon.damageMultiplier }})</span>
                  <span>Durability: <strong>{{ equippedWeapon.currentDurability }} / {{ equippedWeapon.maxDurability }}</strong></span>
                  <span v-if="equippedWeapon.currentDurability < equippedWeapon.maxDurability" class="repair-cost-tag">
                    Service Fee: {{ equippedWeapon.repairCost }} ¢
                  </span>
                </div>
                <div class="durability-meter">
                  <div class="durability-bar" :style="{
                    width: `${Math.round((equippedWeapon.currentDurability / equippedWeapon.maxDurability) * 100)}%`,
                    backgroundColor: (equippedWeapon.currentDurability / equippedWeapon.maxDurability) <= 0.25 ? '#e63946' : ((equippedWeapon.currentDurability / equippedWeapon.maxDurability) <= 0.5 ? '#f4a261' : '#2a9d8f')
                  }"></div>
                </div>
              </div>
              <div class="weapon-card-actions">
                <button
                  v-if="equippedWeapon.currentDurability < equippedWeapon.maxDurability"
                  class="secondary mini-btn repair-btn"
                  :disabled="outfitterActionPending || coins < equippedWeapon.repairCost"
                  @click="repairWeapon(equippedWeapon.instanceId)"
                >
                  Repair ({{ equippedWeapon.repairCost }} ¢)
                </button>
                <button
                  class="secondary mini-btn appraise-btn"
                  :disabled="appraising"
                  @click="requestAppraisal(equippedWeapon.catalogId, equippedWeapon.name, equippedWeapon.currentDurability * 10)"
                >
                  Appraise
                </button>
              </div>
            </div>
            <p v-else class="empty">No weapon equipped. Standard issue fallback in effect.</p>

            <!-- OWNED WEAPONS IN STORAGE -->
            <div class="outfitter-subheading" style="margin-top: 20px;">
              <p class="eyebrow">OWNED ARSENAL ({{ ownedWeapons.length }} weapons)</p>
            </div>
            <div v-if="ownedWeapons.length" class="weapons-grid">
              <div v-for="wpn in ownedWeapons" :key="wpn.instanceId" class="weapon-card" :class="{ 'is-equipped': equippedWeapon?.instanceId === wpn.instanceId }">
                <div class="weapon-main-info">
                  <div class="weapon-title-row">
                    <span v-if="equippedWeapon?.instanceId === wpn.instanceId" class="equipped-tag">ACTIVE</span>
                    <strong>{{ wpn.name }}</strong>
                    <span class="rarity-badge" :class="`rarity-${wpn.rarity}`">T{{ wpn.rarity }}</span>
                  </div>
                  <div class="weapon-stats-row">
                    <span>Dmg: {{ wpn.damageMin }}–{{ wpn.damageMax }}</span>
                    <span>Dur: {{ wpn.currentDurability }}/{{ wpn.maxDurability }}</span>
                  </div>
                  <div class="durability-meter">
                    <div class="durability-bar" :style="{
                      width: `${Math.round((wpn.currentDurability / wpn.maxDurability) * 100)}%`,
                      backgroundColor: (wpn.currentDurability / wpn.maxDurability) <= 0.25 ? '#e63946' : ((wpn.currentDurability / wpn.maxDurability) <= 0.5 ? '#f4a261' : '#2a9d8f')
                    }"></div>
                  </div>
                </div>
                <div class="weapon-card-actions">
                  <button
                    v-if="equippedWeapon?.instanceId !== wpn.instanceId"
                    class="primary mini-btn"
                    :disabled="outfitterActionPending"
                    @click="equipWeapon(wpn.instanceId)"
                  >
                    Equip
                  </button>
                  <button
                    v-if="wpn.currentDurability < wpn.maxDurability"
                    class="secondary mini-btn repair-btn"
                    :disabled="outfitterActionPending || coins < wpn.repairCost"
                    @click="repairWeapon(wpn.instanceId)"
                  >
                    Repair ({{ wpn.repairCost }} ¢)
                  </button>
                  <button
                    class="secondary mini-btn appraise-btn"
                    :disabled="appraising"
                    @click="requestAppraisal(wpn.catalogId, wpn.name, wpn.currentDurability * 10)"
                  >
                    Appraise
                  </button>
                </div>
              </div>
            </div>

            <!-- SHOP WEAPONS REQUISITION -->
            <div class="outfitter-subheading" style="margin-top: 24px;">
              <p class="eyebrow">DESPERANZA QUARTERMASTER / WEAPON REQUISITIONS</p>
            </div>
            <div v-if="shopCatalog?.weapons.length" class="weapons-grid">
              <div v-for="shopWpn in shopCatalog.weapons" :key="shopWpn.id" class="weapon-card shop-weapon-card">
                <div class="weapon-main-info">
                  <div class="weapon-title-row">
                    <strong>{{ shopWpn.name }}</strong>
                    <span class="rarity-badge" :class="`rarity-${shopWpn.rarity}`">T{{ shopWpn.rarity }}</span>
                    <strong class="shop-price">{{ shopWpn.value === 0 ? 'FREE' : `${shopWpn.value} ¢` }}</strong>
                  </div>
                  <small class="weapon-flavor">{{ shopWpn.flavor }}</small>
                  <div class="weapon-stats-row">
                    <span>Base Dmg: {{ shopWpn.damageMin }}–{{ shopWpn.damageMax }} (x{{ shopWpn.damageMultiplier }})</span>
                    <span>Dur: {{ shopWpn.durabilityMax }}</span>
                    <span>Service: {{ shopWpn.repairCost }} ¢</span>
                  </div>
                </div>
                <div class="weapon-card-actions">
                  <button
                    class="primary mini-btn buy-btn"
                    :disabled="outfitterActionPending || (shopWpn.value > 0 && coins < shopWpn.value)"
                    @click="purchaseShopItem(shopWpn.id, 1)"
                  >
                    {{ shopWpn.value === 0 ? 'Claim Free' : `Buy (${shopWpn.value} ¢)` }}
                  </button>
                  <button
                    class="secondary mini-btn appraise-btn"
                    :disabled="appraising"
                    @click="requestAppraisal(shopWpn.id, shopWpn.name, shopWpn.value)"
                  >
                    Appraise
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- TAB 2: FIELD SUPPLIES -->
          <div v-if="outfitterTab === 'supplies'" class="outfitter-content">
            <div class="outfitter-subheading">
              <p class="eyebrow">FIELD CONSUMABLES CATALOG (STORED IN HOME PREP)</p>
            </div>
            <div v-if="shopCatalog?.supplies.length" class="supplies-grid">
              <div v-for="item in shopCatalog.supplies" :key="item.id" class="supply-card">
                <div class="supply-card-header">
                  <div>
                    <strong>{{ item.name }}</strong>
                    <span class="supply-cat-tag" :class="item.category.toLowerCase()">{{ item.category === 'Medical' ? 'MED' : 'SHIELD' }}</span>
                  </div>
                  <strong class="shop-price">{{ item.value }} ¢</strong>
                </div>
                <small class="supply-flavor">{{ item.flavor }}</small>
                <div class="supply-effects">
                  <span v-if="item.healAmount > 0">Heals: +{{ item.healAmount }} HP</span>
                  <span v-if="item.reviveAmount > 0">Revive: +{{ item.reviveAmount }} HP</span>
                  <span v-if="item.chargeAmount > 0">Shield: +{{ item.chargeAmount }} pts</span>
                </div>
                <div class="supply-owned-stock">
                  <span>Home Prep Stock: <strong>{{ getOwnedSupplyQuantity(item.id) }}</strong></span>
                </div>
                <div class="supply-card-actions">
                  <button
                    class="primary mini-btn"
                    :disabled="outfitterActionPending || coins < item.value"
                    @click="purchaseShopItem(item.id, 1)"
                  >
                    Buy 1 ({{ item.value }} ¢)
                  </button>
                  <button
                    v-if="coins >= item.value * 5"
                    class="secondary mini-btn"
                    :disabled="outfitterActionPending"
                    @click="purchaseShopItem(item.id, 5)"
                  >
                    Buy 5 ({{ item.value * 5 }} ¢)
                  </button>
                  <button
                    class="secondary mini-btn appraise-btn"
                    :disabled="appraising"
                    @click="requestAppraisal(item.id, item.name, item.value)"
                  >
                    Appraise
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- TAB 3: LOADOUT STAGING -->
          <div v-if="outfitterTab === 'staging'" class="outfitter-content">
            <div class="staging-banner">
              <div>
                <p class="eyebrow">FIELD LOADOUT PREPARATION</p>
                <h3>Consumable Tactical Deployment</h3>
                <span class="staging-g5-badge">(Prepared for Field / Active in G5)</span>
              </div>
              <div class="staging-limits-summary">
                <div :class="{ limitReached: stagedMedicalCount >= 4 }">
                  <span class="label">MEDICAL STAGED</span>
                  <strong>{{ stagedMedicalCount }} / 4 max</strong>
                </div>
                <div :class="{ limitReached: stagedRechargerCount >= 2 }">
                  <span class="label">RECHARGERS STAGED</span>
                  <strong>{{ stagedRechargerCount }} / 2 max</strong>
                </div>
              </div>
            </div>

            <p class="panel-note">Consumables staged here are packed into the Raider's loadout for field encounters. Unused supplies remain safe in inventory across returns.</p>

            <div v-if="shopCatalog?.supplies.length" class="staging-items-list">
              <div v-for="supply in shopCatalog.supplies" :key="supply.id" class="staging-row">
                <div class="staging-row-info">
                  <strong>{{ supply.name }}</strong>
                  <span class="supply-cat-tag" :class="supply.category.toLowerCase()">{{ supply.category === 'Medical' ? 'MED' : 'SHIELD' }}</span>
                  <small>Owned in prep: {{ getOwnedSupplyQuantity(supply.id) }}</small>
                </div>
                <div class="staging-controls">
                  <button
                    type="button"
                    class="secondary mini-btn"
                    :disabled="(stagedDraft[supply.id] ?? 0) <= 0"
                    @click="adjustStagedDraft(supply.id, -1, supply.category)"
                  >
                    –
                  </button>
                  <span class="staged-qty-display">{{ stagedDraft[supply.id] ?? 0 }}</span>
                  <button
                    type="button"
                    class="secondary mini-btn"
                    :disabled="(stagedDraft[supply.id] ?? 0) >= getOwnedSupplyQuantity(supply.id) || (supply.category === 'Medical' && stagedMedicalCount >= 4) || (supply.category === 'ShieldRecharger' && stagedRechargerCount >= 2)"
                    @click="adjustStagedDraft(supply.id, +1, supply.category)"
                  >
                    +
                  </button>
                </div>
              </div>
            </div>

            <div class="staging-actions">
              <button
                class="primary"
                :disabled="outfitterActionPending"
                @click="saveStagedLoadout"
              >
                {{ outfitterActionPending ? 'Staging...' : 'Authorize Staged Field Loadout' }}
              </button>
            </div>
          </div>
        </section>

        <!-- HOME STASH & VAULT PANEL -->
        <section class="panel stash-panel">
          <div class="stash-header">
            <div>
              <p class="eyebrow">PERSISTENT REPOSITORY / DESPERANZA VAULT</p>
              <h2>Home Stash ({{ stashCount }} / {{ stashCapacity }} units)</h2>
              <span class="dev-stash-badge">IN-MEMORY SESSION (DURABILITY PLANNED FOR S6)</span>
            </div>
            <div class="stash-balance">
              <span class="label">COIN BALANCE</span>
              <strong class="coin-display">{{ coins }} ¢</strong>
            </div>
          </div>

          <div class="capacity-meter">
            <div class="capacity-bar" :style="{ width: `${stashCapacityPercent}%`, backgroundColor: stashCapacityPercent >= 90 ? 'var(--color-danger, #e63946)' : 'var(--color-accent, #4cc9f0)' }"></div>
          </div>
          <p class="stash-subtext">Total Stash Value: <strong>{{ stashValue }} scrap</strong> / Stored items are conserved across raids up to 120 units. Excess units auto-sell on arrival for face value.</p>

          <p v-if="sellSuccessMessage" class="success-notice">{{ sellSuccessMessage }}</p>

          <div v-if="stashGroups.length" class="stash-groups-list">
            <div v-for="group in stashGroups" :key="group.catalogId" class="stash-group-row">
              <div class="group-info">
                <strong>{{ group.name }}</strong>
                <span class="group-qty-badge">x{{ group.quantity }}</span>
                <small class="group-flavor">{{ group.flavor }}</small>
              </div>
              <div class="group-values">
                <span class="unit-price">{{ group.unitValue }} ¢/ea</span>
                <span class="total-price">{{ group.totalValue }} ¢ total</span>
                <button class="secondary mini-btn sell-btn" @click="openSellModal(group)">Liquidate</button>
                <button class="secondary mini-btn appraise-btn" :disabled="appraising" @click="requestAppraisal(group.catalogId, group.name, group.unitValue)">Appraise</button>
              </div>
            </div>
          </div>
          <p v-else class="empty">Home stash is currently empty. Complete raids to accumulate salvage.</p>
        </section>

        <section v-if="projection" class="panel">
          <p class="eyebrow">ONGOING WORLD / HANDLER MONITOR</p>
          <h2>{{ projection.status }} / {{ projection.players.length }} actual Raiders</h2>
          <div v-for="player in projection.players" :key="player.playerId" class="player-row">
            <span><strong>{{ player.displayName }}</strong>
              <small>{{ player.playerId === session?.playerId ? 'Your Raider / returned home' : 'Another actual Raider' }}</small>
              <small>{{ player.location }} / {{ player.activity }}<span v-if="player.destination"> to {{ player.destination }}</span></small></span>
          </div>
          <p class="privacy-note">Your Raider has returned. The shared world remains monitored until every admitted Raider returns.</p>
        </section>
        <section v-if="handlerProjection?.lastReturnSummary" class="panel debrief-panel" :class="{ 'ko-debrief': handlerProjection.lastReturnSummary.outcome === 'KnockedOut' }">
          <!-- G5 TERMINAL KNOCKED OUT DEBRIEF -->
          <div v-if="handlerProjection.lastReturnSummary.outcome === 'KnockedOut'" class="ko-debrief-card">
            <div class="ko-header">
              <div>
                <p class="eyebrow danger-eyebrow">TERMINAL DISPATCH // MISSION DEFEAT</p>
                <h2>KNOCKED OUT / RETURNED MOSTLY</h2>
                <p class="ko-cause">Cause of Defeat: <strong>{{ handlerProjection.lastReturnSummary.causeOfDefeat ?? 'Critical trauma' }}</strong></p>
              </div>
              <div class="medbay-badge">
                <span class="label">MEDBAY PROTOCOL</span>
                <strong>Chassis Recovered</strong>
              </div>
            </div>
            <div class="ko-stats-grid">
              <div class="ko-stat-card lost">
                <span class="label">LOST IN FIELD</span>
                <strong class="highlight-loss">-{{ handlerProjection.lastReturnSummary.lostHaulValue }} scrap</strong>
                <small>Unshielded backpack items destroyed/scavenged</small>
              </div>
              <div class="ko-stat-card preserved">
                <span class="label">SECRET HIDDEN POCKET SALVAGE</span>
                <template v-if="handlerProjection.lastReturnSummary.pocketSalvagedItem">
                  <strong class="highlight-salvage">+{{ handlerProjection.lastReturnSummary.pocketSalvagedItem.value }} scrap</strong>
                  <small>Salvaged: <strong>{{ handlerProjection.lastReturnSummary.pocketSalvagedItem.name }}</strong></small>
                </template>
                <template v-else>
                  <span class="empty-pocket-val">0 scrap</span>
                  <small>No item designated in safe pocket</small>
                </template>
              </div>
              <div class="ko-stat-card mission-id">
                <span class="label">MISSION ID</span>
                <code>{{ handlerProjection.lastReturnSummary.raidId.slice(0, 8) }}</code>
                <small>Desperanza Medbay auto-rebuilt chassis (0¢ copay)</small>
              </div>
            </div>
          </div>
          <!-- STANDARD SUCCESSFUL EXTRACTION DEBRIEF -->
          <div v-else>
            <p class="eyebrow">MISSION DEBRIEF / OFFICIAL RECEIPT</p>
            <h2>{{ handlerProjection.lastReturnSummary.debriefSummary }}</h2>
            <div class="debrief-stats">
              <div><span class="label">SECURED LOOT</span><strong>{{ handlerProjection.lastReturnSummary.securedLootCount }} items</strong></div>
              <div><span class="label">TOTAL VALUE</span><strong>{{ handlerProjection.lastReturnSummary.totalValue }} scrap</strong></div>
              <div><span class="label">MISSION ID</span><code>{{ handlerProjection.lastReturnSummary.raidId.slice(0, 8) }}</code></div>
            </div>
          </div>

          <!-- AAR 3-BULLET DEBRIEF (AI SYSTEMS DESIGN PIPELINE C) -->
          <div v-if="handlerProjection.lastReturnSummary.aarDebrief" class="aar-debrief-card">
            <div class="aar-debrief-header">
              <p class="eyebrow">AFTER-ACTION REPORT (AAR) // DESPERANZA ARCHIVES</p>
              <span class="aar-source-badge" :class="handlerProjection.lastReturnSummary.aarDebrief.source.toLowerCase()">
                {{ handlerProjection.lastReturnSummary.aarDebrief.source === 'Generated' ? 'SLM GENERATED' : 'ARCHIVAL LOG' }}
              </span>
            </div>
            <ul class="aar-bullets">
              <li v-for="(bullet, idx) in handlerProjection.lastReturnSummary.aarDebrief.bullets" :key="idx">
                <span class="aar-bullet-marker">▸</span>
                <span class="aar-bullet-text">{{ bullet }}</span>
              </li>
            </ul>
          </div>

          <!-- TOP ITEM APPRAISAL (AI SYSTEMS DESIGN STEP 3) -->
          <div v-if="handlerProjection.lastReturnSummary.topItemAppraisal" class="appraisal-card-highlight">
            <div class="appraisal-card-header">
              <p class="eyebrow">OBJECT OVERVALUATION // AI STRATEGIC APPRAISAL</p>
              <span class="aar-source-badge" :class="handlerProjection.lastReturnSummary.topItemAppraisal.source.toLowerCase()">
                {{ handlerProjection.lastReturnSummary.topItemAppraisal.source === 'Generated' ? 'SLM GENERATED' : 'ARCHIVAL TEMPLATE' }}
              </span>
            </div>
            <div class="appraisal-body">
              <div class="appraisal-title-row">
                <h3>{{ handlerProjection.lastReturnSummary.topItemAppraisal.itemName }}</h3>
                <span v-if="topAppraisedItem" class="appraisal-cost">{{ topAppraisedItem.value }} scrap</span>
              </div>
              <div class="appraisal-classification">
                <span class="label">CORPORATE CLASSIFICATION:</span>
                <strong>{{ handlerProjection.lastReturnSummary.topItemAppraisal.strategicClassification }}</strong>
              </div>
              <p class="appraisal-quote">"{{ handlerProjection.lastReturnSummary.topItemAppraisal.appraisal }}"</p>
            </div>
          </div>

          <!-- G3 HAUL PAYOFF & AUTO-SELL BREAKDOWN -->
          <div v-if="handlerProjection.lastReturnSummary.payoff" class="payoff-card">
            <p class="eyebrow">STASH SETTLEMENT &amp; PAYOFF</p>
            <div class="payoff-grid">
              <div><span class="label">HAUL VALUE</span><strong>+{{ handlerProjection.lastReturnSummary.payoff.grossSecuredValue }} scrap</strong></div>
              <div><span class="label">RETAINED IN STASH</span><strong>{{ handlerProjection.lastReturnSummary.payoff.unitsRetainedInStash }} items</strong></div>
              <div><span class="label">AUTO-SOLD OVERFLOW</span><strong>{{ handlerProjection.lastReturnSummary.payoff.unitsAutoSold }} items</strong></div>
              <div><span class="label">COINS EARNED</span><strong class="highlight-coins">+{{ handlerProjection.lastReturnSummary.payoff.overflowCoinsEarned }} ¢</strong></div>
              <div><span class="label">STASH SIZE</span><strong>{{ handlerProjection.lastReturnSummary.payoff.stashUnitsAfter }} / 120</strong></div>
              <div><span class="label">TOTAL BALANCE</span><strong>{{ handlerProjection.lastReturnSummary.payoff.coinsBalanceAfter }} ¢</strong></div>
            </div>
            <p v-if="handlerProjection.lastReturnSummary.payoff.unitsAutoSold > 0" class="overflow-notice">
              ⚠️ Stash limit reached (120 units max). {{ handlerProjection.lastReturnSummary.payoff.unitsAutoSold }} lowest-value item(s) were automatically liquidated for full face value (+{{ handlerProjection.lastReturnSummary.payoff.overflowCoinsEarned }} coins).
            </p>
          </div>

          <div v-if="handlerProjection.lastReturnSummary.highlights.length" class="debrief-highlights">
            <p class="eyebrow">MISSION HIGHLIGHTS</p>
            <ul>
              <li v-for="(highlight, idx) in handlerProjection.lastReturnSummary.highlights" :key="idx">
                {{ highlight }}
              </li>
            </ul>
          </div>
          <div class="debrief-items">
            <p class="eyebrow">SECURED MANIFEST</p>
            <div v-for="item in handlerProjection.lastReturnSummary.items" :key="item.instanceId" class="item-row">
              <span><strong>{{ item.name }} / {{ item.value }}</strong><small>{{ item.flavor }}</small></span>
              <button class="secondary mini-btn appraise-btn" :disabled="appraising" @click="requestAppraisal(item.catalogId, item.name, item.value)">Appraise</button>
            </div>
          </div>
        </section>
        <section v-else class="panel">
          <p class="eyebrow">LAST SECURED RESULT / PRIVATE</p>
          <h2>{{ handlerProjection?.myLastExtractedLoot.length ?? 0 }} items / {{ handlerProjection?.myLastExtractedLoot.reduce((sum, item) => sum + item.value, 0) ?? 0 }} value</h2>
          <div v-for="item in handlerProjection?.myLastExtractedLoot ?? []" :key="item.instanceId" class="item-row">
            <span><strong>{{ item.name }} / {{ item.value }}</strong><small>{{ item.flavor }}</small></span>
          </div>
          <p v-if="!handlerProjection?.myLastExtractedLoot.length" class="empty">No secured raid result yet.</p>
        </section>
      </div>
      <div v-if="projection && isRaidActive" class="active-raid">
        <div class="raid-world-heading">
          <div>
            <p class="eyebrow">{{ projection.status }} / {{ projection.rulesVersion }} / REVISION {{ projection.revision }}</p>
            <h2>{{ me?.location }}<span v-if="me"> / {{ me.activity }}<span v-if="me.destination"> to {{ me.destination }}</span></span></h2>
            <p v-if="me?.activityEndsAt" class="subhead">Your next activity in {{ secondsRemaining(me.activityEndsAt) }}s</p>
          </div>
        </div>

        <!-- G5 RAIDER SURVIVAL & SHIELD HUD -->
        <section class="panel survival-hud-panel">
          <div class="hud-header">
            <div>
              <p class="eyebrow">RAIDER VITALS &amp; SHIELD TELEMETRY // MAKESHIFT CONFIDENCE SHIELD</p>
              <h2>{{ me?.displayName }} — {{ me?.activity }} <span v-if="me?.location">({{ me.location }})</span></h2>
            </div>
            <div class="hud-signal-tag">
              <span class="label">HANDLER SIGNAL</span>
              <strong class="highlight-signal">{{ signalCurrent }} / {{ signalMax }} ⚡</strong>
            </div>
          </div>

          <div class="vital-meters-grid">
            <div class="meter-card hp-meter">
              <div class="meter-info">
                <span class="meter-label">CORE HEALTH (HP)</span>
                <strong class="meter-value" :class="{ 'hp-low': myHealth <= 15 }">{{ myHealth }} / {{ myMaxHealth }}</strong>
              </div>
              <div class="progress-bar-bg">
                <div class="progress-bar-fill hp-fill" :style="{ width: `${hpPercent}%` }"></div>
              </div>
              <small v-if="myHealth <= 15" class="meter-note hp-warn">⚠️ CRITICAL: Autonomous medical triage active</small>
            </div>

            <div class="meter-card shield-meter">
              <div class="meter-info">
                <span class="meter-label">SHIELD CHARGE (40% MITIGATION)</span>
                <strong class="meter-value" :class="{ 'shield-depleted': shieldCharge <= 0 }">{{ shieldCharge }} / {{ shieldMaxCharge }} pts</strong>
              </div>
              <div class="progress-bar-bg">
                <div class="progress-bar-fill shield-fill" :style="{ width: `${shieldPercent}%` }"></div>
              </div>
              <div class="shield-meta-row">
                <small class="shield-durability-note">Durability: {{ shieldDurability }}% integrity</small>
                <small v-if="shieldCharge <= 0" class="shield-down-warn">⚠️ SHIELD DOWN — Breakthrough damage unmitigated</small>
              </div>
            </div>
          </div>

          <div v-if="projection?.condition" class="field-supplies-banner">
            <span class="label">{{ projection.location }} / {{ projection.condition.name }} / {{ projection.condition.danger }} danger</span>
            <p>{{ projection.condition.description }}</p>
            <p v-if="projection.deadlineAt">World closes in {{ secondsRemaining(projection.deadlineAt) }}s.
              Exposure {{ Math.round((projection.exposure ?? 0) * 100) }}%.
              Greed {{ projection.myGreed }} / Mood {{ projection.myMood }} /
              Resilience {{ projection.myResilience }} raw damage trim.</p>
            <p>Next eligible exit check: {{ Math.round((projection.myExtractChance ?? 0) * 100) }}%.
              Valuable-item appetite: {{ (projection.myAppetite ?? 0).toFixed(2) }} (not a guaranteed find).
              Zero exit chance can mean the ordinary residence guard is active.</p>
            <p v-for="warning in projection.hazardWarnings ?? []" :key="warning.location">
              Hazard forecast: {{ warning.location }} in {{ secondsRemaining(warning.resolvesAt) }}s;
              {{ warning.rawDamage }} raw damage before resilience/shields. Actual occupancy determines impact.
            </p>
          </div>
          <div v-if="activeFieldSupplies.length" class="field-supplies-banner">
            <span class="label">DEPLOYED FIELD SUPPLIES:</span>
            <div class="supply-chips-list">
              <span v-for="s in activeFieldSupplies" :key="s.catalogId" class="supply-chip" :class="s.category.toLowerCase()">
                {{ s.name }} (x{{ s.quantity }})
              </span>
            </div>
          </div>
        </section>

        <!-- G5 HANDLER SUPPORT DIRECTIVES PANEL -->
        <section class="panel handler-support-panel">
          <div class="support-header">
            <div>
              <p class="eyebrow">EMERGENCY INTERVENTION // HANDLER SUPPORT</p>
              <h3>Remote Tactical Directives</h3>
              <p class="panel-note">Directives cost spendable Signal (+1 per 30s elapsed, max 5). Server-authoritative and non-blocking.</p>
            </div>
            <div class="signal-budget-box">
              <span class="label">SIGNAL BUDGET</span>
              <strong class="signal-large">{{ signalCurrent }} / {{ signalMax }} ⚡</strong>
              <small>Max 5 / Regen: +1/30s</small>
            </div>
          </div>

          <p v-if="supportActionFeedback" class="support-feedback" :class="{ error: supportActionError }">
            {{ supportActionFeedback }}
          </p>

          <div class="support-buttons-grid">
            <button
              type="button"
              class="support-btn extract-btn"
              :disabled="supportActionPending || signalCurrent < 3 || me?.activity === 'Extracted' || me?.activity === 'KnockedOut'"
              @click="triggerSupportAction('CallExtract')"
            >
              <div class="btn-top">
                <strong>CALL EXTRACT</strong>
                <span class="cost-tag">3 ⚡</span>
              </div>
              <p>Override greed checks &amp; immediately route Raider toward Extraction Platform.</p>
            </button>

            <button
              type="button"
              class="support-btn calm-btn"
              :disabled="supportActionPending || signalCurrent < 1 || me?.activity === 'Extracted' || me?.activity === 'KnockedOut'"
              @click="triggerSupportAction('Calm')"
            >
              <div class="btn-top">
                <strong>CALM</strong>
                <span class="cost-tag">1 ⚡</span>
              </div>
              <p>Greed -12, Mood +3: more exit willingness, less valuable-item appetite, stronger positive-mood resilience. Not a guaranteed rescue.</p>
            </button>

            <button
              type="button"
              class="support-btn pressure-btn"
              :disabled="supportActionPending || signalCurrent < 1 || me?.activity === 'Extracted' || me?.activity === 'KnockedOut'"
              @click="triggerSupportAction('Pressure')"
            >
              <div class="btn-top">
                <strong>PRESSURE</strong>
                <span class="cost-tag">1 ⚡</span>
              </div>
              <p>Greed +10, Mood -2: more valuable-item appetite, less exit willingness and resilience. Existing shared loot stays finite.</p>
            </button>
          </div>
        </section>

        <!-- ACTIVE THREAD SPOTLIGHT -->
        <section v-if="currentThread" class="panel thread-panel">
          <div class="thread-header">
            <div>
              <p class="eyebrow">ACTIVE THREAD / {{ currentThread.kind.toUpperCase() }}</p>
              <h2>{{ currentThread.title }}</h2>
              <p class="subhead">{{ currentThread.summary }}</p>
              <small class="thread-meta">Location: {{ currentThread.location }}<span v-if="currentThread.endsAt"> / Ends in {{ secondsRemaining(currentThread.endsAt) }}s</span></small>
            </div>
            <div v-if="currentThread.endsAt" class="thread-countdown">
              <span class="countdown-number">{{ secondsRemaining(currentThread.endsAt) }}s</span>
              <span class="label">REMAINING</span>
            </div>
          </div>
          <div class="progress-container">
            <div class="progress-bar" :style="{ width: `${threadProgressPercent(currentThread)}%` }"></div>
          </div>
          <div v-if="currentThread.logs.length" class="thread-logs">
            <p class="eyebrow">THREAD PROGRESSION</p>
            <div v-for="(log, idx) in currentThread.logs" :key="idx" class="thread-log-entry">
              <span>{{ log }}</span>
            </div>
          </div>
        </section>

        <div v-if="recentThreads.length" class="recent-threads-summary">
          <span class="label">RECENT COMPLETED THREADS:</span>
          <span v-for="t in recentThreads.slice(-3)" :key="t.threadId" class="recent-thread-chip">
            ✓ {{ t.kind }}: {{ t.summary }}
          </span>
        </div>

      <div class="raid-grid">
        <section class="panel people">
          <p class="eyebrow">ACTUAL RAIDERS / SHARED SESSION</p>
          <h2>Who's in the scrapyard?</h2>
          <div v-for="player in projection.players" :key="player.playerId" class="player-row">
            <span><strong>{{ player.displayName }}</strong>
              <small>{{ player.playerId === session?.playerId ? 'Your Raider' : 'Another actual Raider' }} / {{ player.disposition }}</small>
              <small>{{ player.location }} / {{ player.activity }}<span v-if="player.destination"> to {{ player.destination }}</span></small></span>
              <small v-if="player.activityEndsAt">{{ secondsRemaining(player.activityEndsAt) }}s</small>
          </div>
          <p class="privacy-note">Only actual Raiders in this authoritative world participate. Handler connection is not Raider participation.</p>
        </section>
        <section class="panel">
          <p class="eyebrow">SHARED MAP / INDEPENDENT JOURNEYS</p>
          <h2>The scenic route to regret</h2>
          <ol class="route">
            <li v-for="location in projection.locations" :key="location.name" :class="{ current: location.name === me?.location }">
              <strong>{{ location.name }}</strong><small>Connected to {{ location.neighbors.join(' / ') }}</small>
              <small>{{ projection.players.filter(player => player.location === location.name && player.activity !== 'Traveling').map(player => player.displayName).join(' / ') || 'No Raiders currently present' }}</small>
            </li>
          </ol>
          <div v-for="opportunity in projection.opportunities" :key="opportunity.opportunityId" class="privacy-note">
            <strong>{{ opportunity.location }} / {{ opportunity.opportunityId }}</strong>
            <div>{{ opportunity.status }} / {{ opportunity.remainingItems }} items remain
              <span v-if="opportunity.status === 'Searching' && opportunity.closesAt"> / closes in {{ secondsRemaining(opportunity.closesAt) }}s</span></div>
            <div v-if="opportunity.participantIds.length">Search participants: {{ participantNames(opportunity.participantIds) }}</div>
          </div>
          <div class="privacy-note">
            <strong>{{ projection.robot.name }} / {{ projection.robot.location }}</strong>
            <div>{{ projection.robot.status }} / {{ projection.robot.health }} of {{ projection.robot.maxHealth }} HP
              <span v-if="projection.robot.status === 'Active' && projection.robot.nextRoundAt">
                / round {{ projection.robot.roundsResolved + 1 }} in {{ secondsRemaining(projection.robot.nextRoundAt) }}s
              </span>
            </div>
            <div v-if="projection.robot.participantIds.length">
              Actual combat participants: {{ participantNames(projection.robot.participantIds) }}
            </div>
          </div>
        </section>
        <section class="panel backpack-panel">
          <p class="eyebrow">{{ me?.activity === 'Extracted' ? 'EXTRACTED THIS RAID' : 'YOUR BACKPACK / PRIVATE' }}</p>
          <h2>{{ loot.length }} items / {{ lootValue }} value</h2>

          <!-- G5 SECRET HIDDEN POCKET -->
          <div class="hidden-pocket-box">
            <div class="pocket-top">
              <div>
                <span class="pocket-badge">SECRET HIDDEN POCKET</span>
                <strong>Parody Safe Pocket (1 item cap)</strong>
              </div>
              <button
                v-if="myHiddenPocketInstanceId"
                type="button"
                class="secondary mini-btn clear-pocket-btn"
                :disabled="pocketActionPending"
                @click="designatePocket(null)"
              >
                Clear Pocket
              </button>
            </div>
            <p class="pocket-desc">Exactly one designated item safely extracts to Home Stash even if your Raider is <strong>KNOCKED OUT</strong>.</p>
            <div v-if="pocketedItem" class="pocket-protected-item">
              <span class="pocket-icon">🛡️</span>
              <div class="pocket-item-details">
                <strong>{{ pocketedItem.name }} ({{ pocketedItem.value }} scrap)</strong>
                <small class="pocket-protected-tag">PROTECTED AGAINST TERMINAL FAILURE</small>
              </div>
            </div>
            <div v-else class="pocket-empty-note">
              <em>No item currently protected. Click "Pocket" on any backpack item below.</em>
            </div>
            <p v-if="pocketFeedback" class="pocket-feedback-note">{{ pocketFeedback }}</p>
          </div>

          <div v-for="item in loot" :key="item.instanceId" class="item-row" :class="{ 'pocket-selected-row': item.instanceId === myHiddenPocketInstanceId }">
            <span>
              <strong>{{ item.name }} / {{ item.value }}</strong>
              <span v-if="item.instanceId === myHiddenPocketInstanceId" class="pocket-item-tag">🛡️ IN SAFE POCKET</span>
              <small>{{ item.flavor }}</small>
              <small>{{ item.instanceId }}</small>
            </span>
            <div class="item-row-actions">
              <button
                v-if="item.instanceId !== myHiddenPocketInstanceId && me?.activity !== 'Extracted' && me?.activity !== 'KnockedOut'"
                type="button"
                class="secondary mini-btn pocket-btn"
                :disabled="pocketActionPending"
                @click="designatePocket(item.instanceId)"
              >
                Pocket
              </button>
              <button class="secondary mini-btn appraise-btn" :disabled="appraising" @click="requestAppraisal(item.catalogId, item.name, item.value)">Appraise</button>
            </div>
          </div>
          <p v-if="!loot.length" class="empty">Nothing yet. The Raider is handling that. Allegedly.</p>
          <p class="privacy-note">Extracted loot is a session result, not persistent account inventory. Server restarts erase this local slice.</p>
          <p v-if="me?.activity === 'Extracted' && projection.status !== 'Completed'" class="privacy-note">Your Raider returned to Desperanza. Other actual Raiders are still completing this world.</p>
        </section>
      </div>

      <section v-for="encounter in projection.encounters" :key="encounter.encounterId" class="panel receipt">
        <p class="eyebrow">SHARED ENCOUNTER RECEIPT</p>
        <h2>{{ encounter.location }} / {{ encounter.resolution }}</h2>
        <p class="privacy-note">Actual participants: {{ participantNames(encounter.participantIds) }}</p>
        <p class="subhead">{{ encounter.reason }}</p>
        <div class="table-scroll">
          <table>
            <thead><tr><th>Raider</th><th>Disposition</th><th>Greed</th><th>Aptitude</th><th>Bag value</th><th>Initiative</th><th>Score</th></tr></thead>
            <tbody><tr v-for="factor in encounter.factors" :key="factor.playerId">
              <td>{{ factor.displayName }}</td><td>{{ factor.disposition }}</td><td>{{ factor.greed }}</td><td>{{ factor.aptitude }}</td>
              <td>{{ factor.backpackValue }}</td><td>{{ factor.initiative }}</td><td>{{ factor.score }}</td>
            </tr></tbody>
          </table>
        </div>
        <p v-for="award in encounter.awards" :key="award.instanceId" class="award">{{ award.recipientName }} recovered {{ award.itemName }}</p>
        <p class="privacy-note">Experimental salvage rules. Contesting costs eight seconds of recovery; cooperation takes two. No combat or theft yet.</p>
      </section>
      <section v-if="projection.robotEncounter" class="panel receipt">
        <p class="eyebrow">SHARED ROBOT ENCOUNTER / PARTICIPANTS ONLY</p>
        <h2>{{ projection.robotEncounter.location }} / {{ projection.robotEncounter.resolution }}</h2>
        <p class="privacy-note">Actual participants: {{ participantNames(projection.robotEncounter.participantIds) }}</p>
        <p class="subhead">{{ projection.robotEncounter.reason }}</p>
        <div class="table-scroll">
          <table>
            <thead><tr><th>Raider</th><th>Damage to robot</th><th>Damage received</th></tr></thead>
            <tbody><tr v-for="damage in projection.robotEncounter.damage" :key="damage.playerId">
              <td>{{ participantNames([damage.playerId]) }}</td><td>{{ damage.damageDealt }}</td><td>{{ damage.damageTaken }}</td>
            </tr></tbody>
          </table>
        </div>
      </section>
      </div>

      <!-- CONTINUOUS HANDLER COMMS FEED -->
      <section class="panel comms">
        <div class="comms-header">
          <div>
            <p class="eyebrow">CONTINUOUS COMMS JOURNAL / CROSS-RAID PARODY FEED</p>
            <h2>Handler channel / {{ transport }}</h2>
          </div>
          <div class="comms-filter-tabs">
            <button :class="{ active: commsFilter === 'all' }" class="filter-tab" @click="commsFilter = 'all'">All ({{ journalEntries.length }})</button>
            <button :class="{ active: commsFilter === 'diary' }" class="filter-tab" @click="commsFilter = 'diary'">Diary</button>
            <button :class="{ active: commsFilter === 'hub' }" class="filter-tab" @click="commsFilter = 'hub'">Hub</button>
            <button :class="{ active: commsFilter === 'encounters' }" class="filter-tab" @click="commsFilter = 'encounters'">Encounters</button>
          </div>
        </div>

        <div ref="commsContainerRef" class="comms-scroll" @scroll="onCommsScroll">
          <ol>
            <li v-for="event in filteredComms" :key="event.sourceEventId" :class="`channel-${event.channel.toLowerCase()}`">
              <div class="comms-meta">
                <code>{{ new Date(event.at).toLocaleTimeString() }}</code>
                <span class="channel-badge">{{ event.channel }}</span>
                <span class="kind-tag">{{ event.kind }}</span>
                <details>
                  <summary class="kind-tag" :title="enrichmentExplanation(event.enrichmentStatus, event.enrichmentReason)">
                    {{ narrationLabel(event.narrativeSource) }}
                  </summary>
                  <p class="comms-text">{{ enrichmentExplanation(event.enrichmentStatus, event.enrichmentReason) }}</p>
                  <p class="comms-text">Source: {{ event.narrativeSource || 'CanonFallback' }} / Status: {{ event.enrichmentStatus || 'NotRequested' }}</p>
                  <p v-if="event.enrichmentReason" class="comms-text">Reason: {{ event.enrichmentReason }}</p>
                </details>
                <span v-if="event.raidId" class="raid-tag">#{{ event.raidId.slice(0, 6) }}</span>
              </div>
              <p class="comms-text">{{ event.text }}</p>
              <p v-if="event.flavor" class="comms-text">Optional Raider aside [Generated]: {{ event.flavor }}</p>
            </li>
          </ol>
          <p v-if="!filteredComms.length" class="empty">No comms recorded in this channel yet.</p>
        </div>

        <div v-if="hasNewUnreadComms" class="unread-banner">
          <button class="unread-jump" @click="scrollToBottom">
            ↓ New transmission received (Click to jump to latest)
          </button>
        </div>
      </section>

      <!-- SELL STASH MODAL DIALOG -->
      <div v-if="sellingGroup" class="modal-backdrop" @click.self="closeSellModal">
        <div class="modal-card">
          <p class="eyebrow">DESPERANZA SCRAP EXCHANGE / LIQUIDATION</p>
          <h3>Sell {{ sellingGroup.name }}</h3>
          <p class="panel-note">Unit price: <strong>{{ sellingGroup.unitValue }} ¢</strong> | Stored: <strong>{{ sellingGroup.quantity }} units</strong></p>

          <div class="sell-controls">
            <label for="sell-quantity">Quantity to liquidate:</label>
            <div class="qty-input-row">
              <input
                id="sell-quantity"
                type="number"
                min="1"
                :max="sellingGroup.quantity"
                v-model.number="sellQuantity"
              />
              <div class="quick-qty-buttons">
                <button type="button" class="secondary mini-btn" @click="sellQuantity = 1">1</button>
                <button type="button" class="secondary mini-btn" @click="sellQuantity = Math.max(1, Math.floor(sellingGroup.quantity / 2))">Half</button>
                <button type="button" class="secondary mini-btn" @click="sellQuantity = sellingGroup.quantity">All ({{ sellingGroup.quantity }})</button>
              </div>
            </div>
          </div>

          <div class="sell-preview">
            <span class="label">ESTIMATED PROCEEDS</span>
            <strong class="highlight-coins">+{{ (sellQuantity || 0) * sellingGroup.unitValue }} spendable coins</strong>
          </div>

          <div class="modal-actions">
            <button type="button" class="secondary" :disabled="selling" @click="closeSellModal">Cancel</button>
            <button
              type="button"
              class="primary"
              :disabled="selling || !sellQuantity || sellQuantity < 1 || sellQuantity > sellingGroup.quantity"
              @click="executeSell(sellingGroup, sellQuantity)"
            >
              {{ selling ? 'Liquidating...' : `Confirm Sale (+${(sellQuantity || 0) * sellingGroup.unitValue} ¢)` }}
            </button>
          </div>
        </div>
      </div>

      <!-- ITEM APPRAISAL MODAL DIALOG (AI SYSTEMS DESIGN STEP 3) -->
      <div v-if="appraisalModal" class="modal-backdrop" @click.self="closeAppraisalModal">
        <div class="modal-card appraisal-modal-card">
          <div class="appraisal-modal-header">
            <p class="eyebrow">DESPERANZA APPRAISAL DESK // OVERVALUATION AUDIT</p>
            <span class="aar-source-badge" :class="appraisalModal.source.toLowerCase()">
              {{ appraisalModal.source === 'Generated' ? 'SLM GENERATED' : 'ARCHIVAL TEMPLATE' }}
            </span>
          </div>
          <div class="appraisal-title-row">
            <h3>{{ appraisalModal.itemName }}</h3>
            <span class="appraisal-cost">{{ appraisalModal.estimatedValue }} scrap</span>
          </div>
          <div class="appraisal-classification">
            <span class="label">CORPORATE CLASSIFICATION:</span>
            <strong>{{ appraisalModal.strategicClassification }}</strong>
          </div>
          <div class="appraisal-quote-box">
            <p class="appraisal-text">"{{ appraisalModal.appraisal }}"</p>
          </div>
          <div class="appraisal-footer-meta">
            <small>Source: {{ appraisalModal.source }} | Generated: {{ new Date(appraisalModal.generatedAt).toLocaleString() }}</small>
          </div>
          <div class="modal-actions" style="margin-top: 18px;">
            <button type="button" class="primary" @click="closeAppraisalModal">Dismiss Appraisal</button>
          </div>
        </div>
      </div>
    </section>
    <p v-if="error" class="error" role="alert">{{ error }}</p>
  </main>
</template>
