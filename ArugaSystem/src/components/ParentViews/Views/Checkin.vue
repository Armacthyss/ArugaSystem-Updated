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

            <!-- Children load error -->
            <div v-if="childrenError" class="bg-[#FFF1F0] border border-[#F2B8B8] rounded-[28px] p-5 flex items-center gap-3">
              <span>⚠️</span>
              <p class="text-[11px] text-[#8a1f11] font-bold">{{ childrenError }}</p>
            </div>

            <!-- No linked children -->
            <div v-else-if="!childrenLoading && children.length === 0" class="bg-[#FFF9E6] border border-[#F2E4B8] rounded-[28px] p-5 flex items-center gap-3">
              <span>⚠️</span>
              <p class="text-[11px] text-[#856404] font-bold">No children are linked to this account yet. Please contact the clinic to link a child before checking in.</p>
            </div>

            <!-- ═══════════════ SUCCESS / READY FOR QUEUE STATE ═══════════════ -->
            <div v-if="checkInConfirmed" class="bg-white rounded-[28px] border border-slate-100 shadow-sm p-8">
              <div class="text-center mb-6">
                <div class="w-16 h-16 rounded-full bg-[#f0f2ed] flex items-center justify-center text-3xl mx-auto mb-3">✅</div>
                <h2 class="text-xl font-black text-[#2d3a26]">Checked In</h2>
                <p class="text-xs text-slate-400 mt-1">You're ready — please wait to be added to the queue.</p>
              </div>

              <div class="space-y-3 mb-6">
                <div v-for="item in confirmedChildren" :key="item.child.childID"
                     class="p-4 bg-slate-50 rounded-2xl border border-slate-100 flex items-center justify-between">
                  <div class="flex items-center gap-3">
                    <div class="w-10 h-10 rounded-xl bg-[#f0f2ed] flex items-center justify-center text-xl shrink-0">
                      {{ item.child.sex === 'Female' ? '👧' : '👶' }}
                    </div>
                    <div>
                      <p class="text-xs font-black text-slate-800">{{ item.child.firstName }} {{ item.child.lastName }}</p>
                      <p v-if="item.nextDose" class="text-[10px] text-slate-400 mt-0.5">
                        Next: {{ item.nextDose.name }} · Dose {{ item.nextDose.doseNumber }} · {{ formatDisplayDate(item.nextDose.scheduledDate) }}
                      </p>
                      <p v-else class="text-[10px] text-slate-400 mt-0.5">No pending doses on file</p>
                    </div>
                  </div>
                </div>
              </div>

              <button
                @click="resetCheckIn"
                class="w-full px-10 py-3.5 bg-[#2d3a26] hover:bg-[#3d4d34] text-white rounded-2xl font-black text-xs uppercase tracking-widest transition-all"
              >
                Start a New Check-In
              </button>
            </div>

            <!-- ═══════════════ QR SCANNER ═══════════════ -->
            <div v-else class="bg-[#2d3a26] rounded-4xl p-10 text-center text-white relative overflow-hidden shadow-2xl">
              <div class="absolute inset-0 opacity-5" style="background-image: radial-gradient(circle, #99ad7a 1px, transparent 1px); background-size: 24px 24px;"></div>
              <div class="relative">
                <h2 class="text-2xl font-black mb-2">Scan QR Code</h2>
                <p class="text-white/50 text-xs mb-8">Align the clinic's QR code within the frame to check in</p>

                <div class="relative w-56 h-56 mx-auto mb-8 bg-black rounded-2xl overflow-hidden">

                  <!-- REAL QR CAMERA -->
                  <div id="qr-reader" class="w-full h-full"></div>

                  <!-- Camera inactive message -->
                  <div
                    v-if="!cameraActive"
                    class="absolute inset-0 flex flex-col items-center justify-center pointer-events-none"
                  >
                    <span class="text-4xl opacity-40">📷</span>
                    <span class="text-white/60 text-xs mt-3">Camera not active</span>
                  </div>

                  <!-- Scanner corners -->
                  <div class="absolute top-3 left-3 w-7 h-7 border-t-2 border-l-2 border-[#99ad7a] rounded-tl-lg pointer-events-none"></div>
                  <div class="absolute top-3 right-3 w-7 h-7 border-t-2 border-r-2 border-[#99ad7a] rounded-tr-lg pointer-events-none"></div>
                  <div class="absolute bottom-3 left-3 w-7 h-7 border-b-2 border-l-2 border-[#99ad7a] rounded-bl-lg pointer-events-none"></div>
                  <div class="absolute bottom-3 right-3 w-7 h-7 border-b-2 border-r-2 border-[#99ad7a] rounded-br-lg pointer-events-none"></div>
                </div>

                <button
                  v-if="!cameraActive"
                  @click="startScanner"
                  :disabled="childrenLoading || children.length === 0"
                  class="px-10 py-3.5 bg-[#99ad7a] hover:bg-[#b5c99a] disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-lg"
                >
                  📷 Allow Camera Access
                </button>
                <button
                  v-else
                  @click="stopScanner"
                  class="px-10 py-3.5 bg-slate-700 hover:bg-slate-600 text-white rounded-2xl font-black text-xs uppercase tracking-widest transition-all shadow-lg"
                >
                  ⏹ Stop Scanning
                </button>

                <p v-if="qrError" class="text-red-300 text-xs font-bold mt-3">{{ qrError }}</p>

                <p class="text-white/30 text-[9px] mt-6 uppercase tracking-widest font-bold">Check-in is only available during clinic hours · Mon, Wed, Fri · 8:00 AM – 12:00 PM</p>
              </div>
            </div>
          </div>
        </main>
      </div>

      <!-- ═══════════════ CHILD SELECTION MODAL ═══════════════ -->
      <div v-if="showChildSelection" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 px-4">
        <div class="bg-white rounded-[28px] w-full max-w-md p-6 shadow-2xl">
          <h3 class="text-lg font-black text-[#2d3a26] mb-1">Who are you checking in for?</h3>
          <p class="text-xs text-slate-400 mb-5">Select one or more children linked to your account.</p>

          <div class="space-y-2 max-h-80 overflow-y-auto mb-5">
            <button
              v-for="child in children"
              :key="child.childID"
              type="button"
              @click="toggleChildSelection(child)"
              class="w-full flex items-center gap-3 p-3 rounded-2xl border transition-all text-left"
              :class="isChildSelected(child) ? 'border-[#99ad7a] bg-[#f0f2ed]' : 'border-slate-100 hover:border-slate-200'"
            >
              <div class="w-9 h-9 rounded-xl bg-white border border-slate-100 flex items-center justify-center text-lg shrink-0">
                {{ child.sex === 'Female' ? '👧' : '👶' }}
              </div>
              <div class="flex-1 min-w-0">
                <p class="text-xs font-black text-slate-800 truncate">{{ child.firstName }} {{ child.lastName }}</p>
                <p class="text-[10px] text-slate-400">{{ child.relationshipType || 'Linked child' }}</p>
              </div>
              <div
                class="w-5 h-5 rounded-md border-2 flex items-center justify-center shrink-0"
                :class="isChildSelected(child) ? 'bg-[#99ad7a] border-[#99ad7a]' : 'border-slate-300'"
              >
                <span v-if="isChildSelected(child)" class="text-white text-[10px] font-black">✓</span>
              </div>
            </button>
          </div>

          <p v-if="selectedChildren.length === 0" class="text-[10px] text-slate-400 font-bold mb-3">Select at least one child to continue.</p>

          <div class="flex gap-3">
            <button
              @click="cancelChildSelection"
              class="flex-1 px-6 py-3 rounded-2xl border border-slate-200 text-slate-600 font-black text-xs uppercase tracking-widest hover:bg-slate-50 transition-all"
            >
              Cancel
            </button>
            <button
              @click="confirmChildren"
              :disabled="selectedChildren.length === 0 || confirmLoading"
              class="flex-1 px-6 py-3 rounded-2xl bg-[#99ad7a] hover:bg-[#b5c99a] disabled:opacity-40 disabled:cursor-not-allowed text-white font-black text-xs uppercase tracking-widest transition-all"
            >
              {{ confirmLoading ? 'Confirming…' : 'Confirm' }}
            </button>
          </div>
        </div>
      </div>

      <ProfileModal v-if="showProfile" :parent-data="parentData" :children="children" @close="showProfile = false" />
      <NotificationPanel v-if="showNotifications" :parent-data="parentData" @close="showNotifications = false" />

    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { Html5Qrcode } from 'html5-qrcode'
import { useRouter } from 'vue-router'
import { addDays, format, isMonday, isWednesday, isFriday } from 'date-fns'

import HeaderNav from '../Components/Headernav.vue'
import ChildSidebar from '../Components/Childsidebar.vue'
import ProfileModal from '../Components/Profilemodal.vue'
import NotificationPanel from '../Components/Notificationpanel.vue'

import { getAccount } from '@/utils/auth'
import api from '../Composables/api.js'

const router = useRouter()

// =====================================================
// SESSION / PARENT (same pattern as ParentOverview.vue)
// =====================================================

const parentData = ref(null)

function loadParentSession() {
  const savedAccount = getCurrentParent()

  if (!savedAccount) {
    router.push('/')
    return false
  }

  // Support both possible backend response structures
  parentData.value = savedAccount.user ?? savedAccount

  if (!parentData.value?.parentID) {
    console.error('Invalid parent session:', savedAccount)
    router.push('/')
    return false
  }

  return true
}

function handleLogout() {
  logout()
  router.push('/')
}

// =====================================================
// CHILDREN (retrieved from the backend — DB relationship
// is the source of truth, not localStorage)
// =====================================================

const children = ref([])
const childrenLoading = ref(true)
const childrenError = ref('')

// Kept only for the sidebar/header "currently viewing" UX —
// this selection has no bearing on who can check in.
const selectedChild = ref(null)

async function fetchChildren() {
  if (!parentData.value?.parentID) {
    console.error('No ParentID available')
    childrenLoading.value = false
    return
  }

  childrenLoading.value = true
  childrenError.value = ''

  try {
    const response = await api.get(`/Parents/dashboard/${parentData.value.parentID}`)

    children.value = (response.data ?? []).map(child => ({
      childID: child.childID ?? child.ChildID,
      firstName: child.firstName ?? child.FirstName,
      middleName: child.middleName ?? child.MiddleName,
      lastName: child.lastName ?? child.LastName,
      birthDate: child.birthDate ?? child.BirthDate,
      placeOfBirth: child.placeOfBirth ?? child.PlaceOfBirth,
      sex: child.sex ?? child.Sex,
      barangay: child.barangay ?? child.Barangay,
      address: child.address ?? child.Address,
      healthCenter: child.healthCenter ?? child.HealthCenter,
      relationshipType: child.relationshipType ?? child.RelationshipType,
      isPrimaryContact: child.isPrimaryContact ?? child.IsPrimaryContact,
      canReceiveNotifications: child.canReceiveNotifications ?? child.CanReceiveNotifications,
    }))
  } catch (err) {
    console.error('Failed to load parent children:', err)
    children.value = []
    childrenError.value = 'Unable to load your children right now. Please try again later.'
  } finally {
    childrenLoading.value = false
  }
}

function handleSelectChild(child) {
  // Purely a sidebar/header viewing convenience — does NOT
  // determine who is eligible to check in.
  selectedChild.value = child
}

// =====================================================
// NOTIFICATIONS
// =====================================================

const unreadCount = ref(0)

async function fetchUnreadCount() {
  if (!parentData.value?.parentID) return

  try {
    const response = await api.get(`/Notifications/parent/${parentData.value.parentID}`)
    unreadCount.value = (response.data ?? []).filter(n => !n.isRead).length
  } catch (err) {
    console.error('Failed to load notification count:', err)
    unreadCount.value = 0
  }
}

// =====================================================
// UI PANELS
// =====================================================

const showProfile = ref(false)
const showNotifications = ref(false)

// =====================================================
// QR SCANNER
// =====================================================

const scanner = ref(null)
const cameraActive = ref(false)
const qrScanning = ref(false)
const qrError = ref('')
const qrResult = ref('')

async function startScanner() {
  qrError.value = ''

  if (children.value.length === 0) {
    qrError.value = 'No children are linked to this account.'
    return
  }

  try {
    const devices = await Html5Qrcode.getCameras()
    if (!devices || devices.length === 0) {
      qrError.value = 'No camera found on this device.'
      return
    }

    scanner.value = new Html5Qrcode('qr-reader')

    await scanner.value.start(
      { facingMode: 'environment' },
      { fps: 10, qrbox: { width: 200, height: 200 } },
      (decodedText) => {
        handleQrSuccess(decodedText)
        stopScanner()
      },
      () => {
        // per-frame "no QR found" callback — safe to ignore
      }
    )

    cameraActive.value = true
    qrScanning.value = true
  } catch (err) {
    console.error('Failed to start QR scanner:', err)
    qrError.value = 'Camera access was denied or is unavailable.'
    cameraActive.value = false
  }
}

async function stopScanner() {
  if (!scanner.value) return

  try {
    if (scanner.value.isScanning) {
      await scanner.value.stop()
    }
    await scanner.value.clear()
  } catch (err) {
    console.error('Failed to stop QR scanner:', err)
  } finally {
    scanner.value = null
    cameraActive.value = false
    qrScanning.value = false
  }
}

// =====================================================
// QR SUCCESS → open child-selection modal
// =====================================================

function handleQrSuccess(decodedText) {
  qrResult.value = decodedText
  console.log('Successfully scanned QR:', decodedText)

  // Later this is where the backend will verify the QR
  // (belongs to the clinic, is today's QR, not expired, etc.)
  // via something like: await api.get(`/Queue/validate-qr?code=${qrResult.value}`)

  if (children.value.length === 0) {
    qrError.value = 'No children are linked to this account.'
    return
  }

  selectedChildren.value = []
  showChildSelection.value = true
}

// =====================================================
// CHILD SELECTION MODAL
// =====================================================

const showChildSelection = ref(false)
const selectedChildren = ref([])
const confirmLoading = ref(false)
const checkInConfirmed = ref(false)
const confirmedChildren = ref([])

function toggleChildSelection(child) {
  const index = selectedChildren.value.findIndex(c => c.childID === child.childID)

  if (index >= 0) {
    selectedChildren.value.splice(index, 1)
  } else {
    selectedChildren.value.push(child)
  }
}

function isChildSelected(child) {
  return selectedChildren.value.some(c => c.childID === child.childID)
}

function cancelChildSelection() {
  selectedChildren.value = []
  showChildSelection.value = false
}

// =====================================================
// VACCINATION (DOH schedule) — loaded only for the
// children that were actually selected, after confirm.
// =====================================================

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

async function fetchVaccinationRecords(childId) {
  if (!childId) return []
  try {
    const res = await api.get(`/VaccinationRecords/child/${childId}`)
    return res.data ?? []
  } catch (err) {
    console.error('fetchVaccinationRecords error:', err)
    return []
  }
}

// Pure function version of the old `computedVaccineList` — takes a
// specific child + their records instead of relying on a single
// page-level `selectedChild`, so it works for any number of children.
function computeUpcomingDosesFor(child, records) {
  if (!child?.birthDate) return []
  const birth = new Date(child.birthDate)
  const result = []

  for (const vaccine of VACCINE_MASTER) {
    let prevActualDate = null
    let prevOriginalDate = null

    for (const dose of vaccine.doses) {
      const record = records.find(r =>
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

      prevOriginalDate = originalDueDate
      prevActualDate = administeredDate ?? null

      result.push({
        doseId: `${vaccine.vaccineId}-${dose.n}`,
        vaccineId: vaccine.vaccineId,
        name: vaccine.name,
        doseNumber: dose.n,
        scheduledDate,
        isCompleted: !!record,
      })
    }
  }

  return result
    .filter(d => !d.isCompleted)
    .sort((a, b) => a.scheduledDate - b.scheduledDate)
}

// =====================================================
// CONFIRM CHILDREN → temporary success state
// (queue API integration point — see comment below)
// =====================================================

async function confirmChildren() {
  if (selectedChildren.value.length === 0) return

  confirmLoading.value = true

  try {
    const summaries = []

    for (const child of selectedChildren.value) {
      const records = await fetchVaccinationRecords(child.childID)
      const upcoming = computeUpcomingDosesFor(child, records)
      summaries.push({ child, nextDose: upcoming[0] ?? null })
    }

    confirmedChildren.value = summaries
    showChildSelection.value = false
    checkInConfirmed.value = true

    console.log('Ready for queue:', selectedChildren.value.map(c => c.childID))

    /*
     * QUEUE API GOES HERE LATER.
     *
     * await api.post('/Queue', {
     *   parentID: parentData.value.parentID,
     *   childIDs: selectedChildren.value.map(child => child.childID),
     *   qrCode: qrResult.value,
     * })
     */
  } finally {
    confirmLoading.value = false
  }
}

function resetCheckIn() {
  qrResult.value = ''
  qrError.value = ''
  selectedChildren.value = []
  confirmedChildren.value = []
  checkInConfirmed.value = false
  showChildSelection.value = false
}

// =====================================================
// LIFECYCLE
// =====================================================

onMounted(async () => {
  const validSession = loadParentSession()
  if (!validSession) return

  await fetchChildren()
  await fetchUnreadCount()
})

onBeforeUnmount(() => {
  stopScanner()
})
</script>

<style scoped>
@keyframes scanline {
  0%, 100% { top: 8px; opacity: 1; }
  50% { top: calc(100% - 8px); opacity: 0.6; }
}
#qr-reader :deep(video) {
  width: 100% !important;
  height: 100% !important;
  object-fit: cover !important;
}

#qr-reader :deep(img) {
  display: none !important;
}
</style>