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
      <header class="h-16 bg-white border-b border-slate-200 flex items-center justify-between px-6 shrink-0">
        <div class="relative w-80">
          </div>
        <div class="flex items-center gap-4">
          <button class="relative p-2 text-slate-400 hover:text-slate-600">
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

      <main class="flex-1 overflow-y-auto p-6">
        <div class="mb-6">
          <h1 class="text-2xl font-bold text-slate-800">My Account</h1>
          <p class="text-sm text-slate-500 mt-1">Manage your profile and settings</p>
        </div>

        <div class="grid grid-cols-3 gap-6">

          <!-- LEFT: Profile + Password -->
          <div class="col-span-2 space-y-5">

            <!-- Profile Info -->
            <div class="bg-white rounded-xl border border-slate-200 p-6">
              <h3 class="font-semibold text-slate-800 mb-4">Profile Information</h3>
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="text-xs font-medium text-slate-500 uppercase tracking-wide block mb-1.5">
                    <User class="w-3 h-3 inline mr-1" />Full Name
                  </label>
                  <input v-model="form.fullName" type="text"
                    class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                </div>
                <div>
                  <label class="text-xs font-medium text-slate-500 uppercase tracking-wide block mb-1.5">
                    <BadgeCheck class="w-3 h-3 inline mr-1" />Professional License No.
                  </label>
                  <input v-model="form.prcNo" type="text"
                    class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                </div>
                <div>
                  <label class="text-xs font-medium text-slate-500 uppercase tracking-wide block mb-1.5">
                    <Mail class="w-3 h-3 inline mr-1" />Email Address
                  </label>
                  <input v-model="form.email" type="email"
                    class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                </div>
                <div>
                  <label class="text-xs font-medium text-slate-500 uppercase tracking-wide block mb-1.5">
                    <Phone class="w-3 h-3 inline mr-1" />Contact Number
                  </label>
                  <input v-model="form.contactNo" type="text"
                    class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                </div>
                <div class="col-span-2">
                  <label class="text-xs font-medium text-slate-500 uppercase tracking-wide block mb-1.5">Role</label>
                  <input :value="doctor.userType" disabled type="text"
                    class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg bg-slate-50 text-slate-400 cursor-not-allowed" />
                </div>
              </div>
              <div class="mt-4 flex items-center gap-3">
                <button @click="saveProfile"
                  :disabled="savingProfile"
                  class="flex items-center gap-2 px-4 py-2.5 text-sm font-medium text-white bg-emerald-600 rounded-lg hover:bg-emerald-700 disabled:opacity-50 transition-colors">
                  <Save class="w-4 h-4" />
                  {{ savingProfile ? 'Saving…' : 'Save Changes' }}
                </button>
                <p v-if="profileSaved" class="text-xs text-emerald-600 font-medium">✓ Changes saved</p>
              </div>
            </div>

            <!-- Change Password -->
            <div class="bg-white rounded-xl border border-slate-200 p-6">
              <h3 class="font-semibold text-slate-800 mb-4">
                <Lock class="w-4 h-4 inline mr-1.5 text-slate-400" />Change Password
              </h3>
              <div class="space-y-4">
                <div>
                  <label class="text-xs font-medium text-slate-500 uppercase tracking-wide block mb-1.5">Current Password</label>
                  <input v-model="pwForm.current" type="password" placeholder="Enter current password"
                    class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                </div>
                <div>
                  <label class="text-xs font-medium text-slate-500 uppercase tracking-wide block mb-1.5">New Password</label>
                  <input v-model="pwForm.newPw" type="password" placeholder="Enter new password"
                    class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                </div>
                <div>
                  <label class="text-xs font-medium text-slate-500 uppercase tracking-wide block mb-1.5">Confirm New Password</label>
                  <input v-model="pwForm.confirm" type="password" placeholder="Confirm new password"
                    class="w-full px-3 py-2.5 text-sm border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-emerald-500" />
                </div>
                <p v-if="pwError" class="text-xs text-red-500">{{ pwError }}</p>
              </div>
              <button @click="changePassword"
                class="mt-4 flex items-center gap-2 px-4 py-2.5 text-sm font-medium text-white bg-slate-700 rounded-lg hover:bg-slate-800 transition-colors">
                <Lock class="w-4 h-4" />Update Password
              </button>
            </div>

          </div>

          <!-- RIGHT: Avatar card + Notification settings -->
          <div class="space-y-5">

            <!-- Avatar Card -->
            <div class="bg-white rounded-xl border border-slate-200 p-6 flex flex-col items-center text-center">
              <div class="w-20 h-20 rounded-full bg-emerald-700 flex items-center justify-center text-2xl font-bold text-white mb-3">
                {{ doctorInitials }}
              </div>
              <p class="font-bold text-slate-800">{{ doctor.fullName }}</p>
              <p class="text-sm text-slate-400 mt-0.5">{{ doctor.userType }}</p>
              <p v-if="doctor.prcNo" class="text-xs text-slate-400 mt-0.5">{{ doctor.prcNo }}</p>
            </div>

            <!-- Notification Settings -->
            <div class="bg-white rounded-xl border border-slate-200 p-5">
              <h3 class="font-semibold text-slate-800 mb-3">
                <Bell class="w-4 h-4 inline mr-1.5 text-slate-400" />Notification Settings
              </h3>
              <div class="space-y-3">
                <label v-for="n in notifSettings" :key="n.key"
                  class="flex items-center justify-between cursor-pointer">
                  <span class="text-sm text-slate-600" :class="n.highlight ? 'text-red-500 font-medium' : ''">{{ n.label }}</span>
                  <div class="relative">
                    <input type="checkbox" v-model="n.enabled" class="sr-only" />
                    <div class="w-9 h-5 rounded-full transition-colors"
                      :class="n.enabled ? 'bg-emerald-500' : 'bg-slate-200'"
                      @click="n.enabled = !n.enabled">
                      <div class="absolute top-0.5 left-0.5 w-4 h-4 bg-white rounded-full shadow transition-transform"
                        :class="n.enabled ? 'translate-x-4' : 'translate-x-0'"></div>
                    </div>
                  </div>
                </label>
              </div>
              <button class="mt-4 w-full py-2 text-sm font-medium text-white bg-slate-700 rounded-lg hover:bg-slate-800 transition-colors">
                Save Settings
              </button>
            </div>

          </div>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import {
  Home, Users, Calendar, Syringe, FileText, Settings,
  Search, Bell, LogOut, Save, Lock, User, Mail, Phone, BadgeCheck
} from 'lucide-vue-next'

const router = useRouter()
const route  = useRoute()

const doctor = ref({ userId: '', fullName: '', userType: '', prcNo: '' })
const form   = ref({ fullName: '', prcNo: '', email: '', contactNo: '' })
const pwForm = ref({ current: '', newPw: '', confirm: '' })
const pwError       = ref('')
const savingProfile = ref(false)
const profileSaved  = ref(false)

onMounted(() => {
  const stored = localStorage.getItem('aruga_user')
  if (!stored) { router.push('/'); return }
  const u = JSON.parse(stored)
  doctor.value = {
    userId:   u.UserID,
    fullName: `${u.FirstName} ${u.LastName}`,
    userType: u.UserType,
    prcNo:    u.PRCNo || '',
  }
  form.value.fullName  = doctor.value.fullName
  form.value.prcNo     = doctor.value.prcNo
})

const doctorInitials = computed(() =>
  doctor.value.fullName.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
)

const navItems = [
  { id: 'home',     label: 'Home',                path: '/doctor/home',     icon: Home     },
  { id: 'patients', label: 'Patients (Children)', path: '/doctor/patients', icon: Users    },
  { id: 'calendar', label: 'Calendar',            path: '/doctor/calendar', icon: Calendar },
  { id: 'records',  label: 'Vaccination Records', path: '/doctor/records',  icon: Syringe  },
  { id: 'reports',  label: 'Reports',             path: '/doctor/reports',  icon: FileText },
  { id: 'account',  label: 'My Account',          path: '/doctor/account',  icon: Settings },
]

const notifSettings = ref([
  { key: 'email',   label: 'Email Alerts',          enabled: true,  highlight: false },
  { key: 'sms',     label: 'SMS Alerts',            enabled: false, highlight: false },
  { key: 'daily',   label: 'Daily Reports',         enabled: true,  highlight: false },
])

async function saveProfile() {
  savingProfile.value = true
  await new Promise(r => setTimeout(r, 800)) // simulate API call
  // await fetch(`${import.meta.env.VITE_API_URL}/api/users/${doctor.value.userId}`, { method: 'PATCH', ... })
  savingProfile.value = false
  profileSaved.value  = true
  setTimeout(() => profileSaved.value = false, 3000)
}

function changePassword() {
  pwError.value = ''
  if (!pwForm.value.current) { pwError.value = 'Please enter your current password.'; return }
  if (pwForm.value.newPw.length < 6) { pwError.value = 'New password must be at least 6 characters.'; return }
  if (pwForm.value.newPw !== pwForm.value.confirm) { pwError.value = 'Passwords do not match.'; return }
  // await fetch(...)
  console.log('Password change payload ready')
  pwForm.value = { current: '', newPw: '', confirm: '' }
}

function logout() {
  localStorage.removeItem('aruga_user')
  localStorage.removeItem('aruga_token')
  router.push('/')
}
</script>