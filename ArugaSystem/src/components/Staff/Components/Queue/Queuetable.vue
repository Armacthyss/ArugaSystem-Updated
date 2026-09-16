<script setup>
import { RefreshCw } from "lucide-vue-next";
import QueueTableRow from "./Queuetablerow.vue";

const props = defineProps({
  queue: {
    type: Array,
    default: () => [],
  },

  loading: {
    type: Boolean,
    default: false,
  },

  error: {
    type: String,
    default: null,
  },

  updatingQueueID: {
    type: String,
    default: null,
  },
});

const emit = defineEmits([
  "refresh",
  "status-change",
  "view-patient",
  "assign-worker",
]);

const handleRefresh = () => {
  emit("refresh");
};

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
  <section
    class="rounded-2xl border border-stone-200 bg-white shadow-sm"
  >
    <!-- Header -->
    <div
      class="flex items-center justify-between border-b border-stone-200 px-5 py-4"
    >
      <div>
        <h2 class="text-[14px] font-semibold text-stone-900">
          Today's Queue
        </h2>

        <p class="mt-0.5 text-[12px] text-stone-500">
          {{ queue.length }}
          patient{{ queue.length !== 1 ? "s" : "" }} in queue
        </p>
      </div>

      <button
        type="button"
        @click="handleRefresh"
        :disabled="loading"
        class="inline-flex items-center gap-2 rounded-lg border border-stone-200 bg-white px-3 py-2 text-[12px] font-medium text-stone-600 transition hover:bg-stone-50 disabled:cursor-not-allowed disabled:opacity-50"
      >
        <RefreshCw
          :size="14"
          :class="{ 'animate-spin': loading }"
        />

        Refresh
      </button>
    </div>

    <!-- Error -->
    <div
      v-if="error"
      class="border-b border-rose-100 bg-rose-50 px-5 py-3 text-[12px] text-rose-700"
    >
      {{ error }}
    </div>

    <!-- Loading -->
    <div
      v-if="loading && queue.length === 0"
      class="px-5 py-10 text-center text-[12px] text-stone-500"
    >
      Loading today's queue...
    </div>

    <!-- Empty -->
    <div
      v-else-if="queue.length === 0"
      class="px-5 py-10 text-center"
    >
      <p class="text-[13px] font-medium text-stone-600">
        No patients are currently in the queue.
      </p>

      <p class="mt-1 text-[11px] text-stone-400">
        Patients who join today's queue will appear here.
      </p>
    </div>

    <!-- Table -->
    <div v-else class="w-full">
      <table class="w-full table-fixed">
        <thead>
          <tr class="border-b border-stone-200 bg-stone-50/60">
            <th class="w-[12%] px-4 py-3 text-left text-[10px] font-semibold uppercase tracking-wide text-stone-500">
  Queue No.
</th>

<th class="w-[18%] px-3 py-3 text-left text-[10px] font-semibold uppercase tracking-wide text-stone-500">
  Child
</th>

<th class="w-[22%] px-3 py-3 text-left text-[10px] font-semibold uppercase tracking-wide text-stone-500">
  Parent/Guardian
</th>

<th class="w-[9%] px-3 py-3 text-left text-[10px] font-semibold uppercase tracking-wide text-stone-500">
  Brgy.
</th>

<th class="w-[13%] px-3 py-3 text-left text-[10px] font-semibold uppercase tracking-wide text-stone-500">
  Status
</th>

<th class="w-[16%] px-3 py-3 text-left text-[10px] font-semibold uppercase tracking-wide text-stone-500">
  Vaccination Due
</th>

<th class="w-[10%] px-3 py-3 text-right text-[10px] font-semibold uppercase tracking-wide text-stone-500">
  Actions
</th>
          </tr>
        </thead>

        <tbody class="divide-y divide-stone-100">
          <QueueTableRow
            v-for="item in queue"
            :key="item.queueID"
            :queue-item="item"
            :is-updating="updatingQueueID === item.queueID"
            @status-change="handleStatusChange"
            @view-patient="handleViewPatient"
            @assign-worker="handleAssignWorker"
          />
        </tbody>
      </table>
    </div>
  </section>
</template>