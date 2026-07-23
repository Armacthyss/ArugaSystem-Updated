<template>
  <div class="flex h-screen bg-slate-50 font-sans antialiased text-slate-900">
    
    <!-- SIDEBAR -->
   <!-- SIDEBAR -->
<aside class="w-64 bg-white border-r border-slate-200 flex flex-col shrink-0">
  
  <div class="p-6">
    <div class="flex items-center gap-3 px-2">
      <div class="w-8 h-8 bg-emerald-600 rounded flex items-center justify-center text-white font-bold">A</div>
      <span class="text-xl font-bold tracking-tight">Aruga</span>
    </div>
  </div>
  
  <nav class="flex-1 px-3 space-y-1 overflow-y-auto custom-scrollbar">
    <!-- This v-for loop is what generates the menu items -->
    <button v-for="item in navItems" :key="item.id" 
      @click="$router.push(item.path)"
      class="w-full flex items-center gap-3 px-4 py-2.5 rounded-lg text-sm font-medium transition-colors cursor-pointer"
      :class="$route.path === item.path ? 'bg-emerald-50 text-emerald-700' : 'text-slate-500 hover:bg-slate-50 hover:text-slate-900'">
      <component :is="item.icon" class="w-4 h-4" />
      {{ item.label }}
    </button>
  </nav>
</aside>
    <!-- MAIN CONTENT -->
    <div class="flex-1 flex flex-col overflow-hidden">
      <header class="h-16 bg-white border-b border-slate-200 px-8 flex items-center justify-between shrink-0">
        <h2 class="font-bold text-slate-500 uppercase text-[10px] tracking-widest">Aruga System Database</h2>
        <button 
          @click="showAddModal = true" 
          :disabled="isLoading"
          class="bg-emerald-600 text-white px-5 py-2.5 rounded-lg text-xs font-bold hover:bg-emerald-700 transition-all flex items-center gap-2 cursor-pointer shadow-sm active:scale-95 disabled:opacity-50 disabled:cursor-not-allowed"
        >
          <Plus class="w-4 h-4" /> REGISTER PARENT
        </button>
      </header>

      <main class="flex-1 p-8 overflow-y-auto">
        <div class="max-w-6xl mx-auto">
          
          <div class="flex items-center justify-between mb-6">
            <h1 class="text-2xl font-bold text-slate-800">Registered Parents</h1>
            <button 
              @click="fetchParents" 
              class="text-slate-400 hover:text-emerald-600 transition-colors p-2 rounded-full hover:bg-slate-100 cursor-pointer"
            >
              <RefreshCw class="w-5 h-5" :class="{'animate-spin': isLoading}" />
            </button>
          </div>

          <!-- TABLE -->
          <div class="bg-white border border-slate-200 rounded-2xl shadow-sm overflow-hidden">
            <table class="w-full text-left border-collapse">
              <thead>
                <tr class="bg-slate-50 border-b border-slate-200">
                  <th class="px-6 py-4 text-[10px] font-bold text-slate-400 uppercase tracking-widest">Full Name</th>
                  <th class="px-6 py-4 text-[10px] font-bold text-slate-400 uppercase tracking-widest">Contact Info</th>
                  <th class="px-6 py-4 text-[10px] font-bold text-slate-400 uppercase tracking-widest">Address Details</th>
                  <th class="px-6 py-4 text-[10px] font-bold text-slate-400 uppercase tracking-widest text-right">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr v-for="parent in parentList" :key="parent.id" class="hover:bg-slate-50/50 transition-colors group">
                  <td class="px-6 py-4">
                    <div class="flex items-center gap-3">
                      <div class="w-9 h-9 bg-emerald-100 text-emerald-700 rounded-full flex items-center justify-center font-bold text-sm">
                        {{ parent.lastName?.charAt(0) || 'P' }}
                      </div>
                      <div>
                        <p class="font-bold text-slate-700 leading-tight">
                          {{ parent.givenName }} {{ parent.middleName }} {{ parent.lastName }}
                        </p>
                        <p class="text-[11px] text-slate-400 font-medium uppercase tracking-tighter">ID: {{ parent.id?.substring(0,8) }}</p>
                      </div>
                    </div>
                  </td>
                  <td class="px-6 py-4">
                    <p class="text-sm text-slate-600 font-medium">{{ parent.email }}</p>
                    <p class="text-xs text-slate-400">{{ parent.contactNo }}</p>
                  </td>
                  <td class="px-6 py-4">
                    <p class="text-sm text-slate-600 truncate max-w-[200px]">{{ parent.address || 'No Address' }}</p>
                    <p class="text-xs text-emerald-600 font-bold uppercase tracking-tight">Brgy. {{ parent.barangayNo || 'N/A' }}</p>
                  </td>
                  <td class="px-6 py-4 text-right">
                    <button class="bg-slate-100 text-slate-600 px-3 py-1.5 rounded-lg text-xs font-bold hover:bg-emerald-600 hover:text-white transition-all cursor-pointer">
                      Edit
                    </button>
                  </td>
                </tr>
                <tr v-if="parentList.length === 0 && !isLoading">
                  <td colspan="4" class="px-6 py-20 text-center">
                    <div class="flex flex-col items-center">
                      <Users class="w-12 h-12 text-slate-200 mb-2" />
                      <p class="text-slate-400 font-medium">No parent records found.</p>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </main>
    </div>

    <!-- REGISTRATION MODAL -->
    <div v-if="showAddModal" class="fixed inset-0 z-[60] flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-slate-900/60 backdrop-blur-sm" @click="showAddModal = false"></div>
      
      <div class="relative bg-white w-full max-w-2xl rounded-3xl shadow-2xl overflow-hidden flex flex-col">
        <div class="p-6 border-b border-slate-100 flex justify-between items-center">
          <h3 class="font-bold text-slate-800 text-lg">Register Parent</h3>
          <X @click="showAddModal = false" class="w-5 h-5 text-slate-400 cursor-pointer hover:text-slate-600" />
        </div>

        <div class="p-8 space-y-5 overflow-y-auto max-h-[65vh]">
          <div class="grid grid-cols-3 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase ml-1">Given Name</label>
              <input v-model="form.GivenName" type="text" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:border-emerald-500 outline-none transition-all" />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase ml-1">Middle Name</label>
              <input v-model="form.MiddleName" type="text" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:border-emerald-500 outline-none transition-all" />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase ml-1">Last Name</label>
              <input v-model="form.LastName" type="text" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:border-emerald-500 outline-none transition-all" />
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase ml-1">Email Address</label>
              <input v-model="form.Email" type="email" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:border-emerald-500 outline-none transition-all" />
            </div>
            <div class="space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase ml-1">Contact No</label>
              <input v-model="form.ContactNo" type="text" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:border-emerald-500 outline-none transition-all" />
            </div>
          </div>

          <div class="grid grid-cols-3 gap-4">
            <div class="col-span-1 space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase ml-1">Barangay No</label>
              <input v-model="form.BarangayNo" type="text" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:border-emerald-500 outline-none transition-all" />
            </div>
            <div class="col-span-2 space-y-1">
              <label class="text-[10px] font-bold text-slate-400 uppercase ml-1">Complete Address</label>
              <input v-model="form.Address" type="text" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:border-emerald-500 outline-none transition-all" />
            </div>
          </div>

          <div class="space-y-1">
            <div class="flex justify-between items-center">
              <label class="text-[10px] font-bold text-slate-400 uppercase ml-1">Password</label>
              <span :class="isPasswordValid ? 'text-emerald-600' : 'text-rose-500'" class="text-[10px] font-bold">
                {{ form.Password.length }}/8-16 characters
              </span>
            </div>
            <input 
              v-model="form.Password" 
              type="password" 
              maxlength="16"
              placeholder="8 to 16 characters" 
              class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm focus:border-emerald-500 outline-none transition-all" 
            />
          </div>
        </div>

        <div class="p-6 bg-slate-50 border-t border-slate-100 flex gap-3">
          <button @click="showAddModal = false" class="flex-1 py-3 font-bold text-slate-400 hover:text-slate-600 cursor-pointer">Cancel</button>
          <button 
            @click="handleSave" 
            :disabled="isLoading || !isPasswordValid" 
            class="flex-1 py-3 bg-emerald-600 text-white rounded-xl font-bold shadow-lg shadow-emerald-100 disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer transition-all active:scale-95"
          >
            {{ isLoading ? 'PROCESSING...' : 'SAVE RECORD' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import axios from 'axios'
// Add all the missing icon imports here
import { 
  LayoutDashboard, 
  Users, 
  UserRound, 
  Stethoscope, 
  Syringe, 
  Calendar, 
  FileText, 
  Plus, 
  X, 
  RefreshCw 
} from 'lucide-vue-next'

const API_BASE = 'http://localhost:57147/api/Vaccination'
const parentList = ref([])
const showAddModal = ref(false)
const isLoading = ref(false)

const form = reactive({
  GivenName: '', MiddleName: '', LastName: '',
  Email: '', ContactNo: '', BarangayNo: '',
  Address: '', Password: ''
})

// Navigation Menu Items
const navItems = [
  { id: 'dashboard', label: 'Dashboard', icon: LayoutDashboard, path: '/AdminHome' },
  { id: 'parent', label: 'Parent', icon: Users, path: '/StaffParent' },
  { id: 'records', label: 'Children Record', icon: UserRound, path: '/StaffChild' },
  { id: 'staff', label: 'Doctor / Staff', icon: Stethoscope, path: '/StaffDocNurse' },
  { id: 'vaccines', label: 'Vaccines', icon: Syringe, path: '/StaffVaccine' },
  { id: 'calendar', label: 'Calendar', icon: Calendar, path: '/calendar' },
  { id: 'reports', label: 'Reports', icon: FileText, path: '/StaffReport' },
]

// VALIDATION: Password must be between 8 and 16 characters
const isPasswordValid = computed(() => {
  const len = form.Password.length
  return len >= 8 && len <= 16
})

const fetchParents = async () => {
  isLoading.value = true
  try {
    const response = await axios.get(`${API_BASE}/GetAllParents`)
    parentList.value = response.data
  } catch (err) {
    console.error("Fetch Error:", err)
  } finally {
    isLoading.value = false
  }
}

const handleSave = async () => {
  if (!isPasswordValid.value) {
    alert("Password must be between 8 and 16 characters.")
    return
  }

  isLoading.value = true
  try {
    const payload = {
      Id: "00000000-0000-0000-0000-000000000000",
      ...form
    }

    const res = await axios.post(`${API_BASE}/SaveParent`, payload)
    
    if (res.status === 200) {
      alert("Registration Successful!")
      showAddModal.value = false
      resetForm()
      await fetchParents() 
    }
  } catch (err) {
    if (err.response && err.response.status === 500) {
      alert("Error: This email address is already registered.")
    } else {
      alert("Database Error: Check your connection or SQL table structure.")
    }
  } finally {
    isLoading.value = false
  }
}

const resetForm = () => {
  Object.keys(form).forEach(k => form[k] = '')
}

onMounted(fetchParents)
</script>