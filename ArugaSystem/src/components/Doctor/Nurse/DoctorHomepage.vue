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
        <button
          v-for="item in navItems"
          :key="item.id"
          @click="$router.push(item.path)"
          class="w-full flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors cursor-pointer"
          :class="$route.path === item.path ? 'bg-emerald-50 text-emerald-700' : 'text-slate-500 hover:bg-slate-50 hover:text-slate-900'"
        >
          <component :is="item.icon" class="w-4 h-4" />
          {{ item.label }}
        </button>
      </nav>

      <div class="p-3 border-t border-slate-200">
        <button @click="logout" class="w-full flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium text-slate-500 hover:bg-slate-50 hover:text-slate-900 transition-colors">
          <LogOut class="w-4 h-4" />
          Logout
        </button>
      </div>
    </aside>

    <!-- MAIN CONTENT -->
    <div class="flex flex-col flex-1 overflow-hidden">

      <!-- TOP BAR -->
      <header class="h-16 bg-white border-b border-slate-200 flex items-center justify-between px-6 shrink-0">
        <div class="relative w-80">
        </div>
        <div class="flex items-center gap-4">
          <button class="relative p-2 text-slate-400 hover:text-slate-600 transition-colors">
            <Bell class="w-5 h-5" />
            <span v-if="unreadNotifications > 0" class="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full"></span>
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

        <!-- WELCOME -->
        <div class="mb-6">
          <h1 class="text-2xl font-bold text-slate-800">Welcome back, {{ doctor.fullName }}</h1>
          <p class="text-sm text-slate-500 mt-1">{{ todayFormatted }}</p>
        </div>

        <!-- STAT CARDS -->
        <div class="grid grid-cols-4 gap-4 mb-6">
          <div v-for="stat in statCards" :key="stat.label" class="bg-white rounded-xl border border-slate-200 p-5">
            <div class="flex items-center justify-between mb-3">
              <component :is="stat.icon" class="w-5 h-5" :class="stat.iconColor" />
            </div>
            <p class="text-3xl font-bold text-slate-800">{{ stat.value }}</p>
            <p class="text-xs text-slate-400 mt-1">{{ stat.label }}</p>
          </div>
        </div>

        <div class="grid grid-cols-3 gap-6">

          <!-- PENDING VACCINATIONS TODAY -->
          <div class="col-span-2 bg-white rounded-xl border border-slate-200">
            <div class="flex items-center justify-between px-5 py-4 border-b border-slate-100">
              <div>
                <h2 class="font-semibold text-slate-800">Pending Vaccinations Today</h2>
                <p class="text-xs text-slate-400 mt-0.5">Children scheduled for vaccination today</p>
              </div>
              <button class="text-xs text-emerald-600 hover:underline font-medium" @click="$router.push('/doctor/records')">
                View All Records →
              </button>
            </div>

            <div v-if="loadingPending" class="flex items-center justify-center py-12">
              <Loader2 class="w-5 h-5 animate-spin text-slate-400" />
              <span class="ml-2 text-sm text-slate-400">Loading...</span>
            </div>

            <div v-else-if="pendingToday.length === 0" class="flex flex-col items-center justify-center py-12 text-slate-400">
              <CheckCircle class="w-8 h-8 mb-2 opacity-40" />
              <p class="text-sm">All vaccinations for today are done</p>
            </div>

            <div v-else class="overflow-x-auto">
              <table class="w-full text-sm">
                <thead>
                  <tr class="text-xs text-slate-400 uppercase tracking-wide">
                    <th class="px-5 py-3 text-left font-medium">Child</th>
                    <th class="px-5 py-3 text-left font-medium">Parent</th>
                    <th class="px-5 py-3 text-left font-medium">Vaccine</th>
                    <th class="px-5 py-3 text-left font-medium">Dose</th>
                    <th class="px-5 py-3 text-left font-medium">Action</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-slate-50">
                  <tr v-for="item in pendingToday" :key="item.recordId" class="hover:bg-slate-50 transition-colors">
                    <td class="px-5 py-3">
                      <div class="flex items-center gap-2.5">
                        <div class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold text-white shrink-0"
                          :style="{ backgroundColor: avatarColor(item.childName) }">
                          {{ initials(item.childName) }}
                        </div>
                        <span class="font-medium text-slate-800">{{ item.childName }}</span>
                      </div>
                    </td>
                    <td class="px-5 py-3 text-slate-500">{{ item.parentName }}</td>
                    <td class="px-5 py-3 text-slate-700">{{ item.vaccineName }}</td>
                    <td class="px-5 py-3 text-slate-500">Dose {{ item.doseNumber }}</td>
                    <td class="px-5 py-3">
                      <button
                        @click="openVaccinateModal(item)"
                        class="text-xs bg-emerald-600 hover:bg-emerald-700 text-white px-3 py-1.5 rounded-lg font-medium transition-colors"
                      >
                        Mark Vaccinated
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>

          <!-- RIGHT COLUMN -->
          <div class="flex flex-col gap-4">

            <!-- QUICK ACTIONS -->
            <div class="bg-white rounded-xl border border-slate-200 p-5">
              <h2 class="font-semibold text-slate-800 mb-3">Quick Actions</h2>
              <div class="space-y-2">
                <button v-for="action in quickActions" :key="action.label"
                  @click="$router.push(action.path)"
                  class="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm text-slate-600 hover:bg-slate-50 hover:text-slate-900 transition-colors text-left">
                  <component :is="action.icon" class="w-4 h-4 text-emerald-600 shrink-0" />
                  {{ action.label }}
                </button>
              </div>
            </div>

            <!-- VACCINATED TODAY -->
            <div class="bg-white rounded-xl border border-slate-200 p-5 flex-1">
              <h2 class="font-semibold text-slate-800 mb-3">Vaccinated Today</h2>
              <div v-if="recentlyVaccinated.length === 0" class="text-sm text-slate-400 text-center py-4">
                No vaccinations recorded yet today
              </div>
              <div v-else class="space-y-3">
                <div v-for="rec in recentlyVaccinated" :key="rec.recordId" class="flex items-start gap-3">
                  <div class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold text-white shrink-0 mt-0.5"
                    :style="{ backgroundColor: avatarColor(rec.childName) }">
                    {{ initials(rec.childName) }}
                  </div>
                  <div class="flex-1 min-w-0">
                    <p class="text-sm font-medium text-slate-800 truncate">{{ rec.childName }}</p>
                    <p class="text-xs text-slate-400">{{ rec.vaccineName }} · Dose {{ rec.doseNumber }}</p>
                  </div>
                  <span class="text-xs text-emerald-600 font-medium shrink-0">Done</span>
                </div>
              </div>
            </div>

          </div>
        </div>

      </main>
    </div>

    <!-- MARK AS VACCINATED MODAL -->
    <Transition name="modal">
      <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center p-4">
        <div class="absolute inset-0 bg-black/40 backdrop-blur-sm" @click="closeModal"></div>
        <div class="relative bg-white rounded-2xl shadow-2xl w-full max-w-md p-6 z-10">

          <div class="flex items-center justify-between mb-5">
            <div>
              <h3 class="text-lg font-bold text-slate-800">Confirm Vaccination</h3>
              <p class="text-sm text-slate-400 mt-0.5">{{ selectedItem?.childName }}</p>
            </div>
            <button @click="closeModal" class="text-slate-400 hover:text-slate-600 transition-colors">
              <X class="w-5 h-5" />
            </button>
          </div>

          <!-- Patient Summary -->
          <div class="bg-slate-50 rounded-xl p-4 mb-5 flex items-center gap-3">
            <div class="w-10 h-10 rounded-full flex items-center justify-center text-sm font-bold text-white"
              :style="{ backgroundColor: avatarColor(selectedItem?.childName || '') }">
              {{ initials(selectedItem?.childName || '') }}
            </div>
            <div>
              <p class="font-semibold text-slate-800">{{ selectedItem?.childName }}</p>
              <p class="text-sm text-slate-500">{{ selectedItem?.vaccineName }} — Dose {{ selectedItem?.doseNumber }}</p>
              <p class="text-xs text-slate-400">Parent: {{ selectedItem?.parentName }}</p>
            </div>
          </div>

          <!-- Form Fields -->
          <div class="space-y-4">
            <div>
              <label class="text-xs font-medium text-slate-600 uppercase tracking-wide block mb-1.5">
                Date Administered <span class="text-red-400">*</span>
              </label>
              <input type="date" v-model="form.dateAdministered"
                class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500" />
            </div>
            <div>
              <label class="text-xs font-medium text-slate-600 uppercase tracking-wide block mb-1.5">
                Lot / Batch Number <span class="text-red-400">*</span>
              </label>
              <input type="text" v-model="form.lotNumber" placeholder="e.g. PV2026-A123"
                class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500" />
            </div>
            <div>
              <label class="text-xs font-medium text-slate-600 uppercase tracking-wide block mb-1.5">
                Remarks / Adverse Reactions
              </label>
              <textarea v-model="form.remarks" rows="3"
                placeholder="e.g. No adverse reaction, mild fever possible..."
                class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500 resize-none">
              </textarea>
            </div>
          </div>

          <!-- Accountability Notice -->
          <div class="flex items-start gap-2 mt-4 p-3 bg-amber-50 rounded-lg border border-amber-100">
            <Info class="w-4 h-4 text-amber-500 shrink-0 mt-0.5" />
            <p class="text-xs text-amber-700">
              This record will be permanently saved under <strong>{{ doctor.fullName }}</strong>
              and will be visible to the child's parent.
            </p>
          </div>

          <!-- Actions -->
          <div class="flex gap-3 mt-5">
            <button @click="closeModal"
              class="flex-1 px-4 py-2.5 text-sm font-medium text-slate-600 border border-slate-200 rounded-lg hover:bg-slate-50 transition-colors">
              Cancel
            </button>
            <button @click="submitVaccination"
              :disabled="submitting || !form.dateAdministered || !form.lotNumber"
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
import {
  Home, Users, Calendar, Syringe, FileText, Settings,
  Search, Bell, LogOut, Loader2, X, Info, CheckCircle,
  Clock, AlertCircle, TrendingUp
} from 'lucide-vue-next'

const router = useRouter()
const route  = useRoute()

// ─────────────────────────────────────────────────────────────
// AUTH
// Set localStorage during login with these exact keys
// matching dbo.Users columns
// ─────────────────────────────────────────────────────────────
const doctor = ref({ userId: '', fullName: '', userType: '', prcNo: '' })

onMounted(() => {
  const stored = localStorage.getItem('aruga_user')
  //if (!stored) { router.push('/'); return }
  const u = JSON.parse(stored)
  doctor.value = {
    userId:   u.UserID,
    fullName: `${u.FirstName} ${u.LastName}`,
    userType: u.UserType,  // 'Doctor' or 'Nurse'
    prcNo:    u.PRCNo || '',
  }
  fetchStats()
  fetchPendingToday()
  fetchRecentlyVaccinated()
})

const doctorInitials = computed(() =>
  doctor.value.fullName.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
)

// ─────────────────────────────────────────────────────────────
// NAVIGATION
// ─────────────────────────────────────────────────────────────
const navItems = [
  { id: 'home',     label: 'Home',                path: '/doctor/home',     icon: Home     },
  { id: 'patients', label: 'Patients (Children)', path: '/doctor/patients', icon: Users    },
  { id: 'calendar', label: 'Calendar',            path: '/doctor/calendar', icon: Calendar },
  { id: 'records',  label: 'Vaccination Records', path: '/doctor/records',  icon: Syringe  },
  { id: 'reports',  label: 'Reports',             path: '/doctor/reports',  icon: FileText },
  { id: 'account',  label: 'My Account',          path: '/doctor/account',  icon: Settings },
]

const quickActions = [
  { label: 'Patient List',        path: '/doctor/patients', icon: Users    },
  { label: 'View Calendar',       path: '/doctor/calendar', icon: Calendar },
  { label: 'Vaccination Records', path: '/doctor/records',  icon: Syringe  },
  { label: 'Daily Report',        path: '/doctor/reports',  icon: FileText },
]

// ─────────────────────────────────────────────────────────────
// STATS
// C# endpoint: GET /api/vaccinationrecords/stats
// SQL: COUNT from dbo.VaccinationRecords filtered by date/status
// ─────────────────────────────────────────────────────────────
const stats = ref({ vaccinatedToday: 0, pendingToday: 0, missedTotal: 0, weeklyTotal: 0 })

const statCards = computed(() => [
  { label: 'Vaccinated Today',  value: stats.value.vaccinatedToday, icon: Syringe,     iconColor: 'text-emerald-500' },
  { label: 'Pending Today',     value: stats.value.pendingToday,    icon: Clock,        iconColor: 'text-amber-500'   },
  { label: 'Missed Follow-ups', value: stats.value.missedTotal,     icon: AlertCircle,  iconColor: 'text-red-400'     },
  { label: 'Weekly Total',      value: stats.value.weeklyTotal,     icon: TrendingUp,   iconColor: 'text-blue-400'    },
])

async function fetchStats() {
  try {
    // Wire up when C# controller is ready:
    // const res = await fetch(`${import.meta.env.VITE_API_URL}/api/vaccinationrecords/stats`, {
    //   headers: { Authorization: `Bearer ${localStorage.getItem('aruga_token')}` }
    // })
    // stats.value = await res.json()
    stats.value = { vaccinatedToday: 18, pendingToday: 6, missedTotal: 3, weeklyTotal: 89 }
  } catch (e) { console.error('fetchStats:', e) }
}

// ─────────────────────────────────────────────────────────────
// PENDING VACCINATIONS TODAY
// C# endpoint: GET /api/vaccinationrecords/pending-today
// SQL:
//   SELECT vr.RecordID, c.FirstName+' '+c.LastName AS ChildName,
//          p.FirstName+' '+p.LastName AS ParentName,
//          v.VaccineName, vr.DoseNumber
//   FROM dbo.VaccinationRecords vr
//   JOIN dbo.Children c  ON vr.ChildID   = c.ChildID
//   JOIN dbo.Parents  p  ON c.ParentID   = p.ParentID
//   JOIN dbo.Vaccines v  ON vr.VaccineID = v.VaccineID
//   WHERE vr.ScheduledDate = CAST(GETDATE() AS DATE)
//     AND vr.Status = 'Pending'
// ─────────────────────────────────────────────────────────────
const pendingToday  = ref([])
const loadingPending = ref(false)

async function fetchPendingToday() {
  loadingPending.value = true
  try {
    // const res = await fetch(`${import.meta.env.VITE_API_URL}/api/vaccinationrecords/pending-today`, {
    //   headers: { Authorization: `Bearer ${localStorage.getItem('aruga_token')}` }
    // })
    // pendingToday.value = await res.json()
    pendingToday.value = [
      { recordId: 'rec-001', childName: 'Juan Dela Cruz',  parentName: 'Maria Dela Cruz',  vaccineName: 'Pentavalent',  doseNumber: 3 },
      { recordId: 'rec-002', childName: 'Miguel Santos',   parentName: 'Roberto Santos',   vaccineName: 'Pentavalent',  doseNumber: 2 },
      { recordId: 'rec-003', childName: 'Isabella Garcia', parentName: 'Carmen Garcia',    vaccineName: 'OPV',          doseNumber: 3 },
      { recordId: 'rec-004', childName: 'Lucas Tan',       parentName: 'Michelle Tan',     vaccineName: 'Pneumococcal', doseNumber: 3 },
    ]
  } catch (e) { console.error('fetchPendingToday:', e) }
  finally { loadingPending.value = false }
}

// ─────────────────────────────────────────────────────────────
// VACCINATED TODAY (sidebar)
// C# endpoint: GET /api/vaccinationrecords/completed-today
// SQL: same as above but Status = 'Completed'
//      AND DateAdministered = CAST(GETDATE() AS DATE)
// ─────────────────────────────────────────────────────────────
const recentlyVaccinated = ref([])

async function fetchRecentlyVaccinated() {
  try {
    // const res = await fetch(`${import.meta.env.VITE_API_URL}/api/vaccinationrecords/completed-today`, {
    //   headers: { Authorization: `Bearer ${localStorage.getItem('aruga_token')}` }
    // })
    // recentlyVaccinated.value = await res.json()
    recentlyVaccinated.value = [
      { recordId: 'rec-005', childName: 'Sofia Reyes',     vaccineName: 'MMR',         doseNumber: 1 },
      { recordId: 'rec-006', childName: 'Emma Villanueva', vaccineName: 'Hepatitis B', doseNumber: 1 },
    ]
  } catch (e) { console.error('fetchRecentlyVaccinated:', e) }
}

// ─────────────────────────────────────────────────────────────
// MARK AS VACCINATED
// C# endpoint: PATCH /api/vaccinationrecords/{recordId}
// Body: { dateAdministered, lotNumber, remarks,
//         administeredBy, administeredByName, status }
//
// C# updates dbo.VaccinationRecords:
//   SET Status             = 'Completed'
//       DateAdministered   = @dateAdministered
//       LotNumber          = @lotNumber
//       Remarks            = @remarks
//       AdministeredBy     = @userId       ← GUID FK to dbo.Users
//       AdministeredByName = @fullName     ← stored for parent display
//   WHERE RecordID = @recordId
//
// Parent dashboard then reads AdministeredByName + DateAdministered
// to show who gave the vaccine and when (accountability).
// ─────────────────────────────────────────────────────────────
const showModal    = ref(false)
const selectedItem = ref(null)
const submitting   = ref(false)
const form = ref({ dateAdministered: '', lotNumber: '', remarks: '' })

function openVaccinateModal(item) {
  selectedItem.value = item
  form.value = { dateAdministered: new Date().toISOString().split('T')[0], lotNumber: '', remarks: '' }
  showModal.value = true
}
function closeModal() { showModal.value = false; selectedItem.value = null }

async function submitVaccination() {
  if (!form.value.dateAdministered || !form.value.lotNumber) return
  submitting.value = true
  try {
    const payload = {
      dateAdministered:   form.value.dateAdministered,
      lotNumber:          form.value.lotNumber,
      remarks:            form.value.remarks || 'No adverse reaction',
      administeredBy:     doctor.value.userId,   // GUID → AdministeredBy (FK to Users)
      administeredByName: doctor.value.fullName, // string → AdministeredByName
      status:             'Completed',
    }

    // await fetch(`${import.meta.env.VITE_API_URL}/api/vaccinationrecords/${selectedItem.value.recordId}`, {
    //   method: 'PATCH',
    //   headers: {
    //     'Content-Type': 'application/json',
    //     Authorization: `Bearer ${localStorage.getItem('aruga_token')}`
    //   },
    //   body: JSON.stringify(payload)
    // })

    console.log('PATCH payload:', payload)

    // Optimistic UI: move from pending list to vaccinated sidebar
    pendingToday.value = pendingToday.value.filter(i => i.recordId !== selectedItem.value.recordId)
    recentlyVaccinated.value.unshift({
      recordId:   selectedItem.value.recordId,
      childName:  selectedItem.value.childName,
      vaccineName: selectedItem.value.vaccineName,
      doseNumber: selectedItem.value.doseNumber,
    })
    stats.value.vaccinatedToday++
    stats.value.pendingToday = Math.max(0, stats.value.pendingToday - 1)

    const name = selectedItem.value.childName
    closeModal()
    showToast(`${name}'s vaccination recorded successfully`)
  } catch (e) { console.error('submitVaccination:', e) }
  finally { submitting.value = false }
}

// ─────────────────────────────────────────────────────────────
// TOAST & HELPERS
// ─────────────────────────────────────────────────────────────
const toast = ref({ show: false, message: '' })
function showToast(msg) {
  toast.value = { show: true, message: msg }
  setTimeout(() => toast.value.show = false, 3500)
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

const todayFormatted = computed(() =>
  new Date().toLocaleDateString('en-PH', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })
)
const unreadNotifications = ref(2)

function logout() {
  localStorage.removeItem('aruga_user')
  localStorage.removeItem('aruga_token')
  router.push('/')
}
</script>

<style scoped>
.modal-enter-active, .modal-leave-active { transition: opacity 0.2s ease; }
.modal-enter-from, .modal-leave-to       { opacity: 0; }
.toast-enter-active, .toast-leave-active { transition: all 0.3s ease; }
.toast-enter-from  { opacity: 0; transform: translateY(12px); }
.toast-leave-to    { opacity: 0; transform: translateY(12px); }
</style>