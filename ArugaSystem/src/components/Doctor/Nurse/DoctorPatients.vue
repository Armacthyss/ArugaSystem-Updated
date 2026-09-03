<template>
  <div class="flex h-screen bg-slate-50 font-sans antialiased text-slate-900">

    <!-- SIDEBAR -->
    <aside class="w-64 bg-white border-r border-slate-200 flex flex-col shrink-0">
      <div class="p-6">
        <div class="flex items-center gap-3 px-2">
          <div class="w-8 h-8 bg-emerald-600 rounded flex items-center justify-center text-white font-bold">A</div>
          <div>
            <span class="text-xl font-bold tracking-tight">Aruga</span>
            <p class="text-xs text-slate-400">Pediatric Health System</p>
          </div>
        </div>
      </div>
      <nav class="flex-1 px-3 space-y-1 overflow-y-auto">
        <button v-for="item in navItems" :key="item.id"
          @click="$router.push(item.path)"
          class="w-full flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors cursor-pointer"
          :class="$route.path === item.path ? 'bg-emerald-50 text-emerald-700' : 'text-slate-500 hover:bg-slate-50 hover:text-slate-900'">
          <component :is="item.icon" class="w-4 h-4" />
          {{ item.label }}
        </button>
      </nav>
      <div class="p-3 border-t border-slate-200">
        <button @click="logout" class="w-full flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium text-slate-500 hover:bg-slate-50 hover:text-slate-900 transition-colors">
          <LogOut class="w-4 h-4" /> Logout
        </button>
      </div>
    </aside>

    <!-- MAIN -->
    <div class="flex flex-col flex-1 overflow-hidden">

      <!-- TOP BAR -->
      <header class="h-16 bg-white border-b border-slate-200 flex items-center justify-between px-6 shrink-0">
        <div></div>
        <div class="flex items-center gap-4">
          <button class="relative p-2 text-slate-400 hover:text-slate-600 transition-colors">
            <Bell class="w-5 h-5" />
            <span class="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full"></span>
          </button>
          <div class="flex items-center gap-3">
            <div class="text-right">
              <p class="text-sm font-semibold">{{ doctor.fullName }}</p>
              <p class="text-xs text-slate-400">{{ doctor.userType }}</p>
            </div>
            <div class="w-9 h-9 bg-emerald-700 rounded-full flex items-center justify-center text-white text-sm font-bold">
              {{ doctorInitials }}
            </div>
          </div>
        </div>
      </header>

      <!-- PAGE CONTENT -->
      <main class="flex-1 overflow-y-auto p-6">

        <div class="mb-6">
          <h1 class="text-2xl font-bold text-slate-800">Patients (Children)</h1>
          <p class="text-sm text-slate-500 mt-1">Manage pediatric patient records and vaccinations</p>
        </div>

        <!-- FILTER ROW -->
        <div class="flex items-center gap-3 mb-5">
          <div class="relative flex-1 max-w-sm">
            <Search class="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" />
            <input v-model="searchQuery" type="text" placeholder="Search by name, ID, or parent..."
              class="w-full pl-9 pr-4 py-2 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 bg-white" />
          </div>
          <select v-model="statusFilter"
            class="text-sm border border-slate-200 rounded-lg px-3 py-2 focus:outline-none focus:ring-2 focus:ring-emerald-500 bg-white text-slate-600">
            <option value="">All Status</option>
            <option value="Due Soon">Due Soon</option>
            <option value="Overdue">Overdue</option>
            <option value="Up to Date">Up to Date</option>
            <option value="Completed">Completed</option>
            <option value="Not Started">Not Started</option>
          </select>
        </div>

        <!-- TABLE -->
        <div class="bg-white rounded-xl border border-slate-200 overflow-hidden">
          <div v-if="loading" class="flex items-center justify-center py-16">
            <Loader2 class="w-5 h-5 animate-spin text-slate-400" />
            <span class="ml-2 text-sm text-slate-400">Loading patients...</span>
          </div>

          <div v-else-if="filteredPatients.length === 0" class="flex flex-col items-center justify-center py-16 text-slate-400">
            <Users class="w-10 h-10 mb-3 opacity-30" />
            <p class="text-sm">No patients found</p>
          </div>

          <div v-else class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead class="bg-slate-50 border-b border-slate-200">
                <tr class="text-xs text-slate-500 uppercase tracking-wide">
                  <th class="px-5 py-3.5 text-left font-medium">Profile</th>
                  <th class="px-5 py-3.5 text-left font-medium">Child ID</th>
                  <th class="px-5 py-3.5 text-left font-medium">Full Name</th>
                  <th class="px-5 py-3.5 text-left font-medium">Age</th>
                  <th class="px-5 py-3.5 text-left font-medium">Sex</th>
                  <th class="px-5 py-3.5 text-left font-medium">Parent Name</th>
                  <th class="px-5 py-3.5 text-left font-medium">Next Dose Due</th>
                  <th class="px-5 py-3.5 text-left font-medium">Status</th>
                  <th class="px-5 py-3.5 text-left font-medium">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr v-for="child in paginatedPatients" :key="child.childID"
                  class="hover:bg-slate-50 transition-colors">
                  <td class="px-5 py-3.5">
                    <div class="w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold text-white"
                      :style="{ backgroundColor: avatarColor(child.fullName) }">
                      {{ initials(child.fullName) }}
                    </div>
                  </td>
                  <td class="px-5 py-3.5 text-slate-500 font-mono text-xs">
                    {{ child.childID.substring(0, 8).toUpperCase() }}
                  </td>
                  <td class="px-5 py-3.5 font-medium text-slate-800">{{ child.fullName }}</td>
                  <td class="px-5 py-3.5 text-slate-500">{{ child.age }}</td>
                  <td class="px-5 py-3.5 text-slate-500">{{ child.sex }}</td>
                  <td class="px-5 py-3.5 text-slate-500">{{ child.parentName }}</td>
                  <td class="px-5 py-3.5 text-slate-500 text-xs">
                    <span v-if="child.nextDueDate">{{ formatDate(child.nextDueDate) }}</span>
                    <span v-else-if="child.vaccineStatus === 'Completed'" class="text-emerald-600 font-medium">Series complete</span>
                    <span v-else class="text-slate-400">—</span>
                  </td>
                  <td class="px-5 py-3.5">
                    <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                      :class="statusClass(child.vaccineStatus)">
                      {{ child.vaccineStatus }}
                    </span>
                  </td>
                  <td class="px-5 py-3.5">
                    <div class="flex items-center gap-2">
                      <button @click="openViewModal(child)" title="View Profile"
                        class="p-1.5 text-slate-400 hover:text-emerald-600 hover:bg-emerald-50 rounded-lg transition-colors">
                        <Eye class="w-4 h-4" />
                      </button>
                      <button @click="openVaccinateModal(child)" title="Record Vaccination"
                        class="p-1.5 text-slate-400 hover:text-blue-600 hover:bg-blue-50 rounded-lg transition-colors">
                        <Syringe class="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- PAGINATION -->
          <div v-if="filteredPatients.length > 0" class="flex items-center justify-between px-5 py-3.5 border-t border-slate-100">
            <p class="text-xs text-slate-400">
              Showing {{ (currentPage - 1) * pageSize + 1 }}–{{ Math.min(currentPage * pageSize, filteredPatients.length) }}
              of {{ filteredPatients.length }} patients
            </p>
            <div class="flex items-center gap-2">
              <button @click="currentPage--" :disabled="currentPage === 1"
                class="p-1.5 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors">
                <ChevronLeft class="w-4 h-4" />
              </button>
              <span class="text-xs text-slate-600 px-2">Page {{ currentPage }}</span>
              <button @click="currentPage++" :disabled="currentPage >= totalPages"
                class="p-1.5 rounded-lg text-slate-400 hover:text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors">
                <ChevronRight class="w-4 h-4" />
              </button>
            </div>
          </div>
        </div>

        <!-- STATUS LEGEND -->
        <div class="mt-4 flex flex-wrap items-center gap-4 text-xs text-slate-500">
          <span class="font-medium text-slate-400">Status guide:</span>
          <span class="flex items-center gap-1.5"><span class="w-2 h-2 rounded-full bg-amber-400"></span>Due Soon — next dose within 14 days</span>
          <span class="flex items-center gap-1.5"><span class="w-2 h-2 rounded-full bg-red-400"></span>Overdue — past 14-day catch-up window</span>
          <span class="flex items-center gap-1.5"><span class="w-2 h-2 rounded-full bg-blue-400"></span>Up to Date — next dose not yet due</span>
          <span class="flex items-center gap-1.5"><span class="w-2 h-2 rounded-full bg-emerald-500"></span>Completed — full vaccine series done</span>
          <span class="flex items-center gap-1.5"><span class="w-2 h-2 rounded-full bg-slate-400"></span>Not Started — no vaccines recorded yet</span>
        </div>

      </main>
    </div>

    <!-- ── VIEW PROFILE MODAL ───────────────────────────────────────────── -->
    <Transition name="modal">
      <div v-if="showViewModal && selectedChild"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm"
        @click.self="showViewModal = false">
        <div class="bg-white rounded-2xl shadow-2xl w-full max-w-lg overflow-hidden">

          <div class="bg-emerald-600 px-6 py-5 flex items-center justify-between">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-xl bg-white/20 flex items-center justify-center text-white font-bold text-sm">
                {{ initials(selectedChild.fullName) }}
              </div>
              <div>
                <p class="text-white font-bold leading-tight">{{ selectedChild.fullName }}</p>
                <p class="text-emerald-200 text-xs mt-0.5">Patient Profile</p>
              </div>
            </div>
            <button @click="showViewModal = false" class="text-white/70 hover:text-white text-xl">✕</button>
          </div>

          <div class="p-6">
            <div class="grid grid-cols-2 gap-3 mb-5">
              <div class="bg-slate-50 rounded-xl px-4 py-3">
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Age</p>
                <p class="text-sm font-bold text-slate-800">{{ selectedChild.age }}</p>
              </div>
              <div class="bg-slate-50 rounded-xl px-4 py-3">
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Sex</p>
                <p class="text-sm font-bold text-slate-800">{{ selectedChild.sex }}</p>
              </div>
              <div class="bg-slate-50 rounded-xl px-4 py-3">
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Parent / Guardian</p>
                <p class="text-sm font-bold text-slate-800">{{ selectedChild.parentName }}</p>
              </div>
              <div class="bg-slate-50 rounded-xl px-4 py-3">
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Vaccine Status</p>
                <span class="inline-flex items-center px-2 py-0.5 rounded-full text-xs font-medium"
                  :class="statusClass(selectedChild.vaccineStatus)">
                  {{ selectedChild.vaccineStatus }}
                </span>
              </div>
              <div class="bg-slate-50 rounded-xl px-4 py-3 col-span-2">
                <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Next Dose Due</p>
                <p class="text-sm font-bold text-slate-800">
                  {{ selectedChild.nextDueDate
                    ? formatDate(selectedChild.nextDueDate) + ' — ' + selectedChild.nextVaccineName
                    : selectedChild.vaccineStatus === 'Completed' ? 'Series complete' : '—' }}
                </p>
              </div>
            </div>

            <div>
              <p class="text-xs font-semibold text-slate-600 mb-3 uppercase tracking-wide">Vaccination History</p>
              <div v-if="loadingChildRecords" class="py-4 text-center text-slate-400 text-sm">Loading...</div>
              <div v-else-if="childRecords.length === 0" class="py-4 text-center text-slate-400 text-sm">No records yet.</div>
              <div v-else class="space-y-2 max-h-52 overflow-y-auto">
                <div v-for="rec in childRecords" :key="rec.recordID"
                  class="flex items-center justify-between px-3 py-2.5 bg-slate-50 rounded-lg">
                  <div>
                    <p class="text-xs font-semibold text-slate-800">{{ rec.vaccineName }} · Dose {{ rec.doseNumber }}</p>
                    <p class="text-[10px] text-slate-400 mt-0.5">
                      {{ rec.dateAdministered ? formatDate(new Date(rec.dateAdministered)) : 'Not yet given' }}
                      <span v-if="rec.administeredByName" class="ml-1">· {{ rec.administeredByName }}</span>
                    </p>
                  </div>
                  <span class="text-[10px] px-2 py-0.5 rounded-full font-medium"
                    :class="rec.status === 'Completed' ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-100 text-amber-700'">
                    {{ rec.status }}
                  </span>
                </div>
              </div>
            </div>
          </div>

          <div class="px-6 pb-5 flex gap-3">
            <button @click="showViewModal = false"
              class="flex-1 px-4 py-2.5 text-sm font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50 transition-colors">
              Close
            </button>
            <button @click="showViewModal = false; openVaccinateModal(selectedChild)"
              class="flex-1 px-4 py-2.5 text-sm font-medium text-white bg-emerald-600 rounded-lg hover:bg-emerald-700 transition-colors flex items-center justify-center gap-2">
              <Syringe class="w-4 h-4" /> Record Vaccine
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- ── RECORD VACCINATION MODAL (Option B) ─────────────────────────────── -->
    <Transition name="modal">
      <div v-if="showVaccinateModal && selectedChild"
        class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/40 backdrop-blur-sm"
        @click.self="showVaccinateModal = false">
        <div class="bg-white rounded-2xl shadow-2xl w-full max-w-md overflow-hidden">

          <div class="bg-emerald-600 px-6 py-5 flex items-center justify-between">
            <div>
              <p class="text-white font-bold leading-tight">Record Vaccination</p>
              <p class="text-emerald-200 text-xs mt-0.5">{{ selectedChild.fullName }}</p>
            </div>
            <button @click="showVaccinateModal = false" class="text-white/70 hover:text-white text-xl">✕</button>
          </div>

          <div class="p-6 space-y-4">

            <!-- Patient summary -->
            <div class="bg-slate-50 rounded-xl p-4 flex items-center gap-3">
              <div class="w-10 h-10 rounded-full flex items-center justify-center text-sm font-bold text-white"
                :style="{ backgroundColor: avatarColor(selectedChild.fullName) }">
                {{ initials(selectedChild.fullName) }}
              </div>
              <div>
                <p class="font-semibold text-slate-800">{{ selectedChild.fullName }}</p>
                <p class="text-xs text-slate-500">{{ selectedChild.age }} · {{ selectedChild.sex }} · Parent: {{ selectedChild.parentName }}</p>
              </div>
            </div>

            <!-- VACCINE selector (Option B: pick from all vaccines + doses) -->
            <div>
              <label class="text-xs font-medium text-slate-600 uppercase tracking-wide block mb-1.5">
                Vaccine <span class="text-red-400">*</span>
              </label>
              <div v-if="loadingVaccines" class="text-xs text-slate-400 py-2">Loading vaccines...</div>
              <select v-else v-model="vaccinateForm.vaccineId"
                @change="vaccinateForm.doseNumber = ''"
                class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500">
                <option value="">Select a vaccine...</option>
                <option v-for="v in vaccineList" :key="v.vaccineID" :value="v.vaccineID">
                  {{ v.vaccineName }}
                </option>
              </select>
            </div>

            <!-- DOSE NUMBER selector — filtered to selected vaccine -->
            <div>
              <label class="text-xs font-medium text-slate-600 uppercase tracking-wide block mb-1.5">
                Dose Number <span class="text-red-400">*</span>
              </label>
              <select v-model="vaccinateForm.doseNumber"
                :disabled="!vaccinateForm.vaccineId"
                class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 disabled:opacity-50 disabled:cursor-not-allowed">
                <option value="">Select dose...</option>
                <option v-for="d in selectedVaccineDoses" :key="d.doseNumber" :value="d.doseNumber">
                  Dose {{ d.doseNumber }}
                </option>
              </select>
            </div>

            <div>
              <label class="text-xs font-medium text-slate-600 uppercase tracking-wide block mb-1.5">
                Date Administered <span class="text-red-400">*</span>
              </label>
              <input type="date" v-model="vaccinateForm.dateAdministered"
                class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500" />
            </div>

            <div>
              <label class="text-xs font-medium text-slate-600 uppercase tracking-wide block mb-1.5">
                Lot / Batch Number <span class="text-red-400">*</span>
              </label>
              <input type="text" v-model="vaccinateForm.lotNumber" placeholder="e.g. PV2026-A123"
                class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500" />
            </div>

            <div>
              <label class="text-xs font-medium text-slate-600 uppercase tracking-wide block mb-1.5">
                Remarks / Adverse Reactions
              </label>
              <textarea v-model="vaccinateForm.remarks" rows="2"
                placeholder="e.g. No adverse reaction"
                class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 resize-none">
              </textarea>
            </div>

            <!-- Accountability notice -->
            <div class="flex items-start gap-2 p-3 bg-amber-50 rounded-lg border border-amber-100">
              <Info class="w-4 h-4 text-amber-500 shrink-0 mt-0.5" />
              <p class="text-xs text-amber-700">
                This will be permanently saved under <strong>{{ doctor.fullName }}</strong>
                and visible to the parent.
              </p>
            </div>
          </div>

          <div class="px-6 pb-5 flex gap-3">
            <button @click="showVaccinateModal = false"
              class="flex-1 px-4 py-2.5 text-sm font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50 transition-colors">
              Cancel
            </button>
            <button @click="submitVaccination"
              :disabled="submitting || !vaccinateForm.vaccineId || !vaccinateForm.doseNumber || !vaccinateForm.dateAdministered || !vaccinateForm.lotNumber"
              class="flex-1 px-4 py-2.5 text-sm font-medium text-white bg-emerald-600 rounded-lg hover:bg-emerald-700 disabled:opacity-50 disabled:cursor-not-allowed transition-colors flex items-center justify-center gap-2">
              <Loader2 v-if="submitting" class="w-4 h-4 animate-spin" />
              <CheckCircle v-else class="w-4 h-4" />
              {{ submitting ? 'Saving...' : 'Confirm & Save' }}
            </button>
          </div>
        </div>
      </div>
    </Transition>

    <!-- SUCCESS TOAST -->
    <Transition name="toast">
      <div v-if="toast.show"
        class="fixed bottom-6 right-6 z-50 bg-emerald-600 text-white px-5 py-3.5 rounded-xl shadow-lg flex items-center gap-3 text-sm font-medium">
        <CheckCircle class="w-5 h-5" />
        {{ toast.message }}
      </div>
    </Transition>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import axios from 'axios'
import {
  Home, Users, Calendar, Syringe, FileText, Settings,
  Search, Bell, LogOut, Loader2, X, CheckCircle, Info,
  Eye, ChevronLeft, ChevronRight
} from 'lucide-vue-next'

const API    = 'http://localhost:57147'
const router = useRouter()
const route  = useRoute()

// ── Auth ──────────────────────────────────────────────────────────────────
const doctor = ref({ userId: '', fullName: '', userType: '', prcNo: '' })
onMounted(() => {
  const stored = localStorage.getItem('aruga_user')
//  if (!stored) { router.push('/'); return }
  const u = JSON.parse(stored)
  doctor.value = {
    userId:   u.UserID,
    fullName: `${u.FirstName} ${u.LastName}`,
    userType: u.UserType,
    prcNo:    u.PRCNo || '',
  }
  fetchPatients()
  fetchVaccines()   // ← preload vaccines for the modal
})

const doctorInitials = computed(() =>
  doctor.value.fullName.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
)

// ── Nav ───────────────────────────────────────────────────────────────────
const navItems = [
  { id: 'home',     label: 'Home',                path: '/doctor/home',     icon: Home     },
  { id: 'patients', label: 'Patients (Children)', path: '/doctor/patients', icon: Users    },
  { id: 'calendar', label: 'Calendar',            path: '/doctor/calendar', icon: Calendar },
  { id: 'records',  label: 'Vaccination Records', path: '/doctor/records',  icon: Syringe  },
  { id: 'reports',  label: 'Reports',             path: '/doctor/reports',  icon: FileText },
  { id: 'account',  label: 'My Account',          path: '/doctor/account',  icon: Settings },
]

// ── DOH Vaccine master ────────────────────────────────────────────────────
const VACCINE_MASTER = [
  { vaccineId: 1, name: 'BCG Vaccine',                      totalDoses: 1 },
  { vaccineId: 2, name: 'Hepatitis B Vaccine',              totalDoses: 1 },
  { vaccineId: 3, name: 'Pentavalent (DPT-Hep B-HIB)',      totalDoses: 3, gap: 28 },
  { vaccineId: 4, name: 'Oral Polio Vaccine (OPV)',         totalDoses: 3, gap: 28 },
  { vaccineId: 5, name: 'Inactivated Polio Vaccine (IPV)',  totalDoses: 2, gap: 165 },
  { vaccineId: 6, name: 'Pneumococcal Conj. Vaccine (PCV)', totalDoses: 3, gap: 28 },
  { vaccineId: 7, name: 'MMR Vaccine',                      totalDoses: 2, gap: 90 },
]
const TOTAL_DOSES = VACCINE_MASTER.reduce((sum, v) => sum + v.totalDoses, 0)

// ── Data ──────────────────────────────────────────────────────────────────
const patients     = ref([])
const loading      = ref(false)
const searchQuery  = ref('')
const statusFilter = ref('')
const currentPage  = ref(1)
const pageSize     = ref(8)

async function fetchPatients() {
  loading.value = true
  try {
    const childrenRes = await axios.get(`${API}/api/Children/all`)
    const children    = childrenRes.data

    let allRecords = []
    try {
      const recRes = await axios.get(`${API}/api/VaccinationRecords/all`)
      allRecords = recRes.data
    } catch {
      for (const c of children) {
        try {
          const r = await axios.get(`${API}/api/VaccinationRecords/child/${c.childID}`)
          allRecords.push(...r.data.map(rec => ({ ...rec, childID: c.childID })))
        } catch {}
      }
    }

    const today = new Date()
    today.setHours(0, 0, 0, 0)

    patients.value = await Promise.all(children.map(async c => {
      const childRecords = allRecords.filter(r =>
        (r.childID ?? r.ChildID)?.toString().toLowerCase() === c.childID?.toString().toLowerCase()
      )

      const parentName = c.parentName || '—'

      const hasAnyRecords  = childRecords.length > 0
      const completedCount = childRecords.filter(r => r.status === 'Completed').length
      const allDone        = hasAnyRecords && completedCount >= TOTAL_DOSES

      const pendingRecords = childRecords
        .filter(r => r.status !== 'Completed' && r.scheduledDate)
        .sort((a, b) => new Date(a.scheduledDate) - new Date(b.scheduledDate))

      const nextRecord      = pendingRecords[0] ?? null
      const nextDueDate     = nextRecord?.scheduledDate ? new Date(nextRecord.scheduledDate) : null
      const nextVaccineName = nextRecord
        ? (nextRecord.vaccineName ?? `Vaccine Dose ${nextRecord.doseNumber}`)
        : null

      let vaccineStatus = 'Not Started'
      if (!hasAnyRecords) {
        vaccineStatus = 'Not Started'
      } else if (allDone) {
        vaccineStatus = 'Completed'
      } else if (nextDueDate) {
        const diffDays = Math.floor((nextDueDate - today) / 86400000)
        if (diffDays < -14)      vaccineStatus = 'Overdue'
        else if (diffDays <= 14) vaccineStatus = 'Due Soon'
        else                     vaccineStatus = 'Up to Date'
      } else {
        vaccineStatus = 'Up to Date'
      }

      return {
        childID:        c.childID,
        fullName:       `${c.firstName} ${c.lastName}`,
        age:            computeAge(c.birthDate),
        sex:            c.sex ?? '—',
        parentName,
        birthDate:      c.birthDate,
        nextDueDate,
        nextVaccineName,
        vaccineStatus,
        completedCount,
      }
    }))
  } catch (err) {
    console.error('fetchPatients error:', err)
  } finally {
    loading.value = false
  }
}

function computeAge(birthDate) {
  if (!birthDate) return '—'
  const birth = new Date(birthDate)
  const now   = new Date()
  let years   = now.getFullYear() - birth.getFullYear()
  let months  = now.getMonth() - birth.getMonth()
  if (months < 0) { years--; months += 12 }
  if (years === 0) return `${months} mo${months !== 1 ? 's' : ''}`
  if (months === 0) return `${years} yr${years !== 1 ? 's' : ''}`
  return `${years} yr${years !== 1 ? 's' : ''} ${months} mo${months !== 1 ? 's' : ''}`
}

// ── Filters / Pagination ──────────────────────────────────────────────────
const filteredPatients = computed(() => {
  let list = patients.value
  if (searchQuery.value) {
    const q = searchQuery.value.toLowerCase()
    list = list.filter(c =>
      c.fullName.toLowerCase().includes(q) ||
      c.childID.toLowerCase().includes(q) ||
      c.parentName.toLowerCase().includes(q)
    )
  }
  if (statusFilter.value)
    list = list.filter(c => c.vaccineStatus === statusFilter.value)
  return list
})

const totalPages = computed(() => Math.ceil(filteredPatients.value.length / pageSize.value))
const paginatedPatients = computed(() => {
  const start = (currentPage.value - 1) * pageSize.value
  return filteredPatients.value.slice(start, start + pageSize.value)
})

// ── VIEW MODAL ────────────────────────────────────────────────────────────
const showViewModal       = ref(false)
const selectedChild       = ref(null)
const childRecords        = ref([])
const loadingChildRecords = ref(false)

async function openViewModal(child) {
  selectedChild.value = child
  showViewModal.value = true
  loadingChildRecords.value = true
  try {
    const res = await axios.get(`${API}/api/VaccinationRecords/child/${child.childID}`)
    childRecords.value = res.data.sort((a, b) =>
      new Date(a.dateAdministered ?? a.scheduledDate) - new Date(b.dateAdministered ?? b.scheduledDate)
    )
  } catch { childRecords.value = [] }
  finally { loadingChildRecords.value = false }
}

// ── VACCINE LIST (Option B) ───────────────────────────────────────────────
// vaccineList shape: [{ vaccineID, vaccineName, doses: [{ doseNumber, minIntervalDays }] }]
const vaccineList    = ref([])
const loadingVaccines = ref(false)

async function fetchVaccines() {
  loadingVaccines.value = true
  try {
    const res = await axios.get(`${API}/api/Vaccines/with-doses`)
    vaccineList.value = res.data
  } catch (err) {
    console.error('fetchVaccines error:', err)
    vaccineList.value = []
  } finally {
    loadingVaccines.value = false
  }
}

// Doses for whichever vaccine is currently selected in the form
const selectedVaccineDoses = computed(() => {
  if (!vaccinateForm.value.vaccineId) return []
  const match = vaccineList.value.find(v => v.vaccineID === vaccinateForm.value.vaccineId)
  return match ? match.doses : []
})

// ── VACCINATE MODAL (Option B) ────────────────────────────────────────────
const showVaccinateModal = ref(false)
const submitting         = ref(false)
const vaccinateForm      = ref({
  vaccineId:        '',
  doseNumber:       '',
  dateAdministered: '',
  lotNumber:        '',
  remarks:          '',
})

function openVaccinateModal(child) {
  selectedChild.value = child
  vaccinateForm.value = {
    vaccineId:        '',
    doseNumber:       '',
    dateAdministered: new Date().toISOString().split('T')[0],
    lotNumber:        '',
    remarks:          '',
  }
  showVaccinateModal.value = true
}

async function submitVaccination() {
  const f = vaccinateForm.value
  if (!f.vaccineId || !f.doseNumber || !f.dateAdministered || !f.lotNumber) return

  submitting.value = true
  try {
    await axios.post(`${API}/api/VaccinationRecords`, {
      childID:           selectedChild.value.childID,
      vaccineID:         f.vaccineId,
      doseNumber:        Number(f.doseNumber),
      dateAdministered:  f.dateAdministered,
      lotNumber:         f.lotNumber,
      remarks:           f.remarks || 'No adverse reaction',
      administeredBy:    doctor.value.userId,
      administeredByName: doctor.value.fullName,
    })
    showVaccinateModal.value = false
    showToast(`Vaccination recorded for ${selectedChild.value.fullName}`)
    await fetchPatients()
  } catch (err) {
    console.error('submitVaccination error:', err)
  } finally {
    submitting.value = false
  }
}

// ── Toast ─────────────────────────────────────────────────────────────────
const toast = ref({ show: false, message: '' })
function showToast(msg) {
  toast.value = { show: true, message: msg }
  setTimeout(() => toast.value.show = false, 3500)
}

// ── Helpers ───────────────────────────────────────────────────────────────
function statusClass(status) {
  return {
    'Due Soon':    'bg-amber-100 text-amber-700',
    'Overdue':     'bg-red-100 text-red-600',
    'Up to Date':  'bg-blue-100 text-blue-700',
    'Completed':   'bg-emerald-100 text-emerald-700',
    'Not Started': 'bg-slate-100 text-slate-500',
  }[status] || 'bg-slate-100 text-slate-600'
}

function formatDate(date) {
  if (!date) return '—'
  try { return new Date(date).toLocaleDateString('en-PH', { month: 'short', day: 'numeric', year: 'numeric' }) }
  catch { return '—' }
}

const AVATAR_COLORS = ['#4a7c59','#6b7c45','#8b5e3c','#4a6fa5','#7b4f8e','#5a7a6b','#8b6914']
function avatarColor(name) {
  if (!name) return AVATAR_COLORS[0]
  return AVATAR_COLORS[name.charCodeAt(0) % AVATAR_COLORS.length]
}
function initials(name) {
  if (!name) return '?'
  return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
}

function logout() {
  localStorage.removeItem('aruga_user')
  localStorage.removeItem('aruga_token')
  router.push('/')
}
</script>

<style scoped>
.modal-enter-active, .modal-leave-active { transition: opacity 0.2s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; }
.modal-enter-active > div, .modal-leave-active > div { transition: transform 0.25s cubic-bezier(0.4,0,0.2,1); }
.modal-enter-from > div, .modal-leave-to > div { transform: scale(0.95); }

.toast-enter-active, .toast-leave-active { transition: all 0.3s ease; }
.toast-enter-from { opacity: 0; transform: translateY(12px); }
.toast-leave-to { opacity: 0; transform: translateY(12px); }
</style>