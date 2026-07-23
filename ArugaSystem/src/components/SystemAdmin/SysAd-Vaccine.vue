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
const activeNav = ref('Vaccine Management')

/* -------------------------------- Status meta -------------------------------- */
const statusMeta = {
  Active: { tint: 'bg-emerald-50', text: 'text-emerald-700', dot: 'bg-emerald-500' },
  Inactive: { tint: 'bg-slate-100', text: 'text-slate-600', dot: 'bg-slate-400' },
}

const ageCategoryOptions = ['Birth', '6 Weeks', '10 Weeks', '14 Weeks', '9 Months', '12 Months', 'Booster']
const infantCategories = ['6 Weeks', '10 Weeks', '14 Weeks', '9 Months', '12 Months']

/* -------------------------------- Vaccine data -------------------------------- */
const vaccines = ref([
  {
    id: 1, name: 'Bacillus Calmette–Guérin', abbreviation: 'BCG', code: 'VX-001',
    description: 'Protects against severe forms of tuberculosis in children.',
    targetDisease: 'Tuberculosis', recommendedAge: 'At birth', ageCategory: 'Birth',
    route: 'Intradermal', doses: 1, interval: 'Single dose', status: 'Active',
    created: 'Jan 10, 2024', updated: 'Jun 02, 2026',
  },
  {
    id: 2, name: 'Hepatitis B Vaccine', abbreviation: 'HepB', code: 'VX-002',
    description: 'Prevents Hepatitis B virus infection, given within 24 hours of birth.',
    targetDisease: 'Hepatitis B', recommendedAge: 'At birth', ageCategory: 'Birth',
    route: 'Intramuscular', doses: 1, interval: 'Single dose', status: 'Active',
    created: 'Jan 10, 2024', updated: 'Jun 02, 2026',
  },
  {
    id: 3, name: 'Pentavalent Vaccine', abbreviation: 'Penta', code: 'VX-003',
    description: 'Combination vaccine against Diphtheria, Pertussis, Tetanus, Hepatitis B, and Hib.',
    targetDisease: 'DPT-HepB-Hib', recommendedAge: '6, 10, 14 weeks', ageCategory: '6 Weeks',
    route: 'Intramuscular', doses: 3, interval: '4 weeks apart', status: 'Active',
    created: 'Jan 12, 2024', updated: 'May 20, 2026',
  },
  {
    id: 4, name: 'Oral Polio Vaccine', abbreviation: 'OPV', code: 'VX-004',
    description: 'Protects against poliomyelitis through oral administration.',
    targetDisease: 'Poliomyelitis', recommendedAge: '6, 10, 14 weeks', ageCategory: '6 Weeks',
    route: 'Oral', doses: 3, interval: '4 weeks apart', status: 'Active',
    created: 'Jan 12, 2024', updated: 'Apr 15, 2026',
  },
  {
    id: 5, name: 'Inactivated Polio Vaccine', abbreviation: 'IPV', code: 'VX-005',
    description: 'Injectable polio vaccine given alongside OPV doses for added protection.',
    targetDisease: 'Poliomyelitis', recommendedAge: '14 weeks', ageCategory: '14 Weeks',
    route: 'Intramuscular', doses: 1, interval: 'Single dose', status: 'Active',
    created: 'Feb 01, 2024', updated: 'Mar 11, 2026',
  },
  {
    id: 6, name: 'Pneumococcal Conjugate Vaccine', abbreviation: 'PCV', code: 'VX-006',
    description: 'Protects against pneumococcal disease including pneumonia and meningitis.',
    targetDisease: 'Pneumococcal Disease', recommendedAge: '6, 10, 14 weeks', ageCategory: '6 Weeks',
    route: 'Intramuscular', doses: 3, interval: '4 weeks apart', status: 'Active',
    created: 'Feb 01, 2024', updated: 'Jun 20, 2026',
  },
  {
    id: 7, name: 'Measles, Mumps, Rubella Vaccine', abbreviation: 'MMR', code: 'VX-007',
    description: 'Protects against measles, mumps, and rubella infections.',
    targetDisease: 'Measles, Mumps, Rubella', recommendedAge: '9 months', ageCategory: '9 Months',
    route: 'Subcutaneous', doses: 1, interval: 'Single dose', status: 'Active',
    created: 'Mar 05, 2024', updated: 'Jun 09, 2026',
  },
  {
    id: 8, name: 'MMR Booster Dose', abbreviation: 'MMR-2', code: 'VX-008',
    description: 'Second dose of MMR given to reinforce immunity at 12 months.',
    targetDisease: 'Measles, Mumps, Rubella', recommendedAge: '12 months', ageCategory: '12 Months',
    route: 'Subcutaneous', doses: 1, interval: 'Single dose', status: 'Active',
    created: 'Mar 05, 2024', updated: 'Jun 09, 2026',
  },
  {
    id: 9, name: 'Diphtheria-Pertussis-Tetanus Booster', abbreviation: 'DPT Booster', code: 'VX-009',
    description: 'Booster dose reinforcing protection against diphtheria, pertussis, and tetanus.',
    targetDisease: 'Diphtheria, Pertussis, Tetanus', recommendedAge: '18 months (Booster)', ageCategory: 'Booster',
    route: 'Intramuscular', doses: 1, interval: 'Single dose', status: 'Active',
    created: 'Apr 18, 2024', updated: 'Feb 27, 2026',
  },
  {
    id: 10, name: 'Japanese Encephalitis Vaccine', abbreviation: 'JE', code: 'VX-010',
    description: 'Protects against Japanese encephalitis in endemic areas.',
    targetDisease: 'Japanese Encephalitis', recommendedAge: '9 months', ageCategory: '9 Months',
    route: 'Intramuscular', doses: 1, interval: 'Single dose', status: 'Inactive',
    created: 'May 22, 2024', updated: 'Jan 30, 2026',
  },
])

/* ---------------------------- Toolbar / filters ---------------------------- */
const searchQuery = ref('')
const statusFilter = ref('All')
const ageCategoryFilter = ref('All')

const filteredVaccines = computed(() =>
  vaccines.value.filter((v) => {
    const q = searchQuery.value.trim().toLowerCase()
    const matchesSearch =
      !q || v.name.toLowerCase().includes(q) || v.code.toLowerCase().includes(q) || v.abbreviation.toLowerCase().includes(q)
    const matchesStatus = statusFilter.value === 'All' || v.status === statusFilter.value
    const matchesAge = ageCategoryFilter.value === 'All' || v.ageCategory === ageCategoryFilter.value
    return matchesSearch && matchesStatus && matchesAge
  })
)

/* -------------------------------- Summary ---------------------------------- */
const summary = computed(() => ({
  total: vaccines.value.length,
  active: vaccines.value.filter((v) => v.status === 'Active').length,
  inactive: vaccines.value.filter((v) => v.status === 'Inactive').length,
  birth: vaccines.value.filter((v) => v.ageCategory === 'Birth').length,
  infant: vaccines.value.filter((v) => infantCategories.includes(v.ageCategory)).length,
  booster: vaccines.value.filter((v) => v.ageCategory === 'Booster').length,
}))

/* ------------------------------ Row actions menu ---------------------------- */
const openMenuId = ref(null)
const toggleMenu = (id) => (openMenuId.value = openMenuId.value === id ? null : id)
const closeMenu = () => (openMenuId.value = null)

const setStatus = (vaccine, status) => {
  vaccine.status = status
  vaccine.updated = 'Today'
  closeMenu()
}

/* -------------------------------- Details drawer ---------------------------- */
const showDrawer = ref(false)
const selectedVaccine = ref(null)
const openDrawer = (vaccine) => {
  selectedVaccine.value = vaccine
  showDrawer.value = true
  closeMenu()
}
const closeDrawer = () => (showDrawer.value = false)

/* --------------------------------- Add / Edit modal --------------------------------- */
const showFormModal = ref(false)
const formMode = ref('add') // 'add' | 'edit'
const vaccineForm = reactive({
  id: null, name: '', abbreviation: '', description: '', targetDisease: '',
  recommendedAge: '', ageCategory: 'Birth', route: 'Intramuscular', doses: 1,
  interval: '', status: 'Active',
})

const openAddModal = () => {
  Object.assign(vaccineForm, {
    id: null, name: '', abbreviation: '', description: '', targetDisease: '',
    recommendedAge: '', ageCategory: 'Birth', route: 'Intramuscular', doses: 1,
    interval: '', status: 'Active',
  })
  formMode.value = 'add'
  showFormModal.value = true
}

const openEditModal = (vaccine) => {
  Object.assign(vaccineForm, {
    id: vaccine.id, name: vaccine.name, abbreviation: vaccine.abbreviation,
    description: vaccine.description, targetDisease: vaccine.targetDisease,
    recommendedAge: vaccine.recommendedAge, ageCategory: vaccine.ageCategory,
    route: vaccine.route, doses: vaccine.doses, interval: vaccine.interval, status: vaccine.status,
  })
  formMode.value = 'edit'
  showFormModal.value = true
  closeMenu()
}

const saveVaccine = () => {
  if (formMode.value === 'add') {
    vaccines.value.unshift({
      id: Date.now(),
      code: `VX-0${Math.floor(10 + Math.random() * 89)}`,
      name: vaccineForm.name || 'New Vaccine',
      abbreviation: vaccineForm.abbreviation || '—',
      description: vaccineForm.description,
      targetDisease: vaccineForm.targetDisease || '—',
      recommendedAge: vaccineForm.recommendedAge || '—',
      ageCategory: vaccineForm.ageCategory,
      route: vaccineForm.route,
      doses: vaccineForm.doses,
      interval: vaccineForm.interval || 'Single dose',
      status: vaccineForm.status,
      created: 'Today',
      updated: 'Today',
    })
  } else {
    const v = vaccines.value.find((x) => x.id === vaccineForm.id)
    if (v) {
      Object.assign(v, {
        name: vaccineForm.name, abbreviation: vaccineForm.abbreviation,
        description: vaccineForm.description, targetDisease: vaccineForm.targetDisease,
        recommendedAge: vaccineForm.recommendedAge, ageCategory: vaccineForm.ageCategory,
        route: vaccineForm.route, doses: vaccineForm.doses, interval: vaccineForm.interval,
        status: vaccineForm.status, updated: 'Today',
      })
    }
  }
  showFormModal.value = false
}
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex text-slate-900" @click="closeMenu">
    <!-- ============================ SIDEBAR ============================ -->
    <aside
      :class="[isCollapsed ? 'w-20' : 'w-[260px]']"
      class="hidden md:flex flex-col shrink-0 sticky top-0 h-screen bg-white border-r border-slate-200 transition-all duration-300 ease-in-out"
    >
      <div class="h-[70px] flex items-center gap-3 px-5 border-b border-slate-200 shrink-0">
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
          <h1 class="text-lg font-bold text-slate-900 truncate">Vaccine Management</h1>
          <p class="text-xs text-slate-500 truncate">Dashboard / Vaccine Management</p>
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
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Total Vaccines</p>
              <div class="bg-teal-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">💉</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.total }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Active Vaccines</p>
              <div class="bg-emerald-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">✅</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.active }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Inactive Vaccines</p>
              <div class="bg-slate-100 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">💤</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.inactive }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Birth Vaccines</p>
              <div class="bg-sky-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">👶</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.birth }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Infant Vaccines</p>
              <div class="bg-amber-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">🧒</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.infant }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Booster Vaccines</p>
              <div class="bg-violet-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">🔁</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.booster }}</p>
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
                placeholder="Search by vaccine name or vaccine code..."
                class="w-full pl-9 pr-3 py-2 text-sm rounded-lg border border-slate-200 bg-slate-50 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
              />
            </div>

            <select
              v-model="statusFilter"
              class="text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-slate-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
            >
              <option value="All">All Status</option>
              <option>Active</option>
              <option>Inactive</option>
            </select>

            <select
              v-model="ageCategoryFilter"
              class="text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-slate-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
            >
              <option value="All">All Age Categories</option>
              <option v-for="cat in ageCategoryOptions" :key="cat" :value="cat">{{ cat }}</option>
            </select>

            <div class="flex items-center gap-2 shrink-0">
              <button class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500">
                Export
              </button>
              <button
                @click="openAddModal"
                class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
              >
                + Add Vaccine
              </button>
            </div>
          </div>
        </section>

        <!-- Vaccine table -->
        <section class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
          <div class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-slate-200 bg-slate-50/60">
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-5 py-3">Vaccine Name</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Abbreviation</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Recommended Age</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">No. of Doses</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Dose Interval</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Status</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Last Updated</th>
                  <th class="text-right font-semibold text-slate-500 text-xs uppercase tracking-wide px-5 py-3">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="vaccine in filteredVaccines"
                  :key="vaccine.id"
                  class="border-b border-slate-100 last:border-0 hover:bg-slate-50 transition-colors"
                >
                  <td class="px-5 py-3">
                    <div class="flex items-center gap-3">
                      <div class="w-9 h-9 rounded-lg bg-teal-50 flex items-center justify-center text-sm shrink-0">💉</div>
                      <div class="min-w-0">
                        <p class="font-semibold text-slate-900 whitespace-nowrap">{{ vaccine.name }}</p>
                        <p class="text-xs text-slate-400 font-mono">{{ vaccine.code }}</p>
                      </div>
                    </div>
                  </td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ vaccine.abbreviation }}</td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ vaccine.recommendedAge }}</td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ vaccine.doses }}</td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ vaccine.interval }}</td>
                  <td class="px-3 py-3">
                    <span :class="[statusMeta[vaccine.status].tint, statusMeta[vaccine.status].text]" class="inline-flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full whitespace-nowrap">
                      <span :class="statusMeta[vaccine.status].dot" class="w-1.5 h-1.5 rounded-full"></span>
                      {{ vaccine.status }}
                    </span>
                  </td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ vaccine.updated }}</td>
                  <td class="px-5 py-3 text-right relative">
                    <button
                      @click.stop="toggleMenu(vaccine.id)"
                      class="text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg w-8 h-8 inline-flex items-center justify-center transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
                    >
                      ⋮
                    </button>

                    <div
                      v-if="openMenuId === vaccine.id"
                      @click.stop
                      class="absolute right-5 top-11 z-30 w-48 bg-white border border-slate-200 rounded-lg shadow-md py-1 text-left"
                    >
                      <button @click="openDrawer(vaccine)" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">View Details</button>
                      <button @click="openEditModal(vaccine)" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">Edit Vaccine</button>
                      <div class="my-1 border-t border-slate-100"></div>
                      <button v-if="vaccine.status !== 'Active'" @click="setStatus(vaccine, 'Active')" class="w-full text-left px-3.5 py-2 text-sm text-emerald-700 hover:bg-emerald-50 transition-colors">Activate</button>
                      <button v-if="vaccine.status === 'Active'" @click="setStatus(vaccine, 'Inactive')" class="w-full text-left px-3.5 py-2 text-sm text-slate-600 hover:bg-slate-50 transition-colors">Deactivate</button>
                      <button @click="closeMenu" class="w-full text-left px-3.5 py-2 text-sm text-rose-600 hover:bg-rose-50 transition-colors">Archive</button>
                    </div>
                  </td>
                </tr>

                <tr v-if="filteredVaccines.length === 0">
                  <td colspan="8" class="px-5 py-12 text-center text-sm text-slate-400">No vaccines match your search or filters.</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </main>
    </div>

    <!-- ============================ VACCINE DETAILS DRAWER ============================ -->
    <transition name="fade">
      <div v-if="showDrawer" class="fixed inset-0 bg-slate-900/30 z-40" @click="closeDrawer"></div>
    </transition>
    <transition name="slide">
      <aside v-if="showDrawer" class="fixed top-0 right-0 h-screen w-full max-w-sm bg-white border-l border-slate-200 shadow-lg z-50 flex flex-col">
        <div class="h-[70px] flex items-center justify-between px-5 border-b border-slate-200 shrink-0">
          <h2 class="text-sm font-bold text-slate-900">Vaccine Details</h2>
          <button @click="closeDrawer" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
        </div>

        <div v-if="selectedVaccine" class="flex-1 overflow-y-auto p-6 space-y-6">
          <div class="flex flex-col items-center text-center gap-3">
            <div class="w-16 h-16 rounded-full bg-teal-50 flex items-center justify-center text-2xl">💉</div>
            <div>
              <p class="text-base font-bold text-slate-900">{{ selectedVaccine.name }}</p>
              <p class="text-xs text-slate-400 font-mono mt-0.5">{{ selectedVaccine.code }}</p>
              <span :class="[statusMeta[selectedVaccine.status].tint, statusMeta[selectedVaccine.status].text]" class="mt-2 inline-flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full">
                <span :class="statusMeta[selectedVaccine.status].dot" class="w-1.5 h-1.5 rounded-full"></span>
                {{ selectedVaccine.status }}
              </span>
            </div>
          </div>

          <div>
            <h3 class="text-xs font-bold uppercase tracking-wide text-slate-500 mb-2">Description</h3>
            <p class="text-sm text-slate-700 leading-relaxed bg-slate-50 rounded-lg p-4">{{ selectedVaccine.description }}</p>
          </div>

          <div class="bg-slate-50 rounded-lg divide-y divide-slate-200">
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Abbreviation</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedVaccine.abbreviation }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Target Disease</span>
              <span class="text-sm font-medium text-slate-900 text-right ml-4">{{ selectedVaccine.targetDisease }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Recommended Age</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedVaccine.recommendedAge }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Required Doses</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedVaccine.doses }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Dose Interval</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedVaccine.interval }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Administration Route</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedVaccine.route }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Date Created</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedVaccine.created }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Last Updated</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedVaccine.updated }}</span>
            </div>
          </div>
        </div>

        <div class="border-t border-slate-200 p-4 flex items-center gap-2 shrink-0">
          <button @click="openEditModal(selectedVaccine)" class="flex-1 text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors">Edit Vaccine</button>
          <button @click="closeDrawer" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Close</button>
        </div>
      </aside>
    </transition>

    <!-- ============================ ADD / EDIT VACCINE MODAL ============================ -->
    <transition name="fade">
      <div v-if="showFormModal" class="fixed inset-0 bg-slate-900/40 z-40 flex items-center justify-center p-4" @click.self="showFormModal = false">
        <div class="bg-white rounded-xl shadow-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto">
          <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200">
            <h2 class="text-base font-bold text-slate-900">{{ formMode === 'add' ? 'Add Vaccine' : 'Edit Vaccine' }}</h2>
            <button @click="showFormModal = false" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
          </div>

          <div class="p-6 grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Vaccine Name</label>
              <input v-model="vaccineForm.name" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Abbreviation</label>
              <input v-model="vaccineForm.abbreviation" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div class="sm:col-span-2">
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Description</label>
              <textarea v-model="vaccineForm.description" rows="2" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors resize-none"></textarea>
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Target Disease</label>
              <input v-model="vaccineForm.targetDisease" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Recommended Age</label>
              <input v-model="vaccineForm.recommendedAge" type="text" placeholder="e.g. 6, 10, 14 weeks" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Age Category</label>
              <select v-model="vaccineForm.ageCategory" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors">
                <option v-for="cat in ageCategoryOptions" :key="cat" :value="cat">{{ cat }}</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Administration Route</label>
              <select v-model="vaccineForm.route" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors">
                <option>Intramuscular</option>
                <option>Intradermal</option>
                <option>Subcutaneous</option>
                <option>Oral</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Number of Required Doses</label>
              <input v-model.number="vaccineForm.doses" type="number" min="1" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Dose Interval</label>
              <input v-model="vaccineForm.interval" type="text" placeholder="e.g. 4 weeks apart" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Status</label>
              <select v-model="vaccineForm.status" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors">
                <option>Active</option>
                <option>Inactive</option>
              </select>
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 px-6 py-4 border-t border-slate-200">
            <button @click="showFormModal = false" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Cancel</button>
            <button @click="saveVaccine" class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors">Save Vaccine</button>
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