<script setup>
import { ref, computed, onMounted, onUnmounted } from "vue";
import {
  Home, Users, Syringe, Package, Bell, BarChart2, FileText, Settings,
  LogOut, Search, QrCode, UserPlus, CalendarDays, ChevronLeft, ChevronRight,
  Clock, CheckCircle2, AlertTriangle, Activity, MoreHorizontal,
  ClipboardList, Eye, UserCog, DoorOpen, TrendingUp, TrendingDown,
} from "lucide-vue-next";



const collapsed = ref(false);

const navItems = [
  { icon: Home, label: "Dashboard", active: true },
  { icon: Users, label: "Patient Records" },
  { icon: Syringe, label: "Vaccine Schedule" },
  { icon: Package, label: "Inventory" },
  { icon: ClipboardList, label: "Queue Management" },
  { icon: Bell, label: "Notifications" },
  { icon: BarChart2, label: "Clinic Reports" },
  { icon: Settings, label: "Settings" },
];

const waitingCount = computed(() =>
  queue.value.filter(q => q.status === "Waiting").length
);

const readyCount = computed(() =>
  queue.value.filter(q => q.status === "Ready").length
);

const inProgressCount = computed(() =>
  queue.value.filter(q => q.status === "In Progress").length
);

const completedCount = computed(() =>
  queue.value.filter(q => q.status === "Completed").length
);

const lateCount = computed(() =>
  queue.value.filter(q => q.status === "Late").length
);

const summaryCards = computed(() => [
  {
    label: "Today's Appointments",
    value: "—",
    trend: "Coming next",
    up: null,
    icon: CalendarDays,
    tint: "text-emerald-700",
    tintBg: "bg-emerald-50",
  },
  {
    label: "Patients Waiting",
    value: queue.value.filter(q => q.status === "Waiting").length,
    trend: "today",
    up: null,
    icon: Clock,
    tint: "text-amber-700",
    tintBg: "bg-amber-50",
  },
  {
    label: "In Progress",
    value: queue.value.filter(q => q.status === "In Progress").length,
    trend: "today",
    up: null,
    icon: Activity,
    tint: "text-sky-700",
    tintBg: "bg-sky-50",
  },
  {
    label: "Completed Today",
    value: queue.value.filter(q => q.status === "Completed").length,
    trend: "today",
    up: null,
    icon: CheckCircle2,
    tint: "text-emerald-700",
    tintBg: "bg-emerald-50",
  },
  {
    label: "Late Patients",
    value: queue.value.filter(q => q.status === "Late").length,
    trend: "today",
    up: null,
    icon: AlertTriangle,
    tint: "text-rose-700",
    tintBg: "bg-rose-50",
  },
  {
    label: "Available Healthworkers",
    value: "—",
    trend: "Coming next",
    up: null,
    icon: UserCog,
    tint: "text-sky-700",
    tintBg: "bg-sky-50",
  },
]);

const quickActions = [
  { icon: UserPlus, label: "Register Child" },
  { icon: QrCode, label: "Scan Queue QR" },
  { icon: UserPlus, label: "Add Walk-in Patient" },
  { icon: Search, label: "Search Patient" },
  { icon: CalendarDays, label: "View Today's Queue" },
];

const statusStyle = {
  Waiting: "bg-amber-50 text-amber-700",
  Ready: "bg-sky-50 text-sky-700",
  "In Progress": "bg-violet-50 text-violet-700",
  Completed: "bg-emerald-50 text-emerald-700",
  Late: "bg-rose-50 text-rose-700",
};
const statusDot = {
  Waiting: "bg-amber-500",
  Ready: "bg-sky-500",
  "In Progress": "bg-violet-500",
  Completed: "bg-emerald-600",
  Late: "bg-rose-600",
};

const queue = ref([]);
const loadingQueue = ref(false);
const queueError = ref(null);

const expected = [
  { child: "Julian Reyes", time: "9:15 AM", parent: "Mark Reyes", status: "Checked In" },
  { child: "Ava Dizon", time: "9:15 AM", parent: "Carmen Dizon", status: "Checked In" },
  { child: "Noah Bautista", time: "9:30 AM", parent: "Ramon Bautista", status: "Checked In" },
  { child: "Zoe Manalo", time: "10:00 AM", parent: "Paolo Manalo", status: "Not Arrived" },
  { child: "Leo Fernandez", time: "10:15 AM", parent: "Dina Fernandez", status: "Not Arrived" },
  { child: "Sophia Ramos", time: "8:45 AM", parent: "Ana Ramos", status: "Late" },
];
const arrivalStyle = {
  "Checked In": "bg-emerald-50 text-emerald-700",
  "Not Arrived": "bg-stone-100 text-stone-600",
  Late: "bg-rose-50 text-rose-700",
};

const healthworkers = [
  { name: "Dr. Elena Cruz", role: "Pediatrician", status: "Busy", patient: "Sophia Ramos", initials: "EC" },
  { name: "Nurse Bea Fernandez", role: "Vaccination Nurse", status: "Busy", patient: "Mika Santos", initials: "BF" },
  { name: "Nurse Kim Uy", role: "Vaccination Nurse", status: "Available", patient: null, initials: "KU" },
  { name: "Dr. Marco Villar", role: "Pediatrician", status: "Available", patient: null, initials: "MV" },
  { name: "Nurse Ida Ocampo", role: "Triage Nurse", status: "Unavailable", patient: null, initials: "IO" },
];
const workerStyle = {
  Available: "bg-emerald-50 text-emerald-700",
  Busy: "bg-amber-50 text-amber-700",
  Unavailable: "bg-stone-100 text-stone-600",
};

const stockAlerts = [
  { name: "MMR", detail: "20 doses remaining", note: "Expires in 5 days", level: "Critical" },
  { name: "Pentavalent", detail: "15 doses remaining", note: "Low stock", level: "Low" },
  { name: "BCG", detail: "60 doses remaining", note: "Expires in 30 days", level: "Watch" },
];
const stockStyle = {
  Critical: "bg-rose-50 text-rose-700",
  Low: "bg-amber-50 text-amber-700",
  Watch: "bg-sky-50 text-sky-700",
};
const stockNoteColor = {
  Critical: "text-rose-700",
  Low: "text-amber-700",
  Watch: "text-sky-700",
};

const activities = [
  { icon: UserPlus, text: "Zoe Manalo registered as a walk-in patient.", time: "3 min ago" },
  { icon: DoorOpen, text: "Julian Reyes checked in and assigned to Room 1.", time: "10 min ago" },
  { icon: UserCog, text: "Mika Santos assigned to Nurse Bea Fernandez.", time: "16 min ago" },
  { icon: Syringe, text: "Vaccination completed for Gabriel Torres.", time: "24 min ago" },
  { icon: FileText, text: "Parent contact info updated for Ava Dizon.", time: "38 min ago" },
  { icon: AlertTriangle, text: "Sophia Ramos flagged as late for her 8:45 AM slot.", time: "51 min ago" },
];

/* Queue overview donut chart */

const donutData = computed(() => [
  {
    name: "Waiting",
    value: queue.value.filter(q => q.status === "Waiting").length,
    color: "#d97706",
    dot: "bg-amber-500",
  },
  {
    name: "Ready",
    value: queue.value.filter(q => q.status === "Ready").length,
    color: "#0284c7",
    dot: "bg-sky-500",
  },
  {
    name: "In Progress",
    value: queue.value.filter(q => q.status === "In Progress").length,
    color: "#7c3aed",
    dot: "bg-violet-500",
  },
  {
    name: "Completed",
    value: queue.value.filter(q => q.status === "Completed").length,
    color: "#059669",
    dot: "bg-emerald-600",
  },
  {
    name: "Late",
    value: queue.value.filter(q => q.status === "Late").length,
    color: "#e11d48",
    dot: "bg-rose-600",
  },
]);

const donutTotal = computed(() =>
  donutData.value.reduce((sum, item) => sum + item.value, 0)
);

const donutGradient = computed(() => {
  if (donutTotal.value === 0) {
    return "conic-gradient(#e7e5e4 0deg 360deg)";
  }

  let start = 0;

  const stops = donutData.value.map((d) => {
    const end =
      start + (d.value / donutTotal.value) * 360;

    const stop =
      `${d.color} ${start}deg ${end}deg`;

    start = end;

    return stop;
  });

  return `conic-gradient(${stops.join(", ")})`;
});


const API_BASE = "http://localhost:57147/api";

const loadQueue = async () => {
  loadingQueue.value = true;
  queueError.value = null;

  try {
    const response = await fetch(`${API_BASE}/Queue`);

    if (!response.ok) {
      throw new Error(`Failed to load queue. Status: ${response.status}`);
    }

    const data = await response.json();

queue.value = data.map((q) => ({
  queueID: q.queueID,
  queueNumber: q.queueNumber,

  no: `Q-${String(q.queueNumber).padStart(3, "0")}`,

  child:
    q.children?.map(c => c.name).join(", ") || "—",

  parent:
    q.requestBy || "—",

  time: "—",
  worker: "—",
  room: "—",

  status:
    q.status || "Waiting",
}));
  } catch (error) {
    console.error("Queue loading error:", error);
    queueError.value = error.message;
  } finally {
    loadingQueue.value = false;
  }
};
let queueRefreshInterval = null;

onMounted(() => {
  loadQueue();

  queueRefreshInterval = setInterval(() => {
    loadQueue();
  }, 5000);
});

onUnmounted(() => {
  if (queueRefreshInterval) {
    clearInterval(queueRefreshInterval);
  }
});
</script>

<template>
  <div class="flex min-h-screen w-full bg-stone-50 text-stone-900" style="font-family: 'Inter','Segoe UI',sans-serif;">
    <!-- ---------------- Sidebar ---------------- -->
    <aside
      class="flex flex-col shrink-0 border-r border-stone-200 bg-white transition-all duration-200"
      :class="collapsed ? 'w-19' : 'w-66'"
    >
      <div class="flex items-center gap-3 px-5 py-5 border-b border-stone-200">
        <div class="flex h-9 w-9 items-center justify-center rounded-xl text-white font-bold text-sm shrink-0 bg-linear-to-br from-emerald-600 to-emerald-800">
          A
        </div>
        <div v-if="!collapsed" class="leading-tight">
          <p class="font-semibold text-[15px]">Aruga Pediatric System</p>
          <p class="text-[11px] text-stone-500">Staff Portal</p>
        </div>
      </div>

      <nav class="flex-1 px-3 py-4 space-y-1 overflow-y-auto">
        <button
          v-for="item in navItems"
          :key="item.label"
          class="flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-[13.5px] font-medium transition-colors"
          :class="item.active ? 'bg-emerald-50 text-emerald-800' : 'text-stone-500 hover:bg-stone-50'"
        >
          <component :is="item.icon" :size="18" :stroke-width="2" />
          <span v-if="!collapsed">{{ item.label }}</span>
        </button>
      </nav>

      <div class="border-t border-stone-200 px-3 py-4">
        <div class="flex items-center gap-3 rounded-xl px-2 py-2">
          <div class="flex h-9 w-9 items-center justify-center rounded-full text-white text-xs font-semibold shrink-0 bg-sky-700">
            MP
          </div>
          <div v-if="!collapsed" class="leading-tight">
            <p class="text-[13px] font-semibold">Marielle Pascual</p>
            <p class="text-[11px] text-stone-500">Clinic Staff</p>
          </div>
        </div>
        <button class="mt-2 flex w-full items-center gap-3 rounded-xl px-3 py-2.5 text-[13px] font-medium text-rose-700 hover:bg-rose-50">
          <LogOut :size="17" />
          <span v-if="!collapsed">Log out</span>
        </button>
      </div>

      <button
        @click="collapsed = !collapsed"
        class="mx-auto mb-3 flex h-7 w-7 items-center justify-center rounded-full border border-stone-200 text-stone-500"
      >
        <component :is="collapsed ? ChevronRight : ChevronLeft" :size="14" />
      </button>
    </aside>

    <!-- ---------------- Main ---------------- -->
    <main class="flex-1 min-w-0">
      <!-- Top bar -->
      <header class="flex items-center justify-between px-8 py-5 border-b border-stone-200 bg-white">
        <div>
          <h1 class="text-[22px] font-bold">Staff Dashboard</h1>
          <p class="text-[12.5px] mt-0.5 text-stone-500">Aruga / Dashboard</p>
        </div>
        <div class="flex items-center gap-3">
          <button class="relative flex h-9 w-9 items-center justify-center rounded-full bg-stone-50">
            <Bell :size="17" class="text-stone-500" />
            <span class="absolute top-1.5 right-2 h-1.5 w-1.5 rounded-full bg-rose-600" />
          </button>
          <button class="flex h-9 w-9 items-center justify-center rounded-full bg-stone-50">
            <Settings :size="17" class="text-stone-500" />
          </button>
          <div class="flex h-9 w-9 items-center justify-center rounded-full text-white text-xs font-semibold bg-emerald-700">
            MP
          </div>
        </div>
      </header>

      <div class="px-8 py-6 space-y-6">
        <!-- Summary cards -->
        <div class="grid grid-cols-6 gap-4">
          <div
            v-for="c in summaryCards"
            :key="c.label"
            class="rounded-2xl border border-stone-200 bg-white p-4 shadow-sm"
          >
            <div class="flex items-center justify-between">
              <p class="text-[10.5px] font-semibold uppercase tracking-wide text-stone-500">{{ c.label }}</p>
              <div class="flex h-7 w-7 items-center justify-center rounded-lg shrink-0" :class="c.tintBg">
                <component :is="c.icon" :size="14" :class="c.tint" />
              </div>
            </div>
            <p class="text-[26px] font-bold mt-2">{{ c.value }}</p>
            <div class="flex items-center gap-1 mt-1">
              <TrendingUp v-if="c.up === true" :size="12" class="text-emerald-700" />
              <TrendingDown v-if="c.up === false" :size="12" class="text-rose-700" />
              <span
                class="text-[11px] font-medium"
                :class="c.up === true ? 'text-emerald-700' : c.up === false ? 'text-rose-700' : 'text-stone-500'"
              >
                {{ c.trend }}
              </span>
            </div>
          </div>
        </div>

        <!-- Quick actions -->
        <div class="rounded-2xl border border-stone-200 bg-white p-5 shadow-sm">
          <p class="text-[14px] font-semibold mb-3">Quick Actions</p>
          <div class="flex flex-wrap gap-3">
            <button
              v-for="a in quickActions"
              :key="a.label"
              class="flex items-center gap-2 rounded-xl px-4 py-2.5 text-[13px] font-medium border border-stone-200 hover:bg-stone-50 transition-colors"
            >
              <component :is="a.icon" :size="16" class="text-emerald-700" />
              {{ a.label }}
            </button>
          </div>
        </div>

        <!-- Queue + side column -->
        <div class="grid grid-cols-12 gap-6">
          <!-- Today's Queue -->
          <div class="col-span-8 rounded-2xl border border-stone-200 bg-white shadow-sm overflow-hidden">
            
            <div class="flex items-center justify-between px-5 py-4 border-b border-stone-200">
  <div>
    <p class="text-[14px] font-semibold">Today's Queue</p>

    <span class="text-[12px] text-stone-500">
      {{ queue.length }} patients in queue
    </span>
  </div>

  <button
    @click="loadQueue"
    :disabled="loadingQueue"
    class="flex items-center gap-2 rounded-lg border border-stone-200 px-3 py-2 text-[12px] font-medium text-stone-600 hover:bg-stone-50 disabled:opacity-50"
  >
    <RefreshCw
      :size="14"
      :class="{ 'animate-spin': loadingQueue }"
    />

    Refresh
  </button>
</div>
            <div class="overflow-x-auto">
              <table class="w-full text-[13px]">
                <thead>
                  <tr class="bg-stone-50">
                    <th
                      v-for="h in ['Queue No.', 'Child', 'Parent', 'Time', 'Healthworker', 'Room', 'Status', '']"
                      :key="h"
                      class="text-left font-semibold px-5 py-3 whitespace-nowrap text-[11px] uppercase tracking-wide text-stone-500"
                    >
                      {{ h }}
                    </th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="q in queue" :key="q.id" class="border-t border-stone-200">
                    <td class="px-5 py-3 font-medium whitespace-nowrap">{{ q.no }}</td>
                    <td class="px-5 py-3 whitespace-nowrap">{{ q.child }}</td>
                    <td class="px-5 py-3 whitespace-nowrap text-stone-500">{{ q.parent }}</td>
                    <td class="px-5 py-3 whitespace-nowrap text-stone-500">{{ q.time }}</td>
                    <td class="px-5 py-3 whitespace-nowrap">{{ q.worker }}</td>
                    <td class="px-5 py-3 whitespace-nowrap text-stone-500">{{ q.room }}</td>
                    <td class="px-5 py-3 whitespace-nowrap">
                      <span
                        class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium"
                        :class="statusStyle[q.status]"
                      >
                        <span class="h-1.5 w-1.5 rounded-full" :class="statusDot[q.status]" />
                        {{ q.status }}
                      </span>
                    </td>
                    <td class="px-5 py-3 whitespace-nowrap">
                      <div class="flex items-center gap-1">
                        <button class="p-1.5 rounded-lg hover:bg-stone-100" title="View Patient">
                          <Eye :size="15" class="text-stone-500" />
                        </button>
                        <button class="p-1.5 rounded-lg hover:bg-stone-100" title="Assign Healthworker">
                          <UserCog :size="15" class="text-stone-500" />
                        </button>
                        <button class="p-1.5 rounded-lg hover:bg-stone-100" title="Assign Room">
                          <DoorOpen :size="15" class="text-stone-500" />
                        </button>
                        <button class="p-1.5 rounded-lg hover:bg-stone-100" title="More">
                          <MoreHorizontal :size="15" class="text-stone-500" />
                        </button>
                      </div>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Right column: donut + expected patients -->
          <div class="col-span-4 flex flex-col gap-6">
            <div class="rounded-2xl border border-stone-200 bg-white p-5 shadow-sm">
              <p class="text-[14px] font-semibold mb-2">Queue Overview</p>
              <div class="h-40 relative flex items-center justify-center">
                <div class="h-32 w-32 rounded-full" :style="{ background: donutGradient }">
                  <div class="h-full w-full flex items-center justify-center">
                    <div class="h-19 w-19 rounded-full bg-white flex flex-col items-center justify-center">
                      <p class="text-[20px] font-bold">{{ donutTotal }}</p>
                      <p class="text-[10px] text-stone-500">Total today</p>
                    </div>
                  </div>
                </div>
              </div>
              <div class="grid grid-cols-2 gap-x-3 gap-y-1.5 mt-3">
                <div v-for="d in donutData" :key="d.name" class="flex items-center gap-1.5 text-[11.5px]">
                  <span class="h-2 w-2 rounded-full" :class="d.dot" />
                  <span class="text-stone-500">{{ d.name }}</span>
                  <span class="font-semibold ml-auto">{{ d.value }}</span>
                </div>
              </div>
            </div>

            <div class="rounded-2xl border border-stone-200 bg-white p-5 shadow-sm flex-1">
              <div class="flex items-center justify-between mb-3">
                <p class="text-[14px] font-semibold">Expected Patients</p>
                <span class="text-[11px] text-stone-500">{{ expected.length }} today</span>
              </div>
              <div class="space-y-1 max-h-70 overflow-y-auto pr-1">
                <div
                  v-for="p in expected"
                  :key="p.child"
                  class="flex items-center justify-between rounded-xl px-3 py-2.5 bg-stone-50"
                >
                  <div class="min-w-0">
                    <p class="text-[12.5px] font-medium truncate">{{ p.child }}</p>
                    <p class="text-[11px] truncate text-stone-500">{{ p.time }} · {{ p.parent }}</p>
                  </div>
                  <span
                    class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium shrink-0"
                    :class="arrivalStyle[p.status]"
                  >
                    {{ p.status }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Available Healthworkers -->
        <div>
          <p class="text-[14px] font-semibold mb-3">Available Healthworkers</p>
          <div class="grid grid-cols-5 gap-4">
            <div
              v-for="w in healthworkers"
              :key="w.name"
              class="rounded-2xl border border-stone-200 bg-white p-4 shadow-sm"
            >
              <div class="flex items-center gap-3">
                <div class="flex h-10 w-10 items-center justify-center rounded-full text-white text-[12px] font-semibold shrink-0 bg-sky-700">
                  {{ w.initials }}
                </div>
                <div class="min-w-0">
                  <p class="text-[12.5px] font-semibold truncate">{{ w.name }}</p>
                  <p class="text-[11px] truncate text-stone-500">{{ w.role }}</p>
                </div>
              </div>
              <div class="mt-3">
                <span
                  class="inline-flex items-center rounded-full px-2.5 py-1 text-xs font-medium"
                  :class="workerStyle[w.status]"
                >
                  {{ w.status }}
                </span>
              </div>
              <p v-if="w.patient" class="text-[11px] mt-2 text-stone-500">
                With: <span class="text-stone-900">{{ w.patient }}</span>
              </p>
            </div>
          </div>
        </div>

        <!-- Stock alerts + activities -->
        <div class="grid grid-cols-12 gap-6">
          <div class="col-span-4 rounded-2xl border border-stone-200 bg-white p-5 shadow-sm">
            <p class="text-[14px] font-semibold mb-3">Vaccine Stock Alerts</p>
            <div class="space-y-2.5">
              <div
                v-for="s in stockAlerts"
                :key="s.name"
                class="flex items-center justify-between rounded-xl px-3 py-2.5 bg-stone-50"
              >
                <div class="min-w-0">
                  <p class="text-[12.5px] font-semibold">{{ s.name }}</p>
                  <p class="text-[11px] text-stone-500">{{ s.detail }}</p>
                  <p class="text-[11px]" :class="stockNoteColor[s.level]">{{ s.note }}</p>
                </div>
                <span
                  class="text-[10.5px] font-semibold px-2 py-1 rounded-full shrink-0"
                  :class="stockStyle[s.level]"
                >
                  {{ s.level }}
                </span>
              </div>
            </div>
          </div>

          <div class="col-span-8 rounded-2xl border border-stone-200 bg-white p-5 shadow-sm">
            <p class="text-[14px] font-semibold mb-3">Recent Clinic Activities</p>
            <div class="space-y-3 max-h-[260px] overflow-y-auto pr-1">
              <div v-for="(a, i) in activities" :key="i" class="flex items-start gap-3">
                <div class="flex h-8 w-8 items-center justify-center rounded-full shrink-0 bg-emerald-50">
                  <component :is="a.icon" :size="14" class="text-emerald-800" />
                </div>
                <div class="flex-1 min-w-0 flex items-start justify-between gap-3">
                  <p class="text-[12.5px]">{{ a.text }}</p>
                  <span class="text-[11px] whitespace-nowrap text-stone-500">{{ a.time }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>
  </div>
</template>