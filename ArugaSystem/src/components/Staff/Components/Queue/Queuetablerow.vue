<script setup>
import QueueStatusBadge from "./Queuestatusbadge.vue";
import QueueActionMenu from "./Queueactionmenu.vue";

const props = defineProps({
  queueItem: {
    type: Object,
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

const handleStatusChange = (payload) => {
  emit("status-change", payload);
};

const handleViewPatient = (payload) => {
  emit("view-patient", payload);
};

const handleAssignWorker = (payload) => {
  emit("assign-worker", payload);
};
</script>

<template>
  <tr
    class="group bg-white transition-colors hover:bg-stone-50/60"
  >
    <!-- Queue Number -->
    <td class="px-4 py-5 align-middle">
      <span class="text-[14px] font-semibold text-stone-900">
        {{ queueItem.no }}
      </span>
    </td>

    <!-- Child -->
    <td class="px-3 py-5 align-middle">
      <p
        class="truncate text-[14px] font-medium text-stone-800"
        :title="queueItem.child"
      >
        {{ queueItem.child || "—" }}
      </p>
    </td>

    <!-- Parent / Guardian -->
    <td class="px-3 py-5 align-middle">
      <p
        class="truncate text-[14px] text-stone-600"
        :title="queueItem.parent"
      >
        {{ queueItem.parent || "—" }}
      </p>
    </td>

    <!-- Barangay -->
    <td class="px-3 py-5 align-middle">
      <span class="text-[14px] text-stone-600">
        {{ queueItem.barangay || "—" }}
      </span>
    </td>

    <!-- Status -->
    <td class="px-3 py-5 align-middle">
      <QueueStatusBadge :status="queueItem.status" />
    </td>

    <!-- Expected -->
    <td class="px-3 py-5 align-middle">
      <span class="text-[14px] text-stone-600">
        {{ queueItem.expectedDate || "—" }}
      </span>
    </td>

    <!-- Vaccination Due -->
    <td class="px-3 py-5 align-middle">
      <span
        class="block truncate text-[14px] text-stone-600"
        :title="queueItem.vaccinationDueDate"
      >
        {{ queueItem.vaccinationDueDate || "—" }}
      </span>
    </td>

    <!-- Actions -->
    <td class="px-3 py-5 align-middle">
      <div class="flex items-center justify-end gap-2">
        <QueueActionMenu
          :status="queueItem.status"
          :queue-id="queueItem.queueID"
          :is-updating="isUpdating"
          @status-change="handleStatusChange"
          @view-patient="handleViewPatient"
          @assign-worker="handleAssignWorker"
        />
      </div>
    </td>
  </tr>
</template>