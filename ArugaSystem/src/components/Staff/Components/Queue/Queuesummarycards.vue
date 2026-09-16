<script setup>
import { computed } from "vue";
import {
  Clock,
  CheckCircle2,
  AlertTriangle,
  Activity,
  UserCog,
} from "lucide-vue-next";

const props = defineProps({
  queue: {
    type: Array,
    default: () => [],
  },
  availableWorkersCount: {
    type: Number,
    default: 0,
  },
});

// Compute queue statistics based on actual queue data
const waitingCount = computed(() =>
  props.queue.filter((q) => q.status === "Waiting").length
);

const inProgressCount = computed(() =>
  props.queue.filter((q) => q.status === "InProgress").length
);

const completedCount = computed(() =>
  props.queue.filter((q) => q.status === "Completed").length
);

// TODO: determine "late" status calculation from backend
// For now, using placeholder logic
const lateCount = computed(() =>
  props.queue.filter((q) => q.status === "Late").length
);

const summaryCards = computed(() => [
  {
    label: "Waiting",
    value: waitingCount.value,
    icon: Clock,
    tint: "text-amber-700",
    tintBg: "bg-amber-50",
  },
  {
    label: "In Progress",
    value: inProgressCount.value,
    icon: Activity,
    tint: "text-sky-700",
    tintBg: "bg-sky-50",
  },
  {
    label: "Completed",
    value: completedCount.value,
    icon: CheckCircle2,
    tint: "text-emerald-700",
    tintBg: "bg-emerald-50",
  },
  {
    label: "Late",
    value: lateCount.value,
    icon: AlertTriangle,
    tint: "text-rose-700",
    tintBg: "bg-rose-50",
  },
  {
    label: "Available Healthcare Workers",
    value: props.availableWorkersCount,
    icon: UserCog,
    tint: "text-emerald-700",
    tintBg: "bg-emerald-50",
  },
]);
</script>

<template>
  <div class="grid grid-cols-5 gap-4">
    <div
      v-for="card in summaryCards"
      :key="card.label"
      class="rounded-2xl border border-stone-200 bg-white p-4 shadow-sm"
    >
      <div class="flex items-center justify-between">
        <p class="text-[10.5px] font-semibold uppercase tracking-wide text-stone-500 leading-tight">
          {{ card.label }}
        </p>
        <div class="flex h-7 w-7 items-center justify-center rounded-lg shrink-0" :class="card.tintBg">
          <component :is="card.icon" :size="14" :class="card.tint" />
        </div>
      </div>
      <p class="text-[24px] font-bold mt-2">{{ card.value }}</p>
    </div>
  </div>
</template>