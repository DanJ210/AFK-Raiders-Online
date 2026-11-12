<script setup lang="ts">
import { computed } from 'vue'
import { diaryPresentation, type DiaryTextEvent } from '../journal'

const props = defineProps<{ event: DiaryTextEvent }>()
const presentation = computed(() => diaryPresentation(props.event))
</script>

<template>
  <div class="diary-entry-text">
    <p class="comms-text">{{ presentation.main }}</p>
    <details v-if="presentation.changed" class="diary-event-details">
      <summary>Event details</summary>
      <p class="comms-text">{{ presentation.record }}</p>
      <p v-if="event.factsText && !presentation.record.includes(event.factsText)" class="comms-text">{{ event.factsText }}</p>
    </details>
  </div>
</template>
