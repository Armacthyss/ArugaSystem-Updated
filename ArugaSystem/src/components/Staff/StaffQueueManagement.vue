<script setup>
/* =========================================================================
   STAFF QUEUE MANAGEMENT — /staff/queue (REFACTORED)

   This page maintains the same functionality as the original while
   delegating presentation to specialized child components.

   Parent page responsibilities:
   - API calls (loadQueue, updateQueueStatus)
   - queue state and data management
   - loading/error state management
   - periodic refresh logic
   - status update coordination

   Child component responsibilities:
   - Presentation and styling
   - Emitting user actions upward
   - No direct API calls
========================================================================= */

import { ref, computed, onMounted, onUnmounted } from "vue";
import StaffHeader from "../Staff/Components/StaffHeader.vue";
import StaffSidebar from "../Staff/Components/StaffSidebar.vue";
import QueueSummaryCards from "./Components/Queue/Queuesummarycards.vue";
import QueueTable from "./Components/Queue/Queuetable.vue";
import {
  QrCode,
  User,
  MapPin,
  CalendarClock,
  Syringe,
  X,
  Check,
  Search,
  MoreHorizontal,
} from "lucide-vue-next";

const API_BASE = "http://localhost:57147/api";

/* =========================================================================
   1. TODAY'S QUEUE — Real data from API
========================================================================= */
const queue = ref([]);
const loadingQueue = ref(false);
const queueError = ref(null);
const updatingStatus = ref(null);
const selectedQueueID = ref(null);
const selectedWorkerID = ref("");
const showAssignModal = ref(false);
const assigningWorker = ref(false);
const assignError = ref(null);

const loadQueue = async () => {
  loadingQueue.value = true;
  queueError.value = null;
  try {
    const response = await fetch(`${API_BASE}/Queue/today`);
    if (!response.ok) {
      throw new Error(`Failed to load queue. Status: ${response.status}`);
    }
    const data = await response.json();

    const activeStatuses = ["Waiting", "Called", "InProgress"];

queue.value = data
  .filter((q) => activeStatuses.includes(q.status))
  .map((q) => ({
    queueID: q.queueID,
    queueNumber: q.queueNumber,
    no: `Q-${String(q.queueNumber).padStart(3, "0")}`,
    child: q.children?.map((c) => c.name).join(", ") || "—",
    parent: q.requestBy || "—",
    barangay: q.barangayNo ?? "—",
    status: q.status || "Waiting",
    expectedDate: q.expectedDate ?? "—",
    vaccinationDueDate: q.vaccinationDueDate ?? "—",
    worker: q.assignedWorkerName ?? "—",
    room: q.room ?? "—",
  }));
  } catch (e) {
    console.error("Queue loading error:", e);
    queueError.value = e.message;
  } finally {
    loadingQueue.value = false;
  }
};

const updateQueueStatus = async (queueID, status) => {
  updatingStatus.value = queueID;

  try {
    const response = await fetch(`${API_BASE}/Queue/${queueID}/status`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        status,
      }),
    });

    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.message || "Failed to update queue status.");
    }

    // Update the row using the backend response
    const index = queue.value.findIndex((q) => q.queueID === queueID);
    if (index !== -1) {
      queue.value[index].status = data.status;
    }
  } catch (e) {
    console.error("Queue status update error:", e);
    queueError.value = e.message;
  } finally {
    updatingStatus.value = null;
  }
};

const openAssignWorker = (payload) => {
  selectedQueueID.value = payload.queueID;
  selectedWorkerID.value = "";
  assignError.value = null;
  showAssignModal.value = true;
};

const closeAssignWorker = () => {
  if (assigningWorker.value) return;

  showAssignModal.value = false;
  selectedQueueID.value = null;
  selectedWorkerID.value = "";
  assignError.value = null;
};

const assignWorker = async () => {
  if (!selectedQueueID.value || !selectedWorkerID.value) {
    assignError.value = "Please select a healthcare worker.";
    return;
  }

  assigningWorker.value = true;
  assignError.value = null;

  try {
    const token = localStorage.getItem("authToken");

    const response = await fetch(
      `${API_BASE}/Queue/${selectedQueueID.value}/assign`,
      {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        body: JSON.stringify({
          workerID: selectedWorkerID.value,
        }),
      }
    );

    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.message || "Failed to assign healthcare worker.");
    }

    showAssignModal.value = false;
    selectedQueueID.value = null;
    selectedWorkerID.value = "";

    await Promise.all([
      loadQueue(),
      loadHealthcareWorkers(),
    ]);
  } catch (e) {
    console.error("Worker assignment error:", e);
    assignError.value = e.message;
  } finally {
    assigningWorker.value = false;
  }
};

let queueRefreshInterval = null;
onMounted(() => {
  loadQueue();
  loadHealthcareWorkers();

  queueRefreshInterval = setInterval(() => {
    loadQueue();
    loadHealthcareWorkers();
  }, 5000);
});

onUnmounted(() => {
  if (queueRefreshInterval) clearInterval(queueRefreshInterval);
});

/* =========================================================================
   2. SUMMARY CARDS — Derived from real queue data
========================================================================= */
const availableWorkersCount = computed(() =>
  healthworkers.value.filter((w) => w.status === "Online").length
);

/* =========================================================================
   3. EXPECTED PATIENTS — TODO: replace with real endpoint
========================================================================= */
const expectedPatients = ref([
  {
    id: "EXP-1",
    child: "Sofia Dalisay",
    parent: "Test Test",
    barangay: "846",
    time: "9:15 AM",
    vaccines: [{ name: "MMR", dose: 2 }],
    scheduleStatus: "Scheduled",
    queueStatus: "Not in queue",
  },
  {
    id: "EXP-2",
    child: "Ana Dizon",
    parent: "Carmen Dizon",
    barangay: "212",
    time: "9:30 AM",
    vaccines: [{ name: "Pentavalent", dose: 3 }],
    scheduleStatus: "Scheduled",
    queueStatus: "Not in queue",
  },
  {
    id: "EXP-3",
    child: "Noah Bautista",
    parent: "Ramon Bautista",
    barangay: "104",
    time: "9:45 AM",
    vaccines: [{ name: "OPV", dose: 1 }],
    scheduleStatus: "Scheduled",
    queueStatus: "In queue",
  },
]);

const expectedDetail = ref(null);
const openExpectedDetail = (p) => {
  expectedDetail.value = p;
  dueMissedDetail.value = null;
};
const closeExpectedDetail = () => {
  expectedDetail.value = null;
};

/* =========================================================================
   4. DUE / MISSED VACCINATIONS — TODO: replace with real endpoint
========================================================================= */
const dueMissed = ref([
  {
    id: "DM-1",
    child: "Sofia Dalisay",
    parent: "Test Test",
    barangay: "846",
    vaccine: "MMR",
    dose: 2,
    dueDate: "Aug 28, 2026",
    dueLabel: "Aug 28",
    overdue: "7 days overdue",
    queueStatus: "Not in queue",
  },
  {
    id: "DM-2",
    child: "Juan Santos",
    parent: "Rowena Santos",
    barangay: "310",
    vaccine: "Pentavalent",
    dose: 1,
    dueDate: "Aug 30, 2026",
    dueLabel: "Aug 30",
    overdue: "5 days overdue",
    queueStatus: "Not in queue",
  },
  {
    id: "DM-3",
    child: "Ana Reyes",
    parent: "Mark Reyes",
    barangay: "846",
    vaccine: "BCG",
    dose: 1,
    dueDate: "Sep 4, 2026",
    dueLabel: "Today",
    overdue: "Due today",
    queueStatus: "In queue",
  },
]);

const dueMissedDetail = ref(null);
const openDueMissedDetail = (d) => {
  dueMissedDetail.value = d;
  expectedDetail.value = null;
};
const closeDueMissedDetail = () => {
  dueMissedDetail.value = null;
};

const addingToQueue = ref(null);
async function addToQueue(source, item) {
  addingToQueue.value = item.id;
  try {
    // TODO: confirm against backend — placeholder call shape only.
    // await fetch(`${API_BASE}/Queue`, { method: "POST", ... });
    console.log(`[TODO] Add to Queue requested from ${source}:`, item);
    item.queueStatus = "In queue"; // optimistic UI only
  } catch (e) {
    console.error("Add to Queue failed:", e);
  } finally {
    addingToQueue.value = null;
  }
}

/* =========================================================================
   5. ACTIVE HEALTHCARE WORKERS — TODO: replace with real endpoint
========================================================================= */
const healthworkers = ref([]);
const loadingWorkers = ref(false);
const workersError = ref(null);
const workerCardStyle = {
  Online: {
    badge: "bg-emerald-50 text-emerald-700",
    card: "border-stone-200 bg-white",
  },
  "On Break": {
    badge: "bg-amber-50 text-amber-700",
    card: "border-stone-200 bg-white",
  },
  Occupied: {
    badge: "bg-blue-50 text-blue-700",
    card: "border-stone-200 bg-white",
  },
  Offline: {
    badge: "bg-stone-100 text-stone-500",
    card: "border-stone-200 bg-stone-50 opacity-70",
  },
};
const loadHealthcareWorkers = async () => {
  loadingWorkers.value = true;
  workersError.value = null;

  try {
    const token = localStorage.getItem("authToken");

const response = await fetch(`${API_BASE}/Healthcare/workers`, {
  headers: {
    Authorization: `Bearer ${token}`,
  },
});

    if (!response.ok) {
      throw new Error(
        `Failed to load healthcare workers. Status: ${response.status}`
      );
    }

    const data = await response.json();

    healthworkers.value = data.map((w) => ({
      userID: w.userID,
      name: `${w.firstName ?? ""} ${w.lastName ?? ""}`.trim(),
      role: w.position,
      status: w.availabilityStatus,
      room: "—",
      patient: null,
      initials: `${w.firstName?.[0] ?? ""}${w.lastName?.[0] ?? ""}`.toUpperCase(),
    }));
  } catch (e) {
    console.error("Healthcare workers loading error:", e);
    workersError.value = e.message;
  } finally {
    loadingWorkers.value = false;
  }
};
/* =========================================================================
   6. SEARCH PATIENT — TODO: replace with real endpoint
========================================================================= */
const searchQuery = ref("");
const searchResult = ref(null);
const searchDetailOpen = ref(false);

function searchPatient() {
  const q = searchQuery.value.trim().toLowerCase();
  searchDetailOpen.value = false;
  if (!q) {
    searchResult.value = null;
    return;
  }

  const fromExpected = expectedPatients.value.find((p) =>
    p.child.toLowerCase().includes(q)
  );
  const fromDueMissed = dueMissed.value.find((p) =>
    p.child.toLowerCase().includes(q)
  );

  if (fromExpected) {
    searchResult.value = {
      source: "expected",
      child: fromExpected.child,
      parent: fromExpected.parent,
      barangay: fromExpected.barangay,
      scheduled: "Today",
      time: fromExpected.time,
      vaccines: fromExpected.vaccines,
      scheduleStatus: fromExpected.scheduleStatus,
      queueStatus: fromExpected.queueStatus,
      raw: fromExpected,
    };
  } else if (fromDueMissed) {
    searchResult.value = {
      source: "dueMissed",
      child: fromDueMissed.child,
      parent: fromDueMissed.parent,
      barangay: fromDueMissed.barangay,
      scheduled: fromDueMissed.dueLabel,
      time: "—",
      vaccines: [{ name: fromDueMissed.vaccine, dose: fromDueMissed.dose }],
      scheduleStatus: fromDueMissed.overdue,
      queueStatus: fromDueMissed.queueStatus,
      raw: fromDueMissed,
    };
  } else {
    searchResult.value = { notFound: true, query: searchQuery.value };
  }
}

/* =========================================================================
   EVENT HANDLERS
========================================================================= */
const handleTableRefresh = () => {
  loadQueue();
};

const handleStatusChange = async (payload) => {
  await updateQueueStatus(payload.queueID, payload.status);
};

const handleViewPatient = (payload) => {
  console.log("[TODO] View patient:", payload);
  // TODO: implement view patient logic (navigate to patient record, etc.)
};
</script>

<template>

  <!-- Assign Healthcare Worker Modal -->
<div
  v-if="showAssignModal"
  class="fixed inset-0 z-50 flex items-center justify-center bg-stone-900/40 px-4"
>
  <div class="w-full max-w-md rounded-2xl bg-white shadow-xl">
    <!-- Header -->
    <div class="flex items-center justify-between px-6 py-4 border-b border-stone-200">
      <div>
        <p class="text-[15px] font-semibold">Assign Healthcare Worker</p>
        <p class="text-[11px] text-stone-500 mt-0.5">
          Select an available worker for this patient.
        </p>
      </div>

      <button
        @click="closeAssignWorker"
        :disabled="assigningWorker"
        class="p-1.5 rounded-lg hover:bg-stone-100 disabled:opacity-50"
      >
        <X :size="18" class="text-stone-500" />
      </button>
    </div>

    <!-- Body -->
    <div class="p-6">
      <!-- Error -->
      <div
        v-if="assignError"
        class="mb-4 rounded-xl bg-rose-50 border border-rose-100 px-3.5 py-3 text-[12px] text-rose-700"
      >
        {{ assignError }}
      </div>

      <!-- Workers -->
      <div class="space-y-2">
        <p class="text-[12px] font-medium text-stone-600 mb-2">
          Available Workers
        </p>

        <button
          v-for="worker in healthworkers.filter(
            (w) => w.status === 'Online'
          )"
          :key="worker.userID"
          @click="selectedWorkerID = worker.userID"
          :disabled="assigningWorker"
          class="w-full flex items-center justify-between rounded-xl border px-4 py-3 text-left transition-colors"
          :class="
            selectedWorkerID === worker.userID
              ? 'border-emerald-500 bg-emerald-50'
              : 'border-stone-200 hover:bg-stone-50'
          "
        >
          <div class="flex items-center gap-3">
            <div
              class="flex h-9 w-9 items-center justify-center rounded-full bg-sky-700 text-white text-[11px] font-semibold"
            >
              {{ worker.initials }}
            </div>

            <div>
              <p class="text-[12.5px] font-semibold text-stone-900">
                {{ worker.name }}
              </p>
              <p class="text-[11px] text-stone-500">
                {{ worker.role }}
              </p>
            </div>
          </div>

          <div
            v-if="selectedWorkerID === worker.userID"
            class="flex h-5 w-5 items-center justify-center rounded-full bg-emerald-600"
          >
            <Check :size="13" class="text-white" />
          </div>
        </button>

        <!-- No workers -->
        <div
          v-if="healthworkers.filter(
            (w) => w.status === 'Online'
          ).length === 0"
          class="rounded-xl bg-stone-50 px-4 py-5 text-center"
        >
          <p class="text-[12.5px] font-medium text-stone-600">
            No healthcare workers are currently available.
          </p>
          <p class="text-[11px] text-stone-400 mt-1">
            Only workers with Online status can be assigned.
          </p>
        </div>
      </div>
    </div>

    <!-- Footer -->
    <div
      class="flex items-center justify-end gap-3 px-6 py-4 border-t border-stone-200"
    >
      <button
        @click="closeAssignWorker"
        :disabled="assigningWorker"
        class="rounded-xl px-4 py-2.5 text-[13px] font-medium text-stone-600 hover:bg-stone-50 disabled:opacity-50"
      >
        Cancel
      </button>

      <button
        @click="assignWorker"
        :disabled="!selectedWorkerID || assigningWorker"
        class="rounded-xl px-5 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800 disabled:opacity-50"
      >
        {{ assigningWorker ? "Assigning..." : "Assign Worker" }}
      </button>
    </div>
  </div>
</div>
  <div
    class="flex min-h-screen w-full bg-stone-50 text-stone-900"
    style="font-family: 'Inter','Segoe UI',sans-serif;"
  >
    <StaffSidebar />

    <main class="flex-1 min-w-0">
      <StaffHeader />

      <div class="px-8 py-6 space-y-6">
        <!-- Page title + Queue QR shortcut -->
        <div class="flex items-center justify-between">
          <div>
            <h1 class="text-[20px] font-bold">Queue Management</h1>
            <p class="text-[12.5px] text-stone-500 mt-0.5">
              Today's operational clinic queue
            </p>
          </div>
          <div
            class="flex items-center gap-3 rounded-2xl border border-stone-200 bg-white px-4 py-3 shadow-sm"
          >
            <div class="flex h-11 w-11 items-center justify-center rounded-xl bg-emerald-50">
              <QrCode :size="20" class="text-emerald-700" />
            </div>
            <div>
              <p class="text-[12.5px] font-semibold">Today's Queue QR</p>
              <p class="text-[11px] text-stone-500">
                TODO: connect existing QueueQRSetting/QueueQRCode
              </p>
            </div>
          </div>
        </div>

        <!-- SUMMARY CARDS -->
        <QueueSummaryCards
          :queue="queue"
          :available-workers-count="availableWorkersCount"
        />

        <!-- TWO-COLUMN MIDDLE SECTION -->
        <div class="grid grid-cols-12 gap-6">
          <!-- LEFT: Today's Queue -->
          <div class="col-span-7">
            <QueueTable
  :queue="queue"
  :loading="loadingQueue"
  :error="queueError"
  :updating-queue-id="updatingStatus"
  @refresh="handleTableRefresh"
  @status-change="handleStatusChange"
  @view-patient="handleViewPatient"
  @assign-worker="openAssignWorker"
/>
          </div>

          <!-- RIGHT: Expected Patients + Due/Missed, stacked -->
          <div class="col-span-5 flex flex-col gap-6">
            <!-- Expected Patients -->
            <div class="rounded-2xl border border-stone-200 bg-white p-5 shadow-sm">
              <div class="flex items-center justify-between mb-3">
                <p class="text-[14px] font-semibold">Expected Patients</p>
                <span class="text-[11px] text-stone-500">
                  {{ expectedPatients.length }} today
                </span>
              </div>
              <div class="space-y-1.5 max-h-64 overflow-y-auto pr-1">
                <div
                  v-for="p in expectedPatients"
                  :key="p.id"
                  class="flex items-center justify-between rounded-xl px-3 py-2.5 bg-stone-50"
                >
                  <div class="min-w-0">
                    <p class="text-[12.5px] font-medium truncate">{{ p.child }}</p>
                    <p class="text-[11px] truncate text-stone-500">{{ p.time }}</p>
                  </div>
                  <button
                    @click="openExpectedDetail(p)"
                    class="p-1.5 rounded-lg hover:bg-stone-100 shrink-0"
                    title="View details"
                  >
                    <MoreHorizontal :size="15" class="text-stone-500" />
                  </button>
                </div>
                <p v-if="expectedPatients.length === 0" class="text-[12px] text-stone-500 py-2">
                  No expected patients today.
                </p>
              </div>
              <p class="text-[10.5px] text-stone-400 mt-3">
                TODO: confirm against backend — expected-patient endpoint not yet available.
              </p>
            </div>

            <!-- Due / Missed Vaccinations -->
            <div class="rounded-2xl border border-stone-200 bg-white p-5 shadow-sm flex-1">
              <div class="flex items-center justify-between mb-3">
                <p class="text-[14px] font-semibold">Due / Missed Vaccinations</p>
                <span class="text-[11px] text-stone-500">{{ dueMissed.length }} flagged</span>
              </div>
              <div class="space-y-1.5 max-h-64 overflow-y-auto pr-1">
                <div
                  v-for="d in dueMissed"
                  :key="d.id"
                  class="flex items-center justify-between rounded-xl px-3 py-2.5 bg-stone-50"
                >
                  <div class="min-w-0">
                    <p class="text-[12.5px] font-medium truncate">{{ d.child }}</p>
                    <p
                      class="text-[11px] truncate"
                      :class="
                        d.dueLabel === 'Today'
                          ? 'text-rose-700 font-medium'
                          : 'text-stone-500'
                      "
                    >
                      {{ d.dueLabel }}
                    </p>
                  </div>
                  <button
                    @click="openDueMissedDetail(d)"
                    class="p-1.5 rounded-lg hover:bg-stone-100 shrink-0"
                    title="View details"
                  >
                    <MoreHorizontal :size="15" class="text-stone-500" />
                  </button>
                </div>
                <p v-if="dueMissed.length === 0" class="text-[12px] text-stone-500 py-2">
                  No due or missed vaccinations.
                </p>
              </div>
              <p class="text-[10.5px] text-stone-400 mt-3">
                TODO: confirm against backend — due/missed calculation endpoint not yet available.
              </p>
            </div>
          </div>
        </div>

        <!-- ACTIVE HEALTHCARE WORKERS -->
        <div>
          <div class="flex items-center justify-between mb-3">
            <p class="text-[14px] font-semibold">Active Healthcare Workers</p>
            <p class="text-[10.5px] text-stone-400">
              TODO: confirm against backend — inspect existing healthcare-worker model before finalizing
            </p>
          </div>
          <div class="grid grid-cols-5 gap-4">
            <div
              v-for="w in healthworkers"
              :key="w.name"
              class="rounded-2xl border p-4 shadow-sm"
              :class="workerCardStyle[w.status].card"
            >
              <div class="flex items-center gap-3">
                <div
                  class="flex h-10 w-10 items-center justify-center rounded-full text-white text-[12px] font-semibold shrink-0"
                  :class="
  w.status === 'Offline'
    ? 'bg-stone-400'
    : 'bg-sky-700'
"
                >
                  {{ w.initials }}
                </div>
                <div class="min-w-0">
                  <p
                    class="text-[12.5px] font-semibold truncate"
                    :class="
                      w.status === 'Unavailable'
                        ? 'text-stone-500'
                        : ''
                    "
                  >
                    {{ w.name }}
                  </p>
                  <p class="text-[11px] truncate text-stone-500">{{ w.role }}</p>
                </div>
              </div>
              <div class="mt-3 flex items-center gap-2">
                <span
                  class="inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium"
                  :class="workerCardStyle[w.status].badge"
                >
                  {{ w.status }}
                </span>
                <span v-if="w.room !== '—'" class="text-[11px] text-stone-500">{{
                  w.room
                }}</span>
              </div>
              <p v-if="w.status === 'Occupied' && w.patient" class="text-[11px] mt-2 text-stone-500">
                With: <span class="text-stone-900 font-medium">{{ w.patient }}</span>
              </p>
              <p v-else-if="w.status === 'Online'" class="text-[11px] mt-2 text-emerald-700 font-medium">
                Available
              </p>
            </div>
          </div>
        </div>

        <!-- SEARCH PATIENT -->
        <div class="rounded-2xl border border-stone-200 bg-white p-5 shadow-sm">
          <p class="text-[14px] font-semibold mb-1">Search Patient</p>
          <p class="text-[11.5px] text-stone-500 mb-3">
            Confirm a child's schedule or vaccination status — this does not add them to the queue.
          </p>
          <div class="flex items-center gap-2 rounded-xl border border-stone-200 px-3 py-2.5 max-w-md">
            <Search :size="16" class="text-stone-400 shrink-0" />
            <input
              v-model="searchQuery"
              @keyup.enter="searchPatient"
              type="text"
              placeholder="Search by child name..."
              class="flex-1 text-[13px] outline-none placeholder:text-stone-400"
            />
            <button
              @click="searchPatient"
              class="text-[12px] font-semibold text-emerald-700 hover:underline shrink-0"
            >
              Search
            </button>
          </div>
          <p class="text-[10.5px] text-stone-400 mt-2">
            TODO: confirm against backend — real patient-search endpoint not yet available; currently checks placeholder Expected/Due-Missed lists only.
          </p>

          <div v-if="searchResult" class="mt-4 max-w-lg">
            <div
              v-if="searchResult.notFound"
              class="rounded-xl bg-stone-50 p-3.5 text-[12.5px] text-stone-500"
            >
              No match found for "{{ searchResult.query }}".
            </div>
            <div v-else class="rounded-xl border border-stone-200 p-3.5">
              <div class="flex items-start justify-between">
                <div>
                  <p class="text-[13px] font-semibold">{{ searchResult.child }}</p>
                  <p class="text-[11.5px] text-stone-500 mt-0.5">
                    Parent: {{ searchResult.parent }}
                  </p>
                  <p class="text-[11.5px] text-stone-500">Barangay: {{ searchResult.barangay }}</p>
                  <p class="text-[11.5px] text-stone-500">
                    Scheduled: {{ searchResult.scheduled }}
                    <template v-if="searchResult.time !== '—'">· {{ searchResult.time }}</template>
                  </p>
                </div>
                <button
                  @click="searchDetailOpen = !searchDetailOpen"
                  class="p-1.5 rounded-lg hover:bg-stone-100"
                  title="More details"
                >
                  <MoreHorizontal :size="16" class="text-stone-500" />
                </button>
              </div>
              <div v-if="searchDetailOpen" class="mt-3 pt-3 border-t border-stone-100 space-y-1.5">
                <p class="text-[12px] text-stone-600" v-for="v in searchResult.vaccines" :key="v.name">
                  <Syringe :size="12" class="inline mr-1 text-stone-400" />
                  {{ v.name }} · Dose {{ v.dose }}
                </p>
                <p class="text-[12px] text-stone-600">
                  Schedule status: {{ searchResult.scheduleStatus }}
                </p>
                <p class="text-[12px] text-stone-600">
                  Queue status: {{ searchResult.queueStatus }}
                </p>
                <button
                  v-if="searchResult.queueStatus !== 'In queue'"
                  @click="addToQueue('search', searchResult.raw)"
                  :disabled="addingToQueue === searchResult.raw.id"
                  class="mt-2 flex items-center gap-1.5 rounded-lg bg-emerald-700 text-white px-3 py-2 text-[12px] font-semibold hover:bg-emerald-800 disabled:opacity-50"
                >
                  <Check :size="14" /> Add to Queue
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Expected Patient detail modal -->
    <div
      v-if="expectedDetail"
      class="fixed inset-0 z-50 flex items-center justify-center bg-stone-900/40 px-4"
    >
      <div class="w-full max-w-md rounded-2xl bg-white shadow-xl">
        <div class="flex items-center justify-between px-6 py-4 border-b border-stone-200">
          <p class="text-[15px] font-semibold">Expected Patient</p>
          <button @click="closeExpectedDetail" class="p-1.5 rounded-lg hover:bg-stone-100">
            <X :size="18" class="text-stone-500" />
          </button>
        </div>
        <div class="p-6 space-y-3">
          <div>
            <p class="text-[16px] font-bold">{{ expectedDetail.child }}</p>
            <p class="text-[12px] text-stone-500 flex items-center gap-1.5 mt-1">
              <User :size="13" class="text-stone-400" /> {{ expectedDetail.parent }}
            </p>
            <p class="text-[12px] text-stone-500 flex items-center gap-1.5 mt-1">
              <MapPin :size="13" class="text-stone-400" /> Barangay {{ expectedDetail.barangay }}
            </p>
            <p class="text-[12px] text-stone-500 flex items-center gap-1.5 mt-1">
              <CalendarClock :size="13" class="text-stone-400" /> Scheduled
              {{ expectedDetail.time }}
            </p>
          </div>
          <div class="rounded-xl bg-stone-50 p-3.5 space-y-1.5">
            <p class="text-[11.5px] font-medium text-stone-500 mb-1">
              Today's scheduled vaccine(s)
            </p>
            <p v-for="v in expectedDetail.vaccines" :key="v.name" class="text-[13px] flex items-center gap-1.5">
              <Syringe :size="13" class="text-emerald-700" /> {{ v.name }} · Dose {{ v.dose }}
            </p>
          </div>
          <div class="flex items-center justify-between text-[12.5px]">
            <span class="text-stone-500">Schedule status</span
            ><span class="font-medium">{{ expectedDetail.scheduleStatus }}</span>
          </div>
          <div class="flex items-center justify-between text-[12.5px]">
            <span class="text-stone-500">Current queue status</span
            ><span class="font-medium">{{ expectedDetail.queueStatus }}</span>
          </div>
        </div>
        <div class="flex items-center justify-end gap-3 px-6 py-4 border-t border-stone-200">
          <button
            @click="closeExpectedDetail"
            class="rounded-xl px-4 py-2.5 text-[13px] font-medium text-stone-600 hover:bg-stone-50"
          >
            Close
          </button>
          <button
            v-if="expectedDetail.queueStatus !== 'In queue'"
            @click="addToQueue('expected', expectedDetail)"
            :disabled="addingToQueue === expectedDetail.id"
            class="rounded-xl px-5 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800 shadow-sm disabled:opacity-50"
          >
            Add to Queue
          </button>
        </div>
      </div>
    </div>

    <!-- Due/Missed detail modal -->
    <div
      v-if="dueMissedDetail"
      class="fixed inset-0 z-50 flex items-center justify-center bg-stone-900/40 px-4"
    >
      <div class="w-full max-w-md rounded-2xl bg-white shadow-xl">
        <div class="flex items-center justify-between px-6 py-4 border-b border-stone-200">
          <p class="text-[15px] font-semibold">Due / Missed Vaccination</p>
          <button @click="closeDueMissedDetail" class="p-1.5 rounded-lg hover:bg-stone-100">
            <X :size="18" class="text-stone-500" />
          </button>
        </div>
        <div class="p-6 space-y-3">
          <div>
            <p class="text-[16px] font-bold">{{ dueMissedDetail.child }}</p>
            <p class="text-[12px] text-stone-500 flex items-center gap-1.5 mt-1">
              <User :size="13" class="text-stone-400" /> {{ dueMissedDetail.parent }}
            </p>
            <p class="text-[12px] text-stone-500 flex items-center gap-1.5 mt-1">
              <MapPin :size="13" class="text-stone-400" /> Barangay {{ dueMissedDetail.barangay }}
            </p>
          </div>
          <div class="rounded-xl bg-rose-50 p-3.5 space-y-1.5">
            <p class="text-[13px] flex items-center gap-1.5 text-rose-800 font-medium">
              <Syringe :size="13" /> {{ dueMissedDetail.vaccine }} · Dose {{ dueMissedDetail.dose }}
            </p>
            <p class="text-[12px] text-rose-700">Due: {{ dueMissedDetail.dueDate }}</p>
            <p class="text-[12px] text-rose-700">{{ dueMissedDetail.overdue }}</p>
          </div>
          <div class="flex items-center justify-between text-[12.5px]">
            <span class="text-stone-500">Current queue status</span
            ><span class="font-medium">{{ dueMissedDetail.queueStatus }}</span>
          </div>
        </div>
        <div class="flex items-center justify-end gap-3 px-6 py-4 border-t border-stone-200">
          <button
            @click="closeDueMissedDetail"
            class="rounded-xl px-4 py-2.5 text-[13px] font-medium text-stone-600 hover:bg-stone-50"
          >
            Close
          </button>
          <button
            v-if="dueMissedDetail.queueStatus !== 'In queue'"
            @click="addToQueue('dueMissed', dueMissedDetail)"
            :disabled="addingToQueue === dueMissedDetail.id"
            class="rounded-xl px-5 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800 shadow-sm disabled:opacity-50"
          >
            Add to Queue
          </button>
        </div>
      </div>
    </div>
  </div>
</template>