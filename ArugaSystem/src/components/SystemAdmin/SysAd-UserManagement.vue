<script setup>
import { ref, computed, reactive } from 'vue'

/* ----------------------------- Sidebar state ------------------------------ */
const isCollapsed = ref(false)
const toggleSidebar = () => (isCollapsed.value = !isCollapsed.value)

const navItems = [
  { label: 'Dashboard', icon: '🏠' },
  { label: 'User Management', icon: '👥' },
  { label: 'Vaccine Management', icon: '💉' },
  { label: 'Inventory', icon: '📦' },
  { label: 'Notifications', icon: '🔔' },
  { label: 'Reports', icon: '📊' },
  { label: 'Audit Logs', icon: '📋' },
  { label: 'Settings', icon: '⚙️' },
]
const activeNav = ref('User Management')

/* -------------------------------- Role meta -------------------------------- */
const roleMeta = {
  Parent: { tint: 'bg-teal-50', text: 'text-teal-700' },
  Staff: { tint: 'bg-blue-50', text: 'text-blue-700' },
  Healthworker: { tint: 'bg-emerald-50', text: 'text-emerald-700' },
  'System Admin': { tint: 'bg-amber-50', text: 'text-amber-700' },
}

const statusMeta = {
  Active: { tint: 'bg-emerald-50', text: 'text-emerald-700', dot: 'bg-emerald-500' },
  Inactive: { tint: 'bg-slate-100', text: 'text-slate-600', dot: 'bg-slate-400' },
  Archived: { tint: 'bg-rose-50', text: 'text-rose-600', dot: 'bg-rose-500' },
}

/* -------------------------------- User data -------------------------------- */
const users = ref([
  { id: 1, firstName: 'Elena', lastName: 'Cruz', username: 'ecruz', email: 'elena.cruz@aruga.health', contact: '+63 917 200 1145', role: 'Healthworker', status: 'Active', lastLogin: 'Today, 9:41 AM', created: 'Jan 14, 2025' },
  { id: 2, firstName: 'Renzo', lastName: 'Miguel', username: 'rmiguel', email: 'renzo.miguel@aruga.health', contact: '+63 917 322 5590', role: 'System Admin', status: 'Active', lastLogin: 'Today, 8:05 AM', created: 'Nov 02, 2024' },
  { id: 3, firstName: 'Bea', lastName: 'Fernandez', username: 'bfernandez', email: 'bea.fernandez@aruga.health', contact: '+63 918 774 2201', role: 'Healthworker', status: 'Active', lastLogin: 'Yesterday, 4:18 PM', created: 'Feb 20, 2025' },
  { id: 4, firstName: 'Marites', lastName: 'Santos', username: 'msantos', email: 'marites.santos@gmail.com', contact: '+63 920 441 8832', role: 'Parent', status: 'Active', lastLogin: '2 days ago', created: 'Mar 08, 2025' },
  { id: 5, firstName: 'Jhun', lastName: 'Aquino', username: 'jaquino', email: 'jhun.aquino@aruga.health', contact: '+63 917 663 0072', role: 'Staff', status: 'Inactive', lastLogin: '3 weeks ago', created: 'Aug 19, 2024' },
  { id: 6, firstName: 'Liza', lastName: 'Domingo', username: 'ldomingo', email: 'liza.domingo@gmail.com', contact: '+63 919 205 6671', role: 'Parent', status: 'Active', lastLogin: '5 hrs ago', created: 'Apr 30, 2025' },
  { id: 7, firstName: 'Carlo', lastName: 'Reyes', username: 'creyes', email: 'carlo.reyes@aruga.health', contact: '+63 917 880 4432', role: 'Staff', status: 'Archived', lastLogin: '4 months ago', created: 'May 11, 2023' },
  { id: 8, firstName: 'Angeli', lastName: 'Torres', username: 'atorres', email: 'angeli.torres@gmail.com', contact: '+63 921 336 9081', role: 'Parent', status: 'Inactive', lastLogin: '1 month ago', created: 'Jun 25, 2025' },
])

/* ---------------------------- Toolbar / filters ---------------------------- */
const searchQuery = ref('')
const roleFilter = ref('All Users')
const statusFilter = ref('All')

const filteredUsers = computed(() =>
  users.value.filter((u) => {
    const q = searchQuery.value.trim().toLowerCase()
    const matchesSearch =
      !q ||
      `${u.firstName} ${u.lastName}`.toLowerCase().includes(q) ||
      u.username.toLowerCase().includes(q) ||
      u.email.toLowerCase().includes(q)
    const matchesRole = roleFilter.value === 'All Users' || u.role === roleFilter.value
    const matchesStatus = statusFilter.value === 'All' || u.status === statusFilter.value
    return matchesSearch && matchesRole && matchesStatus
  })
)

/* -------------------------------- Summary ---------------------------------- */
const summary = computed(() => ({
  total: users.value.length,
  active: users.value.filter((u) => u.status === 'Active').length,
  inactive: users.value.filter((u) => u.status === 'Inactive').length,
  archived: users.value.filter((u) => u.status === 'Archived').length,
}))

const initials = (u) => `${u.firstName[0] ?? ''}${u.lastName[0] ?? ''}`.toUpperCase()

/* ------------------------------ Row actions menu ---------------------------- */
const openMenuId = ref(null)
const toggleMenu = (id) => (openMenuId.value = openMenuId.value === id ? null : id)
const closeMenu = () => (openMenuId.value = null)

const setStatus = (user, status) => {
  user.status = status
  closeMenu()
}

/* -------------------------------- Details drawer ---------------------------- */
const showDrawer = ref(false)
const selectedUser = ref(null)
const openDrawer = (user) => {
  selectedUser.value = user
  showDrawer.value = true
  closeMenu()
}
const closeDrawer = () => (showDrawer.value = false)

/* --------------------------------- Add modal --------------------------------- */
const showAddModal = ref(false)
const addForm = reactive({
  role: 'Parent',
  firstName: '',
  lastName: '',
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
})
const openAddModal = () => {
  Object.assign(addForm, { role: 'Parent', firstName: '', lastName: '', username: '', email: '', password: '', confirmPassword: '' })
  showAddModal.value = true
}
const createUser = () => {
  users.value.unshift({
    id: Date.now(),
    firstName: addForm.firstName || 'New',
    lastName: addForm.lastName || 'User',
    username: addForm.username || 'newuser',
    email: addForm.email,
    contact: '—',
    role: addForm.role,
    status: 'Active',
    lastLogin: 'Never',
    created: 'Today',
  })
  showAddModal.value = false
}

/* --------------------------------- Edit modal --------------------------------- */
const showEditModal = ref(false)
const editForm = reactive({ id: null, role: '', firstName: '', lastName: '', username: '', email: '' })
const openEditModal = (user) => {
  Object.assign(editForm, { id: user.id, role: user.role, firstName: user.firstName, lastName: user.lastName, username: user.username, email: user.email })
  showEditModal.value = true
  closeMenu()
}
const saveEdit = () => {
  const u = users.value.find((x) => x.id === editForm.id)
  if (u) Object.assign(u, editForm)
  showEditModal.value = false
}
</script>

<template>
  <div class="min-h-screen bg-slate-50 flex text-slate-900" @click="closeMenu">
    <!-- ============================ SIDEBAR ============================ -->
    <aside
      :class="[isCollapsed ? 'w-20' : 'w-[260px]']"
      class="hidden md:flex flex-col shrink-0 sticky top-0 h-screen bg-white border-r border-slate-200 transition-all duration-300 ease-in-out"
    >
      <div class="h-[70px] flex items-center gap-3 px-5 border-b border-slate-200 shrink-0">
        <div class="w-9 h-9 rounded-lg bg-emerald-600 flex items-center justify-center shrink-0">
          <span class="text-white font-bold text-sm">A</span>
        </div>
        <span v-if="!isCollapsed" class="font-bold text-slate-900 tracking-tight whitespace-nowrap overflow-hidden">Aruga</span>
      </div>

      <nav class="flex-1 overflow-y-auto py-4 px-3 space-y-1">
        <button
          v-for="item in navItems"
          :key="item.label"
          @click="activeNav = item.label"
          :class="[
            activeNav === item.label ? 'bg-emerald-50 text-emerald-700' : 'text-slate-600 hover:bg-slate-50 hover:text-slate-900',
          ]"
          class="w-full flex items-center gap-3 rounded-lg px-3 py-2.5 text-sm font-medium transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
        >
          <span class="text-base shrink-0" aria-hidden="true">{{ item.icon }}</span>
          <span v-if="!isCollapsed" class="truncate">{{ item.label }}</span>
        </button>
      </nav>

      <div class="border-t border-slate-200 p-3 shrink-0 space-y-2">
        <div class="flex items-center gap-3 px-2 py-2">
          <div class="w-9 h-9 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-xs font-bold shrink-0">RM</div>
          <div v-if="!isCollapsed" class="min-w-0">
            <p class="text-sm font-semibold text-slate-900 truncate">Renzo Miguel</p>
            <p class="text-xs text-slate-500 truncate">System Admin</p>
          </div>
        </div>
        <button class="w-full flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium text-rose-600 hover:bg-rose-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-rose-400">
          <span class="text-base shrink-0" aria-hidden="true">🚪</span>
          <span v-if="!isCollapsed">Log out</span>
        </button>
        <button @click="toggleSidebar" class="w-full flex items-center justify-center rounded-lg px-3 py-2 text-xs font-medium text-slate-400 hover:bg-slate-50 hover:text-slate-600 transition-colors">
          <span :class="isCollapsed ? 'rotate-180' : ''" class="transition-transform inline-block">◀</span>
        </button>
      </div>
    </aside>

    <!-- ============================ MAIN ============================ -->
    <div class="flex-1 min-w-0 flex flex-col">
      <!-- Top navbar -->
      <header class="h-[70px] sticky top-0 z-20 bg-white border-b border-slate-200 flex items-center justify-between px-6 gap-4">
        <div class="min-w-0">
          <h1 class="text-lg font-bold text-slate-900 truncate">User Management</h1>
          <p class="text-xs text-slate-500 truncate">Dashboard / User Management</p>
        </div>
        <div class="flex items-center gap-3 shrink-0">
          <button class="relative w-9 h-9 rounded-lg flex items-center justify-center text-slate-500 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500">
            <span aria-hidden="true">🔔</span>
            <span class="absolute top-1.5 right-1.5 w-2 h-2 rounded-full bg-rose-500 ring-2 ring-white"></span>
          </button>
          <button class="w-9 h-9 rounded-lg flex items-center justify-center text-slate-500 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500">
            <span aria-hidden="true">⚙️</span>
          </button>
          <div class="w-9 h-9 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-xs font-bold shrink-0">RM</div>
        </div>
      </header>

      <!-- Content -->
      <main class="p-6 space-y-6">
        <!-- Summary cards -->
        <section class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Total Users</p>
              <div class="bg-teal-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">👥</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.total }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Active Users</p>
              <div class="bg-emerald-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">✅</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.active }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Inactive Users</p>
              <div class="bg-slate-100 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">💤</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.inactive }}</p>
          </div>
          <div class="min-w-0 bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
            <div class="flex items-start justify-between gap-2">
              <p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Archived Users</p>
              <div class="bg-rose-50 w-8 h-8 rounded-lg flex items-center justify-center shrink-0 text-sm">🗄️</div>
            </div>
            <p class="mt-2 text-2xl font-extrabold text-slate-900">{{ summary.archived }}</p>
          </div>
        </section>

        <!-- Toolbar -->
        <section class="bg-white border border-slate-200 rounded-xl p-4 shadow-sm">
          <div class="flex flex-col lg:flex-row lg:items-center gap-3">
            <div class="relative flex-1 min-w-0">
              <span class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-400 text-sm" aria-hidden="true">🔍</span>
              <input
                v-model="searchQuery"
                type="text"
                placeholder="Search by name, username, or email..."
                class="w-full pl-9 pr-3 py-2 text-sm rounded-lg border border-slate-200 bg-slate-50 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
              />
            </div>

            <select
              v-model="roleFilter"
              class="text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-slate-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
            >
              <option>All Users</option>
              <option>Parent</option>
              <option>Staff</option>
              <option>Healthworker</option>
              <option>System Admin</option>
            </select>

            <select
              v-model="statusFilter"
              class="text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2 text-slate-700 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors"
            >
              <option>All</option>
              <option>Active</option>
              <option>Inactive</option>
              <option>Archived</option>
            </select>

            <div class="flex items-center gap-2 shrink-0">
              <button class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500">
                Export
              </button>
              <button
                @click="openAddModal"
                class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
              >
                + Add User
              </button>
            </div>
          </div>
        </section>

        <!-- User table -->
        <section class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
          <div class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead>
                <tr class="border-b border-slate-200 bg-slate-50/60">
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-5 py-3">Profile</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Full Name</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Username</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Role</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Status</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Last Login</th>
                  <th class="text-left font-semibold text-slate-500 text-xs uppercase tracking-wide px-3 py-3">Created Date</th>
                  <th class="text-right font-semibold text-slate-500 text-xs uppercase tracking-wide px-5 py-3">Actions</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="user in filteredUsers"
                  :key="user.id"
                  class="border-b border-slate-100 last:border-0 hover:bg-slate-50 transition-colors"
                >
                  <td class="px-5 py-3">
                    <div class="w-9 h-9 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-xs font-bold">
                      {{ initials(user) }}
                    </div>
                  </td>
                  <td class="px-3 py-3 font-semibold text-slate-900 whitespace-nowrap">{{ user.firstName }} {{ user.lastName }}</td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ user.username }}</td>
                  <td class="px-3 py-3">
                    <span :class="[roleMeta[user.role].tint, roleMeta[user.role].text]" class="text-xs font-semibold px-2.5 py-1 rounded-full whitespace-nowrap">
                      {{ user.role }}
                    </span>
                  </td>
                  <td class="px-3 py-3">
                    <span :class="[statusMeta[user.status].tint, statusMeta[user.status].text]" class="inline-flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full whitespace-nowrap">
                      <span :class="statusMeta[user.status].dot" class="w-1.5 h-1.5 rounded-full"></span>
                      {{ user.status }}
                    </span>
                  </td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ user.lastLogin }}</td>
                  <td class="px-3 py-3 text-slate-500 whitespace-nowrap">{{ user.created }}</td>
                  <td class="px-5 py-3 text-right relative">
                    <button
                      @click.stop="toggleMenu(user.id)"
                      class="text-slate-400 hover:text-slate-700 hover:bg-slate-100 rounded-lg w-8 h-8 inline-flex items-center justify-center transition-colors focus:outline-none focus-visible:ring-2 focus-visible:ring-emerald-500"
                    >
                      ⋮
                    </button>

                    <div
                      v-if="openMenuId === user.id"
                      @click.stop
                      class="absolute right-5 top-11 z-30 w-48 bg-white border border-slate-200 rounded-lg shadow-md py-1 text-left"
                    >
                      <button @click="openDrawer(user)" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">View Details</button>
                      <button @click="openEditModal(user)" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">Edit Information</button>
                      <button @click="closeMenu" class="w-full text-left px-3.5 py-2 text-sm text-slate-700 hover:bg-slate-50 transition-colors">Reset Password</button>
                      <div class="my-1 border-t border-slate-100"></div>
                      <button v-if="user.status !== 'Active'" @click="setStatus(user, 'Active')" class="w-full text-left px-3.5 py-2 text-sm text-emerald-700 hover:bg-emerald-50 transition-colors">Activate</button>
                      <button v-if="user.status === 'Active'" @click="setStatus(user, 'Inactive')" class="w-full text-left px-3.5 py-2 text-sm text-slate-600 hover:bg-slate-50 transition-colors">Deactivate</button>
                      <button v-if="user.status !== 'Archived'" @click="setStatus(user, 'Archived')" class="w-full text-left px-3.5 py-2 text-sm text-rose-600 hover:bg-rose-50 transition-colors">Archive</button>
                    </div>
                  </td>
                </tr>

                <tr v-if="filteredUsers.length === 0">
                  <td colspan="8" class="px-5 py-12 text-center text-sm text-slate-400">No users match your search or filters.</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>
      </main>
    </div>

    <!-- ============================ DETAILS DRAWER ============================ -->
    <transition name="fade">
      <div v-if="showDrawer" class="fixed inset-0 bg-slate-900/30 z-40" @click="closeDrawer"></div>
    </transition>
    <transition name="slide">
      <aside v-if="showDrawer" class="fixed top-0 right-0 h-screen w-full max-w-sm bg-white border-l border-slate-200 shadow-lg z-50 flex flex-col">
        <div class="h-[70px] flex items-center justify-between px-5 border-b border-slate-200 shrink-0">
          <h2 class="text-sm font-bold text-slate-900">User Details</h2>
          <button @click="closeDrawer" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
        </div>

        <div v-if="selectedUser" class="flex-1 overflow-y-auto p-6 space-y-6">
          <div class="flex flex-col items-center text-center gap-3">
            <div class="w-16 h-16 rounded-full bg-emerald-100 text-emerald-700 flex items-center justify-center text-lg font-bold">
              {{ initials(selectedUser) }}
            </div>
            <div>
              <p class="text-base font-bold text-slate-900">{{ selectedUser.firstName }} {{ selectedUser.lastName }}</p>
              <span :class="[roleMeta[selectedUser.role].tint, roleMeta[selectedUser.role].text]" class="mt-1 inline-block text-xs font-semibold px-2.5 py-1 rounded-full">
                {{ selectedUser.role }}
              </span>
            </div>
          </div>

          <div class="bg-slate-50 rounded-lg divide-y divide-slate-200">
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Username</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedUser.username }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Email</span>
              <span class="text-sm font-medium text-slate-900 truncate ml-4">{{ selectedUser.email }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Contact Number</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedUser.contact }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Account Status</span>
              <span :class="[statusMeta[selectedUser.status].tint, statusMeta[selectedUser.status].text]" class="inline-flex items-center gap-1.5 text-xs font-semibold px-2.5 py-1 rounded-full">
                <span :class="statusMeta[selectedUser.status].dot" class="w-1.5 h-1.5 rounded-full"></span>
                {{ selectedUser.status }}
              </span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Date Created</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedUser.created }}</span>
            </div>
            <div class="flex items-center justify-between px-4 py-3">
              <span class="text-xs text-slate-500">Last Login</span>
              <span class="text-sm font-medium text-slate-900">{{ selectedUser.lastLogin }}</span>
            </div>
          </div>
        </div>

        <div class="border-t border-slate-200 p-4 flex items-center gap-2 shrink-0">
          <button @click="openEditModal(selectedUser)" class="flex-1 text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors">Edit User</button>
          <button class="flex-1 text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Reset Password</button>
          <button @click="closeDrawer" class="text-sm font-semibold px-4 py-2 rounded-lg text-slate-500 hover:bg-slate-50 transition-colors">Close</button>
        </div>
      </aside>
    </transition>

    <!-- ============================ ADD USER MODAL ============================ -->
    <transition name="fade">
      <div v-if="showAddModal" class="fixed inset-0 bg-slate-900/40 z-40 flex items-center justify-center p-4" @click.self="showAddModal = false">
        <div class="bg-white rounded-xl shadow-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto">
          <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200">
            <h2 class="text-base font-bold text-slate-900">Add User</h2>
            <button @click="showAddModal = false" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
          </div>

          <div class="p-6 grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="sm:col-span-2">
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Role</label>
              <select v-model="addForm.role" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors">
                <option>Parent</option>
                <option>Staff</option>
                <option>Healthworker</option>
                <option>System Admin</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">First Name</label>
              <input v-model="addForm.firstName" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Last Name</label>
              <input v-model="addForm.lastName" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Username</label>
              <input v-model="addForm.username" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Email</label>
              <input v-model="addForm.email" type="email" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Password</label>
              <input v-model="addForm.password" type="password" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Confirm Password</label>
              <input v-model="addForm.confirmPassword" type="password" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 px-6 py-4 border-t border-slate-200">
            <button @click="showAddModal = false" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Cancel</button>
            <button @click="createUser" class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors">Create User</button>
          </div>
        </div>
      </div>
    </transition>

    <!-- ============================ EDIT USER MODAL ============================ -->
    <transition name="fade">
      <div v-if="showEditModal" class="fixed inset-0 bg-slate-900/40 z-40 flex items-center justify-center p-4" @click.self="showEditModal = false">
        <div class="bg-white rounded-xl shadow-lg w-full max-w-2xl max-h-[90vh] overflow-y-auto">
          <div class="flex items-center justify-between px-6 py-4 border-b border-slate-200">
            <h2 class="text-base font-bold text-slate-900">Edit User</h2>
            <button @click="showEditModal = false" class="w-8 h-8 rounded-lg flex items-center justify-center text-slate-400 hover:bg-slate-50 transition-colors">✕</button>
          </div>

          <div class="p-6 grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div class="sm:col-span-2">
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Role</label>
              <select v-model="editForm.role" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors">
                <option>Parent</option>
                <option>Staff</option>
                <option>Healthworker</option>
                <option>System Admin</option>
              </select>
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">First Name</label>
              <input v-model="editForm.firstName" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Last Name</label>
              <input v-model="editForm.lastName" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Username</label>
              <input v-model="editForm.username" type="text" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
            <div>
              <label class="block text-xs font-semibold text-slate-500 mb-1.5">Email</label>
              <input v-model="editForm.email" type="email" class="w-full text-sm rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-emerald-500 focus:bg-white transition-colors" />
            </div>
          </div>

          <div class="flex items-center justify-end gap-2 px-6 py-4 border-t border-slate-200">
            <button @click="showEditModal = false" class="text-sm font-semibold px-4 py-2 rounded-lg border border-slate-200 text-slate-600 hover:bg-slate-50 transition-colors">Cancel</button>
            <button @click="saveEdit" class="text-sm font-semibold px-4 py-2 rounded-lg bg-emerald-600 text-white hover:bg-emerald-700 transition-colors">Save Changes</button>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
.slide-enter-active, .slide-leave-active { transition: transform 0.25s ease; }
.slide-enter-from, .slide-leave-to { transform: translateX(100%); }

@media (prefers-reduced-motion: reduce) {
  * { transition-duration: 0.01ms !important; }
}
</style>