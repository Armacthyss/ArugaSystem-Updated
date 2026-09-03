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
          <div class="space-y-5 animate-in slide-in-from-bottom-4 duration-500">
            <div class="bg-white rounded-4xl border border-slate-100 shadow-sm p-8 flex flex-col md:flex-row justify-between items-center gap-6">
              <div>
                <p class="text-[10px] font-black text-slate-400 uppercase tracking-widest mb-2">Today's Priority Ticket</p>
                <h2 class="text-4xl font-black text-[#2d3a26]">You are Queue <span class="text-[#99ad7a]">#015</span></h2>
                <p class="text-sm font-bold text-slate-500 mt-1">Active for {{ selectedChild ? `${selectedChild.firstName} ${selectedChild.lastName}` : 'Select a child' }}</p>
              </div>
              <div class="bg-[#f0f2ed] px-10 py-6 rounded-[28px] border border-[#99ad7a]/20 text-center min-w-45">
                <p class="text-[10px] font-black text-[#5d6b52] uppercase mb-1">Now Serving</p>
                <p class="text-5xl font-black text-[#2d3a26]">#012</p>
                <p class="text-[10px] font-bold text-[#5d6b52] mt-2 uppercase">3rd in line</p>
              </div>
            </div>

            <div v-if="selectedChild" class="bg-white rounded-4xl border border-slate-100 shadow-sm overflow-hidden">
              <div class="bg-[#5d6b52] px-8 py-5 flex items-center gap-5">
                <div class="w-14 h-14 rounded-2xl bg-white/20 flex items-center justify-center text-3xl shadow">{{ selectedChild.sex === 'Female' ? '👧' : '👶' }}</div>
                <div>
                  <p class="text-white font-black text-lg leading-tight">{{ selectedChild.firstName }} {{ selectedChild.lastName }}</p>
                  <p class="text-[#c8d9b0] text-[10px] font-bold uppercase mt-0.5">{{ selectedChild.healthCenter || 'Leveriza Health Center' }}</p>
                </div>
                <div class="ml-auto text-right hidden sm:block">
                  <p class="text-[10px] text-white/60 font-black uppercase">Patient ID</p>
                  <p class="text-white font-mono text-xs font-bold">#{{ selectedChild.childID.substring(0, 8).toUpperCase() }}</p>
                </div>
              </div>
              <div class="grid grid-cols-2 sm:grid-cols-3 divide-x divide-y divide-slate-50">
                <div class="px-6 py-5"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Date of Birth</p><p class="text-sm font-black text-slate-800">{{ formatDisplayDate(new Date(selectedChild.birthDate)) }}</p></div>
                <div class="px-6 py-5"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Age</p><p class="text-sm font-black text-slate-800">{{ childAge }}</p></div>
                <div class="px-6 py-5"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Sex</p><p class="text-sm font-black text-slate-800">{{ selectedChild.sex || '—' }}</p></div>
                <div class="px-6 py-5"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Birth Weight</p><p class="text-sm font-black text-slate-800">{{ selectedChild.birthWeight ? selectedChild.birthWeight + ' kg' : '—' }}</p></div>
                <div class="px-6 py-5"><p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Birth Height</p><p class="text-sm font-black text-slate-800">{{ selectedChild.birthHeight ? selectedChild.birthHeight + ' cm' : '—' }}</p></div>
                <div class="px-6 py-5">
                  <p class="text-[9px] font-black text-slate-400 uppercase tracking-widest mb-1">Barangay</p>
                  <p class="text-sm font-black text-slate-800">{{ selectedChild.barangay || selectedChild.barangayNo || selectedChild.Barangay || '—' }}</p>
                </div>
              </div>
            </div>

            <div class="bg-white rounded-4xl border border-slate-100 p-7 shadow-sm">
              <div class="flex justify-between items-center mb-5">
                <h3 class="text-base font-black text-slate-800">Upcoming Doses</h3>
                <router-link to="/ParentSchedule" class="text-[10px] font-black text-[#99ad7a] uppercase tracking-widest hover:text-[#5d6b52] transition-colors">View Schedule →</router-link>
              </div>
              <div class="space-y-3">
                <div v-for="vax in upcomingDoses.slice(0, 3)" :key="vax.doseId"
                    class="p-4 bg-slate-50 rounded-2xl border border-slate-100 flex items-center justify-between">
                  <div class="flex items-center gap-3">
                    <div class="w-10 h-10 bg-[#5d6b52] rounded-xl flex items-center justify-center text-white text-sm">💉</div>
                    <div>
                      <p class="text-xs font-bold text-slate-800">{{ vax.name }} · Dose {{ vax.doseNumber }}</p>
                      <p class="text-[10px] text-slate-400 mt-0.5">{{ formatDisplayDate(vax.scheduledDate) }}</p>
                    </div>
                  </div>
                  <span class="px-3 py-1 bg-white rounded-lg text-[9px] font-black text-[#5d6b52] border border-slate-200 uppercase">Scheduled</span>
                </div>
                <p v-if="upcomingDoses.length > 3" class="text-center text-[10px] text-slate-400 font-bold pt-1">
                  +{{ upcomingDoses.length - 3 }} more doses scheduled
                </p>
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

import HeaderNav from '../Components/Headernav.vue'
import ChildSidebar from '../Components/Childsidebar.vue'
import ProfileModal from '../Components/Profilemodal.vue'
import NotificationPanel from '../Components/Notificationpanel.vue'

import { getAccount } from '@/utils/auth'
import api from '../Composables/api.js'

const router = useRouter()

// =====================================================
// SESSION
// =====================================================

const account = ref(null)
const parentData = ref(null)


// =====================================================
// PARENT / CHILDREN
// =====================================================

const children = ref([])
const selectedChild = ref(null)


// =====================================================
// VACCINATION
// =====================================================

const vaccinationTimeline = ref([])
const timelineLoading = ref(false)


// =====================================================
// NOTIFICATIONS
// =====================================================

const unreadCount = ref(0)


// =====================================================
// UI
// =====================================================

const showProfile = ref(false)
const showNotifications = ref(false)


// =====================================================
// SELECTED CHILD
// =====================================================

const selectedChildStorageKey = 'selectedParentChild'


// =====================================================
// HELPERS
// =====================================================

function formatDisplayDate(date) {
  if (!date) return '—'

  const parsed = new Date(date)

  if (Number.isNaN(parsed.getTime())) {
    return '—'
  }

  return parsed.toLocaleDateString('en-PH', {
    month: 'short',
    day: 'numeric',
    year: 'numeric'
  })
}


function calculateAge(birthDate) {
  if (!birthDate) return '—'

  const birth = new Date(birthDate)
  const today = new Date()

  if (Number.isNaN(birth.getTime())) {
    return '—'
  }

  let years =
    today.getFullYear() -
    birth.getFullYear()

  let months =
    today.getMonth() -
    birth.getMonth()

  if (
    months < 0 ||
    (
      months === 0 &&
      today.getDate() < birth.getDate()
    )
  ) {
    years--
  }

  months =
    (
      today.getMonth() -
      birth.getMonth() +
      12
    ) % 12

  if (years === 0) {
    return `${months} month${months !== 1 ? 's' : ''}`
  }

  if (months === 0) {
    return `${years} year${years !== 1 ? 's' : ''}`
  }

  return `${years} year${years !== 1 ? 's' : ''} ${months} month${months !== 1 ? 's' : ''}`
}


// =====================================================
// COMPUTED
// =====================================================

const childAge = computed(() => {
  return calculateAge(
    selectedChild.value?.birthDate
  )
})


const upcomingDoses = computed(() => {
  return vaccinationTimeline.value
    .filter(item =>
      item.status === 'Pending'
    )
    .sort((a, b) =>
      new Date(a.scheduledDate) -
      new Date(b.scheduledDate)
    )
})


// =====================================================
// LOAD LOGGED-IN PARENT
// =====================================================

function loadParentSession() {
  const savedAccount = getCurrentParent()

  if (!savedAccount) {
    router.push('/')
    return false
  }

  account.value = savedAccount

  // Support both possible backend response structures
  parentData.value =
    savedAccount.user ?? savedAccount

  if (!parentData.value?.parentID) {
    console.error(
      'Invalid parent session:',
      savedAccount
    )

    router.push('/')
    return false
  }

  return true
}

// =====================================================
// LOAD CHILDREN
// =====================================================

async function fetchChildren() {

  if (!parentData.value?.parentID) {
    console.error('No ParentID available')
    return
  }

  try {

    console.log(
      'Fetching children for ParentID:',
      parentData.value.parentID
    )

    const response = await api.get(
      `/Parents/dashboard/${parentData.value.parentID}`
    )

    console.log(
      'Parent dashboard response:',
      response.data
    )

    children.value =
      (response.data ?? []).map(child => ({
        childID:
          child.childID ??
          child.ChildID,

        firstName:
          child.firstName ??
          child.FirstName,

        middleName:
          child.middleName ??
          child.MiddleName,

        lastName:
          child.lastName ??
          child.LastName,

        birthDate:
          child.birthDate ??
          child.BirthDate,

        placeOfBirth:
          child.placeOfBirth ??
          child.PlaceOfBirth,

        sex:
          child.sex ??
          child.Sex,

        barangay:
          child.barangay ??
          child.Barangay,

        address:
          child.address ??
          child.Address,

        healthCenter:
          child.healthCenter ??
          child.HealthCenter,

        relationshipType:
          child.relationshipType ??
          child.RelationshipType,

        isPrimaryContact:
          child.isPrimaryContact ??
          child.IsPrimaryContact,

        canReceiveNotifications:
          child.canReceiveNotifications ??
          child.CanReceiveNotifications
      }))

    console.log(
      'Loaded children:',
      children.value
    )

  } catch (error) {

    console.error(
      'Failed to load parent children:',
      error
    )

    children.value = []
  }
}
// =====================================================
// RESTORE SELECTED CHILD
// =====================================================

function restoreSelectedChild() {

  if (children.value.length === 0) {
    selectedChild.value = null
    return
  }

  const saved =
    localStorage.getItem(
      selectedChildStorageKey
    )

  if (!saved) {
    selectedChild.value =
      children.value[0]

    return
  }

  try {

    const savedChild =
      JSON.parse(saved)

    selectedChild.value =
      children.value.find(
        child =>
          child.childID ===
          savedChild.childID
      ) ??
      children.value[0]

  } catch {

    selectedChild.value =
      children.value[0]
  }
}


// =====================================================
// SELECT CHILD
// =====================================================

async function handleSelectChild(child) {

  selectedChild.value = child

  localStorage.setItem(
    selectedChildStorageKey,
    JSON.stringify(child)
  )

  await fetchVaccinationTimeline(
    child.childID
  )
}


// =====================================================
// LOAD VACCINATION TIMELINE
// =====================================================

async function fetchVaccinationTimeline(childId) {

  if (!childId) {
    vaccinationTimeline.value = []
    return
  }

  timelineLoading.value = true

  try {

    const response = await api.get(
      `/VaccinationTimeline/child/${childId}`
    )

    vaccinationTimeline.value =
      response.data ?? []

  } catch (error) {

    console.error(
      'Failed to load vaccination timeline:',
      error
    )

    vaccinationTimeline.value = []

  } finally {

    timelineLoading.value = false

  }
}


// =====================================================
// LOAD UNREAD NOTIFICATION COUNT
// =====================================================

async function fetchUnreadCount() {

  if (!parentData.value?.parentID) {
    return
  }

  try {

    const response = await api.get(
      `/Notifications/parent/${parentData.value.parentID}`
    )

    unreadCount.value =
      (response.data ?? [])
        .filter(notification =>
          !notification.isRead
        )
        .length

  } catch (error) {

    console.error(
      'Failed to load notification count:',
      error
    )

    unreadCount.value = 0
  }
}


// =====================================================
// LOGOUT
// =====================================================

function handleLogout() {

  logout()

  router.push('/')
}


// =====================================================
// INITIAL LOAD
// =====================================================

onMounted(async () => {

  // 1. Identify currently logged-in parent
  const validSession =
    loadParentSession()

  if (!validSession) {
    return
  }


  // 2. Load all children
  await fetchChildren()


  // 3. Restore selected child
  restoreSelectedChild()


  // 4. Load vaccination timeline
  if (selectedChild.value) {

    await fetchVaccinationTimeline(
      selectedChild.value.childID
    )
  }


  // 5. Load unread notifications
  await fetchUnreadCount()
})
</script>