<script setup>
import axios from "axios"
import { ref, computed, onMounted } from "vue"

/* ------------------------------- API config ------------------------------- */
const api = "http://localhost:57147/api/Vaccines"
const doseApi = "http://localhost:57147/api/VaccineDoses"

/* --------------------------------- Sidebar --------------------------------- */
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
  true: { tint: 'bg-emerald-50', text: 'text-emerald-700', dot: 'bg-emerald-500', label: 'Active' },
  false: { tint: 'bg-slate-100', text: 'text-slate-600', dot: 'bg-slate-400', label: 'Inactive' },
}
const getStatusMeta = (status) => statusMeta[String(!!status)]

const ageCategoryOptions = ['Birth', '6 Weeks', '10 Weeks', '14 Weeks', '9 Months', '12 Months', 'Booster']
const infantCategories = ['6 Weeks', '10 Weeks', '14 Weeks', '9 Months', '12 Months']

const routeOptions = ['Intramuscular', 'Intradermal', 'Subcutaneous', 'Oral']

/* -------------------------------- Vaccine data (API) -------------------------------- */
const vaccines = ref([])
const allDoses = ref([])

async function load() {
  const res = await axios.get(api)
  vaccines.value = res.data
}

async function loadAllDoses() {
  const res = await axios.get(doseApi)
  allDoses.value = res.data
}

onMounted(() => {
  load()
  loadAllDoses()
})

/* ------------------------- Doses grouped by vaccine ------------------------- */
const dosesByVaccine = computed(() => {
  const map = {}
  for (const d of allDoses.value) {
    if (!map[d.vaccineID]) map[d.vaccineID] = []
    map[d.vaccineID].push(d)
  }
  Object.values(map).forEach((list) => list.sort((a, b) => a.doseNumber - b.doseNumber))
  return map
})
const getDoses = (vaccineID) => dosesByVaccine.value[vaccineID] || []

/* ---------------------------- Toolbar / filters ---------------------------- */
const search = ref("")
const statusFilter = ref("All")
const ageCategoryFilter = ref("All")

const filteredVaccines = computed(() => {
  return vaccines.value.filter((v) => {
    const q = search.value.trim().toLowerCase()
    const matchesSearch =
      !q ||
      v.vaccineName.toLowerCase().includes(q) ||
      (v.abbreviation || "").toLowerCase().includes(q)
    const matchesStatus =
      statusFilter.value === "All" ||
      (statusFilter.value === "Active") === !!v.status
    const matchesAge =
      ageCategoryFilter.value === "All" || v.ageCategory === ageCategoryFilter.value
    return matchesSearch && matchesStatus && matchesAge
  })
})

/* -------------------------------- Summary ---------------------------------- */
const summary = computed(() => ({
  total: vaccines.value.length,
  active: vaccines.value.filter((v) => v.status).length,
  inactive: vaccines.value.filter((v) => !v.status).length,
  birth: vaccines.value.filter((v) => v.ageCategory === 'Birth').length,
  infant: vaccines.value.filter((v) => infantCategories.includes(v.ageCategory)).length,
  booster: vaccines.value.filter((v) => v.ageCategory === 'Booster').length,
}))

/* ------------------------------ Row actions menu ---------------------------- */
const openMenuId = ref(null)
const toggleMenu = (id) => (openMenuId.value = openMenuId.value === id ? null : id)
const closeMenu = () => (openMenuId.value = null)

async function setStatus(vaccine, activate) {
  const updated = { ...vaccine, status: activate }
  await axios.put(`${api}/${vaccine.vaccineID}`, updated)
  closeMenu()
  load()
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

/* --------------------------------- Create / Edit modal --------------------------------- */
const showModal = ref(false)
const isEdit = ref(false)
const form = ref({})
const savingVaccine = ref(false)
const saveError = ref("")

/* Dose Schedule builder state (lives inside the Add/Edit Vaccine modal) */
const doseSchedule = ref([]) // [{ doseID, doseNumber, minIntervalDays }]
const originalDoseIds = ref(new Set())
const doseScheduleLoading = ref(false)

function addDoseRow() {
  doseSchedule.value.push({
    doseID: 0,
    doseNumber: doseSchedule.value.length + 1,
    minIntervalDays: 0,
  })
}

function removeDoseRow(index) {
  doseSchedule.value.splice(index, 1)
  // renumber remaining doses
  doseSchedule.value.forEach((d, i) => (d.doseNumber = i + 1))
}

async function loadDosesForEdit(vaccineId) {
  doseScheduleLoading.value = true
  try {
    const res = await axios.get(`${doseApi}/vaccine/${vaccineId}`)
    const sorted = [...res.data].sort((a, b) => a.doseNumber - b.doseNumber)
    doseSchedule.value = sorted.map((d) => ({
      doseID: d.doseID,
      doseNumber: d.doseNumber,
      minIntervalDays: d.minIntervalDays,
    }))
    originalDoseIds.value = new Set(sorted.map((d) => d.doseID))
  } catch (err) {
    saveError.value = "Unable to load the dose schedule for this vaccine."
    doseSchedule.value = []
    originalDoseIds.value = new Set()
  } finally {
    doseScheduleLoading.value = false
  }
}

function openCreate() {
  isEdit.value = false
  saveError.value = ""
  form.value = {
    vaccineName: "",
    abbreviation: "",
    description: "",
    targetDisease: "",
    recommendedAge: "",
    ageCategory: "Birth",
    administrationRoute: "Intramuscular",
    status: true,
  }
  doseSchedule.value = [{ doseID: 0, doseNumber: 1, minIntervalDays: 0 }]
  originalDoseIds.value = new Set()
  doseScheduleLoading.value = false
  showModal.value = true
}

function edit(v) {
  isEdit.value = true
  saveError.value = ""
  form.value = { ...v }
  doseSchedule.value = []
  showModal.value = true
  closeMenu()
  loadDosesForEdit(v.vaccineID)
}

async function save() {
  saveError.value = ""
  savingVaccine.value = true
  try {
    if (isEdit.value) {
      const vaccineID = form.value.vaccineID
      await axios.put(`${api}/${vaccineID}`, form.value)

      // Diff dose schedule: delete removed, update existing, insert new
      const currentIds = new Set(
        doseSchedule.value.filter((d) => d.doseID).map((d) => d.doseID)
      )
      const toDelete = [...originalDoseIds.value].filter((id) => !currentIds.has(id))

      await Promise.all(toDelete.map((id) => axios.delete(`${doseApi}/${id}`)))

      await Promise.all(
        doseSchedule.value.map((d, idx) => {
          const doseNumber = idx + 1
          if (d.doseID) {
            return axios.put(`${doseApi}/${d.doseID}`, {
              doseID: d.doseID,
              vaccineID,
              doseNumber,
              minIntervalDays: d.minIntervalDays,
            })
          }
          return axios.post(doseApi, {
            vaccineID,
            doseNumber,
            minIntervalDays: d.minIntervalDays,
          })
        })
      )
    } else {
      const res = await axios.post(api, form.value)
      const vaccineID = res.data.vaccineID ?? res.data.VaccineID

      await Promise.all(
        doseSchedule.value.map((d, idx) =>
          axios.post(doseApi, {
            vaccineID,
            doseNumber: idx + 1,
            minIntervalDays: d.minIntervalDays,
          })
        )
      )
    }

    showModal.value = false
    await Promise.all([load(), loadAllDoses()])
  } catch (err) {
    saveError.value = "Unable to save this vaccine. Please check the details and try again."
  } finally {
    savingVaccine.value = false
  }
}

async function remove(id) {
  if (!confirm("Delete vaccine?")) return
  await axios.delete(`${api}/${id}`)
  closeMenu()
  closeDrawer()
  await Promise.all([load(), loadAllDoses()])
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
                v-model="search"
                type="text"
                placeholder="Search by vaccine name or abbreviation..."
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
                @click="openCreate"
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
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Dose Schedule</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Status</th>
                  <th class="text-right font-semibold text-slate-500 text-xs uppercase tracking-wide px-5 py-3">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="vaccine in filteredVaccines"
                  :key="vaccine.vaccineID"
                  class="border-b border-slate-100 last:border-0 hover:bg-slate-50 transition-colors"
                >
                  <td class="px-5 py-3">
                    <div class="flex items-center gap-3">
                      <div class="w-9 h-9 rounded-lg bg-teal-50 flex items-center justify-center text-sm shrink-0">💉</div>
                      <div class="min-w-0">
                        <p class="font-semibold text-slate-900 whitespace-nowrap">{{ vaccine.vaccineName }}</p>
                        <p class="text-xs text-slate-400 font-mono">ID: {{ vaccine.vaccineID }}</p>
                      </div>
                    </div>
                  </td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ vaccine.abbreviation }}</td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ vaccine.recommendedAge }}</td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ getDoses(vaccine.vaccineID).length }}</td>
                  <td class="px-3 py-3">
                    <div v-if="getDoses(vaccine.vaccineID).length" class="flex flex-wrap gap-1 max-w-[240px]">
                      <span
                        v-for="d in getDoses(vaccine.vaccineID)"
                        :key="d.doseID"
                        class="inline-flex items-center text-xs font-medium px-2 py-0.5 rounded-full bg-teal-50 text-teal-700 whitespace-nowrap"
                      >
                        Dose {{ d.doseNumber }}<span v-if="d.minIntervalDays">&nbsp;(+{{ d.minIntervalDays }}d)</span>
                      </span>
                    </div>
                    <span v-else class="text-xs text-slate-400">No doses configured</span>
                  </td>
                  <td class="px-3 py-3">
                    <span :class="[getStatusMeta(vaccine.status).tint, getStatusMeta(vaccine.status).text]" class="inline-flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full whitespace-nowrap">
                      <span :class="getStatusMeta(vaccine.status).dot" class="w-1.5 h-1.5 rounded-full"></span>
                      {{ getStatusMeta(vaccine.status).label }}
                    </span>
                  </td>
                  <td class="px-5 py-3 text-right relative">
                    <button
                      @click.stop="toggleMenu(vaccine.vaccineID)"
                      class="text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg w-8 h-8 inline-flex items-center justify-center transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
                    >
                      ⋮
                    </button>

                    <div
                      v-if="openMenuId === vaccine.vaccineID"
                      @click.stop
                      class="absolute right-5 top-11 z-30 w-48 bg-white border border-slate-200 rounded-lg shadow-md py-1 text-left"
                    >
                      <button @click="openDrawer(vaccine)" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">View Details</button>
                      <button @click="edit(vaccine)" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">Edit Vaccine</button>
                      <div class="my-1 border-t border-slate-100"></div>
                      <button v-if="!vaccine.status" @click="setStatus(vaccine, true)" class="w-full text-left px-3.5 py-2 text-sm text-emerald-700 hover:bg-emerald-50 transition-colors">Activate</button>
                      <button v-if="vaccine.status" @click="setStatus(vaccine, false)" class="w-full text-left px-3.5 py-2 text-sm text-slate-600 hover:bg-slate-50 transition-colors">Deactivate</button>
                      <button @click="remove(vaccine.vaccineID)" class="w-full text-left px-3.5 py-2 text-sm text-rose-600 hover:bg-rose-50 transition-colors">Delete</button>
                    </div>
                  </td>
                </tr>

                <tr v-if="filteredVaccines.length === 0">
                  <td colspan="7" class="px-5 py-12 text-center text-sm text-slate-400">No vaccines match your search or filters.</td>
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
              <p class="text-base font-bold text-slate-900">{{ selectedVaccine.vaccineName }}</p>
              <p class="text-xs text-slate-400 font-mono mt-0.5">ID: {{ selectedVaccine.vaccineID }}</p>
              <span :class="[getStatusMeta(selectedVaccine.status).tint, getStatusMeta(selectedVaccine.status).text]" class="mt-2 inline-flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full">
                <span :class="getStatusMeta(selectedVaccine.status).dot" class="w-1.5 h-1.5 rounded-full"></span>
                {{ getStatusMeta(selectedVaccine.status).label }}
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
              <span class="text-xs text-slate-500">Age Category</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedVaccine.ageCategory }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Administration Route</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedVaccine.administrationRoute }}</span>
            </div>
          </div>

          <div>
            <h3 class="text-xs font-bold uppercase tracking-wide text-slate-500 mb-2">Dose Schedule</h3>
            <div v-if="getDoses(selectedVaccine.vaccineID).length" class="bg-slate-50 rounded-lg divide-y divide-slate-200">
              <div
                v-for="d in getDoses(selectedVaccine.vaccineID)"
                :key="d.doseID"
                class="flex items-center justify-between px-4 py-3"
              >
                <span class="text-sm font-medium text-slate-900">Dose {{ d.doseNumber }}</span>
                <span class="text-xs text-slate-500">{{ d.minIntervalDays }} day minimum interval</span>
              </div>
            </div>
            <p v-else class="text-sm text-slate-400 bg-slate-50 rounded-lg p-4">No doses configured yet.</p>
          </div>
        </div>

        <div class="border-t border-slate-200 p-4 flex items-center gap-2 shrink-0">
          <button @click="edit(selectedVaccine)" class="flex-1 text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors">Edit Vaccine</button>
          <button @click="closeDrawer" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Close</button>
        </div>
      </aside>
    </transition>

    <!-- ============================ ADD / EDIT VACCINE MODAL (Vaccine Builder) ============================ -->
    <transition name="fade">
      <div v-if="showModal" class="fixed inset-0 bg-slate-900/40 z-40 flex items-center justify-center p-4" @click.self="!savingVaccine && (showModal = false)">
        <div class="bg-white rounded-xl shadow-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto">
          <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200">
            <h2 class="text-base font-bold text-slate-900">{{ isEdit ? 'Edit Vaccine' : 'Add Vaccine' }}</h2>
            <button @click="showModal = false" :disabled="savingVaccine" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors disabled:opacity-40">✕</button>
          </div>

          <div class="p-6 space-y-6">
            <p v-if="saveError" class="text-sm text-rose-600 bg-rose-50 border border-rose-100 rounded-lg px-4 py-2.5">{{ saveError }}</p>

            <!-- Core vaccine fields -->
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div>
                <label class="block text-xs font-semibold text-slate-500 mb-1.5">Vaccine Name</label>
                <input v-model="form.vaccineName" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
              </div>
              <div>
                <label class="block text-xs font-semibold text-slate-500 mb-1.5">Abbreviation</label>
                <input v-model="form.abbreviation" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
              </div>
              <div class="sm:col-span-2">
                <label class="block text-xs font-semibold text-slate-500 mb-1.5">Description</label>
                <textarea v-model="form.description" rows="2" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors resize-none"></textarea>
              </div>
              <div>
                <label class="block text-xs font-semibold text-slate-500 mb-1.5">Target Disease</label>
                <input v-model="form.targetDisease" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
              </div>
              <div>
                <label class="block text-xs font-semibold text-slate-500 mb-1.5">Recommended Age</label>
                <input v-model="form.recommendedAge" type="text" placeholder="e.g. 6, 10, 14 weeks" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
              </div>
              <div>
                <label class="block text-xs font-semibold text-slate-500 mb-1.5">Age Category</label>
                <select v-model="form.ageCategory" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors">
                  <option v-for="cat in ageCategoryOptions" :key="cat" :value="cat">{{ cat }}</option>
                </select>
              </div>
              <div>
                <label class="block text-xs font-semibold text-slate-500 mb-1.5">Administration Route</label>
                <select v-model="form.administrationRoute" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors">
                  <option v-for="r in routeOptions" :key="r" :value="r">{{ r }}</option>
                </select>
              </div>
              <div class="flex items-end pb-2.5">
                <label class="inline-flex items-center gap-2 text-sm font-medium text-slate-700">
                  <input type="checkbox" v-model="form.status" class="w-4 h-4 rounded border-slate-300 text-emerald-600 focus:ring-emerald-500" />
                  Active
                </label>
              </div>
            </div>

            <!-- Dose Schedule builder -->
            <div>
              <div class="flex items-center justify-between mb-2">
                <div>
                  <h3 class="text-sm font-bold text-slate-900">Dose Schedule</h3>
                  <p class="text-xs text-slate-500">Dose numbers are automatic. Set the minimum interval before each dose can be given.</p>
                </div>
                <button
                  type="button"
                  @click="addDoseRow"
                  class="text-sm font-semibold px-3 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors shrink-0 focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
                >
                  + Add Dose
                </button>
              </div>

              <div v-if="doseScheduleLoading" class="text-sm text-slate-400 bg-slate-50 rounded-lg px-4 py-8 text-center">
                Loading dose schedule…
              </div>

              <div v-else class="space-y-2">
                <div
                  v-for="(dose, index) in doseSchedule"
                  :key="dose.doseID ? 'd-' + dose.doseID : 'new-' + index"
                  class="flex items-center gap-3 bg-slate-50 border border-slate-200 rounded-lg px-4 py-3"
                >
                  <div class="w-16 shrink-0">
                    <p class="text-xs font-semibold text-slate-500">Dose</p>
                    <p class="text-sm font-bold text-slate-900">{{ index + 1 }}</p>
                  </div>
                  <div class="flex-1 min-w-0">
                    <label class="block text-xs font-semibold text-slate-500 mb-1">Minimum Interval (days)</label>
                    <input
                      v-model.number="dose.minIntervalDays"
                      type="number"
                      min="0"
                      class="w-full text-sm rounded-lg border border-slate-200 bg-white px-3 py-2 focus:outline-none focus:ring-2 focus:ring-emerald-500 transition-colors"
                    />
                  </div>
                  <button
                    type="button"
                    @click="removeDoseRow(index)"
                    class="shrink-0 text-rose-500 hover:bg-rose-50 rounded-lg w-8 h-8 flex items-center justify-center transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-rose-400"
                    title="Remove dose"
                  >
                    ✕
                  </button>
                </div>

                <p v-if="doseSchedule.length === 0" class="text-sm text-slate-400 bg-slate-50 rounded-lg px-4 py-8 text-center">
                  No doses configured yet. Add at least one dose.
                </p>
              </div>
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 px-6 py-4 border-t border-slate-200">
            <button @click="showModal = false" :disabled="savingVaccine" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors disabled:opacity-40">Cancel</button>
            <button
              @click="save"
              :disabled="savingVaccine || doseScheduleLoading"
              class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors disabled:opacity-50"
            >
              {{ savingVaccine ? 'Saving…' : 'Save Vaccine' }}
            </button>
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