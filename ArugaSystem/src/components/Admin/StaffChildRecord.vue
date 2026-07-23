<template>
  <div class="flex h-screen bg-slate-50 font-sans antialiased text-slate-900">
    
    <!-- SIDEBAR -->
    <aside class="w-64 bg-white border-r border-slate-200 flex flex-col shrink-0">
      <div class="p-6 text-center">
        <div class="flex items-center gap-3 px-2">
          <div class="w-8 h-8 bg-emerald-600 rounded flex items-center justify-center text-white font-bold">A</div>
          <span class="text-xl font-bold tracking-tight">Aruga</span>
        </div>
      </div>
      
      <nav class="flex-1 px-3 space-y-1 overflow-y-auto custom-scrollbar">
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
        <button @click="openAddModal" :disabled="isLoading" class="bg-emerald-600 text-white px-5 py-2.5 rounded-lg text-xs font-bold hover:bg-emerald-700 transition-all flex items-center gap-2 shadow-sm active:scale-95 disabled:opacity-50">
          <Plus class="w-4 h-4" /> REGISTER CHILD
        </button>
      </header>

      <main class="flex-1 p-8 overflow-y-auto custom-scrollbar">
        <div class="max-w-6xl mx-auto">
          <div class="flex items-center justify-between mb-6">
            <h1 class="text-2xl font-bold text-slate-800">Children Records</h1>
            <button @click="fetchChildren" class="text-slate-400 hover:text-emerald-600 transition-colors p-2 rounded-full hover:bg-slate-100">
              <RefreshCw class="w-5 h-5" :class="{'animate-spin': isLoading}" />
            </button>
          </div>

          <!-- TABLE -->
          <div class="bg-white border border-slate-200 rounded-2xl shadow-sm overflow-hidden">
            <table class="w-full text-left border-collapse">
              <thead>
                <tr class="bg-slate-50 border-b border-slate-200">
                  <th class="px-6 py-4 text-[10px] font-bold text-slate-400 uppercase tracking-widest">Full Name</th>
                  <th class="px-6 py-4 text-[10px] font-bold text-slate-400 uppercase tracking-widest">Family No / Brgy</th>
                  <th class="px-6 py-4 text-[10px] font-bold text-slate-400 uppercase tracking-widest text-center">Actions</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr v-for="child in childrenList" :key="child.childId" class="hover:bg-slate-50/50 transition-colors">
                  <td class="px-6 py-4">
                    <p class="font-bold text-slate-700">{{ child.firstName }} {{ child.lastName }}</p>
                    <p class="text-[11px] text-slate-400">#{{ child.childId?.substring(0,8) }}</p>
                  </td>
                  <td class="px-6 py-4">
                    <p class="text-sm text-slate-600">ID: {{ child.familyNo }}</p>
                    <p class="text-[10px] text-emerald-600 font-bold uppercase">Brgy {{ child.barangay }}</p>
                  </td>
                  <td class="px-6 py-4 text-center">
                    <button @click="viewProfile(child)" class="bg-white border border-slate-200 text-slate-600 px-4 py-2 rounded-xl text-xs font-bold hover:bg-emerald-600 hover:text-white transition-all shadow-sm">
                      VIEW PROFILE
                    </button>
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
      
      <div class="relative bg-white w-full max-w-4xl rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[95vh]">
        <!-- Header -->
        <div class="p-6 border-b border-slate-100 flex justify-between items-center bg-white sticky top-0 z-10">
          <div>
            <h3 class="font-bold text-slate-800 text-lg">Register New Child</h3>
            <p class="text-[11px] text-slate-400 uppercase font-bold tracking-wider">Health Intake Form</p>
          </div>
          <X @click="showAddModal = false" class="w-5 h-5 text-slate-400 cursor-pointer hover:text-slate-600" />
        </div>

        <!-- Scrollable Form Content -->
        <div class="p-8 space-y-10 overflow-y-auto custom-scrollbar">
          
          <!-- STEP 1: PARENT INFORMATION (LINK OR MANUAL) -->
          <div class="p-6 bg-emerald-50 rounded-2xl border border-emerald-100 space-y-4">
            <div class="flex justify-between items-end">
                <label class="text-[10px] font-bold text-emerald-700 uppercase tracking-widest">Step 1: Parent/Guardian Information</label>
                <span class="text-[9px] text-emerald-600 bg-white px-2 py-1 rounded border border-emerald-100">Search to link existing account</span>
            </div>

            <div class="grid grid-cols-2 gap-4">
              <div class="space-y-1">
                <label class="text-[11px] font-bold text-emerald-800 ml-1">Assign To Role</label>
                <select v-model="selectedRole" class="w-full px-4 py-2.5 bg-white border border-emerald-200 rounded-xl text-sm outline-none">
                  <option value="Mother">Mother</option>
                  <option value="Father">Father</option>
                  <option value="Guardian">Guardian</option>
                </select>
              </div>
              <div class="space-y-1 relative">
                <label class="text-[11px] font-bold text-emerald-800 ml-1">Type Name or Search Email</label>
                <input 
                    v-model="parentSearch" 
                    @input="handleManualNameEntry"
                    type="text" 
                    placeholder="Type name manually or search account..." 
                    class="w-full px-4 py-2.5 bg-white border border-emerald-200 rounded-xl text-sm outline-none" 
                />
                
                <div v-if="filteredParents.length > 0" class="absolute left-0 right-0 z-[70] bg-white border border-slate-200 shadow-xl rounded-xl mt-1 overflow-hidden">
                  <div v-for="p in filteredParents" :key="p.email" @click="linkExistingAccount(p)" class="px-4 py-3 hover:bg-emerald-50 cursor-pointer border-b border-slate-50 last:border-none">
                    <p class="text-sm font-bold text-slate-700">{{ p.lastName }}, {{ p.givenName }}</p>
                    <p class="text-[10px] text-emerald-600 font-bold">LINK ACCOUNT: {{ p.email }}</p>
                  </div>
                </div>
              </div>
            </div>
            
            <!-- Selection Status Preview -->
            <div class="grid grid-cols-3 gap-3 pt-2">
                <div v-for="role in ['Mother', 'Father', 'Guardian']" :key="role" 
                     class="p-3 rounded-xl border transition-all"
                     :class="childForm[role+'Name'] ? 'bg-white border-emerald-200 shadow-sm' : 'bg-emerald-50/50 border-dashed border-emerald-200'">
                    <div class="flex justify-between items-start mb-1">
                        <span class="text-[9px] font-black uppercase text-emerald-800">{{ role }}</span>
                        <button v-if="childForm[role+'Name']" @click="clearParentRole(role)" class="text-emerald-300 hover:text-red-400">
                            <X class="w-3 h-3"/>
                        </button>
                    </div>
                    <p class="text-xs font-bold text-slate-700 truncate">
                        {{ childForm[role+'Name'] || 'Empty' }}
                    </p>
                    <p v-if="childForm[role+'Email']" class="text-[8px] text-emerald-500 font-bold uppercase mt-1">
                        Linked: {{ childForm[role+'Email'] }}
                    </p>
                    <p v-else-if="childForm[role+'Name']" class="text-[8px] text-slate-400 font-medium uppercase mt-1">
                        Manual Entry
                    </p>
                </div>
            </div>
          </div>

          <!-- STEP 2: CHILD IDENTITY -->
          <div class="space-y-4">
            <label class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Step 2: Child Identity</label>
            <div class="grid grid-cols-3 gap-4">
              <div class="space-y-1">
                <label class="text-[11px] font-bold text-slate-500 ml-1">First Name</label>
                <input v-model="childForm.FirstName" type="text" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none focus:border-emerald-500" />
              </div>
              <div class="space-y-1">
                <label class="text-[11px] font-bold text-slate-500 ml-1">Middle Name</label>
                <input v-model="childForm.MiddleName" type="text" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none focus:border-emerald-500" />
              </div>
              <div class="space-y-1">
                <label class="text-[11px] font-bold text-slate-500 ml-1">Last Name</label>
                <input v-model="childForm.LastName" type="text" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none focus:border-emerald-500" />
              </div>
            </div>

            <div class="grid grid-cols-3 gap-4">
              <div class="space-y-1">
                <label class="text-[11px] font-bold text-slate-500 ml-1">Birth Date</label>
                <input v-model="childForm.BirthDate" type="date" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none" />
              </div>
              <div class="space-y-1">
                <label class="text-[11px] font-bold text-slate-500 ml-1">Sex</label>
                <select v-model="childForm.Sex" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none">
                  <option value="Male">Male</option>
                  <option value="Female">Female</option>
                </select>
              </div>
              <div class="space-y-1">
                <label class="text-[11px] font-bold text-slate-500 ml-1">Place of Birth</label>
                <input v-model="childForm.PlaceOfBirth" type="text" placeholder="City/Hospital" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none" />
              </div>
            </div>
          </div>

          <!-- STEP 3: PHYSICAL METRICS & ADDRESS -->
          <div class="space-y-4">
            <label class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">Step 3: Medical & Location</label>
            <div class="grid grid-cols-4 gap-4">
              <div class="space-y-1">
                <label class="text-[11px] font-bold text-slate-500 ml-1">Birth Weight (kg)</label>
                <input v-model="childForm.BirthWeight" type="number" step="0.01" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none" />
              </div>
              <div class="space-y-1">
                <label class="text-[11px] font-bold text-slate-500 ml-1">Birth Height (cm)</label>
                <input v-model="childForm.BirthHeight" type="number" step="0.1" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none" />
              </div>
              <div class="space-y-1">
                <label class="text-[11px] font-bold text-slate-500 ml-1">Barangay</label>
                <input v-model="childForm.Barangay" type="text" placeholder="e.g. 704" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none" />
              </div>
              <div class="space-y-1">
                <label class="text-[11px] font-bold text-slate-500 ml-1">Family No.</label>
                <input v-model="childForm.FamilyNo" type="text" class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none" />
              </div>
            </div>
            <div class="space-y-1">
              <label class="text-[11px] font-bold text-slate-500 ml-1">Full Home Address</label>
              <input v-model="childForm.Address" type="text" placeholder="Street Name, Building, etc." class="w-full px-4 py-2.5 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none" />
            </div>
          </div>
        </div>

        <!-- Footer Actions -->
        <div class="p-6 bg-slate-50 border-t border-slate-100 flex gap-3 sticky bottom-0 z-10">
          <button @click="showAddModal = false" class="flex-1 py-3 font-bold text-slate-400 hover:text-slate-600 transition-colors">Cancel</button>
          <button @click="handleSave" :disabled="isLoading" class="flex-1 py-3 bg-emerald-600 text-white rounded-xl font-bold shadow-lg hover:bg-emerald-700 active:scale-95 transition-all flex items-center justify-center">
            <span v-if="isLoading" class="animate-spin mr-2"><RefreshCw class="w-4 h-4" /></span>
            {{ isLoading ? 'SAVING...' : 'REGISTER CHILD' }}
          </button>
        </div>  
      </div>
    </div>

    <!-- VIEW / EDIT PROFILE MODAL -->
    <div v-if="showProfileModal" class="fixed inset-0 z-[60] flex items-center justify-center p-4">
      <div class="absolute inset-0 bg-slate-900/60 backdrop-blur-sm" @click="showProfileModal = false"></div>
      
      <div class="relative bg-white w-full max-w-4xl rounded-3xl shadow-2xl overflow-hidden flex flex-col max-h-[95vh]">
        <div class="p-6 border-b border-slate-100 flex justify-between items-center bg-white sticky top-0 z-10">
          <div>
            <h3 class="font-bold text-slate-800 text-lg">{{ isEditing ? 'Edit Profile' : 'Child Profile' }}</h3>
            <p class="text-[11px] text-slate-400 uppercase font-bold tracking-wider">
              {{ isEditing ? 'Modify health records' : 'Information Overview' }}
            </p>
          </div>
          <div class="flex items-center gap-3">
            <button @click="isEditing = !isEditing" class="text-xs font-bold px-3 py-1.5 rounded-lg border border-slate-200 hover:bg-slate-50 transition-colors">
              {{ isEditing ? 'CANCEL EDIT' : 'EDIT PROFILE' }}
            </button>
            <X @click="showProfileModal = false" class="w-5 h-5 text-slate-400 cursor-pointer hover:text-slate-600" />
          </div>
        </div>

        <div class="p-8 space-y-6 overflow-y-auto custom-scrollbar">
          <!-- Identity Section -->
          <div class="grid grid-cols-3 gap-6">
            <div v-for="field in ['firstName', 'middleName', 'lastName']" :key="field" class="space-y-1">
              <label class="text-[11px] font-bold text-slate-400 uppercase">{{ field.replace(/([A-Z])/g, ' $1') }}</label>
              <input v-if="isEditing" v-model="selectedChild[field]" class="w-full px-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none focus:border-emerald-500" />
              <p v-else class="text-sm font-bold text-slate-700">{{ selectedChild[field] || '---' }}</p>
            </div>
          </div>

          <div class="grid grid-cols-3 gap-6">
            <div class="space-y-1">
              <label class="text-[11px] font-bold text-slate-400 uppercase">Birth Date</label>
              <input v-if="isEditing" type="date" v-model="formattedBirthDate" class="w-full px-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none" />
              <p v-else class="text-sm font-bold text-slate-700">{{ selectedChild.birthDate ? new Date(selectedChild.birthDate).toLocaleDateString() : '---' }}</p>
            </div>
            <div class="space-y-1">
              <label class="text-[11px] font-bold text-slate-400 uppercase">Sex</label>
              <select v-if="isEditing" v-model="selectedChild.sex" class="w-full px-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-sm outline-none">
                <option value="Male">Male</option>
                <option value="Female">Female</option>
              </select>
              <p v-else class="text-sm font-bold text-slate-700">{{ selectedChild.sex }}</p>
            </div>
            <div class="space-y-1">
              <label class="text-[11px] font-bold text-slate-400 uppercase">Family Number</label>
              <input v-if="isEditing" v-model="selectedChild.familyNo" class="w-full px-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-sm" />
              <p v-else class="text-sm font-bold text-slate-700">{{ selectedChild.familyNo || 'N/A' }}</p>
            </div>
          </div>

          <hr class="border-slate-100" />

          <!-- Parent Names Section -->
          <div class="grid grid-cols-2 gap-6">
            <div v-for="role in ['mother', 'father', 'guardian']" :key="role" class="space-y-1">
              <label class="text-[11px] font-bold text-slate-400 uppercase">{{ role }} Name</label>
              <input v-if="isEditing" v-model="selectedChild[role + 'Name']" class="w-full px-4 py-2 bg-slate-50 border border-slate-200 rounded-xl text-sm" />
              <p v-else class="text-sm font-bold text-slate-700">{{ selectedChild[role + 'Name'] || 'Not Listed' }}</p>
            </div>
          </div>
        </div>

        <div v-if="isEditing" class="p-6 bg-slate-50 border-t border-slate-100 sticky bottom-0">
          <button @click="handleUpdate" :disabled="isLoading" class="w-full py-3 bg-emerald-600 text-white rounded-xl font-bold shadow-lg hover:bg-emerald-700 transition-all">
            {{ isLoading ? 'UPDATING...' : 'SAVE CHANGES' }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, computed } from 'vue'
import axios from 'axios'
import { 
  LayoutDashboard, Users, UserRound, Stethoscope, 
  Syringe, Plus, X, RefreshCw 
} from 'lucide-vue-next'

const API_BASE = 'http://localhost:57147/api/Vaccination'

// ==========================================
// STATE MANAGEMENT
// ==========================================
const childrenList = ref([])
const parentOptions = ref([]) 
const isLoading = ref(false)

// Registration Modal State
const showAddModal = ref(false)
const parentSearch = ref('')
const selectedRole = ref('Mother')
const childForm = reactive({
  FirstName: '', MiddleName: '', LastName: '', BirthDate: '', Sex: 'Male',
  PlaceOfBirth: '', Address: '', 
  MotherName: '', MotherEmail: '', MotherID: null,
  FatherName: '', FatherEmail: '', FatherID: null, 
  GuardianName: '', GuardianEmail: '', GuardianID: null, 
  BirthWeight: null, BirthHeight: null, 
  HealthCenter: 'Leveriza Health Center', Barangay: '', FamilyNo: ''
})

// Profile & Edit Modal State
const showProfileModal = ref(false)
const isEditing = ref(false)
const selectedChild = ref(null)

const navItems = [
  { id: 'dashboard', label: 'Dashboard', icon: LayoutDashboard, path: '/AdminHome' },
  { id: 'parent', label: 'Parent', icon: Users, path: '/StaffParent' },
  { id: 'records', label: 'Children Record', icon: UserRound, path: '/StaffChild' },
  { id: 'staff', label: 'Doctor / Staff', icon: Stethoscope, path: '/StaffDocNurse' },
  { id: 'vaccines', label: 'Vaccines', icon: Syringe, path: '/StaffVaccine' },
]

// ==========================================
// COMPUTED PROPERTIES
// ==========================================

// Search Logic for Parent Linking
const filteredParents = computed(() => {
  if (!parentSearch.value) return []
  const q = parentSearch.value.toLowerCase()
  return parentOptions.value.filter(p => 
    p.email?.toLowerCase().includes(q) || 
    p.givenName?.toLowerCase().includes(q) || 
    p.lastName?.toLowerCase().includes(q)
  ).slice(0, 5)
})

// Formats SQL Date (ISO) to HTML Input Date (YYYY-MM-DD)
const formattedBirthDate = computed({
  get: () => selectedChild.value?.birthDate ? selectedChild.value.birthDate.split('T')[0] : '',
  set: (val) => { if(selectedChild.value) selectedChild.value.birthDate = val }
})

// ==========================================
// METHODS
// ==========================================

const fetchChildren = async () => {
  isLoading.value = true
  try {
    const res = await axios.get(`${API_BASE}/GetAllChildren`)
    childrenList.value = res.data
  } catch (err) { 
    console.error("Fetch Error:", err) 
  } finally { 
    isLoading.value = false 
  }
}

const fetchParents = async () => {
  try {
    const res = await axios.get(`${API_BASE}/GetAllParents`)
    parentOptions.value = res.data
  } catch (err) { 
    console.error(err) 
  }
}

// Logic to open the Profile (View Mode)
const viewProfile = (child) => {
  selectedChild.value = { ...child } 
  isEditing.value = false
  showProfileModal.value = true
  fetchParents() // Load parents list for potential editing
}

// --- PARENT LINKING LOGIC ---

// 1. Handle manual typing
const handleManualNameEntry = () => {
  const role = selectedRole.value;
  childForm[`${role}Name`] = parentSearch.value;
  childForm[`${role}ID`] = null;     // Manual entry = No ID
  childForm[`${role}Email`] = '';    // Manual entry = No Email
}

// 2. Handle selecting from search (Link Account)
const linkExistingAccount = (parent) => {
  const role = selectedRole.value;
  const fullName = `${parent.givenName} ${parent.lastName}`;
  
  childForm[`${role}Name`] = fullName;
  childForm[`${role}ID`] = parent.id;
  childForm[`${role}Email`] = parent.email;
  
  parentSearch.value = ''; // Reset search input
}

// 3. Clear selected role
const clearParentRole = (role) => {
  childForm[`${role}Name`] = '';
  childForm[`${role}ID`] = null;
  childForm[`${role}Email`] = '';
}

// CREATE: Save New Child
const handleSave = async () => {
  if (!childForm.FirstName || !childForm.LastName) {
    alert("Please enter Child's Name.")
    return
  }
  isLoading.value = true
  try {
    const res = await axios.post(`${API_BASE}/SaveChild`, childForm)
    if (res.data.success) {
      alert("Child registered successfully!")
      showAddModal.value = false
      fetchChildren()
    }
  } catch (err) {
    alert("Error saving record.")
  } finally { 
    isLoading.value = false 
  }
}

// UPDATE: Save Changes to Existing Profile
const handleUpdate = async () => {
  if (!selectedChild.value.firstName || !selectedChild.value.lastName) {
    alert("Name fields are required.")
    return
  }

  isLoading.value = true
  try {
    const res = await axios.put(`${API_BASE}/UpdateChild/${selectedChild.value.childId}`, selectedChild.value)
    if (res.data.success) {
      alert("Profile updated successfully!")
      showProfileModal.value = false
      isEditing.value = false
      fetchChildren()
    }
  } catch (err) {
    console.error(err)
    alert("Failed to update profile.")
  } finally {
    isLoading.value = false
  }
}

const openAddModal = () => {
  Object.assign(childForm, {
    FirstName: '', MiddleName: '', LastName: '', BirthDate: '', Sex: 'Male',
    PlaceOfBirth: '', Address: '', 
    MotherName: '', MotherEmail: '', MotherID: null,
    FatherName: '', FatherEmail: '', FatherID: null, 
    GuardianName: '', GuardianEmail: '', GuardianID: null, 
    BirthWeight: null, BirthHeight: null, 
    HealthCenter: 'Leveriza Health Center', Barangay: '', FamilyNo: ''
  })
  parentSearch.value = '';
  showAddModal.value = true;
  fetchParents();
}

onMounted(fetchChildren)
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #E2E8F0; border-radius: 10px; }
</style>