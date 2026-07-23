<template>
  <div class="flex h-screen bg-[#F9FAFB] font-sans antialiased text-slate-900">
    
    <!-- SIDEBAR (Matches your other pages) -->
    <aside class="w-64 bg-white border-r border-slate-200 flex flex-col">
      <div class="p-6">
        <div class="flex items-center gap-3 px-2">
          <div class="w-8 h-8 bg-emerald-600 rounded flex items-center justify-center text-white font-bold">A</div>
          <span class="text-xl font-bold tracking-tight">Aruga</span>
        </div>
      </div>
      <nav class="flex-1 px-3 space-y-1 overflow-y-auto">
        <button v-for="item in navItems" :key="item.id" 
          @click="$router.push(item.path)"
          class="w-full flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors"
          :class="$route.path === item.path ? 'bg-emerald-50 text-emerald-700' : 'text-slate-500 hover:bg-slate-50 hover:text-slate-900'">
          <component :is="item.icon" class="w-4 h-4" />
          {{ item.label }}
        </button>
      </nav>
    </aside>

    <!-- MAIN SECTION -->
    <div class="flex-1 flex flex-col overflow-hidden">
      <header class="h-16 bg-white border-b border-slate-200 px-8 flex items-center justify-between shrink-0">
        <div>
          <h2 class="font-bold text-slate-800">Vaccination Schedule</h2>
          <p class="text-[10px] text-slate-400 uppercase font-bold tracking-wider">Patient Appointments & Follow-ups</p>
        </div>
        <div class="flex gap-2">
          <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-500"><ChevronLeft class="w-5 h-5" /></button>
          <span class="px-4 py-2 font-bold text-sm">May 2026</span>
          <button class="p-2 hover:bg-slate-100 rounded-lg text-slate-500"><ChevronRight class="w-5 h-5" /></button>
        </div>
      </header>

      <main class="flex-1 p-8 overflow-y-auto custom-scrollbar">
        <div class="max-w-7xl mx-auto">
          
          <!-- CALENDAR GRID -->
          <div class="bg-white border border-slate-200 rounded-2xl shadow-sm overflow-hidden">
            <!-- Days of Week Header -->
            <div class="grid grid-cols-7 bg-slate-50 border-b border-slate-200">
              <div v-for="day in ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']" :key="day" 
                class="py-3 text-center text-[10px] font-bold text-slate-400 uppercase tracking-widest">
                {{ day }}
              </div>
            </div>

            <!-- Calendar Days -->
            <div class="grid grid-cols-7 auto-rows-[120px]">
              <div v-for="n in 31" :key="n" class="border-r border-b border-slate-100 p-2 hover:bg-slate-50 transition-colors relative">
                <span class="text-xs font-bold text-slate-400">{{ n }}</span>
                
                <!-- Appointment Pills -->
                <div class="mt-1 space-y-1">
                  <div v-if="n === 4" class="bg-emerald-100 text-emerald-700 p-1 rounded text-[10px] font-bold border border-emerald-200 truncate">
                    10:00 - Baby Mateo (BCG)
                  </div>
                  <div v-if="n === 4" class="bg-blue-100 text-blue-700 p-1 rounded text-[10px] font-bold border border-blue-200 truncate">
                    14:30 - Sophia (Polio)
                  </div>
                  <div v-if="n === 12" class="bg-amber-100 text-amber-700 p-1 rounded text-[10px] font-bold border border-amber-200 truncate">
                    09:00 - Liam (Hepa B)
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </div>
  </div>
</template>

<script setup>
import { 
  LayoutDashboard, Users, UserRound, Stethoscope, 
  Syringe, Calendar, FileText, ShieldCheck, 
  ChevronLeft, ChevronRight 
} from 'lucide-vue-next'

const navItems = [
  { id: 'dashboard', label: 'Dashboard', icon: LayoutDashboard, path: '/AdminHome' },
  { id: 'parent', label: 'Parent', icon: Users, path: '/StaffParent' },
  { id: 'records', label: 'Children Record', icon: UserRound, path: '/StaffChild' },
  { id: 'staff', label: 'Doctor / Staff', icon: Stethoscope, path: '/StaffDocNurse' },
  { id: 'vaccines', label: 'Vaccines', icon: Syringe, path: '/StaffVaccine' },
  { id: 'calendar', label: 'Calendar', icon: Calendar, path: '/calendar' },
  { id: 'reports', label: 'Reports', icon: FileText, path: '/StaffReport' },
]
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #E2E8F0; border-radius: 10px; }
</style>