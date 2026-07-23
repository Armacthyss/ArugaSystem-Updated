<script setup>
import { ref, computed } from "vue";
import {
  Home, Users, Syringe, Package, Bell, BarChart2, Settings, LogOut,
  ClipboardList, ChevronLeft, ChevronRight, Search, Filter, ChevronDown,
  Download, Eye, UserCog, Printer, MoreHorizontal,
  List, CalendarDays, X, Clock, CheckCircle2, AlertTriangle, XCircle,
  ChevronUp, ArrowLeft, Check, Square, CheckSquare, User, Phone,
} from "lucide-vue-next";

/* ---------------------------------------------------------
   Aruga Pediatric System — Vaccine Schedule (Vue 3 + Tailwind)
   Same design language as the Staff Portal: sidebar, top bar,
   card system, table system, radii, shadows, green healthcare
   palette. Table now groups by appointment (one row per child
   visit, covering all vaccines due that day) instead of one
   row per vaccine.
--------------------------------------------------------- */

const collapsed = ref(false);

const navItems = [
  { icon: Home, label: "Dashboard" },
  { icon: Users, label: "Patient Records" },
  { icon: Syringe, label: "Vaccine Schedule", active: true },
  { icon: Package, label: "Inventory" },
  { icon: ClipboardList, label: "Queue Management" },
  { icon: Bell, label: "Notifications" },
  { icon: BarChart2, label: "Clinic Reports" },
  { icon: Settings, label: "Settings" },
];

const summaryCards = [
  { label: "Vaccinations Due Today", value: "23", tint: "text-emerald-700", tintBg: "bg-emerald-50", icon: Syringe },
  { label: "Upcoming This Week", value: "87", tint: "text-sky-700", tintBg: "bg-sky-50", icon: CalendarDays },
  { label: "Overdue Vaccinations", value: "14", tint: "text-rose-700", tintBg: "bg-rose-50", icon: AlertTriangle },
  { label: "Completed Today", value: "16", tint: "text-emerald-700", tintBg: "bg-emerald-50", icon: CheckCircle2 },
  { label: "Tomorrow's Schedule", value: "19", tint: "text-sky-700", tintBg: "bg-sky-50", icon: Clock },
  { label: "Children Scheduled This Month", value: "412", tint: "text-emerald-700", tintBg: "bg-emerald-50", icon: Users },
];

/* View toggle */
const view = ref("list"); // "list" | "calendar"

/* Toolbar state */
const searchByOptions = ["Child Name", "Parent Name", "Patient ID"];
const searchBy = ref("Child Name");
const searchQuery = ref("");
const scheduleStatusFilter = ref("All");
const vaccineFilter = ref("All Vaccines");
const ageGroupFilter = ref("All Ages");

/* Appointment-level status (not per-vaccine) */
const statusStyle = {
  Waiting: "bg-amber-50 text-amber-700",
  "In Progress": "bg-sky-50 text-sky-700",
  Completed: "bg-emerald-50 text-emerald-700",
  Delayed: "bg-rose-50 text-rose-700",
  Cancelled: "bg-stone-100 text-stone-500",
};
const statusDot = {
  Waiting: "bg-amber-500",
  "In Progress": "bg-sky-500",
  Completed: "bg-emerald-600",
  Delayed: "bg-rose-600",
  Cancelled: "bg-stone-400",
};

/* Each appointment = one child's visit, with all vaccines due that day */
const appointments = ref([
  {
    queueNo: "Q-014", id: "PT-10214", child: "Mika Santos", age: "1y 3m", parent: "Liza Santos", contact: "0917 555 0142",
    due: "Jul 12, 2026", worker: "Dr. Elena Cruz", room: "Room 1", status: "In Progress",
    vaccines: [
      { name: "MMR", dose: "Dose 2", done: true },
      { name: "OPV", dose: "Dose 3", done: false },
    ],
  },
  {
    queueNo: "Q-015", id: "PT-10215", child: "Julian Reyes", age: "2y 1m", parent: "Mark Reyes", contact: "0917 555 0198",
    due: "Jul 12, 2026", worker: "Nurse Bea Fernandez", room: "Room 2", status: "Completed",
    vaccines: [
      { name: "Pentavalent", dose: "Dose 3", done: true },
      { name: "OPV", dose: "Dose 3", done: true },
      { name: "IPV", dose: "—", done: true },
      { name: "PCV", dose: "Dose 3", done: true },
    ],
  },
  {
    queueNo: "Q-016", id: "PT-10216", child: "Ava Dizon", age: "8m", parent: "Carmen Dizon", contact: "0917 555 0231",
    due: "Jun 30, 2026", worker: "Nurse Kim Uy", room: "Room 3", status: "Delayed",
    vaccines: [
      { name: "Pentavalent", dose: "Dose 2", done: false },
      { name: "OPV", dose: "Dose 2", done: false },
    ],
  },
  {
    queueNo: "Q-017", id: "PT-10217", child: "Noah Bautista", age: "4y", parent: "Ramon Bautista", contact: "0917 555 0276",
    due: "Jul 14, 2026", worker: "Dr. Elena Cruz", room: "Room 1", status: "Waiting",
    vaccines: [
      { name: "DPT Booster", dose: "Booster", done: false },
    ],
  },
  {
    queueNo: "Q-018", id: "PT-10218", child: "Sophia Ramos", age: "6m", parent: "Ana Ramos", contact: "0917 555 0309",
    due: "Jul 14, 2026", worker: "—", room: "—", status: "Waiting",
    vaccines: [
      { name: "Pentavalent", dose: "Dose 1", done: false },
      { name: "OPV", dose: "Dose 1", done: false },
      { name: "IPV", dose: "—", done: false },
    ],
  },
  {
    queueNo: "Q-020", id: "PT-10220", child: "Zoe Manalo", age: "3m", parent: "Paolo Manalo", contact: "0917 555 0333",
    due: "Jul 12, 2026", worker: "Nurse Kim Uy", room: "Room 3", status: "Cancelled",
    vaccines: [
      { name: "BCG", dose: "Dose 1", done: false },
    ],
  },
  {
    queueNo: "Q-021", id: "PT-10222", child: "Ella Cruz", age: "5m", parent: "Vic Cruz", contact: "0917 555 0361",
    due: "Jul 13, 2026", worker: "Nurse Bea Fernandez", room: "Room 2", status: "Waiting",
    vaccines: [
      { name: "Hepatitis B", dose: "Dose 2", done: false },
      { name: "Pentavalent", dose: "Dose 1", done: false },
    ],
  },
]);

function progressOf(appt) {
  const done = appt.vaccines.filter((v) => v.done).length;
  return { done, total: appt.vaccines.length };
}

/* Keep an appointment's status in sync with its checklist */
function recalcStatus(appt) {
  if (appt.status === "Cancelled") return;
  const { done, total } = progressOf(appt);
  if (done === 0) appt.status = appt.status === "Delayed" ? "Delayed" : "Waiting";
  else if (done === total) appt.status = "Completed";
  else appt.status = "In Progress";
}

const filteredAppointments = computed(() => {
  return appointments.value.filter((a) => {
    const q = searchQuery.value.trim().toLowerCase();
    if (q) {
      if (searchBy.value === "Patient ID" && !a.id.toLowerCase().includes(q)) return false;
      if (searchBy.value === "Parent Name" && !a.parent.toLowerCase().includes(q)) return false;
      if (searchBy.value === "Child Name" && !a.child.toLowerCase().includes(q)) return false;
    }
    if (scheduleStatusFilter.value === "Overdue" && a.status !== "Delayed") return false;
    if (scheduleStatusFilter.value === "Completed" && a.status !== "Completed") return false;
    if (vaccineFilter.value !== "All Vaccines" && !a.vaccines.some((v) => v.name === vaccineFilter.value)) return false;
    return true;
  });
});

/* Expand / collapse rows */
const expandedRows = ref(new Set());
function toggleExpand(queueNo) {
  const next = new Set(expandedRows.value);
  if (next.has(queueNo)) next.delete(queueNo);
  else next.add(queueNo);
  expandedRows.value = next;
}

const actionsMenuOpenFor = ref(null);
function toggleActionsMenu(id) {
  actionsMenuOpenFor.value = actionsMenuOpenFor.value === id ? null : id;
}

/* Right sidebar data */
const todaysSummary = [
  { label: "Appointments", value: 24, icon: CalendarDays, tint: "text-sky-700" },
  { label: "Completed", value: 16, icon: CheckCircle2, tint: "text-emerald-700" },
  { label: "Waiting", value: 6, icon: Clock, tint: "text-amber-700" },
  { label: "Late", value: 2, icon: AlertTriangle, tint: "text-rose-700" },
  { label: "Cancelled", value: 1, icon: XCircle, tint: "text-stone-500" },
];

const upcomingVaccines = [
  { vaccine: "Pentavalent", count: 12, window: "Next 7 days" },
  { vaccine: "OPV", count: 9, window: "Next 7 days" },
  { vaccine: "MMR", count: 7, window: "Next 7 days" },
  { vaccine: "DPT Booster", count: 5, window: "Next 7 days" },
];

const overdueList = computed(() =>
  appointments.value.filter((a) => a.status === "Delayed").slice(0, 5)
);

/* Vaccine distribution (simple bar chart, no chart library) */
const distribution = [
  { name: "BCG", value: 58, color: "bg-emerald-600" },
  { name: "Hepatitis B", value: 74, color: "bg-sky-600" },
  { name: "Pentavalent", value: 96, color: "bg-emerald-600" },
  { name: "OPV", value: 88, color: "bg-sky-600" },
  { name: "IPV", value: 41, color: "bg-emerald-600" },
  { name: "MMR", value: 63, color: "bg-sky-600" },
  { name: "Booster", value: 29, color: "bg-emerald-600" },
];
const distMax = Math.max(...distribution.map((d) => d.value));

/* ---------------- Calendar view ---------------- */
const calendarMonthLabel = "July 2026";
const daysInMonth = 31;
const firstWeekday = 3; // Jul 1, 2026 is a Wednesday (0 = Sunday)

const dueCountByDay = computed(() => {
  const map = {};
  appointments.value.forEach((a) => {
    const match = a.due.match(/Jul (\d+), 2026/);
    if (match) {
      const day = parseInt(match[1], 10);
      map[day] = (map[day] || 0) + 1;
    }
  });
  return map;
});

const calendarCells = computed(() => {
  const cells = [];
  for (let i = 0; i < firstWeekday; i++) cells.push(null);
  for (let d = 1; d <= daysInMonth; d++) cells.push(d);
  return cells;
});

const selectedDay = ref(null);
function selectDay(day) {
  if (!day) return;
  selectedDay.value = day;
}
const selectedDaySchedule = computed(() => {
  if (!selectedDay.value) return [];
  const label = `Jul ${selectedDay.value}, 2026`;
  return appointments.value.filter((a) => a.due === label);
});

/* ---------------- Vaccination Session (full page) ---------------- */
const sessionAppt = ref(null);
function openSession(appt) {
  sessionAppt.value = appt;
}
function closeSession() {
  sessionAppt.value = null;
}
function toggleVaccineDone(v) {
  v.done = !v.done;
  recalcStatus(sessionAppt.value);
}
function completeSession() {
  sessionAppt.value.vaccines.forEach((v) => (v.done = true));
  recalcStatus(sessionAppt.value);
  closeSession();
}
</script>

<template>
  <div class="flex min-h-screen w-full bg-stone-50 text-stone-900" style="font-family: 'Inter','Segoe UI',sans-serif;">
    <!-- ---------------- Sidebar ---------------- -->
    <aside
      class="flex flex-col shrink-0 border-r border-stone-200 bg-white transition-all duration-200"
      :class="collapsed ? 'w-[76px]' : 'w-[264px]'"
    >
      <div class="flex items-center gap-3 px-5 py-5 border-b border-stone-200">
        <div class="flex h-9 w-9 items-center justify-center rounded-xl text-white font-bold text-sm shrink-0 bg-gradient-to-br from-emerald-600 to-emerald-800">
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
      <!-- ===================================================
           VACCINATION SESSION (full page, replaces content)
      ==================================================== -->
      <template v-if="sessionAppt">
        <header class="flex items-center justify-between px-8 py-5 border-b border-stone-200 bg-white">
          <div class="flex items-center gap-3">
            <button @click="closeSession" class="flex h-9 w-9 items-center justify-center rounded-full bg-stone-50">
              <ArrowLeft :size="17" class="text-stone-500" />
            </button>
            <div>
              <h1 class="text-[22px] font-bold">Vaccination Session</h1>
              <p class="text-[12.5px] mt-0.5 text-stone-500">Dashboard &gt; Vaccine Schedule &gt; {{ sessionAppt.child }}</p>
            </div>
          </div>
          <span class="inline-flex items-center gap-1.5 rounded-full px-3 py-1.5 text-xs font-medium" :class="statusStyle[sessionAppt.status]">
            <span class="h-1.5 w-1.5 rounded-full" :class="statusDot[sessionAppt.status]" />
            {{ sessionAppt.status }}
          </span>
        </header>

        <div class="px-8 py-6 max-w-4xl space-y-6">
          <!-- Patient summary -->
          <div class="rounded-2xl border border-stone-200 bg-white shadow-sm p-5">
            <div class="flex items-center gap-4">
              <div class="flex h-14 w-14 items-center justify-center rounded-2xl bg-sky-50 shrink-0">
                <User :size="24" class="text-sky-700" />
              </div>
              <div class="flex-1">
                <p class="text-[16px] font-bold">{{ sessionAppt.child }}</p>
                <p class="text-[12px] text-stone-500">{{ sessionAppt.id }} · {{ sessionAppt.age }} · Queue {{ sessionAppt.queueNo }}</p>
              </div>
              <div class="text-right">
                <p class="text-[12px] text-stone-500">Assigned Healthworker</p>
                <p class="text-[13px] font-medium">{{ sessionAppt.worker }}</p>
              </div>
            </div>
            <div class="grid grid-cols-3 gap-3 mt-4">
              <div class="rounded-xl bg-stone-50 p-3">
                <p class="text-[10.5px] uppercase tracking-wide text-stone-500">Parent / Guardian</p>
                <p class="text-[13px] font-medium mt-0.5">{{ sessionAppt.parent }}</p>
              </div>
              <div class="rounded-xl bg-stone-50 p-3 flex items-center gap-2">
                <Phone :size="14" class="text-stone-400" />
                <div>
                  <p class="text-[10.5px] uppercase tracking-wide text-stone-500">Contact</p>
                  <p class="text-[13px] font-medium">{{ sessionAppt.contact }}</p>
                </div>
              </div>
              <div class="rounded-xl bg-stone-50 p-3">
                <p class="text-[10.5px] uppercase tracking-wide text-stone-500">Room</p>
                <p class="text-[13px] font-medium mt-0.5">{{ sessionAppt.room }}</p>
              </div>
            </div>
          </div>

          <!-- Vaccine checklist -->
          <div class="rounded-2xl border border-stone-200 bg-white shadow-sm p-5">
            <div class="flex items-center justify-between mb-1">
              <p class="text-[14px] font-semibold">Vaccines for This Visit</p>
              <span class="text-[12.5px] font-semibold text-emerald-700">
                {{ progressOf(sessionAppt).done }} / {{ progressOf(sessionAppt).total }} Completed
              </span>
            </div>
            <div class="h-2 w-full rounded-full bg-stone-200 overflow-hidden mb-4">
              <div
                class="h-full rounded-full bg-emerald-600 transition-all"
                :style="{ width: (progressOf(sessionAppt).done / progressOf(sessionAppt).total) * 100 + '%' }"
              ></div>
            </div>

            <div class="space-y-2">
              <button
                v-for="(v, i) in sessionAppt.vaccines"
                :key="i"
                @click="toggleVaccineDone(v)"
                class="flex w-full items-center gap-3 rounded-xl border px-4 py-3 text-left transition-colors"
                :class="v.done ? 'border-emerald-200 bg-emerald-50' : 'border-stone-200 hover:bg-stone-50'"
              >
                <CheckSquare v-if="v.done" :size="19" class="text-emerald-700 shrink-0" />
                <Square v-else :size="19" class="text-stone-400 shrink-0" />
                <div class="flex-1">
                  <p class="text-[13.5px] font-medium" :class="v.done ? 'text-emerald-800 line-through decoration-emerald-400' : 'text-stone-800'">
                    {{ v.name }}
                  </p>
                  <p class="text-[11.5px] text-stone-500">{{ v.dose }}</p>
                </div>
                <span v-if="v.done" class="text-[11px] font-medium text-emerald-700">Administered</span>
              </button>
            </div>
          </div>

          <!-- Session actions -->
          <div class="flex items-center justify-end gap-3">
            <button @click="closeSession" class="rounded-xl px-4 py-2.5 text-[13px] font-medium text-stone-600 hover:bg-stone-100">
              Save &amp; Exit
            </button>
            <button
              @click="completeSession"
              class="flex items-center gap-2 rounded-xl px-5 py-2.5 text-[13px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800 shadow-sm"
            >
              <Check :size="16" /> Complete Session
            </button>
          </div>
        </div>
      </template>

      <!-- ===================================================
           VACCINE SCHEDULE (default view)
      ==================================================== -->
      <template v-else>
        <!-- Top bar -->
        <header class="flex items-center justify-between px-8 py-5 border-b border-stone-200 bg-white">
          <div>
            <h1 class="text-[22px] font-bold">Vaccine Schedule</h1>
            <p class="text-[12.5px] mt-0.5 text-stone-500">Dashboard &gt; Vaccine Schedule</p>
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
                <p class="text-[10.5px] font-semibold uppercase tracking-wide text-stone-500 leading-tight">{{ c.label }}</p>
                <div class="flex h-7 w-7 items-center justify-center rounded-lg shrink-0" :class="c.tintBg">
                  <component :is="c.icon" :size="14" :class="c.tint" />
                </div>
              </div>
              <p class="text-[24px] font-bold mt-2">{{ c.value }}</p>
            </div>
          </div>

          <!-- Toolbar -->
          <div class="rounded-2xl border border-stone-200 bg-white p-4 shadow-sm">
            <div class="flex flex-wrap items-center gap-3">
              <div class="flex items-center gap-2 flex-1 min-w-[240px] rounded-xl border border-stone-200 px-3 py-2.5">
                <Search :size="16" class="text-stone-400 shrink-0" />
                <input
                  v-model="searchQuery"
                  type="text"
                  :placeholder="`Search by ${searchBy.toLowerCase()}...`"
                  class="flex-1 text-[13px] outline-none placeholder:text-stone-400"
                />
                <div class="relative shrink-0">
                  <select v-model="searchBy" class="appearance-none bg-stone-50 rounded-lg pl-3 pr-7 py-1.5 text-[12px] font-medium text-stone-600 outline-none cursor-pointer">
                    <option v-for="opt in searchByOptions" :key="opt" :value="opt">{{ opt }}</option>
                  </select>
                  <ChevronDown :size="13" class="pointer-events-none absolute right-2 top-1/2 -translate-y-1/2 text-stone-400" />
                </div>
              </div>

              <div class="relative">
                <select v-model="scheduleStatusFilter" class="appearance-none rounded-xl border border-stone-200 pl-9 pr-8 py-2.5 text-[13px] font-medium text-stone-600 outline-none cursor-pointer">
                  <option>All</option>
                  <option>Due Today</option>
                  <option>Upcoming</option>
                  <option>Overdue</option>
                  <option>Completed</option>
                </select>
                <Filter :size="14" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-stone-400" />
                <ChevronDown :size="13" class="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-stone-400" />
              </div>

              <div class="relative">
                <select v-model="vaccineFilter" class="appearance-none rounded-xl border border-stone-200 pl-9 pr-8 py-2.5 text-[13px] font-medium text-stone-600 outline-none cursor-pointer">
                  <option>All Vaccines</option>
                  <option>BCG</option>
                  <option>Hepatitis B</option>
                  <option>Pentavalent</option>
                  <option>OPV</option>
                  <option>IPV</option>
                  <option>MMR</option>
                  <option>PCV</option>
                  <option>DPT Booster</option>
                </select>
                <Syringe :size="14" class="pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-stone-400" />
                <ChevronDown :size="13" class="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-stone-400" />
              </div>

              <div class="relative">
                <select v-model="ageGroupFilter" class="appearance-none rounded-xl border border-stone-200 pl-3 pr-8 py-2.5 text-[13px] font-medium text-stone-600 outline-none cursor-pointer">
                  <option>All Ages</option>
                  <option>Birth</option>
                  <option>0–6 Months</option>
                  <option>7–12 Months</option>
                  <option>1–2 Years</option>
                </select>
                <ChevronDown :size="13" class="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-stone-400" />
              </div>

              <div class="flex-1"></div>

              <div class="flex items-center rounded-xl border border-stone-200 p-1">
                <button
                  @click="view = 'list'"
                  class="flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-[12.5px] font-medium"
                  :class="view === 'list' ? 'bg-emerald-700 text-white' : 'text-stone-500'"
                >
                  <List :size="14" /> List
                </button>
                <button
                  @click="view = 'calendar'"
                  class="flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-[12.5px] font-medium"
                  :class="view === 'calendar' ? 'bg-emerald-700 text-white' : 'text-stone-500'"
                >
                  <CalendarDays :size="14" /> Calendar
                </button>
              </div>

              <button class="flex items-center gap-2 rounded-xl px-4 py-2.5 text-[13px] font-medium border border-stone-200 hover:bg-stone-50">
                <Download :size="16" class="text-stone-500" />
                Export Schedule
              </button>
            </div>
          </div>

          <!-- Content grid: main + right sidebar -->
          <div class="grid grid-cols-12 gap-6">
            <!-- Main column -->
            <div class="col-span-8 space-y-6">
              <!-- LIST VIEW -->
              <div v-if="view === 'list'" class="rounded-2xl border border-stone-200 bg-white shadow-sm overflow-visible">
                <div class="flex items-center justify-between px-5 py-4 border-b border-stone-200">
                  <p class="text-[14px] font-semibold">Today's Schedule</p>
                  <span class="text-[12px] text-stone-500">{{ filteredAppointments.length }} appointments</span>
                </div>
                <div class="overflow-x-auto">
                  <table class="w-full text-[13px]">
                    <thead>
                      <tr class="bg-stone-50">
                        <th
                          v-for="h in ['', 'Queue No.', 'Patient ID', 'Child Name', &quot;Today's Vaccines&quot;, 'Progress', 'Healthworker', 'Status', '']"
                          :key="h"
                          class="text-left font-semibold px-4 py-3 whitespace-nowrap text-[11px] uppercase tracking-wide text-stone-500"
                        >
                          {{ h }}
                        </th>
                      </tr>
                    </thead>
                    <tbody>
                      <template v-for="appt in filteredAppointments" :key="appt.queueNo">
                        <tr class="border-t border-stone-200 cursor-pointer hover:bg-stone-50/60" @click="toggleExpand(appt.queueNo)">
                          <td class="px-3 py-3 text-center">
                            <ChevronUp v-if="expandedRows.has(appt.queueNo)" :size="15" class="text-stone-400 mx-auto" />
                            <ChevronDown v-else :size="15" class="text-stone-400 mx-auto" />
                          </td>
                          <td class="px-4 py-3 font-medium whitespace-nowrap">{{ appt.queueNo }}</td>
                          <td class="px-4 py-3 whitespace-nowrap text-stone-500">{{ appt.id }}</td>
                          <td class="px-4 py-3 whitespace-nowrap">
                            <p class="font-medium">{{ appt.child }}</p>
                            <p class="text-[11px] text-stone-500">{{ appt.age }}</p>
                          </td>
                          <td class="px-4 py-3">
                            <div class="flex flex-wrap items-center gap-1.5 max-w-[220px]">
                              <span class="inline-flex items-center gap-1 rounded-full bg-emerald-50 text-emerald-700 text-[11px] font-medium px-2 py-1 whitespace-nowrap">
                                💉 {{ appt.vaccines.length }} {{ appt.vaccines.length === 1 ? 'Vaccine' : 'Vaccines' }}
                              </span>
                              <span
                                v-for="(v, i) in appt.vaccines.slice(0, 2)"
                                :key="i"
                                class="inline-flex rounded-full bg-sky-50 text-sky-700 text-[11px] font-medium px-2 py-1 whitespace-nowrap"
                              >
                                {{ v.name }}
                              </span>
                              <span
                                v-if="appt.vaccines.length > 2"
                                class="inline-flex rounded-full bg-stone-100 text-stone-600 text-[11px] font-medium px-2 py-1 whitespace-nowrap"
                              >
                                +{{ appt.vaccines.length - 2 }} More
                              </span>
                            </div>
                          </td>
                          <td class="px-4 py-3 whitespace-nowrap">
                            <div class="flex items-center gap-2 w-28">
                              <div class="h-1.5 flex-1 rounded-full bg-stone-200 overflow-hidden">
                                <div
                                  class="h-full rounded-full bg-emerald-600"
                                  :style="{ width: (progressOf(appt).done / progressOf(appt).total) * 100 + '%' }"
                                ></div>
                              </div>
                              <span class="text-[11px] font-semibold text-stone-600 shrink-0">
                                {{ progressOf(appt).done }}/{{ progressOf(appt).total }}
                              </span>
                            </div>
                          </td>
                          <td class="px-4 py-3 whitespace-nowrap">{{ appt.worker }}</td>
                          <td class="px-4 py-3 whitespace-nowrap">
                            <span class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-medium" :class="statusStyle[appt.status]">
                              <span class="h-1.5 w-1.5 rounded-full" :class="statusDot[appt.status]" />
                              {{ appt.status }}
                            </span>
                          </td>
                          <td class="px-4 py-3 whitespace-nowrap" @click.stop>
                            <div class="flex items-center gap-1 relative">
                              <button
                                @click="openSession(appt)"
                                class="flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-[12px] font-semibold text-white bg-emerald-700 hover:bg-emerald-800"
                                title="Open Vaccination Session"
                              >
                                <Syringe :size="14" /> Session
                              </button>
                              <button @click="toggleActionsMenu(appt.queueNo)" class="p-1.5 rounded-lg hover:bg-stone-100" title="More actions">
                                <MoreHorizontal :size="15" class="text-stone-500" />
                              </button>
                              <div
                                v-if="actionsMenuOpenFor === appt.queueNo"
                                class="absolute right-0 top-9 z-10 w-52 rounded-xl border border-stone-200 bg-white shadow-lg py-1.5"
                              >
                                <button class="flex w-full items-center gap-2.5 px-3.5 py-2 text-[12.5px] text-stone-700 hover:bg-stone-50">
                                  <Eye :size="15" class="text-stone-500" /> View Patient
                                </button>
                                <button class="flex w-full items-center gap-2.5 px-3.5 py-2 text-[12.5px] text-stone-700 hover:bg-stone-50">
                                  <UserCog :size="15" class="text-stone-500" /> Assign Healthworker
                                </button>
                                <button class="flex w-full items-center gap-2.5 px-3.5 py-2 text-[12.5px] text-stone-700 hover:bg-stone-50">
                                  <Printer :size="15" class="text-stone-500" /> Print Schedule
                                </button>
                              </div>
                            </div>
                          </td>
                        </tr>

                        <!-- Expanded checklist row -->
                        <tr v-if="expandedRows.has(appt.queueNo)" class="border-t border-stone-100 bg-stone-50/60">
                          <td colspan="8" class="px-6 py-4">
                            <div class="flex items-center justify-between mb-2">
                              <p class="text-[12.5px] font-semibold text-stone-700">Vaccines Scheduled for This Appointment</p>
                              <span class="text-[12px] font-semibold text-emerald-700">
                                {{ progressOf(appt).done }} / {{ progressOf(appt).total }} Completed
                              </span>
                            </div>
                            <div class="grid grid-cols-2 gap-2">
                              <div
                                v-for="(v, i) in appt.vaccines"
                                :key="i"
                                class="flex items-center gap-2.5 rounded-lg bg-white border border-stone-200 px-3 py-2"
                              >
                                <CheckSquare v-if="v.done" :size="16" class="text-emerald-700 shrink-0" />
                                <Square v-else :size="16" class="text-stone-400 shrink-0" />
                                <span class="text-[12.5px]" :class="v.done ? 'text-stone-500 line-through decoration-stone-300' : 'text-stone-700'">
                                  {{ v.name }} {{ v.dose !== '—' ? v.dose : '' }}
                                </span>
                              </div>
                            </div>
                          </td>
                        </tr>
                      </template>
                    </tbody>
                  </table>
                </div>
              </div>

              <!-- CALENDAR VIEW -->
              <div v-else class="rounded-2xl border border-stone-200 bg-white shadow-sm p-5">
                <div class="flex items-center justify-between mb-4">
                  <p class="text-[14px] font-semibold">{{ calendarMonthLabel }}</p>
                  <div class="flex items-center gap-1">
                    <button class="flex h-8 w-8 items-center justify-center rounded-lg border border-stone-200 text-stone-500">
                      <ChevronLeft :size="15" />
                    </button>
                    <button class="flex h-8 w-8 items-center justify-center rounded-lg border border-stone-200 text-stone-500">
                      <ChevronRight :size="15" />
                    </button>
                  </div>
                </div>
                <div class="grid grid-cols-7 gap-2 mb-2">
                  <div v-for="d in ['Sun','Mon','Tue','Wed','Thu','Fri','Sat']" :key="d" class="text-center text-[11px] font-semibold text-stone-500 uppercase">
                    {{ d }}
                  </div>
                </div>
                <div class="grid grid-cols-7 gap-2">
                  <button
                    v-for="(day, i) in calendarCells"
                    :key="i"
                    @click="selectDay(day)"
                    :disabled="!day"
                    class="aspect-square rounded-xl border p-1.5 flex flex-col items-start justify-between text-left transition-colors"
                    :class="[
                      !day ? 'border-transparent' : 'border-stone-200 hover:border-emerald-400',
                      day === selectedDay ? 'ring-2 ring-emerald-500 border-emerald-400' : '',
                      day === 12 ? 'bg-emerald-50' : 'bg-white',
                    ]"
                  >
                    <span v-if="day" class="text-[12px] font-medium" :class="day === 12 ? 'text-emerald-800' : 'text-stone-700'">{{ day }}</span>
                    <span
                      v-if="day && dueCountByDay[day]"
                      class="text-[10px] font-semibold px-1.5 py-0.5 rounded-full bg-emerald-600 text-white self-end"
                    >
                      {{ dueCountByDay[day] }}
                    </span>
                  </button>
                </div>

                <!-- Selected day detail -->
                <div v-if="selectedDay" class="mt-5 rounded-xl bg-stone-50 p-4">
                  <div class="flex items-center justify-between mb-2">
                    <p class="text-[13px] font-semibold">Jul {{ selectedDay }}, 2026 — Scheduled Appointments</p>
                    <button @click="selectedDay = null" class="p-1 rounded-lg hover:bg-stone-200">
                      <X :size="14" class="text-stone-500" />
                    </button>
                  </div>
                  <div v-if="selectedDaySchedule.length === 0" class="text-[12.5px] text-stone-500 py-2">
                    No appointments scheduled on this date.
                  </div>
                  <div v-else class="space-y-2">
                    <div
                      v-for="appt in selectedDaySchedule"
                      :key="appt.queueNo"
                      class="flex items-center justify-between rounded-lg bg-white px-3 py-2 border border-stone-200"
                    >
                      <div>
                        <p class="text-[12.5px] font-medium">{{ appt.child }}</p>
                        <p class="text-[11px] text-stone-500">{{ appt.vaccines.length }} vaccines · {{ appt.worker }}</p>
                      </div>
                      <span class="inline-flex items-center gap-1.5 rounded-full px-2 py-0.5 text-[11px] font-medium" :class="statusStyle[appt.status]">
                        {{ appt.status }}
                      </span>
                    </div>
                  </div>
                </div>
              </div>

              <!-- Vaccine distribution -->
              <div class="rounded-2xl border border-stone-200 bg-white shadow-sm p-5">
                <p class="text-[14px] font-semibold mb-4">Vaccine Distribution</p>
                <div class="space-y-3">
                  <div v-for="d in distribution" :key="d.name" class="flex items-center gap-3">
                    <span class="w-28 text-[12px] text-stone-500 shrink-0">{{ d.name }}</span>
                    <div class="flex-1 h-2.5 rounded-full bg-stone-100 overflow-hidden">
                      <div class="h-full rounded-full" :class="d.color" :style="{ width: (d.value / distMax) * 100 + '%' }"></div>
                    </div>
                    <span class="w-8 text-right text-[12px] font-semibold">{{ d.value }}</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- Right sidebar -->
            <div class="col-span-4 space-y-6">
              <!-- Today's summary -->
              <div class="rounded-2xl border border-stone-200 bg-white shadow-sm p-5">
                <div class="flex items-center justify-between mb-3">
                  <p class="text-[14px] font-semibold">Today's Summary</p>
                  <span class="text-[11.5px] text-stone-500">Jul 12, 2026</span>
                </div>
                <div class="space-y-2">
                  <div
                    v-for="t in todaysSummary"
                    :key="t.label"
                    class="flex items-center justify-between rounded-xl bg-stone-50 px-3 py-2.5"
                  >
                    <div class="flex items-center gap-2">
                      <component :is="t.icon" :size="15" :class="t.tint" />
                      <span class="text-[12.5px] text-stone-600">{{ t.label }}</span>
                    </div>
                    <span class="text-[13px] font-semibold">{{ t.value }}</span>
                  </div>
                </div>
              </div>

              <!-- Upcoming vaccines -->
              <div class="rounded-2xl border border-stone-200 bg-white shadow-sm p-5">
                <p class="text-[14px] font-semibold mb-3">Upcoming Vaccines</p>
                <div class="space-y-2">
                  <div
                    v-for="u in upcomingVaccines"
                    :key="u.vaccine"
                    class="flex items-center justify-between rounded-xl bg-stone-50 px-3 py-2.5"
                  >
                    <div>
                      <p class="text-[12.5px] font-medium">{{ u.vaccine }}</p>
                      <p class="text-[11px] text-stone-500">{{ u.window }}</p>
                    </div>
                    <span class="text-[13px] font-semibold text-sky-700">{{ u.count }}</span>
                  </div>
                </div>
              </div>

              <!-- Overdue list -->
              <div class="rounded-2xl border border-stone-200 bg-white shadow-sm p-5">
                <div class="flex items-center justify-between mb-3">
                  <p class="text-[14px] font-semibold">Overdue List</p>
                  <span class="text-[11.5px] text-rose-700 font-medium">{{ overdueList.length }} shown</span>
                </div>
                <div class="space-y-2">
                  <div
                    v-for="appt in overdueList"
                    :key="appt.queueNo"
                    class="flex items-center justify-between rounded-xl bg-rose-50 px-3 py-2.5"
                  >
                    <div class="min-w-0">
                      <p class="text-[12.5px] font-medium truncate">{{ appt.child }}</p>
                      <p class="text-[11px] text-stone-500 truncate">{{ appt.vaccines.length }} vaccines · Due {{ appt.due }}</p>
                    </div>
                    <span class="text-[10.5px] font-semibold text-rose-700 shrink-0">Delayed</span>
                  </div>
                  <p v-if="overdueList.length === 0" class="text-[12.5px] text-stone-500 py-2">No delayed appointments.</p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </template>
    </main>
  </div>
</template>