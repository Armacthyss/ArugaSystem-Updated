<script setup>
import { computed, ref } from "vue";
import { ChevronDown, Eye, Phone, Play, CheckCircle2, X, Ban } from "lucide-vue-next";

const props = defineProps({
  status: {
    type: String,
    required: true,
  },
  queueID: {
    type: String,
    required: true,
  },
  isUpdating: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits([
  "status-change",
  "view-patient",
  "assign-worker",
]);

const showMenu = ref(false);

// Define available actions for each status
const actionsByStatus = {
  Waiting: [
  { label: "Assign", action: "assign", primary: true },
  { label: "Cancel", status: "Cancelled" },
  { label: "No Show", status: "NoShow" },
],
  Called: [
    { label: "Start", status: "InProgress", icon: Play, color: "text-violet-700" },
    { label: "Cancel", status: "Cancelled", icon: X, color: "text-stone-600" },
    { label: "No Show", status: "NoShow", icon: Ban, color: "text-rose-700" },
  ],
  InProgress: [
    { label: "Complete", status: "Completed", icon: CheckCircle2, color: "text-emerald-700" },
    { label: "Cancel", status: "Cancelled", icon: X, color: "text-stone-600" },
    { label: "No Show", status: "NoShow", icon: Ban, color: "text-rose-700" },
  ],
  Completed: [
    { label: "View Patient", action: "view-patient", icon: Eye, color: "text-stone-600" },
  ],
  Cancelled: [
    { label: "View Patient", action: "view-patient", icon: Eye, color: "text-stone-600" },
  ],
  NoShow: [
    { label: "View Patient", action: "view-patient", icon: Eye, color: "text-stone-600" },
  ],
};

const availableActions = computed(() => actionsByStatus[props.status] || []);

const handleAction = (action) => {
  if (action.action === "assign") {
    emit("assign-worker", {
      queueID: props.queueID,
    });
  } else if (action.action === "view-patient") {
    emit("view-patient", {
      queueID: props.queueID,
    });
  } else if (action.status) {
    emit("status-change", {
      queueID: props.queueID,
      status: action.status,
    });
  }

  showMenu.value = false;
};

// Workflow statuses have primary + secondary actions
const isWorkflowStatus = computed(() => {
  return (
    props.status === "Waiting" ||
    props.status === "Called" ||
    props.status === "InProgress"
  );
});

// Terminal statuses (Completed, Cancelled, NoShow) have only single action
const isTerminalStatus = computed(() => {
  return (
    props.status === "Completed" ||
    props.status === "Cancelled" ||
    props.status === "NoShow"
  );
});

const primaryAction = computed(() => availableActions.value[0] || null);
const secondaryActions = computed(() => availableActions.value.slice(1) || []);
</script>

<template>
  <div class="relative inline-block">
    <!-- Workflow statuses: Primary action + dropdown for secondary actions -->
    <div v-if="isWorkflowStatus" class="flex items-center gap-1">
      <!-- Primary action button -->
      <button
        v-if="primaryAction"
        @click="handleAction(primaryAction)"
        :disabled="isUpdating"
        :title="primaryAction.label"
        class="flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-xs font-medium transition-colors bg-emerald-700 text-white hover:bg-emerald-800 disabled:opacity-50"
      >
        <component :is="primaryAction.icon" :size="14" />
        {{ primaryAction.label }}
      </button>

      <!-- Secondary actions dropdown -->
      <div class="relative">
        <button
          @click="showMenu = !showMenu"
          :disabled="isUpdating"
          class="flex items-center justify-center rounded-lg px-1.5 py-1.5 text-xs font-medium text-stone-600 hover:bg-stone-100 disabled:opacity-50 border border-stone-200 transition-colors"
          :title="secondaryActions.length > 0 ? 'More actions' : ''"
        >
          <ChevronDown :size="14" />
        </button>

        <!-- Dropdown menu for secondary actions -->
        <transition
          enter-active-class="transition ease-out duration-100"
          enter-from-class="transform opacity-0 scale-95"
          enter-to-class="transform opacity-100 scale-100"
          leave-active-class="transition ease-in duration-75"
          leave-from-class="transform opacity-100 scale-100"
          leave-to-class="transform opacity-0 scale-95"
        >
          <div
            v-if="showMenu && secondaryActions.length > 0"
            class="absolute right-0 top-full mt-1 z-20 w-40 rounded-lg border border-stone-200 bg-white shadow-lg"
          >
            <button
              v-for="(action, idx) in secondaryActions"
              :key="idx"
              @click="handleAction(action)"
              :disabled="isUpdating"
              class="flex w-full items-center gap-2 px-3 py-2 text-sm hover:bg-stone-50 first:rounded-t-lg last:rounded-b-lg disabled:opacity-50 transition-colors"
              :class="[idx < secondaryActions.length - 1 && 'border-b border-stone-100']"
            >
              <component :is="action.icon" :size="14" :class="action.color" />
              <span>{{ action.label }}</span>
            </button>
          </div>
        </transition>

        <!-- Click outside to close menu -->
        <div
          v-if="showMenu"
          @click="showMenu = false"
          class="fixed inset-0 z-10"
        />
      </div>
    </div>

    <!-- Terminal statuses: Single View Patient button -->
    <button
      v-else-if="isTerminalStatus && primaryAction"
      @click="handleAction(primaryAction)"
      :disabled="isUpdating"
      :title="primaryAction.label"
      class="flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-xs font-medium transition-colors text-stone-600 hover:bg-stone-100 disabled:opacity-50"
    >
      <component :is="primaryAction.icon" :size="14" />
      {{ primaryAction.label }}
    </button>
  </div>
</template>