<template>
  <div class="flex h-screen bg-[#F9FAFB] font-sans antialiased text-slate-900">
    
    <!-- SIDEBAR -->
    <aside class="w-64 bg-white border-r border-slate-200 flex flex-col">
      <div class="p-6">
        <div class="flex items-center gap-3 px-2">
          <div class="w-8 h-8 bg-emerald-600 rounded flex items-center justify-center text-white font-bold">A</div>
          <span class="text-xl font-bold tracking-tight">Aruga</span>
        </div>
      </div>

      <nav class="flex-1 px-3 space-y-1 overflow-y-auto">
        <button v-for="item in navItems" :key="item.id" 
          @click="currentPage = item.id; $router.push(item.path)"
          class="w-full flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors cursor-pointer"
          :class="currentPage === 'vaccines' ? 'bg-emerald-50 text-emerald-700' : 'text-slate-500 hover:bg-slate-50 hover:text-slate-900'">
          <component :is="item.icon" class="w-4 h-4" />
          {{ item.label }}
        </button>
      </nav>

      <div class="p-4 border-t border-slate-100">
        <button @click="$router.push('/StaffLogin')" class="w-full flex items-center gap-3 px-4 py-2.5 text-sm font-bold text-rose-600 hover:bg-rose-50 rounded-lg transition-colors cursor-pointer">
          <LogOut class="w-4 h-4" />
          Log Out
        </button>
      </div>
    </aside>

    <!-- MAIN SECTION -->
    <div class="flex-1 flex flex-col overflow-hidden">
      <header class="h-16 bg-white border-b border-slate-200 px-8 flex items-center justify-between shrink-0">
        <div>
          <h2 class="font-bold text-slate-800">Vaccine Inventory</h2>
          <p class="text-[10px] text-slate-400 uppercase font-bold tracking-wider">Stock Management & Logistics</p>
        </div>
        
        <button @click="showAddModal = true" class="bg-emerald-600 text-white px-5 py-2.5 rounded-lg text-xs font-bold hover:bg-emerald-700 transition-all flex items-center gap-2 shadow-sm">
          <Plus class="w-4 h-4" /> ADD NEW BATCH
        </button>
      </header>

      <main class="flex-1 p-8 overflow-y-auto custom-scrollbar">
        <div class="max-w-7xl mx-auto">
          
          <!-- SUMMARY STATS -->
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
            <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex items-center gap-4">
              <div class="w-12 h-12 bg-blue-50 text-blue-600 rounded-xl flex items-center justify-center"><Package class="w-6 h-6" /></div>
              <div>
                <p class="text-[10px] font-bold text-slate-400 uppercase">Total Doses</p>
                <p class="text-xl font-bold text-slate-800">4,250</p>
              </div>
            </div>
            <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex items-center gap-4">
              <div class="w-12 h-12 bg-amber-50 text-amber-600 rounded-xl flex items-center justify-center"><AlertTriangle class="w-6 h-6" /></div>
              <div>
                <p class="text-[10px] font-bold text-slate-400 uppercase">Expiring Soon</p>
                <p class="text-xl font-bold text-slate-800">12 Batches</p>
              </div>
            </div>
            <div class="bg-white p-5 rounded-2xl border border-slate-200 shadow-sm flex items-center gap-4">
              <div class="w-12 h-12 bg-rose-50 text-rose-600 rounded-xl flex items-center justify-center"><Trash2 class="w-6 h-6" /></div>
              <div>
                <p class="text-[10px] font-bold text-slate-400 uppercase">Expired Stocks</p>
                <p class="text-xl font-bold text-slate-800">0</p>
              </div>
            </div>
          </div>

          <!-- INVENTORY TABLE -->
          <div class="bg-white border border-slate-200 rounded-2xl shadow-sm overflow-hidden">
            <table class="w-full text-left text-sm">
              <thead class="bg-slate-50/50 border-b border-slate-200 text-[11px] uppercase font-bold text-slate-500">
                <tr>
                  <th class="px-6 py-4">Vaccine Name</th>
                  <th class="px-6 py-4">Lot Number</th>
                  <th class="px-6 py-4">Manufactured</th>
                  <th class="px-6 py-4">Expiry Date</th>
                  <th class="px-6 py-4">Stock Level</th>
                  <th class="px-6 py-4 text-right">Status</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr v-for="batch in inventory" :key="batch.lotNo" class="hover:bg-slate-50/50 transition-colors">
                  <td class="px-6 py-4 font-bold text-slate-800">{{ batch.name }}</td>
                  <td class="px-6 py-4">
                    <span class="bg-slate-100 text-slate-600 px-2 py-1 rounded text-[10px] font-mono font-bold tracking-widest uppercase">
                      #{{ batch.lotNo }}
                    </span>
                  </td>
                  <td class="px-6 py-4 text-slate-500">{{ batch.mfgDate }}</td>
                  <td class="px-6 py-4">
                    <span :class="isNearExpiry(batch.expiryDate) ? 'text-rose-600 font-bold' : 'text-slate-600'">
                      {{ batch.expiryDate }}
                    </span>
                  </td>
                  <td class="px-6 py-4">
                    <div class="flex items-center gap-2">
                      <div class="w-16 bg-slate-100 h-1.5 rounded-full overflow-hidden">
                        <div class="bg-emerald-500 h-full" :style="{ width: (batch.stock / 500 * 100) + '%' }"></div>
                      </div>
                      <span class="text-xs font-bold">{{ batch.stock }}</span>
                    </div>
                  </td>
                  <td class="px-6 py-4 text-right">
                    <span :class="batch.stock > 100 ? 'bg-emerald-50 text-emerald-600' : 'bg-rose-50 text-rose-600'" 
                      class="px-2 py-0.5 rounded-full text-[10px] font-bold uppercase">
                      {{ batch.stock > 100 ? 'In Stock' : 'Low Stock' }}
                    </span>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </main>

      <!-- ADD BATCH MODAL -->
      <div v-if="showAddModal" class="fixed inset-0 bg-slate-900/60 backdrop-blur-sm flex items-center justify-center z-50 p-4">
        <div class="bg-white w-full max-w-md rounded-2xl shadow-2xl overflow-hidden border border-slate-200">
          <div class="p-6 border-b border-slate-100 flex justify-between items-center">
            <h3 class="font-bold text-slate-800">Register New Batch</h3>
            <button @click="showAddModal = false" class="text-slate-400 hover:text-slate-600"><X class="w-5 h-5" /></button>
          </div>
          
          <div class="p-6 space-y-4">
            <div>
              <label class="block text-[10px] font-bold text-slate-400 uppercase mb-1">Vaccine Name</label>
              <input type="text" placeholder="e.g. BCG, Polio" class="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:border-emerald-500 outline-none">
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-[10px] font-bold text-slate-400 uppercase mb-1">Lot Number</label>
                <input type="text" placeholder="LOT-99X" class="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:border-emerald-500 outline-none">
              </div>
              <div>
                <label class="block text-[10px] font-bold text-slate-400 uppercase mb-1">Doses Received</label>
                <input type="number" placeholder="0" class="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:border-emerald-500 outline-none">
              </div>
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="block text-[10px] font-bold text-slate-400 uppercase mb-1">Manufactured Date</label>
                <input type="date" class="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:border-emerald-500 outline-none">
              </div>
              <div>
                <label class="block text-[10px] font-bold text-slate-400 uppercase mb-1">Expiration Date</label>
                <input type="date" class="w-full px-3 py-2 bg-slate-50 border border-slate-200 rounded-lg text-sm focus:border-emerald-500 outline-none">
              </div>
            </div>
          </div>

          <div class="p-4 bg-slate-50 border-t border-slate-100 flex gap-2">
             <button @click="showAddModal = false" class="flex-1 py-2.5 text-xs font-bold text-slate-500 hover:bg-slate-100 rounded-lg">CANCEL</button>
             <button class="flex-1 py-2.5 text-xs font-bold bg-emerald-600 text-white hover:bg-emerald-700 rounded-lg shadow-sm transition-all">ADD TO INVENTORY</button>
          </div>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { 
  LayoutDashboard, Users, UserRound, Stethoscope, 
  Syringe, Calendar, FileText, ShieldCheck, LogOut,
  Plus, Package, AlertTriangle, Trash2, X
} from 'lucide-vue-next'

const currentPage = ref('vaccines')
const showAddModal = ref(false)

const navItems = [
  { id: 'dashboard', label: 'Dashboard', icon: LayoutDashboard, path: '/AdminHome' },
  { id: 'parent', label: 'Parent', icon: Users, path: '/StaffParent' },
  { id: 'records', label: 'Children Record', icon: UserRound, path: '/StaffChild' },
  { id: 'staff', label: 'Doctor / Staff', icon: Stethoscope, path: '/StaffDocNurse' },
  { id: 'vaccines', label: 'Vaccines', icon: Syringe, path: '/StaffVaccine' },
  { id: 'calendar', label: 'Calendar', icon: Calendar, path: '/StaffCalendar' },
  { id: 'reports', label: 'Reports', icon: FileText, path: '/StaffReport' },
  { id: 'audit', label: 'Audit Log', icon: ShieldCheck, path: '/audit' },
]

const inventory = ref([
  { name: 'BCG (Tuberculosis)', lotNo: 'BCG2024-09', mfgDate: '2024-01-15', expiryDate: '2026-01-15', stock: 450 },
  { name: 'Polio (IPV)', lotNo: 'IPV-LX92', mfgDate: '2023-11-20', expiryDate: '2025-11-20', stock: 85 },
  { name: 'Hepatitis B', lotNo: 'HEPB-55-A', mfgDate: '2024-02-10', expiryDate: '2026-05-10', stock: 320 },
  { name: 'MMR (Measles)', lotNo: 'MMR-X22', mfgDate: '2024-03-01', expiryDate: '2025-06-01', stock: 12 },
])

const isNearExpiry = (dateStr) => {
  const expiry = new Date(dateStr)
  const today = new Date()
  const diffInMonths = (expiry.getFullYear() - today.getFullYear()) * 12 + (expiry.getMonth() - today.getMonth())
  return diffInMonths < 3
}
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #E2E8F0; border-radius: 10px; }
</style>