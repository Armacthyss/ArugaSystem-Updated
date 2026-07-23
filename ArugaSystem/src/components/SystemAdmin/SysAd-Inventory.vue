<script setup>
import { ref, computed, reactive } from 'vue'

/* ----------------------------- Sidebar state ------------------------------ */
const isCollapsed = ref(false)
const toggleSidebar = () => (isCollapsed.value = !isCollapsed.value)

const navItems = [
  { label: 'Dashboard', icon: '🏠' },
  { label: 'User Management', icon: '👥' },
  { label: 'Patient Management', icon: '🧒' },
  { label: 'Vaccine Management', icon: '💉' },
  { label: 'Inventory', icon: '📦' },
  { label: 'Notifications', icon: '🔔' },
  { label: 'Reports', icon: '📊' },
  { label: 'Audit Logs', icon: '📋' },
  { label: 'Settings', icon: '⚙️' },
]
const activeNav = ref('Inventory')

/* -------------------------------- Status meta -------------------------------- */
const statusMeta = {
  Healthy: { tint: 'bg-emerald-50', text: 'text-emerald-700', dot: 'bg-emerald-500' },
  'Low Stock': { tint: 'bg-amber-50', text: 'text-amber-700', dot: 'bg-amber-500' },
  Critical: { tint: 'bg-red-50', text: 'text-red-700', dot: 'bg-red-500' },
  Expired: { tint: 'bg-red-200', text: 'text-red-900', dot: 'bg-red-800' },
}

const vaccineOptions = [
  'BCG', 'Hepatitis B', 'Pentavalent', 'OPV', 'IPV', 'PCV', 'MMR', 'MMR-2', 'DPT Booster', 'Japanese Encephalitis',
]

/* -------------------------------- Batch data -------------------------------- */
const batches = ref([
  {
    id: 1, batchNumber: 'BT-2026-041', vaccine: 'BCG', manufacturer: 'Serum Institute of India', lotNumber: 'LOT-88214',
    qtyReceived: 200, qtyRemaining: 18, expirationDate: 'Aug 02, 2026', dateReceived: 'Feb 10, 2026',
    storageLocation: 'Fridge A - Shelf 1', status: 'Critical',
    history: [
      { date: 'Feb 10, 2026', action: 'Received', quantity: '+200', performedBy: 'Renzo Miguel', remarks: 'Initial stock delivery' },
      { date: 'Mar 05, 2026 – Jul 09, 2026', action: 'Vaccinated', quantity: '-182', performedBy: 'Elena Cruz', remarks: 'Routine administration' },
    ],
  },
  {
    id: 2, batchNumber: 'BT-2026-052', vaccine: 'Hepatitis B', manufacturer: 'Bio Farma', lotNumber: 'LOT-77310',
    qtyReceived: 150, qtyRemaining: 96, expirationDate: 'Oct 14, 2026', dateReceived: 'Mar 22, 2026',
    storageLocation: 'Fridge A - Shelf 2', status: 'Healthy',
    history: [
      { date: 'Mar 22, 2026', action: 'Received', quantity: '+150', performedBy: 'Renzo Miguel', remarks: 'Initial stock delivery' },
      { date: 'Mar 22, 2026 – Jul 08, 2026', action: 'Vaccinated', quantity: '-54', performedBy: 'Bea Fernandez', remarks: 'Routine administration' },
    ],
  },
  {
    id: 3, batchNumber: 'BT-2026-033', vaccine: 'Pentavalent', manufacturer: 'GSK', lotNumber: 'LOT-65120',
    qtyReceived: 300, qtyRemaining: 41, expirationDate: 'Jul 25, 2026', dateReceived: 'Jan 18, 2026',
    storageLocation: 'Fridge B - Shelf 1', status: 'Low Stock',
    history: [
      { date: 'Jan 18, 2026', action: 'Received', quantity: '+300', performedBy: 'Renzo Miguel', remarks: 'Quarterly delivery' },
      { date: 'Jan 20, 2026 – Jul 10, 2026', action: 'Vaccinated', quantity: '-254', performedBy: 'Elena Cruz', remarks: 'Routine administration' },
      { date: 'Jun 02, 2026', action: 'Adjusted', quantity: '-5', performedBy: 'Renzo Miguel', remarks: 'Damaged vials removed' },
    ],
  },
  {
    id: 4, batchNumber: 'BT-2025-198', vaccine: 'OPV', manufacturer: 'Bio Farma', lotNumber: 'LOT-51092',
    qtyReceived: 250, qtyRemaining: 12, expirationDate: 'Jul 18, 2026', dateReceived: 'Oct 02, 2025',
    storageLocation: 'Fridge B - Shelf 2', status: 'Critical',
    history: [
      { date: 'Oct 02, 2025', action: 'Received', quantity: '+250', performedBy: 'Renzo Miguel', remarks: 'Initial stock delivery' },
      { date: 'Oct 05, 2025 – Jul 09, 2026', action: 'Vaccinated', quantity: '-238', performedBy: 'Bea Fernandez', remarks: 'Routine administration' },
    ],
  },
  {
    id: 5, batchNumber: 'BT-2025-176', vaccine: 'IPV', manufacturer: 'Sanofi Pasteur', lotNumber: 'LOT-44087',
    qtyReceived: 120, qtyRemaining: 0, expirationDate: 'Jun 15, 2026', dateReceived: 'Sep 11, 2025',
    storageLocation: 'Fridge B - Shelf 1', status: 'Expired',
    history: [
      { date: 'Sep 11, 2025', action: 'Received', quantity: '+120', performedBy: 'Renzo Miguel', remarks: 'Initial stock delivery' },
      { date: 'Sep 15, 2025 – Jun 10, 2026', action: 'Vaccinated', quantity: '-112', performedBy: 'Elena Cruz', remarks: 'Routine administration' },
      { date: 'Jun 15, 2026', action: 'Expired', quantity: '-8', performedBy: 'System', remarks: 'Remaining doses marked expired' },
    ],
  },
  {
    id: 6, batchNumber: 'BT-2026-061', vaccine: 'PCV', manufacturer: 'Pfizer', lotNumber: 'LOT-90211',
    qtyReceived: 180, qtyRemaining: 143, expirationDate: 'Dec 20, 2026', dateReceived: 'May 06, 2026',
    storageLocation: 'Fridge A - Shelf 3', status: 'Healthy',
    history: [
      { date: 'May 06, 2026', action: 'Received', quantity: '+180', performedBy: 'Renzo Miguel', remarks: 'Initial stock delivery' },
      { date: 'May 08, 2026 – Jul 09, 2026', action: 'Vaccinated', quantity: '-37', performedBy: 'Bea Fernandez', remarks: 'Routine administration' },
    ],
  },
  {
    id: 7, batchNumber: 'BT-2026-058', vaccine: 'MMR', manufacturer: 'Merck', lotNumber: 'LOT-83456',
    qtyReceived: 100, qtyRemaining: 27, expirationDate: 'Aug 09, 2026', dateReceived: 'Apr 14, 2026',
    storageLocation: 'Fridge A - Shelf 2', status: 'Low Stock',
    history: [
      { date: 'Apr 14, 2026', action: 'Received', quantity: '+100', performedBy: 'Renzo Miguel', remarks: 'Initial stock delivery' },
      { date: 'Apr 16, 2026 – Jul 07, 2026', action: 'Vaccinated', quantity: '-73', performedBy: 'Elena Cruz', remarks: 'Routine administration' },
    ],
  },
  {
    id: 8, batchNumber: 'BT-2026-070', vaccine: 'DPT Booster', manufacturer: 'GSK', lotNumber: 'LOT-99120',
    qtyReceived: 90, qtyRemaining: 90, expirationDate: 'Jan 30, 2027', dateReceived: 'Jul 01, 2026',
    storageLocation: 'Fridge B - Shelf 3', status: 'Healthy',
    history: [
      { date: 'Jul 01, 2026', action: 'Received', quantity: '+90', performedBy: 'Renzo Miguel', remarks: 'Initial stock delivery' },
    ],
  },
])

/* ---------------------------- Toolbar / filters ---------------------------- */
const searchQuery = ref('')
const vaccineFilter = ref('All Vaccines')
const statusFilter = ref('All')
const sortBy = ref('Expiration Date')

const parseDate = (d) => new Date(d.split(' – ')[0])

const filteredBatches = computed(() => {
  let list = batches.value.filter((b) => {
    const q = searchQuery.value.trim().toLowerCase()
    const matchesSearch = !q || b.vaccine.toLowerCase().includes(q) || b.batchNumber.toLowerCase().includes(q)
    const matchesVaccine = vaccineFilter.value === 'All Vaccines' || b.vaccine === vaccineFilter.value
    const matchesStatus = statusFilter.value === 'All' || b.status === statusFilter.value
    return matchesSearch && matchesVaccine && matchesStatus
  })

  list = [...list].sort((a, b) => {
    if (sortBy.value === 'Expiration Date') return parseDate(a.expirationDate) - parseDate(b.expirationDate)
    if (sortBy.value === 'Date Received') return parseDate(b.dateReceived) - parseDate(a.dateReceived)
    if (sortBy.value === 'Quantity Remaining') return a.qtyRemaining - b.qtyRemaining
    return 0
  })

  return list
})

/* -------------------------------- Summary ---------------------------------- */
const summary = computed(() => ({
  totalBatches: batches.value.length,
  totalDoses: batches.value.reduce((sum, b) => sum + b.qtyRemaining, 0),
  lowStock: batches.value.filter((b) => b.status === 'Low Stock').length,
  expiringSoon: batches.value.filter((b) => {
    const days = Math.ceil((parseDate(b.expirationDate) - new Date('2026-07-11')) / 86400000)
    return days >= 0 && days <= 30 && b.status !== 'Expired'
  }).length,
  expired: batches.value.filter((b) => b.status === 'Expired').length,
  receivedThisMonth: batches.value.filter((b) => b.dateReceived.includes('Jul') && b.dateReceived.includes('2026')).length,
}))

/* ------------------------------ Alerts panel ---------------------------- */
const daysUntil = (dateStr) => {
  const d = Math.ceil((parseDate(dateStr) - new Date('2026-07-11')) / 86400000)
  return d
}

const criticalStock = computed(() => batches.value.filter((b) => b.status === 'Critical'))
const expiringSoonList = computed(() =>
  batches.value.filter((b) => {
    const d = daysUntil(b.expirationDate)
    return d >= 0 && d <= 30 && b.status !== 'Expired'
  })
)
const expiredList = computed(() => batches.value.filter((b) => b.status === 'Expired'))

/* ------------------------------ Row actions menu ---------------------------- */
const openMenuId = ref(null)
const toggleMenu = (id) => (openMenuId.value = openMenuId.value === id ? null : id)
const closeMenu = () => (openMenuId.value = null)

const markExpired = (batch) => {
  batch.status = 'Expired'
  batch.history.unshift({ date: 'Today', action: 'Expired', quantity: `-${batch.qtyRemaining}`, performedBy: 'Renzo Miguel', remarks: 'Marked expired manually' })
  batch.qtyRemaining = 0
  closeMenu()
}

/* -------------------------------- Details drawer ---------------------------- */
const showDrawer = ref(false)
const selectedBatch = ref(null)
const openDrawer = (batch) => {
  selectedBatch.value = batch
  showDrawer.value = true
  closeMenu()
}
const closeDrawer = () => (showDrawer.value = false)

/* --------------------------------- Receive stock modal --------------------------------- */
const showReceiveModal = ref(false)
const receiveForm = reactive({
  vaccine: vaccineOptions[0], batchNumber: '', lotNumber: '', manufacturer: '',
  quantity: '', expirationDate: '', dateReceived: '', storageLocation: '', remarks: '',
})
const openReceiveModal = () => {
  Object.assign(receiveForm, {
    vaccine: vaccineOptions[0], batchNumber: '', lotNumber: '', manufacturer: '',
    quantity: '', expirationDate: '', dateReceived: '', storageLocation: '', remarks: '',
  })
  showReceiveModal.value = true
}
const receiveStock = () => {
  const qty = Number(receiveForm.quantity) || 0
  batches.value.unshift({
    id: Date.now(),
    batchNumber: receiveForm.batchNumber || `BT-2026-${Math.floor(100 + Math.random() * 899)}`,
    vaccine: receiveForm.vaccine,
    manufacturer: receiveForm.manufacturer || '—',
    lotNumber: receiveForm.lotNumber || '—',
    qtyReceived: qty,
    qtyRemaining: qty,
    expirationDate: receiveForm.expirationDate || '—',
    dateReceived: receiveForm.dateReceived || 'Today',
    storageLocation: receiveForm.storageLocation || '—',
    status: 'Healthy',
    history: [{ date: 'Today', action: 'Received', quantity: `+${qty}`, performedBy: 'Renzo Miguel', remarks: receiveForm.remarks || 'New stock delivery' }],
  })
  showReceiveModal.value = false
}

/* --------------------------------- Adjust stock modal --------------------------------- */
const showAdjustModal = ref(false)
const adjustForm = reactive({ id: null, type: 'Increase', quantity: '', reason: '' })
const openAdjustModal = (batch) => {
  Object.assign(adjustForm, { id: batch.id, type: 'Increase', quantity: '', reason: '' })
  showAdjustModal.value = true
  closeMenu()
}
const saveAdjustment = () => {
  const b = batches.value.find((x) => x.id === adjustForm.id)
  const qty = Number(adjustForm.quantity) || 0
  if (b && qty > 0) {
    if (adjustForm.type === 'Increase') {
      b.qtyRemaining += qty
      b.history.unshift({ date: 'Today', action: 'Adjusted', quantity: `+${qty}`, performedBy: 'Renzo Miguel', remarks: adjustForm.reason || 'Manual adjustment' })
    } else {
      b.qtyRemaining = Math.max(0, b.qtyRemaining - qty)
      b.history.unshift({ date: 'Today', action: 'Adjusted', quantity: `-${qty}`, performedBy: 'Renzo Miguel', remarks: adjustForm.reason || 'Manual adjustment' })
    }
  }
  showAdjustModal.value = false
}
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex text-slate-900" @click="closeMenu">
    <!-- ============================ SIDEBAR ============================ -->
    <aside
      :class="[isCollapsed ? 'w-20' : 'w-65']"
      class="hidden md:flex flex-col shrink-0 sticky top-0 h-screen bg-white border-r border-slate-200 transition-all duration-300 ease-in-out"
    >
      <div class="h-17.5 flex items-center gap-3 px-5 border-b border-slate-200 shrink-0">
        <div class="w-9 h-9 rounded-lg bg-emerald-600 flex items-center justify-center shrink-0">
          <span class="text-white font-bold text-sm">A</span>
        </div>
        <span v-if="!isCollapsed" class="font-bold text-slate-900 tracking-tight whitespace-nowrap overflow-hidden">Aruga</span>
      </div>

      <nav class="flex-1 overflow-y-auto py-4 px-3 space-y-1">
        <button
          v-for="item in navItems"
          :key="item.label"
          @click="activeNav = item.label"
          :class="[
            activeNav === item.label ? 'bg-emerald-50 text-emerald-700' : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900',
          ]"
          class="w-full flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
        >
          <span class="text-base shrink-0" aria-hidden="true">{{ item.icon }}</span>
          <span v-if="!isCollapsed" class="truncate">{{ item.label }}</span>
        </button>
      </nav>

      <div class="border-t border-slate-200 p-3 shrink-0 space-y-2">
        <div class="flex items-center gap-3 px-2 py-2">
          <div class="w-9 h-9 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-xs font-bold shrink-0">RM</div>
          <div v-if="!isCollapsed" class="min-w-0">
            <p class="text-sm font-semibold text-slate-900 truncate">Renzo Miguel</p>
            <p class="text-xs text-slate-500 truncate">System Admin</p>
          </div>
        </div>
        <button class="w-full flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-rose-600 hover:bg-rose-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-rose-400">
          <span class="text-base shrink-0" aria-hidden="true">🚪</span>
          <span v-if="!isCollapsed">Log out</span>
        </button>
        <button @click="toggleSidebar" class="w-full flex items-center justify-center rounded-lg px-3 py-2 text-xs font-medium text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors">
          <span :class="isCollapsed ? 'rotate-180' : ''" class="transition-transform inline-block">◀</span>
        </button>
      </div>
    </aside>

    <!-- ============================ MAIN ============================ -->
    <div class="flex-1 min-w-0 flex flex-col">
      <!-- Top navbar -->
      <header class="h-[70px] sticky top-0 z-20 bg-white border-b border-slate-200 flex items-center justify-between px-6 gap-4">
        <div class="min-w-0">
          <h1 class="text-lg font-bold text-slate-900 truncate">Inventory Management</h1>
          <p class="text-xs text-slate-500 truncate">Dashboard / Inventory</p>
        </div>
        <div class="flex items-center gap-3 shrink-0">
          <button class="relative w-9 h-9 rounded-lg flex items-center justify-center text-slate-500 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500">
            <span aria-hidden="true">🔔</span>
            <span class="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-rose-500 ring-2 ring-white"></span>
          </button>
          <button class="w-9 h-9 rounded-lg flex items-center justify-center text-slate-500 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500">
            <span aria-hidden="true">⚙️</span>
          </button>
          <div class="w-9 h-9 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-xs font-bold shrink-0">RM</div>
        </div>
      </header>

      <!-- Content -->
      <main class="p-6 space-y-6">
        <!-- Summary cards -->
        <section class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-6 gap-4">
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Total Batches</p>
              <div class="bg-teal-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">📦</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.totalBatches }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Available Doses</p>
              <div class="bg-emerald-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">💉</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.totalDoses }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Low Stock Batches</p>
              <div class="bg-amber-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">⚠️</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.lowStock }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Expiring in 30 Days</p>
              <div class="bg-orange-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">⏳</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.expiringSoon }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Expired Batches</p>
              <div class="bg-red-100 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">🚫</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.expired }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Received This Month</p>
              <div class="bg-sky-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">📥</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.receivedThisMonth }}</p>
          </div>
        </section>

        <!-- Toolbar -->
        <section class="bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
          <div class="flex flex-col lg:flex-row lg:items-center gap-3">
            <div class="relative flex-1 min-w-0">
              <span class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm" aria-hidden="true">🔍</span>
              <input
                v-model="searchQuery"
                type="text"
                placeholder="Search by vaccine name or batch number..."
                class="w-full pl-9 pr-3 py-2 text-sm rounded-lg border border-slate-200 bg-slate-50 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
              />
            </div>

            <select
              v-model="vaccineFilter"
              class="text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-slate-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
            >
              <option>All Vaccines</option>
              <option v-for="v in vaccineOptions" :key="v">{{ v }}</option>
            </select>

            <select
              v-model="statusFilter"
              class="text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-slate-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
            >
              <option value="All">All Status</option>
              <option>Healthy</option>
              <option>Low Stock</option>
              <option>Critical</option>
              <option>Expired</option>
            </select>

            <select
              v-model="sortBy"
              class="text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-slate-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
            >
              <option>Expiration Date</option>
              <option>Date Received</option>
              <option>Quantity Remaining</option>
            </select>

            <div class="flex items-center gap-2 shrink-0">
              <button class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500">
                Export Inventory
              </button>
              <button
                @click="openReceiveModal"
                class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
              >
                + Receive Stock
              </button>
            </div>
          </div>
        </section>

        <!-- Table + Alerts panel -->
        <section class="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">
          <!-- Inventory table -->
          <div class="lg:col-span-2 bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
            <div class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="border-b border-slate-200 bg-slate-50/60">
                    <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-5 py-3">Batch Number</th>
                    <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Vaccine</th>
                    <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Received</th>
                    <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Remaining</th>
                    <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Expiration</th>
                    <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Date Received</th>
                    <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Location</th>
                    <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Status</th>
                    <th class="text-right font-semibold text-slate-500 text-xs uppercase tracking-wide px-5 py-3">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr
                    v-for="batch in filteredBatches"
                    :key="batch.id"
                    class="border-b border-slate-100 last:border-0 hover:bg-slate-50 transition-colors"
                  >
                    <td class="px-5 py-3 font-mono text-xs text-slate-500 whitespace-nowrap">{{ batch.batchNumber }}</td>
                    <td class="px-3 py-3 font-semibold text-slate-900 whitespace-nowrap">{{ batch.vaccine }}</td>
                    <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ batch.qtyReceived }}</td>
                    <td class="px-3 py-3 text-slate-900 font-semibold whitespace-nowrap">{{ batch.qtyRemaining }}</td>
                    <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ batch.expirationDate }}</td>
                    <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ batch.dateReceived }}</td>
                    <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ batch.storageLocation }}</td>
                    <td class="px-3 py-3">
                      <span :class="[statusMeta[batch.status].tint, statusMeta[batch.status].text]" class="inline-flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full whitespace-nowrap">
                        <span :class="statusMeta[batch.status].dot" class="w-1.5 h-1.5 rounded-full"></span>
                        {{ batch.status }}
                      </span>
                    </td>
                    <td class="px-5 py-3 text-right relative">
                      <button
                        @click.stop="toggleMenu(batch.id)"
                        class="text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg w-8 h-8 inline-flex items-center justify-center transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
                      >
                        ⋮
                      </button>

                      <div
                        v-if="openMenuId === batch.id"
                        @click.stop
                        class="absolute right-5 top-11 z-30 w-48 bg-white border border-slate-200 rounded-lg shadow-md py-1 text-left"
                      >
                        <button @click="openDrawer(batch)" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">View Batch Details</button>
                        <button @click="closeMenu" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">Edit Batch</button>
                        <button @click="openAdjustModal(batch)" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">Adjust Quantity</button>
                        <div class="my-1 border-t border-slate-100"></div>
                        <button v-if="batch.status !== 'Expired'" @click="markExpired(batch)" class="w-full text-left px-3.5 py-2 text-sm text-red-700 hover:bg-red-50 transition-colors">Mark as Expired</button>
                        <button @click="closeMenu" class="w-full text-left px-3.5 py-2 text-sm text-slate-600 hover:bg-slate-50 transition-colors">Archive Batch</button>
                      </div>
                    </td>
                  </tr>

                  <tr v-if="filteredBatches.length === 0">
                    <td colspan="9" class="px-5 py-12 text-center text-sm text-slate-400">No batches match your search or filters.</td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- Inventory Alerts panel -->
          <div class="lg:col-span-1 bg-white border border-slate-200 rounded-xl shadow-sm p-5 space-y-5">
            <h2 class="text-sm font-bold text-slate-900">Inventory Alerts</h2>

            <div>
              <p class="text-xs font-bold uppercase tracking-wide text-red-600 mb-2">Critical Stock</p>
              <div v-if="criticalStock.length === 0" class="text-xs text-slate-400">No critical batches.</div>
              <div v-for="b in criticalStock" :key="'c-' + b.id" class="rounded-lg bg-red-50 px-3 py-2.5 mb-2 last:mb-0">
                <p class="text-sm font-semibold text-slate-900">{{ b.vaccine }}</p>
                <div class="flex items-center justify-between mt-0.5">
                  <span class="text-xs text-slate-500 font-mono">{{ b.batchNumber }}</span>
                  <span class="text-xs font-semibold text-red-700">{{ b.qtyRemaining }} left</span>
                </div>
              </div>
            </div>

            <div>
              <p class="text-xs font-bold uppercase tracking-wide text-amber-600 mb-2">Expiring Soon</p>
              <div v-if="expiringSoonList.length === 0" class="text-xs text-slate-400">Nothing expiring within 30 days.</div>
              <div v-for="b in expiringSoonList" :key="'e-' + b.id" class="rounded-lg bg-amber-50 px-3 py-2.5 mb-2 last:mb-0">
                <p class="text-sm font-semibold text-slate-900">{{ b.vaccine }}</p>
                <div class="flex items-center justify-between mt-0.5">
                  <span class="text-xs text-slate-500 font-mono">{{ b.batchNumber }}</span>
                  <span class="text-xs font-semibold text-amber-700">{{ daysUntil(b.expirationDate) }} days left</span>
                </div>
              </div>
            </div>

            <div>
              <p class="text-xs font-bold uppercase tracking-wide text-slate-500 mb-2">Expired Vaccines</p>
              <div v-if="expiredList.length === 0" class="text-xs text-slate-400">No expired batches.</div>
              <div v-for="b in expiredList" :key="'x-' + b.id" class="rounded-lg bg-slate-100 px-3 py-2.5 mb-2 last:mb-0">
                <p class="text-sm font-semibold text-slate-900">{{ b.vaccine }}</p>
                <div class="flex items-center justify-between mt-0.5">
                  <span class="text-xs text-slate-500 font-mono">{{ b.batchNumber }}</span>
                  <span class="text-xs font-semibold text-slate-600">{{ b.qtyRemaining }} left</span>
                </div>
              </div>
            </div>
          </div>
        </section>
      </main>
    </div>

    <!-- ============================ BATCH DETAILS DRAWER ============================ -->
    <transition name="fade">
      <div v-if="showDrawer" class="fixed inset-0 bg-slate-900/30 z-40" @click="closeDrawer"></div>
    </transition>
    <transition name="slide">
      <aside v-if="showDrawer" class="fixed top-0 right-0 h-screen w-full max-w-xl bg-white border-l border-slate-200 shadow-lg z-50 flex flex-col">
        <div class="h-[70px] flex items-center justify-between px-6 border-b border-slate-200 shrink-0">
          <h2 class="text-sm font-bold text-slate-900">Batch Details</h2>
          <button @click="closeDrawer" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
        </div>

        <div v-if="selectedBatch" class="flex-1 overflow-y-auto p-6 space-y-6">
          <div class="bg-slate-50 rounded-xl border border-slate-200 p-5">
            <div class="flex items-start justify-between gap-3 mb-4">
              <div>
                <p class="text-base font-bold text-slate-900">{{ selectedBatch.vaccine }}</p>
                <p class="text-xs text-slate-500 font-mono mt-0.5">{{ selectedBatch.batchNumber }}</p>
              </div>
              <span :class="[statusMeta[selectedBatch.status].tint, statusMeta[selectedBatch.status].text]" class="inline-flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full shrink-0">
                <span :class="statusMeta[selectedBatch.status].dot" class="w-1.5 h-1.5 rounded-full"></span>
                {{ selectedBatch.status }}
              </span>
            </div>
            <div class="grid grid-cols-2 gap-x-4 gap-y-3">
              <div><p class="text-xs text-slate-500">Manufacturer</p><p class="text-sm font-medium text-slate-900">{{ selectedBatch.manufacturer }}</p></div>
              <div><p class="text-xs text-slate-500">Lot Number</p><p class="text-sm font-medium text-slate-900">{{ selectedBatch.lotNumber }}</p></div>
              <div><p class="text-xs text-slate-500">Quantity Received</p><p class="text-sm font-medium text-slate-900">{{ selectedBatch.qtyReceived }}</p></div>
              <div><p class="text-xs text-slate-500">Remaining Quantity</p><p class="text-sm font-medium text-slate-900">{{ selectedBatch.qtyRemaining }}</p></div>
              <div><p class="text-xs text-slate-500">Expiration Date</p><p class="text-sm font-medium text-slate-900">{{ selectedBatch.expirationDate }}</p></div>
              <div><p class="text-xs text-slate-500">Date Received</p><p class="text-sm font-medium text-slate-900">{{ selectedBatch.dateReceived }}</p></div>
              <div class="col-span-2"><p class="text-xs text-slate-500">Storage Location</p><p class="text-sm font-medium text-slate-900">{{ selectedBatch.storageLocation }}</p></div>
            </div>
          </div>

          <div>
            <h3 class="text-xs font-bold uppercase tracking-wide text-slate-500 mb-3">Inventory Movement History</h3>
            <div class="space-y-3">
              <div v-for="(m, idx) in selectedBatch.history" :key="idx" class="flex gap-3">
                <div class="flex flex-col items-center pt-1">
                  <span
                    :class="m.quantity.startsWith('+') ? 'bg-emerald-500' : 'bg-rose-500'"
                    class="w-2.5 h-2.5 rounded-full shrink-0"
                  ></span>
                  <span v-if="idx !== selectedBatch.history.length - 1" class="w-px flex-1 bg-slate-200 mt-1"></span>
                </div>
                <div class="flex-1 min-w-0 pb-3">
                  <div class="flex items-center justify-between gap-2">
                    <p class="text-sm font-semibold text-slate-900">{{ m.action }}</p>
                    <span :class="m.quantity.startsWith('+') ? 'text-emerald-700' : 'text-rose-600'" class="text-sm font-bold shrink-0">{{ m.quantity }}</span>
                  </div>
                  <p class="text-xs text-slate-500 mt-0.5">{{ m.date }} · {{ m.performedBy }}</p>
                  <p class="text-xs text-slate-400 mt-0.5">{{ m.remarks }}</p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="border-t border-slate-200 p-4 flex items-center gap-2 shrink-0">
          <button @click="openAdjustModal(selectedBatch)" class="flex-1 text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors">Adjust Stock</button>
          <button class="flex-1 text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Edit Batch</button>
          <button @click="closeDrawer" class="text-sm font-semibold px-4 py-2 rounded-lg text-slate-500 hover:bg-slate-50 transition-colors">Close</button>
        </div>
      </aside>
    </transition>

    <!-- ============================ RECEIVE STOCK MODAL ============================ -->
    <transition name="fade">
      <div v-if="showReceiveModal" class="fixed inset-0 bg-slate-900/40 z-40 flex items-center justify-center p-4" @click.self="showReceiveModal = false">
        <div class="bg-white rounded-xl shadow-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto">
          <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200">
            <h2 class="text-base font-bold text-slate-900">Receive Stock</h2>
            <button @click="showReceiveModal = false" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
          </div>

          <div class="p-6 grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="sm:col-span-2">
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Select Vaccine</label>
              <select v-model="receiveForm.vaccine" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors">
                <option v-for="v in vaccineOptions" :key="v">{{ v }}</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Batch Number</label>
              <input v-model="receiveForm.batchNumber" type="text" placeholder="e.g. BT-2026-081" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Lot Number</label>
              <input v-model="receiveForm.lotNumber" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Manufacturer (optional)</label>
              <input v-model="receiveForm.manufacturer" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Quantity Received</label>
              <input v-model="receiveForm.quantity" type="number" min="1" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Expiration Date</label>
              <input v-model="receiveForm.expirationDate" type="date" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Date Received</label>
              <input v-model="receiveForm.dateReceived" type="date" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Storage Location</label>
              <input v-model="receiveForm.storageLocation" type="text" placeholder="e.g. Fridge A - Shelf 1" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div class="sm:col-span-2">
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Remarks</label>
              <textarea v-model="receiveForm.remarks" rows="2" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors resize-none"></textarea>
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 px-6 py-4 border-t border-slate-200">
            <button @click="showReceiveModal = false" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Cancel</button>
            <button @click="receiveStock" class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors">Receive Stock</button>
          </div>
        </div>
      </div>
    </transition>

    <!-- ============================ ADJUST STOCK MODAL ============================ -->
    <transition name="fade">
      <div v-if="showAdjustModal" class="fixed inset-0 bg-slate-900/40 z-40 flex items-center justify-center p-4" @click.self="showAdjustModal = false">
        <div class="bg-white rounded-xl shadow-lg w-full max-w-md">
          <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200">
            <h2 class="text-base font-bold text-slate-900">Adjust Stock</h2>
            <button @click="showAdjustModal = false" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
          </div>

          <div class="p-6 space-y-4">
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Adjustment Type</label>
              <div class="grid grid-cols-2 gap-2">
                <button
                  @click="adjustForm.type = 'Increase'"
                  :class="adjustForm.type === 'Increase' ? 'bg-emerald-600 text-white' : 'border border-slate-200 text-slate-600 hover:bg-slate-50'"
                  class="text-sm font-semibold px-3 py-2 rounded-lg transition-colors"
                >Increase</button>
                <button
                  @click="adjustForm.type = 'Decrease'"
                  :class="adjustForm.type === 'Decrease' ? 'bg-rose-600 text-white' : 'border border-slate-200 text-slate-600 hover:bg-slate-50'"
                  class="text-sm font-semibold px-3 py-2 rounded-lg transition-colors"
                >Decrease</button>
              </div>
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Adjustment Quantity</label>
              <input v-model="adjustForm.quantity" type="number" min="1" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Reason</label>
              <textarea v-model="adjustForm.reason" rows="3" placeholder="e.g. Damaged vials, recount correction..." class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors resize-none"></textarea>
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 px-6 py-4 border-t border-slate-200">
            <button @click="showAdjustModal = false" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Cancel</button>
            <button @click="saveAdjustment" class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors">Save Adjustment</button>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
.slide-enter-active, .slide-leave-active { transition: transform 0.25s ease; }
.slide-enter-from, .slide-leave-to { transform: translateX(100%); }

@media (prefers-reduced-motion: reduce) {
  * { transition-duration: 0.01ms !important; }
}
</style>