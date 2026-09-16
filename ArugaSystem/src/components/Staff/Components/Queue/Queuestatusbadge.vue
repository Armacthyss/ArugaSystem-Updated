<script setup>
const props = defineProps({
  status: {
    type: String,
    required: true,
    // Accepts: Waiting, Called, InProgress, Completed, Cancelled, NoShow
  },
});

const statusStyles = {
  Waiting: "bg-amber-50 text-amber-700",
  Called: "bg-sky-50 text-sky-700",
  InProgress: "bg-violet-50 text-violet-700",
  Completed: "bg-emerald-50 text-emerald-700",
  Cancelled: "bg-stone-100 text-stone-600",
  NoShow: "bg-rose-50 text-rose-700",
};

const statusDots = {
  Waiting: "bg-amber-500",
  Called: "bg-sky-500",
  InProgress: "bg-violet-500",
  Completed: "bg-emerald-600",
  Cancelled: "bg-stone-500",
  NoShow: "bg-rose-600",
};

const statusLabels = {
  Waiting: "Waiting",
  Called: "Called",
  InProgress: "In Progress",
  Completed: "Completed",
  Cancelled: "Cancelled",
  NoShow: "No Show",
};

const displayText = statusLabels[props.status] || props.status;
const badgeStyle = statusStyles[props.status] || "bg-stone-100 text-stone-600";
const dotStyle = statusDots[props.status] || "bg-stone-500";
</script>

<template>
  <span
    class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium"
    :class="badgeStyle"
  >
    <span class="h-1.5 w-1.5 rounded-full" :class="dotStyle" />
    {{ displayText }}
  </span>
</template>