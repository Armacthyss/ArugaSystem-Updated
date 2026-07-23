<script setup>
import { ref, computed } from 'vue'

/* ----------------------------- Sidebar state ----------------------------- */
const isCollapsed = ref(false)
const toggleSidebar = () => (isCollapsed.value = !isCollapsed.value)

/* ------------------------------ Navigation -------------------------------- */
const navItems = [
  { label: 'Dashboard', icon: '🏠' },
  { label: 'User Management', icon: '👥' },
  { label: 'Vaccine Management', icon: '💉' },
  { label: 'Inventory', icon: '📦' },
  { label: 'Notifications', icon: '🔔' },
  { label: 'Reports', icon: '📊' },
  { label: 'Audit Logs', icon: '📋' },
  { label: 'Settings', icon: '⚙️' },
]
const activeNav = ref('Dashboard')

/* -------------------------------- KPI data -------------------------------- */
const kpis = [
  { label: 'Registered Children', value: '4,812', icon: '🧒', tint: 'bg-teal-50', trend: '+8%', up: true },
  { label: 'Doses Given Today', value: '146', icon: '💉', tint: 'bg-emerald-50', trend: '+12%', up: true },
  { label: 'Fully Immunized', value: '3,204', icon: '✅', tint: 'bg-emerald-50', trend: '+3%', up: true },
  { label: 'Due This Week', value: '318', icon: '📅', tint: 'bg-amber-50', trend: '+5%', up: true },
  { label: 'Overdue / Missed', value: '92', icon: '⚠️', tint: 'bg-rose-50', trend: '-8%', up: false },
  { label: 'Active Clinic Staff', value: '27', icon: '🩺', tint: 'bg-blue-50', trend: '+2%', up: true },
]

/* ---------------------------- Recent activity ----------------------------- */
const activities = [
  { user: 'Dr. Elena Cruz', time: '2 min ago', text: 'Created a new child profile for Mika Santos', icon: '🧒', color: 'bg-emerald-500' },
  { user: 'Renzo Miguel', time: '18 min ago', text: 'Updated inventory count for MMR vaccine batch #A203', icon: '📦', color: 'bg-blue-500' },
  { user: 'Nurse Bea Fernandez', time: '41 min ago', text: 'Logged a completed dose for patient #4471', icon: '💉', color: 'bg-emerald-500' },
  { user: 'System', time: '1 hr ago', text: 'Flagged 6 patient records as overdue for DPT booster', icon: '⚠️', color: 'bg-rose-500' },
  { user: 'Dr. Elena Cruz', time: '3 hrs ago', text: 'Generated the weekly immunization coverage report', icon: '📊', color: 'bg-amber-500' },
]

/* ---------------------------- Operational summary -------------------------- */
const summaryCards = [
  { label: "Today's Appointments", value: 24, dot: 'bg-blue-500' },
  { label: 'Late Vaccinations', value: 92, dot: 'bg-rose-500' },
  { label: 'Upcoming (7 days)', value: 318, dot: 'bg-amber-500' },
  { label: 'Inactive Records', value: 41, dot: 'bg-slate-400' },
]

/* ------------------------------ Bar chart data ----------------------------- */
const monthlyDoses = [
  { month: 'Jan', doses: 320 },
  { month: 'Feb', doses: 410 },
  { month: 'Mar', doses: 380 },
  { month: 'Apr', doses: 460 },
  { month: 'May', doses: 512 },
  { month: 'Jun', doses: 470 },
  { month: 'Jul', doses: 540 },
  { month: 'Aug', doses: 505 },
  { month: 'Sep', doses: 430 },
  { month: 'Oct', doses: 395 },
  { month: 'Nov', doses: 360 },
  { month: 'Dec', doses: 300 },
]
const maxDoses = Math.max(...monthlyDoses.map((m) => m.doses))
const hoveredMonth = ref(null)

/* ----------------------------- Inventory ---------------------------- */
const inventory = [
  { vaccine: "BCG", quantity: 250, percent: 100, status: "Healthy" },
  { vaccine: "Hepatitis B", quantity: 180, percent: 72, status: "Healthy" },
  { vaccine: "Pentavalent", quantity: 90, percent: 36, status: "Low" },
  { vaccine: "PCV", quantity: 140, percent: 56, status: "Healthy" },
  { vaccine: "MMR", quantity: 30, percent: 12, status: "Critical" },
  { vaccine: "IPV", quantity: 110, percent: 44, status: "Healthy" },
  { vaccine: "Rotavirus", quantity: 70, percent: 28, status: "Low" },
  { vaccine: "Japanese Encephalitis", quantity: 20, percent: 8, status: "Critical" }
]
/* ------------------------------ Quick actions ------------------------------ */
const quickActions = [
  { label: 'Add Child Profile', icon: '🧒', tint: 'bg-emerald-50' },
  { label: 'Record Vaccination', icon: '💉', tint: 'bg-teal-50' },
  { label: 'Receive Inventory', icon: '📦', tint: 'bg-blue-50' },
  { label: 'Add Vaccine', icon: '➕', tint: 'bg-emerald-50' },
  { label: 'Generate Report', icon: '📊', tint: 'bg-amber-50' },
  { label: 'Send Reminder', icon: '🔔', tint: 'bg-rose-50' },
]

const barHeight = (doses) => `${Math.round((doses / maxDoses) * 100)}%`
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex text-slate-900">
    <!-- ============================ SIDEBAR ============================ -->
    <aside
      :class="[isCollapsed ? 'w-20' : 'w-[260px]']"
      class="hidden md:flex flex-col shrink-0 sticky top-0 h-screen bg-white border-r border-slate-200 transition-all duration-300 ease-in-out"
    >
      <!-- Logo -->
      <div class="h-[70px] flex items-center gap-3 px-5 border-b border-slate-200 shrink-0">
        <div class="w-9 h-9 rounded-lg bg-emerald-600 flex items-center justify-center shrink-0">
          <span class="text-white font-bold text-sm">A</span>
        </div>
        <span v-if="!isCollapsed" class="font-bold text-slate-900 tracking-tight whitespace-nowrap overflow-hidden">Aruga Pediatric System</span>
      </div>

      <!-- Nav -->
      <nav class="flex-1 overflow-y-auto py-4 px-3 space-y-1">
        <button
          v-for="item in navItems"
          :key="item.label"
          @click="activeNav = item.label"
          :class="[
            activeNav === item.label
              ? 'bg-emerald-50 text-emerald-700'
              : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900',
          ]"
          class="w-full flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
        >
          <span class="text-base shrink-0" aria-hidden="true">{{ item.icon }}</span>
          <span v-if="!isCollapsed" class="truncate">{{ item.label }}</span>
        </button>
      </nav>

      <!-- User + collapse -->
      <div class="border-t border-slate-200 p-3 shrink-0 space-y-2">
        <div class="flex items-center gap-3 px-2 py-2">
          <div class="w-9 h-9 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-xs font-bold shrink-0">
            RM
          </div>
          <div v-if="!isCollapsed" class="min-w-0">
            <p class="text-sm font-semibold text-slate-900 truncate">Renzo Miguel</p>
            <p class="text-xs text-slate-500 truncate">System Admin</p>
          </div>
        </div>
        <button
          class="w-full flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-rose-600 hover:bg-rose-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-rose-400"
        >
          <span class="text-base shrink-0" aria-hidden="true">🚪</span>
          <span v-if="!isCollapsed">Log out</span>
        </button>
        <button
          @click="toggleSidebar"
          class="w-full flex items-center justify-center rounded-lg px-3 py-2 text-xs font-medium text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors"
        >
          <span :class="isCollapsed ? 'rotate-180' : ''" class="transition-transform">◀</span>
        </button>
      </div>
    </aside>

    <!-- ============================ MAIN ============================ -->
    <div class="flex-1 min-w-0 flex flex-col">
      <!-- Top navbar -->
      <header class="h-[70px] sticky top-0 z-10 bg-white border-b border-slate-200 flex items-center justify-between px-6 gap-4">
        <div class="min-w-0">
          <h1 class="text-lg font-bold text-slate-900 truncate">Dashboard Overview</h1>
          <p class="text-xs text-slate-500 truncate">Aruga / Dashboard</p>
        </div>

        <div class="flex items-center gap-3 shrink-0">
          

          <button class="relative w-9 h-9 rounded-lg flex items-center justify-center text-slate-500 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500">
            <span aria-hidden="true">🔔</span>
            <span class="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-rose-500 ring-2 ring-white"></span>
          </button>

          <button class="w-9 h-9 rounded-lg flex items-center justify-center text-slate-500 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500">
            <span aria-hidden="true">⚙️</span>
          </button>

          <div class="w-9 h-9 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-xs font-bold shrink-0">
            RM
          </div>
        </div>
      </header>

      <!-- Content -->
      <main class="p-6 space-y-6">
        <!-- ROW 1: KPI cards -->
        <section class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
          <div
            v-for="kpi in kpis"
            :key="kpi.label"
            class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm hover:shadow-md transition-shadow"
          >
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500 truncate">{{ kpi.label }}</p>
              <div :class="kpi.tint" class="w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">
                {{ kpi.icon }}
              </div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900 truncate">{{ kpi.value }}</p>
            <span
              :class="kpi.up ? 'bg-emerald-50 text-emerald-700' : 'bg-rose-50 text-rose-600'"
              class="mt-2 inline-flex items-center gap-1 text-xs font-semibold px-2 py-0.5 rounded-full"
            >
              {{ kpi.up ? '↑' : '↓' }} {{ kpi.trend }}
            </span>
          </div>
        </section>

        <!-- ROW 2: Activity + Summary -->
        <section class="grid grid-cols-1 lg:grid-cols-12 gap-6">
          <!-- Recent Activity -->
          <div class="lg:col-span-7 min-w-0 bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
            <div class="flex items-center justify-between mb-4">
              <h2 class="text-sm font-bold text-slate-900">Recent System Activity</h2>
              <button class="text-xs font-semibold text-emerald-700 hover:underline">View all</button>
            </div>
            <div class="relative pl-9">
              <div class="absolute left-[15px] top-1 bottom-1 w-px bg-slate-200"></div>
              <div v-for="(item, i) in activities" :key="i" class="relative pb-5 last:pb-0">
                <span
                  :class="item.color"
                  class="absolute -left-9 top-0 w-8 h-8 rounded-full flex items-center justify-center text-xs ring-4 ring-white"
                >
                  <span class="text-white" aria-hidden="true">{{ item.icon }}</span>
                </span>
                <div class="bg-slate-50 rounded-lg px-4 py-3">
                  <div class="flex items-center justify-between gap-2 mb-1">
                    <p class="text-sm font-semibold text-slate-900 truncate">{{ item.user }}</p>
                    <p class="text-xs text-slate-400 shrink-0">{{ item.time }}</p>
                  </div>
                  <p class="text-xs text-slate-500">{{ item.text }}</p>
                </div>
              </div>
            </div>
          </div>

          <!-- Operational Summary -->
          <div class="lg:col-span-5 min-w-0 bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
            <h2 class="text-sm font-bold text-slate-900 mb-4">Clinic Operational Summary</h2>
            <div class="grid grid-cols-2 gap-3">
              <div
                v-for="card in summaryCards"
                :key="card.label"
                class="relative bg-slate-50 rounded-lg p-3.5"
              >
                <span :class="card.dot" class="absolute top-3 right-3 w-2 h-2 rounded-full"></span>
                <p class="text-xs text-slate-500 leading-none">{{ card.label }}</p>
                <p class="text-2xl font-extrabold text-slate-900 leading-none mt-1.5">{{ card.value }}</p>
              </div>
            </div>
          </div>
        </section>

        <!-- ROW 3: Charts -->
        <section class="grid grid-cols-1 lg:grid-cols-2 gap-6">
          <!-- Monthly vaccinations bar chart -->
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-5 shadow-sm h-[380px] flex flex-col">
            <h2 class="text-sm font-bold text-slate-900 mb-4">Monthly Vaccinations</h2>
            <div class="h-48 flex items-end gap-2">
              <div
                v-for="m in monthlyDoses"
                :key="m.month"
                class="relative flex-1 h-full flex items-end"
                @mouseenter="hoveredMonth = m.month"
                @mouseleave="hoveredMonth = null"
              >
                <div
                  v-if="hoveredMonth === m.month"
                  class="absolute -top-7 left-1/2 -translate-x-1/2 bg-slate-900 text-white text-[10px] font-semibold px-2 py-1 rounded whitespace-nowrap z-10"
                >
                  {{ m.doses }} doses
                </div>
                <div
                  :style="{ height: barHeight(m.doses) }"
                  class="w-full rounded-t-md bg-emerald-600 hover:bg-emerald-500 transition-all duration-300 cursor-pointer"
                ></div>
              </div>
            </div>
            <div class="flex gap-2 mt-2">
              <span v-for="m in monthlyDoses" :key="m.month" class="flex-1 text-center text-[10px] text-slate-400">{{ m.month }}</span>
            </div>
          </div>

<!-- Inventory -->
<div class="min-w-0 bg-white border border-slate-200 rounded-xl p-5 shadow-sm h-[380px] flex flex-col">
  <h2 class="text-sm font-bold text-slate-900 mb-5">Vaccine Inventory</h2>

  <!-- Scrollable Area -->
  <div class="space-y-4 max-h-80 overflow-y-auto pr-2">
    <div v-for="item in inventory" :key="item.vaccine">
      <div class="flex items-center justify-between mb-1.5">
        <span class="text-sm font-medium text-slate-700">
          {{ item.vaccine }}
        </span>

        <span
          class="text-sm font-bold"
          :class="{
            'text-emerald-600': item.status === 'Healthy',
            'text-amber-500': item.status === 'Low',
            'text-red-500': item.status === 'Critical'
          }"
        >
          {{ item.quantity }} doses
        </span>
      </div>

      <div class="h-2 rounded-full bg-slate-100 overflow-hidden">
        <div
          :style="{ width: item.percent + '%' }"
          class="h-full rounded-full transition-all duration-500"
          :class="{
            'bg-emerald-500': item.status === 'Healthy',
            'bg-amber-400': item.status === 'Low',
            'bg-red-500': item.status === 'Critical'
          }"
        ></div>
      </div>
    </div>
  </div>
</div>
</section>

        <!-- ROW 4: Quick actions -->
        <section class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
          <h2 class="text-sm font-bold text-slate-900 mb-4">Administrative Quick Actions</h2>
          <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4">
            <button
              v-for="action in quickActions"
              :key="action.label"
              class="flex flex-col items-center gap-2 rounded-xl border border-slate-200 p-4 hover:border-emerald-200 hover:bg-emerald-50/50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500 group"
            >
              <div
                :class="action.tint"
                class="w-12 h-12 rounded-xl flex items-center justify-center relative overflow-hidden group-hover:scale-105 transition-transform"
              >
                <span class="text-xl" aria-hidden="true">{{ action.icon }}</span>
              </div>
              <span class="text-xs font-medium text-slate-600 text-center truncate w-full">{{ action.label }}</span>
            </button>
          </div>
        </section>
      </main>
    </div>
  </div>
</template>

<style scoped>
@media (prefers-reduced-motion: reduce) {
  * {
    transition-duration: 0.01ms !important;
  }
}
</style>