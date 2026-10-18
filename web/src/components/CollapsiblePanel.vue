<script setup lang="ts">
import { ref, useId } from 'vue'

defineProps<{ label: string }>()

const isCollapsed = ref(false)
const contentId = useId()
</script>

<template>
  <section class="collapsible-panel" :class="{ 'is-collapsed': isCollapsed }">
    <button
      type="button"
      class="panel-collapse-toggle"
      :aria-expanded="!isCollapsed"
      :aria-controls="contentId"
      :aria-label="`${isCollapsed ? 'Expand' : 'Collapse'} ${label}`"
      @click="isCollapsed = !isCollapsed"
    >
      <span class="panel-collapse-icon" aria-hidden="true">{{ isCollapsed ? '▸' : '▾' }}</span>
      <span v-if="isCollapsed" class="panel-collapse-label">{{ label }}</span>
    </button>
    <div :id="contentId" v-show="!isCollapsed" class="panel-content">
      <slot />
    </div>
  </section>
</template>
