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
          <h1 class="text-2xl font-bold text-slate-800">Reports</h1>
          <p class="text-sm text-slate-500 mt-1">Vaccination statistics and analytics</p>
        </div>

        <!-- STAT CARDS -->
        <div class="grid grid-cols-4 gap-4 mb-6">
          <div v-for="stat in statCards" :key="stat.label"
            class="bg-white rounded-xl border border-slate-200 p-5">
            <div class="mb-3">
              <div class="w-9 h-9 rounded-lg flex items-center justify-center" :class="stat.bg">
                <component :is="stat.icon" class="w-4 h-4" :class="stat.color" />
              </div>
            </div>
            <p class="text-3xl font-bold text-slate-800">{{ stat.value }}</p>
            <p class="text-xs text-slate-400 mt-1">{{ stat.label }}</p>
          </div>
        </div>

        <div class="grid grid-cols-2 gap-6">

          <!-- BAR CHART: Vaccinations Per Week -->
          <div class="bg-white rounded-xl border border-slate-200 p-5">
            <h3 class="font-semibold text-slate-800 mb-4">Vaccinations Per Week</h3>
            <div class="flex items-end gap-2 h-40">
              <div v-for="bar in weeklyBars" :key="bar.day"
                class="flex-1 flex flex-col items-center gap-1">
                <span class="text-xs text-slate-400">{{ bar.count }}</span>
                <div class="w-full rounded-t-md bg-emerald-500 transition-all duration-500"
                  :style="{ height: (bar.count / maxBar * 100) + '%' }"></div>
                <span class="text-xs text-slate-400">{{ bar.day }}</span>
              </div>
            </div>
          </div>

          <!-- PIE CHART: Most Given Vaccines -->
          <div class="bg-white rounded-xl border border-slate-200 p-5">
            <h3 class="font-semibold text-slate-800 mb-4">Most Given Vaccines</h3>
            <div class="flex items-center gap-6">
              <!-- SVG Pie -->
              <svg viewBox="0 0 100 100" class="w-36 h-36 -rotate-90 shrink-0">
                <circle v-for="(seg, i) in pieSegments" :key="i"
                  cx="50" cy="50" r="40"
                  fill="none"
                  :stroke="seg.color"
                  stroke-width="20"
                  :stroke-dasharray="`${seg.dash} ${251.2 - seg.dash}`"
                  :stroke-dashoffset="-seg.offset" />
              </svg>
              <!-- Legend -->
              <div class="space-y-2">
                <div v-for="v in vaccineBreakdown" :key="v.name" class="flex items-center justify-between gap-8">
                  <div class="flex items-center gap-2">
                    <div class="w-2.5 h-2.5 rounded-full shrink-0" :style="{ backgroundColor: v.color }"></div>
                    <span class="text-xs text-slate-600">{{ v.name }}</span>
                  </div>
                  <span class="text-xs font-medium text-slate-700">{{ v.pct }}%</span>
                </div>
              </div>
            </div>
          </div>

          <!-- STATUS BREAKDOWN -->
          <div class="bg-white rounded-xl border border-slate-200 p-5">
            <h3 class="font-semibold text-slate-800 mb-4">Vaccination Status Breakdown</h3>
            <div class="space-y-3">
              <div v-for="s in statusBreakdown" :key="s.label">
                <div class="flex items-center justify-between mb-1">
                  <span class="text-xs text-slate-600">{{ s.label }}</span>
                  <span class="text-xs font-medium text-slate-700">{{ s.count }} ({{ s.pct }}%)</span>
                </div>
                <div class="h-2 bg-slate-100 rounded-full overflow-hidden">
                  <div class="h-full rounded-full transition-all duration-500"
                    :class="s.color" :style="{ width: s.pct + '%' }"></div>
                </div>
              </div>
            </div>
          </div>

          <!-- RECENT ACTIVITY -->
          <div class="bg-white rounded-xl border border-slate-200 p-5">
            <h3 class="font-semibold text-slate-800 mb-4">Recent Vaccinations</h3>
            <div class="space-y-3">
              <div v-for="rec in recentActivity" :key="rec.id"
                class="flex items-center gap-3">
                <div class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold text-white shrink-0"
                  :style="{ backgroundColor: avatarColor(rec.name) }">
                  {{ initials(rec.name) }}
                </div>
                <div class="flex-1 min-w-0">
                  <p class="text-sm font-medium text-slate-800 truncate">{{ rec.name }}</p>
                  <p class="text-xs text-slate-400">{{ rec.vaccine }} · {{ rec.date }}</p>
                </div>
                <span class="text-xs text-emerald-600 font-medium shrink-0">Done</span>
              </div>
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
  Search, Bell, LogOut, TrendingUp, AlertCircle, Clock, UserCheck
} from 'lucide-vue-next'

const router = useRouter()
const route  = useRoute()

const doctor = ref({ userId: '', fullName: '', userType: '' })
onMounted(() => {
  const stored = localStorage.getItem('aruga_user')
  if (!stored) { router.push('/'); return }
  const u = JSON.parse(stored)
  doctor.value = { userId: u.UserID, fullName: `${u.FirstName} ${u.LastName}`, userType: u.UserType }
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

const statCards = [
  { label: 'Vaccinated Today', value: 18, icon: Syringe,      color: 'text-emerald-600', bg: 'bg-emerald-50' },
  { label: 'Weekly Total',     value: 89, icon: TrendingUp,   color: 'text-blue-600',    bg: 'bg-blue-50'    },
  { label: 'Missed Vaccines',  value:  3, icon: AlertCircle,  color: 'text-red-500',     bg: 'bg-red-50'     },
  { label: 'Follow-up Cases',  value: 12, icon: UserCheck,    color: 'text-amber-600',   bg: 'bg-amber-50'   },
]

const weeklyBars = [
  { day: 'Mon', count: 12 }, { day: 'Tue', count: 18 }, { day: 'Wed', count: 20 },
  { day: 'Thu', count: 15 }, { day: 'Fri', count: 14 }, { day: 'Sat', count: 16 }, { day: 'Sun', count: 8 },
]
const maxBar = computed(() => Math.max(...weeklyBars.map(b => b.count)))

const vaccineBreakdown = [
  { name: 'Pentavalent', pct: 35, color: '#1a3a2a' },
  { name: 'MMR',         pct: 25, color: '#6b7c45' },
  { name: 'OPV',         pct: 20, color: '#8b9a6b' },
  { name: 'Pneumococcal',pct: 15, color: '#c8d4b0' },
  { name: 'Others',      pct:  5, color: '#e8edd8' },
]

const pieSegments = computed(() => {
  const circumference = 251.2
  let offset = 0
  return vaccineBreakdown.map(v => {
    const dash = (v.pct / 100) * circumference
    const seg  = { color: v.color, dash, offset }
    offset += dash
    return seg
  })
})

const statusBreakdown = [
  { label: 'Completed', count: 58, pct: 65, color: 'bg-emerald-500' },
  { label: 'Pending',   count: 18, pct: 20, color: 'bg-amber-400'   },
  { label: 'Scheduled', count:  9, pct: 10, color: 'bg-blue-400'    },
  { label: 'Missed',    count:  4, pct:  5, color: 'bg-red-400'     },
]

const recentActivity = [
  { id: 1, name: 'Sofia Reyes',     vaccine: 'MMR',         date: 'Today' },
  { id: 2, name: 'Emma Villanueva', vaccine: 'Hepatitis B', date: 'Today' },
  { id: 3, name: 'Juan Dela Cruz',  vaccine: 'Pentavalent', date: 'Yesterday' },
  { id: 4, name: 'Miguel Santos',   vaccine: 'Pentavalent', date: 'Yesterday' },
  { id: 5, name: 'Isabella Garcia', vaccine: 'OPV',         date: 'May 7' },
]

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