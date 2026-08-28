<template>
  <div class="w-full min-h-screen bg-[#FDFCF7] flex justify-center font-sans antialiased text-slate-900">
    <div class="w-full max-w-312.5 px-6 py-6">

      <HeaderNav
        :parent-data="parentData"
        :children="children"
        :unread-count="unreadCount"
        @open-profile="showProfile = true"
        @open-notifications="showNotifications = true"
        @logout="handleLogout"
        @select-child="handleSelectChild"
      />

      <div class="grid grid-cols-12 gap-8">

        <ChildSidebar :children="children" :selected-child="selectedChild" @select-child="handleSelectChild" />

        <main class="col-span-12 lg:col-span-9">
          <div class="animate-in fade-in zoom-in-95 duration-500 space-y-4">
            <div v-if="selectedChild" class="bg-white rounded-[28px] border border-slate-100 shadow-sm p-5 flex items-center gap-5">
              <div class="w-12 h-12 rounded-2xl bg-[#f0f2ed] flex items-center justify-center text-2xl shrink-0">{{ selectedChild.sex === 'Female' ? '👧' : '👶' }}</div>
              <div class="flex-1 min-w-0">
                <p class="text-[9px] font-black text-[#99ad7a] uppercase tracking-widest mb-0.5">Checking in as</p>
                <p class="text-sm font-black text-[#2d3a26] truncate">{{ selectedChild.firstName }} {{ selectedChild.lastName }}</p>
                <p class="text-[10px] text-slate-400">{{ selectedChild.healthCenter || 'Leveriza Health Center' }}</p>
              </div>
              <div v-if="upcomingDoses.length > 0" class="text-right hidden sm:block shrink-0">
                <p class="text-[9px] font-black text-slate-400 uppercase mb-0.5">Next vaccine</p>
                <p class="text-[10px] font-black text-[#5d6b52]">{{ upcomingDoses[0]?.name }}</p>
                <p class="text-[9px] text-slate-400">Dose {{ upcomingDoses[0]?.doseNumber }} · {{ formatDisplayDate(upcomingDoses[0]?.scheduledDate) }}</p>
              </div>
            </div>
            <div v-else class="bg-[#FFF9E6] border border-[#F2E4B8] rounded-[28px] p-5 flex items-center gap-3">
              <span>⚠️</span>
              <p class="text-[11px] text-[#856404] font-bold">Please select a child from Family Profiles before checking in.</p>
            </div>
            <div class="bg-[#2d3a26] rounded-4xl p-10 text-center text-white relative overflow-hidden shadow-2xl">
              <div class="absolute inset-0 opacity-5" style="background-image: radial-gradient(circle, #99ad7a 1px, transparent 1px); background-size: 24px 24px;"></div>
              <div class="relative">
                <h2 class="text-2xl font-black mb-2">Scan QR Code</h2>
                <p class="text-white/50 text-xs mb-8">Align the clinic's QR code within the frame to check in</p>
                <div class="relative w-56 h-56 mx-auto mb-8">
                  <div class="absolute inset-0 bg-black/50 rounded-2xl"></div>
                  <div class="absolute inset-x-0 top-0 h-0.5 bg-linear-to-r from-transparent via-[#99ad7a] to-transparent shadow-[0_0_12px_#99ad7a]" style="animation: scanline 2s ease-in-out infinite;"></div>
                  <div class="absolute top-2 left-2 w-6 h-6 border-t-2 border-l-2 border-[#99ad7a] rounded-tl-lg"></div>
                  <div class="absolute top-2 right-2 w-6 h-6 border-t-2 border-r-2 border-[#99ad7a] rounded-tr-lg"></div>
                  <div class="absolute bottom-2 left-2 w-6 h-6 border-b-2 border-l-2 border-[#99ad7a] rounded-bl-lg"></div>
                  <div class="absolute bottom-2 right-2 w-6 h-6 border-b-2 border-r-2 border-[#99ad7a] rounded-br-lg"></div>
                  <div class="absolute inset-0 flex items-center justify-center"><span class="text-4xl opacity-20">📷</span></div>
                </div>
                <div class="flex flex-col sm:flex-row gap-3 justify-center">
                  <button class="px-10 py-3.5 bg-[#99ad7a] hover:bg-[#b5c99a] text-white rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-lg">Allow Camera Access</button>
                  <router-link to="/parent/overview" class="px-10 py-3.5 bg-white/10 hover:bg-white/20 text-white rounded-2xl font-black text-xs uppercase tracking-widest transition-all">Cancel</router-link>
                </div>
                <p class="text-white/30 text-[9px] mt-6 uppercase tracking-widest font-bold">Check-in is only available during clinic hours · Mon, Wed, Fri · 8:00 AM – 12:00 PM</p>
              </div>
            </div>
          </div>
        </main>
      </div>

      <ProfileModal v-if="showProfile" :parent-data="parentData" :children="children" @close="showProfile = false" />
      <NotificationPanel v-if="showNotifications" :parent-data="parentData" @close="showNotifications = false" />

    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { addDays, format, isMonday, isWednesday, isFriday } from 'date-fns'
import HeaderNav from '../Components/Headernav.vue'
import ChildSidebar from '../Components/Childsidebar.vue'
import ProfileModal from '../Components/Profilemodal.vue'
import NotificationPanel from '../Components/Notificationpanel.vue'


const API_BASE_URL = 'http://localhost:57147'
const router = useRouter()

// ── State ──────────────────────────────────────────────────────────────────
const parentData        = ref(null)
const children           = ref([])
const selectedChild      = ref(null)
const showProfile        = ref(false)
const showNotifications  = ref(false)
const completedRecords   = ref([])
const unreadCount        = ref(0)

// ── Vaccine master (DOH schedule) — duplicated per-page, see HARD RULE 1 ───
const VACCINE_MASTER = [
  { vaccineId: 1, name: 'BCG Vaccine',                      doses: [{ n: 1, gap: 0   }] },
  { vaccineId: 2, name: 'Hepatitis B Vaccine',              doses: [{ n: 1, gap: 0   }] },
  { vaccineId: 3, name: 'Pentavalent (DPT-Hep B-HIB)',      doses: [{ n: 1, gap: 45  }, { n: 2, gap: 28 }, { n: 3, gap: 28 }] },
  { vaccineId: 4, name: 'Oral Polio Vaccine (OPV)',         doses: [{ n: 1, gap: 45  }, { n: 2, gap: 28 }, { n: 3, gap: 28 }] },
  { vaccineId: 5, name: 'Inactivated Polio Vaccine (IPV)',  doses: [{ n: 1, gap: 105 }, { n: 2, gap: 165}] },
  { vaccineId: 6, name: 'Pneumococcal Conj. Vaccine (PCV)', doses: [{ n: 1, gap: 45  }, { n: 2, gap: 28 }, { n: 3, gap: 28 }] },
  { vaccineId: 7, name: 'MMR Vaccine',                      doses: [{ n: 1, gap: 270 }, { n: 2, gap: 90 }] },
]

function snapToClinicDay(date) {
  let d = new Date(date)
  while (!(isMonday(d) || isWednesday(d) || isFriday(d))) d = addDays(d, 1)
  return d
}
function formatDisplayDate(date) {
  if (!date) return '—'
  try { return format(new Date(date), 'MMM d, yyyy') } catch { return '—' }
}

const computedVaccineList = computed(() => {
  if (!selectedChild.value?.birthDate) return []
  const birth = new Date(selectedChild.value.birthDate)
  const result = []

  for (const vaccine of VACCINE_MASTER) {
    let prevActualDate   = null
    let prevOriginalDate = null

    for (const dose of vaccine.doses) {
      const record = completedRecords.value.find(r =>
        Number(r.vaccineID ?? r.vaccineId) === vaccine.vaccineId &&
        Number(r.doseNumber ?? r.DoseNumber) === dose.n &&
        r.status === 'Completed' &&
        r.dateAdministered
      )

      const originalDueDate = dose.n === 1
        ? snapToClinicDay(addDays(birth, dose.gap))
        : snapToClinicDay(addDays(prevOriginalDate ?? birth, dose.gap))

      let scheduledDate
      if (record) {
        scheduledDate = new Date(record.dateAdministered)
      } else if (dose.n === 1) {
        scheduledDate = snapToClinicDay(addDays(birth, dose.gap))
      } else {
        const base = prevActualDate ?? prevOriginalDate ?? birth
        scheduledDate = snapToClinicDay(addDays(base, dose.gap))
      }

      const administeredDate = record ? new Date(record.dateAdministered) : null
      const wasLate = record ? administeredDate > originalDueDate : false
      const daysLate = wasLate
        ? Math.max(0, Math.round((administeredDate - originalDueDate) / 86400000))
        : 0

      prevOriginalDate = originalDueDate
      prevActualDate   = administeredDate ?? null

      result.push({
        doseId:          `${vaccine.vaccineId}-${dose.n}`,
        vaccineId:       vaccine.vaccineId,
        name:            vaccine.name,
        doseNumber:      dose.n,
        scheduledDate,
        originalDueDate,
        isCompleted:     !!record,
        administeredDate,
        wasLate,
        daysLate,
      })
    }
  }
  return result
})

const upcomingDoses = computed(() =>
  computedVaccineList.value.filter(v => !v.isCompleted).sort((a, b) => a.scheduledDate - b.scheduledDate)
)

// ── Actions ────────────────────────────────────────────────────────────────
function handleLogout() {
  localStorage.removeItem('parentUser')
 // router.push('/Login')
}

function handleSelectChild(child) {
  selectedChild.value = child
  localStorage.setItem('selectedChild', JSON.stringify(child))
  fetchRecords(child.childID)
}

async function fetchRecords(childId) {
  if (!childId) return
  try {
    const res = await axios.get(`${API_BASE_URL}/api/VaccinationRecords/child/${childId}`)
    completedRecords.value = res.data
  } catch (err) {
    console.error('fetchRecords error:', err)
    completedRecords.value = []
  }
}

async function fetchUnreadCount() {
  if (!parentData.value?.parentID) return
  try {
    const res = await axios.get(`${API_BASE_URL}/api/Notifications/parent/${parentData.value.parentID}`)
    unreadCount.value = res.data.filter(n => !n.isRead).length
  } catch {
    unreadCount.value = 0
  }
}

// ── Lifecycle ──────────────────────────────────────────────────────────────
onMounted(async () => {
  const savedUser = localStorage.getItem('parentUser')
  if (!savedUser) {
    // router.push('/Login'); 
     return }
  parentData.value = JSON.parse(savedUser)

  try {
    const res = await axios.get(`${API_BASE_URL}/api/Parents/${parentData.value.parentID}/dashboard`)
    children.value = res.data.map(child => ({
      childID: child.ChildID ?? child.childID,
      firstName: child.FirstName ?? child.firstName,
      middleName: child.MiddleName ?? child.middleName,
      lastName: child.LastName ?? child.lastName,
      birthDate: child.BirthDate ?? child.birthDate,
      placeOfBirth: child.PlaceOfBirth ?? child.placeOfBirth,
      sex: child.Sex ?? child.sex,
      barangay: child.Barangay ?? child.barangay,
      address: child.Address ?? child.address,
      healthCenter: child.HealthCenter ?? child.healthCenter,
      relationshipType: child.RelationshipType ?? child.relationshipType,
      isPrimaryContact: child.IsPrimaryContact ?? child.isPrimaryContact,
      canReceiveNotifications: child.CanReceiveNotifications ?? child.canReceiveNotifications,
      motherName: child.MotherName ?? child.motherName ?? null,
      fatherName: child.FatherName ?? child.fatherName ?? null,
      guardianName: child.GuardianName ?? child.guardianName ?? null,
    }))
  } catch (err) {
    console.error('Error fetching parent dashboard:', err)
    children.value = []
  }

  const savedChildRaw = localStorage.getItem('selectedChild')
  if (savedChildRaw) {
    try {
      const savedChild = JSON.parse(savedChildRaw)
      selectedChild.value = children.value.find(c => c.childID === savedChild.childID) || children.value[0] || null
    } catch {
      selectedChild.value = children.value[0] || null
    }
  } else {
    selectedChild.value = children.value[0] || null
  }

  if (selectedChild.value) {
    localStorage.setItem('selectedChild', JSON.stringify(selectedChild.value))
    await fetchRecords(selectedChild.value.childID)
  }

  await fetchUnreadCount()
})
</script>

<style scoped>
@keyframes scanline {
  0%, 100% { top: 8px; opacity: 1; }
  50% { top: calc(100% - 8px); opacity: 0.6; }
}
</style>